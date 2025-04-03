using UnityEngine;

public class ChangeMaterial : MonoBehaviour
{
    [SerializeField]
    private Material[] _materials = null;
    [SerializeField]
    private Renderer _renderer = null;

    public void SetMaterialFromIndex(int idx)
	{
        if (idx < 0)
            idx = -idx;
        int i = idx % _materials.Length;
        _renderer.material = _materials[i];
    }
}
