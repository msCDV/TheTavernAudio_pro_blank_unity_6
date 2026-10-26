using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class SunShaftsEffect : MonoBehaviour
{
    [Header("Źródło światła")]
    [SerializeField] private Transform sunTransform;

    [Header("Wygląd promieni")]
    [SerializeField] private Color sunColor = new Color(1.0f, 0.78f, 0.48f, 1.0f);
    [Range(0.1f, 2.5f)]
    [SerializeField] private float exposure = 0.55f;
    [Range(0.1f, 1.5f)]
    [SerializeField] private float density = 0.7f;
    [Range(0.005f, 0.08f)]
    [SerializeField] private float weight = 0.028f;
    [Range(0.85f, 0.99f)]
    [SerializeField] private float decay = 0.96f;

    private Material sunShaftsMaterial;
    private Camera cam;

    void OnEnable()
    {
        cam = GetComponent<Camera>();
        cam.depthTextureMode |= DepthTextureMode.Depth;

        Shader shader = Shader.Find("Hidden/SunShaftsPostProcess");
        if (shader != null && shader.isSupported)
        {
            sunShaftsMaterial = new Material(shader) { hideFlags = HideFlags.DontSave };
        }
    }

    void OnDisable()
    {
        if (sunShaftsMaterial != null) DestroyImmediate(sunShaftsMaterial);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (sunShaftsMaterial == null || sunTransform == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        Vector3 worldDirToSun = -sunTransform.forward;
        float forwardDot = Vector3.Dot(cam.transform.forward, worldDirToSun);

        // Płynny fade: pełne promienie gdy patrzysz w stronę słońca,
        // łagodny zanik przy obrocie w bok, 0 gdy słońce jest za plecami (likwiduje białą łunę na niebie)
        float sunVisibility = Mathf.Clamp01((forwardDot + 0.1f) / 0.5f);

        Vector3 viewPos = cam.WorldToViewportPoint(cam.transform.position + worldDirToSun * 1000f);

        sunShaftsMaterial.SetVector("_SunScreenPos", new Vector4(viewPos.x, viewPos.y, sunVisibility, 1.0f));
        sunShaftsMaterial.SetColor("_SunColor", sunColor);
        sunShaftsMaterial.SetFloat("_Density", density);
        sunShaftsMaterial.SetFloat("_Weight", weight);
        sunShaftsMaterial.SetFloat("_Decay", decay);
        sunShaftsMaterial.SetFloat("_Exposure", exposure);

        Graphics.Blit(source, destination, sunShaftsMaterial);
    }
}