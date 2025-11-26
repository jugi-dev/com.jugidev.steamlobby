using Steamworks;
using Steamworks.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SteamLobby
{
    using LobbyEvent = LobbyManager.LobbyEvent;

    public class LobbyUI : MonoBehaviour
    {
        public static LobbyUI Instance { get; private set; }

        [SerializeField]
        private Button createLobbyButton;

        [SerializeField]
        private Button leaveButton;

        [SerializeField]
        private Transform playerList;

        [SerializeField]
        private GameObject playerLobbyObjectPrefab;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        void Start()
        {
            createLobbyButton.onClick.AddListener(OnCreateLobby);
            leaveButton.onClick.AddListener(OnLeaveLobby);

            ToggleCreateLobbyButton(true);
            ToggleLeaveLobbyButton(false);

            LobbyManager.Instance.OnLobbyEvent += OnLobbyEvent;
        }

        void OnDestroy()
        {
            createLobbyButton.onClick.RemoveListener(OnCreateLobby);
            leaveButton.onClick.RemoveListener(OnLeaveLobby);
            LobbyManager.Instance.OnLobbyEvent -= OnLobbyEvent;
        }

        private void OnLobbyEvent(Lobby lobby, LobbyEvent @event)
        {
            switch (@event)
            {
                case LobbyEvent.NONE:
                    ToggleCreateLobbyButton(true);
                    ToggleLeaveLobbyButton(false);
                    break;
                case LobbyEvent.LOBBY_ENTERED:
                    UpdateLobbyMembersUI(lobby);
                    ToggleCreateLobbyButton(false);
                    ToggleLeaveLobbyButton(true);
                    break;
                case LobbyEvent.MEMBER_JOINED:
                case LobbyEvent.MEMBER_LEFT:
                case LobbyEvent.MEMBER_DISCONNECTED:
                case LobbyEvent.HOST_MEMBER_LEAVE:
                    UpdateLobbyMembersUI(lobby);
                    break;
                case LobbyEvent.HOST_LEAVE:
                    UpdateLobbyMembersUI(lobby);
                    ToggleCreateLobbyButton(true);
                    ToggleLeaveLobbyButton(false);
                    break;
            }
        }

        public void OnCreateLobby()
        {
            LobbyManager.Instance.StartLobby();
        }

        public void OnLeaveLobby()
        {
            LobbyManager.Instance.LeaveLobby();
        }

        private void UpdateLobbyMembersUI(Lobby lobby)
        {
            ClearPlayerList();

            if (!SteamClient.IsLoggedOn || !lobby.Id.IsValid)
                return;

            foreach (var member in lobby.Members)
            {
                if (!member.Id.IsValid)
                    return;
                CreateLobbyEntryUI(member, lobby, playerLobbyObjectPrefab, playerList);
            }
        }

        private void ToggleCreateLobbyButton(bool isEnabled)
        {
            createLobbyButton.gameObject.SetActive(isEnabled);
        }

        private void ToggleLeaveLobbyButton(bool isEnabled)
        {
            leaveButton.gameObject.SetActive(isEnabled);
        }

        public void ClearPlayerList()
        {
            if (playerList.childCount > 0)
            {
                foreach (Transform t in playerList)
                {
                    Destroy(t.gameObject);
                }
            }
        }

        private void CreateLobbyEntryUI(
            Friend member,
            Lobby lobby,
            GameObject playerLobbyObjectPrefab,
            Transform playerList
        )
        {
            PlayerInfo info = new PlayerInfo
            {
                Friend = member,
                IsLobbyOwner = lobby.IsOwnedBy(member.Id),
            };
            GameObject playerLobbyObjectInstance = Instantiate(playerLobbyObjectPrefab, playerList);
            playerLobbyObjectInstance
                .GetComponentInChildren<TMP_Text>()
                .SetText(info.ConstructLobbyEntry());
        }
    }
}
