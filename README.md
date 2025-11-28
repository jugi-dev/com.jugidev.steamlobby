## Steam Lobby template

A simple Steam lobby template for Unity. Uses Netcode for GameObjects and Facepunch Transport.
## Dependency installation (required)

Package Manager -> Install package from git URL -> paste the link below

```bash
https://github.com/Unity-Technologies/multiplayer-community-contributions.git?path=/Transports/com.community.netcode.transport.facepunch
```

Credits:

Nico Thomas (Facepunch Transport), Floris van Onna (Facepunch Transport), Garry Newman (Author of Facepunch.Steamworks, used in Facepunch Transport)

## Install this package

Get the source from Releases, clone this repo or in Unity Package Manager -> Install package from git URL -> paste the link below

```bash
https://github.com/jugi-dev/com.jugidev.steamlobby.git
```
## Example
A script for a simple UI. You should always call ```csharp base.Start()``` first because this subscribes to the lobby's events.
```csharp
using Steamworks;
using Steamworks.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SteamLobby
{
    public class LobbyUI : LobbyUIBase
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

        public override void Start()
        {
            base.Start(); // base.Start should always be called!
            createLobbyButton.onClick.AddListener(OnCreateLobby);
            leaveButton.onClick.AddListener(OnLeaveLobby);
        }

        public override void OnDestroy()
        {
            base.OnDestroy(); // base.OnDestroy should always be called!
            createLobbyButton.onClick.RemoveListener(OnCreateLobby);
            leaveButton.onClick.RemoveListener(OnLeaveLobby);
        }

        public override void UI_OnCloseConnection(Lobby lobby)
        {
            UpdateLobbyMembersUI(lobby);
            ToggleCreateLeaveButtons(true, false);
        }

        public override void UI_OnMemberLeft(Lobby lobby)
        {
            UpdateLobbyMembersUI(lobby);
        }

        public override void UI_OnMemberJoined(Lobby lobby)
        {
            UpdateLobbyMembersUI(lobby);
        }

        public override void UI_OnMemberDisconnected(Lobby lobby)
        {
            UpdateLobbyMembersUI(lobby);
        }

        public override void UI_OnLobbyEntered(Lobby lobby)
        {
            UpdateLobbyMembersUI(lobby);
            ToggleCreateLeaveButtons(false, true);
        }

        public override void UI_OnHostMemberLeave(Lobby lobby)
        {
            UpdateLobbyMembersUI(lobby);
            ToggleCreateLeaveButtons(true, false);
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

        private void ToggleCreateLeaveButtons(bool isCreateEnabled, bool isLeaveEnabled)
        {
            createLobbyButton.gameObject.SetActive(isCreateEnabled);
            leaveButton.gameObject.SetActive(isLeaveEnabled);
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
            PlayerInfo info = new PlayerInfo(member, lobby.IsOwnedBy(member.Id));

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
