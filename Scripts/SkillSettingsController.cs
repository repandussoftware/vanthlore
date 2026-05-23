using UnityEngine;
using UnityEngine.UI;

public class SkillSettingsController : MonoBehaviour
{
    [SerializeField] private Slider lockSlider;    // 0-1 (Off/On)
    [SerializeField] private Slider scaleSlider;   // 0.5-2.0
    [SerializeField] private Slider opacitySlider; // 0.2-1.0

    private void OnEnable() => RefreshUI();

    public void RefreshUI()
    {
        var stats = StatsManager.Instance;
        lockSlider.SetValueWithoutNotify(stats.isSkillHUDLocked ? 1 : 0);
        scaleSlider.SetValueWithoutNotify(stats.skillHUDScale);
        opacitySlider.SetValueWithoutNotify(stats.skillHUDOpacity);
    }

    public void OnLockChanged(float val)
    {
        StatsManager.Instance.isSkillHUDLocked = val > 0.5f;
        StatsManager.OnSkillHUDUpdated?.Invoke();
        // HER DEĞİŞİKLİKTE KAYDET
        if (MenuController.Instance != null) MenuController.Instance.SaveOnlySettings();
    }

    public void OnOpacityChanged(float val)
    {
        StatsManager.Instance.skillHUDOpacity = val;
        StatsManager.OnSkillHUDUpdated?.Invoke();
        // HER DEĞİŞİKLİKTE KAYDET
        if (MenuController.Instance != null) MenuController.Instance.SaveOnlySettings();
    }

    public void OnScaleChanged(float val)
    {
        StatsManager.Instance.skillHUDScale = val;
        StatsManager.OnSkillHUDUpdated?.Invoke();

        // Değişiklik anında MenuController üzerinden diske yazsın (opsiyonel ama garanti)
        if (MenuController.Instance != null) MenuController.Instance.SaveOnlySettings();
    }
}