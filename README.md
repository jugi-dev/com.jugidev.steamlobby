## Steam Lobby Manager

A simple Steam lobby manager for Unity. Uses Netcode for GameObjects.
## Dependency installation (required)

Package Manager -> Install package from git URL

```bash
https://github.com/Unity-Technologies/multiplayer-community-contributions.git?path=/Transports/com.community.netcode.transport.facepunch
```

Credits:

Nico Thomas (Facepunch Transport), Floris van Onna (Facepunch Transport), Garry Newman (Author of Facepunch.Steamworks, used in Facepunch Transport)

## Install this package

Get the source from Releases, clone this repo or Package Manager -> Install package from git URL

```bash
https://github.com/jugi-dev/com.jugidev.steamlobby.git
```
## Example
A script for a simple UI
```csharp
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

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void Start()
        {
            createLobbyButton.onClick.AddListener(OnCreateLobby);
            leaveButton.onClick.AddListener(OnLeaveLobby);

            ToggleCreateLobbyButton(true);
            ToggleLeaveLobbyButton(false);

            LobbyManager.Instance.OnLobbyEvent += OnLobbyEvent;
        }

        private void OnDestroy()
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
                case LobbyEvent.HOST_MEMBER_LEAVE:
                case LobbyEvent.HOST_LEAVE:
                    UpdateLobbyMembersUI(lobby);
                    ToggleCreateLobbyButton(true);
                    ToggleLeaveLobbyButton(false);
                    break;
                case LobbyEvent.MEMBER_JOINED:
                case LobbyEvent.MEMBER_LEFT:
                case LobbyEvent.MEMBER_DISCONNECTED:
                    UpdateLobbyMembersUI(lobby);
                    break;
            }
        }

        public void OnCreateLobby()
        {
            LobbyManager.Instance.StartLobby(
                new LobbyConfiguration(2, LobbyConfiguration.LobbyType.FriendsOnly)
            );
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
                    continue;
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

        private void ClearPlayerList()
        {
            if (playerList.childCount > 0)
            {
                foreach (Transform t in playerList)
                {
                    Destroy(t.gameObject);
                }
            }
        }

        private async void CreateLobbyEntryUI(
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

            Texture2D texture2D = await info.GetAvatar();

            if (texture2D == null)
            {
                return;
            }
            var sprite = Sprite.Create(
                texture2D,
                new Rect(0, 0, texture2D.width, texture2D.height),
                default
            );

            UnityEngine.UI.Image avatarObject = playerLobbyObjectInstance
                .transform.GetChild(0)
                .transform.GetChild(0)
                .GetComponent<UnityEngine.UI.Image>();

            avatarObject.sprite = sprite;
        }
    }
}

```
