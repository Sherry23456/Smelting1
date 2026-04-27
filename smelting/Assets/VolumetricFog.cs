using UnityEngine;

[RequireComponent(typeof(Camera))]
public class VolumetricFog : MonoBehaviour
{
    public Shader fogShader;
    private Material fogMaterial;

    [Header("Fog Settings")]
    [Range(0.0f, 1.0f)] public float Density = 0.05f;
    public Color FogColor = Color.gray;
    public Vector3 FogOffset = Vector3.zero;
    public float Anisotropy = 0.1f; // 各向异性，让雾更有方向感

    [Header("Light Interaction")]
    public Light MainLight;
    [Range(0.0f, 2.0f)] public float LightInfluence = 0.5f;

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (fogMaterial == null)
            fogMaterial = new Material(fogShader);
        fogMaterial.hideFlags = HideFlags.HideAndDontSave;

        // 传递参数
        fogMaterial.SetFloat("_Density", Density);
        fogMaterial.SetColor("_FogColor", FogColor);
        fogMaterial.SetVector("_FogOffset", FogOffset);
        fogMaterial.SetFloat("_Anisotropy", Anisotropy);

        if (MainLight != null)
        {
            fogMaterial.SetVector("_LightDir", MainLight.transform.forward);
            fogMaterial.SetColor("_LightColor", MainLight.color);
            fogMaterial.SetFloat("_LightInfluence", LightInfluence);
        }

        Graphics.Blit(source, destination, fogMaterial);
    }
}