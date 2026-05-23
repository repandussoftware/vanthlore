using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

public class VanthLoreAtmosManager : MonoBehaviour
{
    public static VanthLoreAtmosManager Instance { get; private set; }

    [Header("--- TIME & COLOR SETTINGS ---")]
    public Color dayColor = new Color(1f, 0.95f, 0.8f);
    public Color nightColor = new Color(0.2f, 0.3f, 1f);

    [Header("--- RUNTIME LIGHT REFERENCES ---")]
    public List<Light2D> windowLights = new List<Light2D>();
    public Light2D globalLight;
    public List<Light2D> roomCandles = new List<Light2D>();

    [Header("--- INTENSITY MULTIPLIERS ---")]
    public float windowLightDayTimeIntensityMultiplier = 1.5f;
    public float windowLightNightIntensityMultiplier = 0.8f;
    public float globalLightDayTimeIntensityMultiplier = 1.5f;
    public float globalLightNightIntensityMultiplier = 0.8f;
    public float candleLightIntensityMultiplier = 1.0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetLocalReferences(
        List<Light2D> wLights, float wDayInt, float wNightInt,
        Light2D gLight, float gDayInt, float gNightInt,
        List<Light2D> candles, float cInt)
    {
        windowLights = new List<Light2D>(wLights);
        roomCandles = new List<Light2D>(candles);

        windowLightDayTimeIntensityMultiplier = wDayInt;
        windowLightNightIntensityMultiplier = wNightInt;
        globalLight = gLight;
        globalLightDayTimeIntensityMultiplier = gDayInt;
        globalLightNightIntensityMultiplier = gNightInt;
        candleLightIntensityMultiplier = cInt;

        ApplyCurrentEnvironment();
    }

    public void ApplyCurrentEnvironment()
    {
        if (StatsManager.Instance == null) return;

        // 🪟 Pencere Işıkları
        if (windowLights != null)
        {
            foreach (var wLight in windowLights)
            {
                if (wLight != null)
                {
                    wLight.color = StatsManager.Instance.isDayTime ? dayColor : nightColor;
                    wLight.intensity = StatsManager.Instance.isDayTime ? windowLightDayTimeIntensityMultiplier : windowLightNightIntensityMultiplier;
                }
            }
        }

        // 🌍 Küresel Işık
        if (globalLight != null)
            globalLight.intensity = StatsManager.Instance.isDayTime ? globalLightDayTimeIntensityMultiplier : globalLightNightIntensityMultiplier;

        // 🕯️ Mumlar
        foreach (var candle in roomCandles)
        {
            if (candle != null)
            {
                candle.enabled = !StatsManager.Instance.isDayTime;
                candle.intensity = candleLightIntensityMultiplier;
            }
        }

        // 🎯 KUTSAL KÖPRÜ: Arayüz metinlerini güncellemesi için tüy sıklet UIManager'ı tetikliyoruz
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateZamanMetniAndImages(StatsManager.Instance.isDayTime);
        }
    }
}