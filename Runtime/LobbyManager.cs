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

        private Lobby currentLobby;
        public Lobby CurrentLobby => currentLobby;

        public const string HostID = "HostID";

        public enum LobbyEvent
        {
            NONE,
            LOBBY_ENTERED,
            MEMBER_JOINED,
            MEMBER_LEFT,
            MEMBER_DISCONNECTED,
            HOST_LEAVE,
            HOST_MEMBER_LEAVE,
        }

        private LobbyEvent lobbyEvent;

        public Action<Lobby, LobbyEvent> OnLobbyEvent;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        void Start() => SubscribeToCallbacks();

        void OnDestroy() => UnsubscribeToCallbacks();

        #region Lobby callbacks
        void SubscribeToCallbacks()
        {
            SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
            SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
            SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
            SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnected;

            SteamFriends.OnGameLobbyJoinRequested += OnLobbyJoinRequested;

            lobbyEvent = LobbyEvent.NONE;
        }

        void UnsubscribeToCallbacks()
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
                    Debug.Log("Lobby created successfully");
                    break;
                default:
                    Debug.LogError($"Error creating lobby: {Enum.GetName(typeof(Result), result)}");
                    break;
            }
        }

        private void OnLobbyEntered(Lobby lobby)
        {
            Debug.Log($"Entered lobby as: {SteamClient.Name} ({SteamClient.SteamId})");

            if (!currentLobby.IsOwnedBy(SteamClient.SteamId))
            {
                var transport = NetworkManager.Singleton.GetComponent<FacepunchTransport>();
                ulong target = ulong.Parse(lobby.GetData(HostID));
                transport.targetSteamId = target;
                NetworkManager.Singleton.StartClient();
            }

            lobbyEvent = LobbyEvent.LOBBY_ENTERED;
            OnLobbyEvent?.Invoke(lobby, lobbyEvent);
        }

        private void OnLobbyMemberLeave(Lobby lobby, Friend friend)
        {
            if (!NetworkManager.Singleton.IsConnectedClient)
            {
                lobby.Leave();
                lobbyEvent = LobbyEvent.MEMBER_LEFT;
                OnLobbyEvent?.Invoke(lobby, lobbyEvent);
            }
            lobbyEvent = LobbyEvent.HOST_MEMBER_LEAVE;
            OnLobbyEvent?.Invoke(lobby, lobbyEvent);

            Debug.Log($"{friend.Name} ({friend.Id}) left from the lobby.");
        }

        private void OnLobbyMemberJoined(Lobby lobby, Friend friend)
        {
            lobbyEvent = LobbyEvent.MEMBER_JOINED;
            OnLobbyEvent?.Invoke(lobby, lobbyEvent);

            Debug.Log($"{friend.Name} ({friend.Id}) entered the lobby");
        }

        private void OnLobbyMemberDisconnected(Lobby lobby, Friend friend)
        {
            lobbyEvent = LobbyEvent.MEMBER_DISCONNECTED;
            OnLobbyEvent?.Invoke(lobby, lobbyEvent);

            Debug.Log($"{friend.Name} ({friend.Id}) disconnected from the lobby.");
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
                    Debug.LogError(
                        $"Error joining lobby: {Enum.GetName(typeof(RoomEnter), joinResult)}"
                    );
                    break;
            }
        }
        #endregion

        #region Button events
        public async void StartLobby()
        {
            if (!SteamClient.IsLoggedOn || !SteamClient.IsValid)
            {
                Debug.Log("Could not communicate with Steam. Is it down?");
                return;
            }

            var transport = NetworkManager.Singleton.GetComponent<FacepunchTransport>();
            transport.targetSteamId = SteamClient.SteamId;
            NetworkManager.Singleton.StartHost();

            var result = await SteamMatchmaking.CreateLobbyAsync(2);

            currentLobby = result.Value;
            currentLobby.SetFriendsOnly();
            currentLobby.SetJoinable(true);
            currentLobby.SetData(HostID, SteamClient.SteamId.Value.ToString());
        }

        public void LeaveLobby()
        {
            if (!SteamClient.IsLoggedOn)
                return;

            NetworkManager.Singleton.Shutdown();
            currentLobby.Leave();

            lobbyEvent = LobbyEvent.HOST_LEAVE;
            OnLobbyEvent?.Invoke(currentLobby, lobbyEvent);
        }
        #endregion
    }
}
