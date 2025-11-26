using Steamworks.Data;

/// <summary>
/// A class for configuring the lobby.
/// </summary>
public class LobbyConfiguration
{
    private int m_maxMembers;
    private LobbyType m_lobbyType;

    public int MaxMembers => m_maxMembers;

    public enum LobbyType
    {
        Private,
        FriendsOnly,
        Public,
    }

    public LobbyConfiguration(int maxMembers, LobbyType lobbyType)
    {
        m_maxMembers = maxMembers;
        m_lobbyType = lobbyType;
    }

    public void ChangeLobbyType(Lobby lobby)
    {
        switch (m_lobbyType)
        {
            case LobbyType.Private:
                lobby.SetPrivate();
                m_lobbyType = LobbyType.Private;
                break;
            case LobbyType.FriendsOnly:
                lobby.SetFriendsOnly();
                m_lobbyType = LobbyType.FriendsOnly;
                break;
            case LobbyType.Public:
                lobby.SetPublic();
                m_lobbyType = LobbyType.Public;
                break;
        }
    }

    public LobbyType GetLobbyType()
    {
        return m_lobbyType;
    }
}
