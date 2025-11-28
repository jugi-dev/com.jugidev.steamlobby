using SteamLobby;
using Steamworks.Data;
using UnityEngine;

public abstract class LobbyUIBase : MonoBehaviour
{
    public virtual void Start()
    {
        LobbyManager.Instance.UIUpdate_OnLobbyEntered += UI_OnLobbyEntered;
        LobbyManager.Instance.UIUpdate_OnMemberJoined += UI_OnMemberJoined;
        LobbyManager.Instance.UIUpdate_OnMemberLeft += UI_OnMemberLeft;
        LobbyManager.Instance.UIUpdate_OnMemberDisconnected += UI_OnMemberDisconnected;
        LobbyManager.Instance.UIUpdate_OnHostMemberLeave += UI_OnHostMemberLeave;
        LobbyManager.Instance.UIUpdate_OnCloseConnection += UI_OnCloseConnection;
    }

    public virtual void OnDestroy()
    {
        LobbyManager.Instance.UIUpdate_OnLobbyEntered -= UI_OnLobbyEntered;
        LobbyManager.Instance.UIUpdate_OnMemberJoined -= UI_OnMemberJoined;
        LobbyManager.Instance.UIUpdate_OnMemberLeft -= UI_OnMemberLeft;
        LobbyManager.Instance.UIUpdate_OnMemberDisconnected -= UI_OnMemberDisconnected;
        LobbyManager.Instance.UIUpdate_OnHostMemberLeave -= UI_OnHostMemberLeave;
        LobbyManager.Instance.UIUpdate_OnCloseConnection -= UI_OnCloseConnection;
    }

    /// <summary>
    /// Called on host when closing connection. You can clear your playerlist and re-enable create lobby button here.
    /// </summary>
    /// <param name="lobby"></param>
    public abstract void UI_OnCloseConnection(Lobby lobby);

    /// <summary>
    /// Called on everyone when a member of the lobby leaves. You can update your playerlist ui here.
    /// </summary>
    /// <param name="lobby"></param>
    public abstract void UI_OnMemberLeft(Lobby lobby);

    /// <summary>
    /// Called on everyone when a member of the lobby joins. You can update your playerlist ui here.
    /// </summary>
    /// <param name="lobby"></param>
    public abstract void UI_OnMemberJoined(Lobby lobby);

    /// <summary>
    /// Called on the client that entered lobby. You can update your playerlist ui here and enable the leave button.
    /// </summary>
    /// <param name="lobby"></param>
    public abstract void UI_OnLobbyEntered(Lobby lobby);

    /// <summary>
    /// Called on clients when host leaves the lobby. You can update your playerlist ui here and enable create lobby button.
    /// </summary>
    /// <param name="lobby"></param>
    public abstract void UI_OnHostMemberLeave(Lobby lobby);

    /// <summary>
    /// Called on everyone when a member of the lobby disconnets. You can update your playerlist ui here.
    /// </summary>
    /// <param name="lobby"></param>
    public abstract void UI_OnMemberDisconnected(Lobby lobby);
}