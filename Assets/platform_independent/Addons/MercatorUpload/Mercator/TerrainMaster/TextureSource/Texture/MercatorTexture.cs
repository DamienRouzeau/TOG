using UnityEngine;

namespace ThreeEyedGames.Mercator
{
    [AddComponentMenu("Mercator/Texture", 200)]
    public class MercatorTexture : MercatorTextureSource
    {
        public Texture2D Texture;

        public override Texture GetTexture()
        {
            return Texture;
        }
    }
}
