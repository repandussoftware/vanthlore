using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

public class SceneInitializer : MonoBehaviour
{
    [Header("Bu Sahneye Özel Işıklar")]
    public List<Light2D> localWindowLights; 
    public float localWindowLightDayTimeIntensityMultiplier = 1.5f;
    public float localWindowLightNightIntensityMultiplier = 0.8f;

    public Light2D localGlobalLight;
    public float localGlobalLightDayTimeIntensityMultiplier = 1.5f;
    public float localGlobalLightNightIntensityMultiplier = 0.8f;

    public List<Light2D> localCandles;
    public float localCandleLightIntensityMultiplier = 1.0f;

    [Header("Karakter & Etkileşim Referansları")]
    public Animator helenAnimator;

    [Header("Sahne Ayarları Kataloğu")]
    public SceneSettings settings;

    void Start()
    {
        // 👑 Işıkları artık doğrudan VanthLoreAtmosManager (Atmosfer Şefi) üzerine mühürlüyoruz canım!
        if (VanthLoreAtmosManager.Instance != null)
        {
            VanthLoreAtmosManager.Instance.SetLocalReferences(
                localWindowLights, 
                localWindowLightDayTimeIntensityMultiplier, 
                localWindowLightNightIntensityMultiplier, 
                localGlobalLight, 
                localGlobalLightDayTimeIntensityMultiplier, 
                localGlobalLightNightIntensityMultiplier, 
                localCandles, 
                localCandleLightIntensityMultiplier
            );
            
            Debug.Log("<color=lime>[VanthLore Scene]</color> Sahneler arası ışık ve ambiyans referansları başarıyla Atmosfer Şefine teslim edildi canım!");
        }
        else
        {
            Debug.LogWarning("⚠️ [VanthLore Scene] VanthLoreAtmosManager bulunamadı! Işıklar köprülenemedi canım.");
        }
    }
}