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
            Debug.LogError("Nie znaleziono shadera Hidden/SunShaftsPostProcess!");
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

        // Kierunek OD ziemi DO słońca w świecie 3D
        Vector3 worldDirToSun = -sunTransform.forward;

        // Przeliczamy ten wektor na układ kamery (X = prawo/lewo, Y = góra/dół, Z = przód/tył)
        Vector3 localSunDir = cam.transform.InverseTransformDirection(worldDirToSun);

        // Wyznaczamy kierunek padania promieni na ekranie 2D z poprawką na proporcje ekranu (aspect ratio)
        Vector2 rayDir2D = new Vector2(localSunDir.x / cam.aspect, localSunDir.y);
        if (rayDir2D.sqrMagnitude < 0.0001f)
            rayDir2D = Vector2.up;
        else
            rayDir2D.Normalize();

        // Sprawdzamy, czy słońce jest bezpośrednio w kadrze przed nami
        Vector3 viewPos = cam.WorldToViewportPoint(cam.transform.position + worldDirToSun * 1000f);
        
        // Płynne przejście (blend) między promieniami rozchodzącymi się z tarczy słońca (gdy patrzysz prosto w nie)
        // a równoległymi promieniami padającymi pod stałym kątem (gdy patrzysz w bok lub tyłem)
        float radialBlend = Mathf.Clamp01(localSunDir.z);

        sunShaftsMaterial.SetVector("_SunScreenPos", new Vector4(viewPos.x, viewPos.y, radialBlend, 1.0f));
        sunShaftsMaterial.SetVector("_SunRayDir", new Vector4(rayDir2D.x, rayDir2D.y, 0f, 0f));
        sunShaftsMaterial.SetColor("_SunColor", sunColor);
        sunShaftsMaterial.SetFloat("_Density", density);
        sunShaftsMaterial.SetFloat("_Weight", weight);
        sunShaftsMaterial.SetFloat("_Decay", decay);
        sunShaftsMaterial.SetFloat("_Exposure", exposure);

        Graphics.Blit(source, destination, sunShaftsMaterial);
    }
}