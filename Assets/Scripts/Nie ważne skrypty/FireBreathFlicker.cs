using UnityEngine;

[RequireComponent(typeof(Light))]
public class FireBreathFlicker : MonoBehaviour
{
    private Light fireLight;
    private Vector3 basePosition;

    [Header("Jasność bazowa i oddychanie")]
    [SerializeField] private float baseIntensity = 2.2f;
    [SerializeField] private float breathIntensityAmplitude = 0.5f; 
    [SerializeField] private float breathSpeed = 1.2f;              

    [Header("Mienienie się / Iskrzenie (Flicker)")]
    [SerializeField] private float microFlickerAmplitude = 0.35f;   
    [SerializeField] private float flickerSpeed = 14.0f;            

    [Header("Ruch płomienia (Jitter)")]
    [SerializeField] private float positionJitterAmount = 0.04f;    
    [SerializeField] private float positionJitterSpeed = 6.0f;

    private float noiseSeedIntensity;
    private float noiseSeedPos;

    void Start()
    {
        fireLight = GetComponent<Light>();
        basePosition = transform.localPosition;
        noiseSeedIntensity = Random.Range(0f, 100f);
        noiseSeedPos = Random.Range(0f, 100f);
    }

    void Update()
    {
        float breath = Mathf.Sin(Time.time * breathSpeed) * breathIntensityAmplitude;

        float microFlicker = (Mathf.PerlinNoise(Time.time * flickerSpeed + noiseSeedIntensity, 0f) - 0.5f) * 2f * microFlickerAmplitude;

        fireLight.intensity = Mathf.Max(0.1f, baseIntensity + breath + microFlicker);

        float xOffset = (Mathf.PerlinNoise(Time.time * positionJitterSpeed + noiseSeedPos, 0f) - 0.5f) * positionJitterAmount;
        float yOffset = (Mathf.PerlinNoise(0f, Time.time * positionJitterSpeed + noiseSeedPos) - 0.5f) * positionJitterAmount;
        transform.localPosition = basePosition + new Vector3(xOffset, yOffset, 0f);
    }
}