using System;
using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using Unity.Netcode;
using UnityEngine;

namespace SteamLobby
{
    public class LobbyManager : MonoBehaviour
    {
        public static LobbyManager Instance { get; private set; }
        private LobbyConfiguration lobbyConfiguration;

        public LobbyConfiguration Configuration => lobbyConfiguration;

        private Lobby currentLobby;
        public Lobby CurrentLobby => currentLobby;

        public Action<Lobby> UIUpdate_OnLobbyEntered;
        public Action<Lobby> UIUpdate_OnMemberJoined;
        public Action<Lobby> UIUpdate_OnMemberLeft;
        public Action<Lobby> UIUpdate_OnMemberDisconnected;
        public Action<Lobby> UIUpdate_OnCloseConnection;
        public Action<Lobby> UIUpdate_OnHostMemberLeave;

        public ulong LocalId => SteamClient.SteamId;

        private const string HostID = "HostID";

        private enum LogType
        {
            INFO,
            WARNING,
            ERROR,
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void Start() => SubscribeToCallbacks();

        private void OnDestroy() => UnsubscribeToCallbacks();

        #region Lobby callbacks
        private void SubscribeToCallbacks()
        {
            if (!SteamClient.IsLoggedOn || !SteamClient.IsValid || !SteamClient.SteamId.IsValid)
            {
                SteamLobbyLog(LogType.ERROR, "Could not communicate with steam. Is it down?");
                return;
            }

            SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
            SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
            SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
            SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnected;

            SteamFriends.OnGameLobbyJoinRequested += OnLobbyJoinRequested;
        }

        private void UnsubscribeToCallbacks()
        {
            SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
            SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
            SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
            SteamMatchmaking.OnLobbyMemberDisconnected -= OnLobbyMemberDisconnected;

            SteamFriends.OnGameLobbyJoinRequested -= OnLobbyJoinRequested;
        }

        private void OnLobbyCreated(Result result, Lobby lobby)
        {
            switch (result)
            {
                case Result.OK:
                    SteamLobbyLog(LogType.INFO, "Lobby created successfully");
                    break;
                default:
                    SteamLobbyLog(
                        LogType.ERROR,
                        $"Error creating lobby: {Enum.GetName(typeof(Result), result)}"
                    );
                    break;
            }
        }

        private void OnLobbyEntered(Lobby lobby)
        {
            if (!lobby.IsOwnedBy(SteamClient.SteamId))
            {
                var transport = NetworkManager.Singleton.GetComponent<FacepunchTransport>();
                ulong target = ulong.Parse(lobby.GetData(HostID));
                transport.targetSteamId = target;
                NetworkManager.Singleton.StartClient();
            }

            UIUpdate_OnLobbyEntered?.Invoke(lobby);

            SteamLobbyLog(
                LogType.INFO,
                $"Entered lobby as: {SteamClient.Name} ({SteamClient.SteamId})"
            );
        }

        private void OnLobbyMemberLeave(Lobby lobby, Friend friend)
        {
            if (!NetworkManager.Singleton.IsConnectedClient) // The host was the one leaving so we are now disconnected and should also leave the lobby.
            {
                lobby.Leave();
                UIUpdate_OnHostMemberLeave?.Invoke(lobby);
                SteamLobbyLog(LogType.INFO, $"The host [{friend.Name} ({friend.Id})] left from the lobby.");
                return;
            }
            UIUpdate_OnMemberLeft?.Invoke(lobby);
            SteamLobbyLog(LogType.INFO, $"{friend.Name} ({friend.Id}) left from the lobby.");
        }

        private void OnLobbyMemberJoined(Lobby lobby, Friend friend)
        {
            UIUpdate_OnMemberJoined?.Invoke(lobby);

            SteamLobbyLog(LogType.INFO, $"{friend.Name} ({friend.Id}) entered to the lobby.");
        }

        private void OnLobbyMemberDisconnected(Lobby lobby, Friend friend)
        {
            UIUpdate_OnMemberDisconnected?.Invoke(lobby);

            SteamLobbyLog(
                LogType.INFO,
                $"{friend.Name} ({friend.Id}) disconnected from the lobby."
            );
        }

        private async void OnLobbyJoinRequested(Lobby lobby, SteamId id)
        {
            if (currentLobby.Id.IsValid) // if we are already a member in the lobby, leave current one
            {
                NetworkManager.Singleton.Shutdown();
                currentLobby.Leave();
            }

            RoomEnter joinResult = await lobby.Join();
            switch (joinResult)
            {
                case RoomEnter.Success:
                    currentLobby = lobby;
                    break;
                default:
                    SteamLobbyLog(LogType.ERROR, "Error joining lobby");
                    break;
            }
        }
        #endregion

        #region Button events
        /// <summary>
        /// Starts the lobby with the given configuration.
        /// </summary>
        /// <param name="lobbyConfiguration"></param>
        /// <returns></returns>
        public async void StartLobby(LobbyConfiguration lobbyConfiguration)
        {
            if (!SteamClient.IsLoggedOn || !SteamClient.IsValid)
            {
                SteamLobbyLog(LogType.ERROR, "Could not communicate with steam. Is it down?");
                return;
            }

            this.lobbyConfiguration = lobbyConfiguration;

            var transport = NetworkManager.Singleton.GetComponent<FacepunchTransport>();
            transport.targetSteamId = SteamClient.SteamId;
            NetworkManager.Singleton.StartHost();

            var result = await SteamMatchmaking.CreateLobbyAsync(lobbyConfiguration.MaxMembers);

            currentLobby = result.Value;
            currentLobby.SetFriendsOnly();
            currentLobby.SetJoinable(true);
            currentLobby.SetData(HostID, SteamClient.SteamId.Value.ToString());
        }

        /// <summary>
        /// Shuts down connection and leaves the lobby.
        /// </summary>
        /// <param name="lobbyConfiguration"></param>
        /// <returns></returns>
        public void LeaveLobby()
        {
            if (!SteamClient.IsLoggedOn)
                return;

            NetworkManager.Singleton.Shutdown();
            currentLobby.Leave();
            UIUpdate_OnCloseConnection?.Invoke(currentLobby);
        }
        #endregion

        #region Utils

        /// <summary>
        /// Changes the lobby type of the specified lobby.
        /// </summary>
        /// <param name="lobby"></param>
        public void ChangeLobbyType(Lobby lobby)
        {
            if (!lobby.Id.IsValid)
            {
                SteamLobbyLog(
                    LogType.ERROR,
                    "Can not change lobby type because lobby does not exist!"
                );

                return;
            }

            lobbyConfiguration.ChangeLobbyType(lobby);
        }

        /// <summary>
        /// Gets the member count of the specified lobby.
        /// </summary>
        /// <param name="lobby"></param>
        public int GetMemberCount(Lobby lobby)
        {
            if (!lobby.Id.IsValid)
            {
                SteamLobbyLog(
                    LogType.ERROR,
                    "Can not get member count because lobby does not exist!"
                );

                return -1;
            }
            return lobby.MemberCount;
        }

        private void SteamLobbyLog(LogType logType, string logMessage)
        {
            string printMessage = $"[{nameof(LobbyManager)}] {logMessage}";
            switch (logType)
            {
                case LogType.INFO:
                    Debug.Log(printMessage);
                    break;
                case LogType.WARNING:
                    Debug.LogWarning(printMessage);
                    break;
                case LogType.ERROR:
                    Debug.LogError(printMessage);
                    break;
            }
        }
        #endregion
    }
}
