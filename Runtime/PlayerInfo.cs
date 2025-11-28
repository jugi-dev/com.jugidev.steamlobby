using System;
using System.Text;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace SteamLobby
{
    public class PlayerInfo
    {
        private Friend m_friend;
        private bool m_isLobbyOwner;

        private ulong[] m_developerIds;

        public PlayerInfo(Friend friend, bool isLobbyOwner, ulong[] developerIds = null)
        {
            m_friend = friend;
            m_isLobbyOwner = isLobbyOwner;
            m_developerIds = developerIds;
        }

        public bool IsDeveloper()
        {
            if (m_developerIds == null || m_developerIds.Length <= 0) return false;

            foreach (var id in m_developerIds)
            {
                if (m_friend.Id == id)
                    return true;
            }
            return false;
        }

        public string ConstructLobbyEntry()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"{m_friend.Name}");
            if (m_isLobbyOwner)
            {
                stringBuilder.Append("(Owner) ");
            }
            if (IsDeveloper())
            {
                stringBuilder.Append("[Developer]");
            }

            return stringBuilder.ToString();
        }

        public async Task<Texture2D> GetAvatar()
        {
            try
            {
                Image? avatar = await m_friend.GetMediumAvatarAsync();

                if (avatar.HasValue)
                {
                    Texture2D createdTexture = new Texture2D(
                        (int)avatar.Value.Width,
                        (int)avatar.Value.Height,
                        TextureFormat.RGBA32,
                        false
                    );

                    createdTexture.SetPixelData(avatar.Value.Data, 0);

                    Texture2D flippedTexture = FlipTexture(createdTexture, false, true);

                    flippedTexture.Apply();
                    return flippedTexture;
                }
                return null;
            }
            catch (Exception e)
            {
                Debug.Log($"Could not get avatar: {e.Message}");
                return null;
            }
        }

        private Texture2D FlipTexture(
            Texture2D original,
            bool flipHorizontally,
            bool flipVertically
        )
        {
            Texture2D flipped = new Texture2D(original.width, original.height);
            int width = original.width;
            int height = original.height;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    flipped.SetPixel(
                        flipHorizontally ? width - i - 1 : i,
                        flipVertically ? height - j - 1 : j,
                        original.GetPixel(i, j)
                    );
                }
            }
            return flipped;
        }
    }
}
