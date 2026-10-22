using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class SunShaftsEffect : MonoBehaviour
{
    [Header("Źródło światła")]
    [Tooltip("Przeciągnij swój Directional Light (Słońce)")]
    [SerializeField] private Transform sunTransform;

    [Header("Kolor i Ekspozycja")]
    [SerializeField] private Color sunColor = new Color(1.0f, 0.72f, 0.42f, 1.0f);
    [Range(0.1f, 3.0f)]
    [SerializeField] private float exposure = 0.6f;

    [Header("Ustawienia promieni")]
    [Range(0.1f, 2.0f)]
    [SerializeField] private float density = 0.77f;
    [Range(0.01f, 0.2f)]
    [SerializeField] private float weight = 0.035f;
    [Range(0.8f, 1.0f)]
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
            sunShaftsMaterial = new Material(shader);
            sunShaftsMaterial.hideFlags = HideFlags.DontSave;
        }
        else
        {
            Debug.LogError("Nie znaleziono shadera Hidden/SunShaftsPostProcess lub nie jest on wspierany!");
            enabled = false;
        }
    }

    void OnDisable()
    {
        if (sunShaftsMaterial != null)
        {
            DestroyImmediate(sunShaftsMaterial);
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (sunShaftsMaterial == null || sunTransform == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        // Prawdziwy kierunek do słońca w świecie 3D
        Vector3 worldDirToSun = -sunTransform.forward;

        // Przeliczamy kierunek słońca względem aktualnego obrotu kamery
        Vector3 localSunDir = cam.transform.InverseTransformDirection(worldDirToSun);

        Vector2 sunScreenPos;

        if (localSunDir.z > 0.05f)
        {
            // Słońce jest przed nami lub z boku – używamy dokładnej pozycji na ekranie
            Vector3 viewPos = cam.WorldToViewportPoint(cam.transform.position + worldDirToSun * 1000f);
            sunScreenPos = new Vector2(viewPos.x, viewPos.y);
        }
        else
        {
            // Słońce jest za plecami lub pod kątem > 90 stopni:
            // Nie wrzucamy go na środek ekranu (żeby nie świeciło w twarz),
            // tylko umieszczamy źródło daleko poza odpowiednią krawędzią ekranu (góra/bok),
            // zachowując oryginalny kąt padania promieni.
            Vector2 dir2D = new Vector2(localSunDir.x, localSunDir.y);
            if (dir2D.sqrMagnitude < 0.001f) dir2D = Vector2.up;
            dir2D.Normalize();

            sunScreenPos = new Vector2(0.5f, 0.5f) + dir2D * 2.5f;
        }

        sunShaftsMaterial.SetVector("_SunScreenPos", new Vector4(sunScreenPos.x, sunScreenPos.y, 1.0f, 1.0f));
        sunShaftsMaterial.SetColor("_SunColor", sunColor);
        sunShaftsMaterial.SetFloat("_Density", density);
        sunShaftsMaterial.SetFloat("_Weight", weight);
        sunShaftsMaterial.SetFloat("_Decay", decay);
        sunShaftsMaterial.SetFloat("_Exposure", exposure);

        Graphics.Blit(source, destination, sunShaftsMaterial);
    }
}