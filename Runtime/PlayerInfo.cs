using System;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace SteamLobby
{
    public class PlayerInfo
    {
        public Friend Friend;
        public bool IsLobbyOwner;

        private ulong[] developerIds = new ulong[] { };

        public bool IsDeveloper()
        {
            foreach (var id in developerIds)
            {
                if (Friend.Id == id)
                    return true;
            }
            return false;
        }

        public string ConstructLobbyEntry()
        {
            string entry = $"{Friend.Name} ";
            if (IsLobbyOwner)
            {
                entry += "(Owner) ";
            }
            if (IsDeveloper())
            {
                entry += "[Developer]";
            }

            return entry;
        }

        public async Task<Texture2D> GetAvatar()
        {
            try
            {
                Image? avatar = await Friend.GetMediumAvatarAsync();

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
            int xN = original.width;
            int yN = original.height;

            for (int i = 0; i < xN; i++)
            {
                for (int j = 0; j < yN; j++)
                {
                    flipped.SetPixel(
                        flipHorizontally ? xN - i - 1 : i,
                        flipVertically ? yN - j - 1 : j,
                        original.GetPixel(i, j)
                    );
                }
            }
            return flipped;
        }
    }
}
