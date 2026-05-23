using UnityEngine;
using UnityEngine.Rendering.Universal; // 2D Light için zorunlu

public class LightFlicker : MonoBehaviour
{
    private Light2D myLight;
    [Header("Titreme Ayarları")]
    public float minIntensity = 0.9f;
    public float maxIntensity = 1.2f;
    public float speed = 0.15f;

    void Start()
    {
        myLight = GetComponent<Light2D>();
    }

    void Update()
    {
        // PerlinNoise kullanarak yumuşak ve doğal bir titreme sağlar
        float noise = Mathf.PerlinNoise(Time.time * speed, 0);
        myLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
    }
}