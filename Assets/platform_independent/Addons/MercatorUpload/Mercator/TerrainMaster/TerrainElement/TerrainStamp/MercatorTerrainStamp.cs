using UnityEngine;
using UnityEngine.Rendering;

namespace ThreeEyedGames.Mercator
{
    [AddComponentMenu("Mercator/Terrain Stamp", 100)]
    public class MercatorTerrainStamp : MercatorTerrainElement
    {
        public enum TerrainStampBlendMode
        {
            Default,
            Raise,
            Lower,
            Modulate
        }

        public TerrainStampBlendMode Blend = TerrainStampBlendMode.Default;

        [Range(0.0f, 1.0f)]
        public float Opacity = 1.0f;

        [Header("Height")]
        public MercatorTextureSource HeightTexture;

        public Vector2 Tiling = Vector2.one;
        public Vector2 Offset = Vector2.zero;

        [Range(0.1f, 10.0f)]
        public float HeightPower = 1.0f;

        [Header("Mask")]
        public MercatorTextureSource MaskTexture;

        [Range(0.1f, 10.0f)]
        public float MaskPower = 1.0f;

        protected Material _material;
        protected Vector3[] _pos = new Vector3[4];

        protected void ResetPos()
        {
            // Object space plane positions
            _pos[0] = new Vector3(-0.5f, 0.0f, -0.5f);
            _pos[1] = new Vector3(-0.5f, 0.0f, 0.5f);
            _pos[2] = new Vector3(0.5f, 0.0f, 0.5f);
            _pos[3] = new Vector3(0.5f, 0.0f, -0.5f);

            // Transform to world space
            for (int i = 0; i < _pos.Length; ++i)
                _pos[i] = transform.TransformPoint(_pos[i]);
        }

        private void OnDrawGizmosSelected()
        {
            ResetPos();

            Vector3 yOffset = Vector3.up * (transform.TransformPoint(Vector3.up * 0.25f).y - transform.TransformPoint(-Vector3.up * 0.25f).y);

            Gizmos.color = Color.red;
            for (int i = 0; i < 4; ++i)
                Gizmos.DrawLine(_pos[i] - yOffset, _pos[(i + 1) % 4] - yOffset);
            for (int i = 0; i < 4; ++i)
                Gizmos.DrawLine(_pos[i] + yOffset, _pos[(i + 1) % 4] + yOffset);
            for (int i = 0; i < 4; ++i)
                Gizmos.DrawLine(_pos[i] - yOffset, _pos[i] + yOffset);
        }

        public override void Apply(RenderTexture heightMap, Vector3 terrainOffset, Vector3 terrainSize)
        {
            if (_material == null)
                _material = new Material(Shader.Find("Hidden/Mercator/TerrainStamp"));

            BlendMode srcFactor = BlendMode.SrcAlpha;
            BlendMode dstFactor = BlendMode.OneMinusSrcAlpha;
            BlendOp blendOp = BlendOp.Add;
            Vector2 range = new Vector2(0, 1);
            float premulPow = 0.0f;
            float premulValue = 0.0f;
            switch (Blend)
            {
                case TerrainStampBlendMode.Modulate:
                    dstFactor = BlendMode.One;
                    range = new Vector2(-0.5f, 0.5f);
                    break;
                case TerrainStampBlendMode.Raise:
                    // Max doesn't respect blend factors, needs premultiplication
                    premulPow = 1.0f;
                    blendOp = BlendOp.Max;
                    break;
                case TerrainStampBlendMode.Lower:
                    // Min doesn't respect blend factors, needs premultiplication
                    premulPow = 1.0f;
                    premulValue = 1.0f;
                    blendOp = BlendOp.Min;
                    break;
                case TerrainStampBlendMode.Default:
                    break;
                default:
                    Debug.LogError("Unsupported blend mode: " + Blend);
                    break;
            }

            _material.SetInt("_SrcFactor", (int)srcFactor);
            _material.SetInt("_DstFactor", (int)dstFactor);
            _material.SetInt("_BlendOp", (int)blendOp);
            _material.SetFloat("_PremulPow", premulPow);
            _material.SetFloat("_PremulValue", premulValue);
            _material.SetFloat("_RangeFrom", range.x);
            _material.SetFloat("_RangeTo", range.y);
            _material.SetFloat("_Opacity", Opacity);
            _material.SetFloat("_HeightPow", HeightPower);
            if (HeightTexture != null)
                _material.SetTexture("_HeightTex", HeightTexture.GetTexture());
            _material.SetTextureScale("_HeightTex", Tiling);
            _material.SetTextureOffset("_HeightTex", Offset);
            _material.SetFloat("_HeightPow", HeightPower);
            _material.SetFloat("_MaskPow", MaskPower);
            if (MaskTexture != null)
                _material.SetTexture("_MaskTex", MaskTexture.GetTexture());

            _material.SetPass(0);

            ResetPos();
            for (int i = 0; i < 4; ++i)
            {
                _pos[i] -= terrainOffset;
                _pos[i].x /= terrainSize.x * (1.0f + 1.0f / heightMap.width);
                _pos[i].y /= terrainSize.y;
                _pos[i].z /= terrainSize.z * (1.0f + 1.0f / heightMap.height);
            }

            float heightOffset = (transform.TransformPoint(Vector3.up * 0.25f).y - transform.TransformPoint(Vector3.up * -0.25f).y) / terrainSize.y;

            RenderTexture.active = heightMap;
            GL.Begin(GL.QUADS);
            {
                GL.MultiTexCoord2(0, 1, 1);
                GL.MultiTexCoord2(1, _pos[0].y - heightOffset, _pos[0].y + heightOffset);
                GL.Vertex3(_pos[0].x, _pos[0].z, 0);

                GL.MultiTexCoord2(0, 1, 0);
                GL.MultiTexCoord2(1, _pos[1].y - heightOffset, _pos[1].y + heightOffset);
                GL.Vertex3(_pos[1].x, _pos[1].z, 0);

                GL.MultiTexCoord2(0, 0, 0);
                GL.MultiTexCoord2(1, _pos[2].y - heightOffset, _pos[2].y + heightOffset);
                GL.Vertex3(_pos[2].x, _pos[2].z, 0);

                GL.MultiTexCoord2(0, 0, 1);
                GL.MultiTexCoord2(1, _pos[3].y - heightOffset, _pos[3].y + heightOffset);
                GL.Vertex3(_pos[3].x, _pos[3].z, 0);
            }
            GL.End();
        }

        public override bool UseHeightTexture()
        {
            return true;
        }

        public override void SetHeightTexture(MercatorTextureSource texture)
        {
            HeightTexture = texture;
        }

        public override bool UseMaskTexture()
        {
            return true;
        }

        public override void SetMaskTexture(MercatorTextureSource texture)
        {
            MaskTexture = texture;
        }
    }
}