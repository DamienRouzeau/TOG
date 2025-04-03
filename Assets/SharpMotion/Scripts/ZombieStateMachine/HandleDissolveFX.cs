using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleDissolveFX : MonoBehaviour
{
    private Material[] materials;

    void Start()
    {
        Renderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
        SkinnedMeshRenderer[] skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

        int totalMaterials = 0;
        foreach (Renderer renderer in meshRenderers)
        {
            totalMaterials += renderer.materials.Length;
        }
        foreach (SkinnedMeshRenderer renderer in skinnedMeshRenderers)
        {
            totalMaterials += renderer.materials.Length;
        }

        materials = new Material[totalMaterials];
        int index = 0;

        foreach (MeshRenderer renderer in meshRenderers)
        {
            foreach (Material mat in renderer.materials)
            {
                materials[index++] = mat;
            }
        }
        foreach (SkinnedMeshRenderer renderer in skinnedMeshRenderers)
        {
            foreach (Material mat in renderer.materials)
            {
                materials[index++] = mat;
            }
        }
    }

    [ContextMenu("Dissolve")]
    public void Dissolve()
    {
        StartCoroutine(DissolveCoroutine(1.0f, 3.0f)); // Dissolve to 1 over 2 seconds
        DisableShadowsAndLighting();
    }

    private IEnumerator DissolveCoroutine(float targetAmount, float duration)
    {
        float startAmount = 0f;
        float elapsedTime = 0f;

        // Assuming all materials start with the same dissolve amount
        if (materials.Length > 0 && materials[0].HasProperty("_DissolveAmount"))
        {
            startAmount = materials[0].GetFloat("_DissolveAmount");
        }

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float currentAmount = Mathf.Lerp(startAmount, targetAmount, elapsedTime / duration);

            foreach (Material mat in materials)
            {
                if (mat.HasProperty("_DissolveAmount"))
                {
                    mat.SetFloat("_DissolveAmount", currentAmount);
                }
            }

            yield return null;
        }

        // Ensure the final value is set
        foreach (Material mat in materials)
        {
            if (mat.HasProperty("_DissolveAmount"))
            {
                mat.SetFloat("_DissolveAmount", targetAmount);
            }
        }
    }

    [ContextMenu("DisableShadowsAndLighting")]
    public void DisableShadowsAndLighting()
    {
        Renderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
        SkinnedMeshRenderer[] skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (Renderer renderer in meshRenderers)
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        foreach (SkinnedMeshRenderer renderer in skinnedMeshRenderers)
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }
}
