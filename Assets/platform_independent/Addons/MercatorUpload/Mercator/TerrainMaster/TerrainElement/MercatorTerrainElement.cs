using UnityEngine;

namespace ThreeEyedGames.Mercator
{
    public abstract class MercatorTerrainElement : MonoBehaviour
    {
        public abstract bool UseHeightTexture();
        public abstract void SetHeightTexture(MercatorTextureSource texture);

        public abstract bool UseMaskTexture();
        public abstract void SetMaskTexture(MercatorTextureSource texture);

        public abstract void Apply(RenderTexture heightMap, Vector3 terrainOffset, Vector3 terrainSize);
    }
}