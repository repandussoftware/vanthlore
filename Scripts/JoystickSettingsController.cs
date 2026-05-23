using UnityEngine;
using UnityEngine.UI;

public class JoystickSettingsController : MonoBehaviour
{
    [Header("Slider-Toggle References")]
    [SerializeField] private Slider lockToggleSlider;       // 0: Off, 1: On
    [SerializeField] private Slider backgroundToggleSlider; // 0: Off, 1: On

    [Header("Standard Slider References")]
    [SerializeField] private Slider sizeSlider;
    [SerializeField] private Slider opacitySlider;

    private void OnEnable()
    {
        RefreshUI();
    }

    // JoystickSettingsController içindeki RefreshUI metodunu şu şekilde güncelle:
    public void RefreshUI()
    {
        var stats = StatsManager.Instance;
        if (stats == null) return;

        // value = ... yerine SetValueWithoutNotify kullanıyoruz
        if (lockToggleSlider != null)
            lockToggleSlider.SetValueWithoutNotify(stats.isJoystickPositionLocked ? 1f : 0f);

        if (backgroundToggleSlider != null)
            backgroundToggleSlider.SetValueWithoutNotify(stats.isJoystickBackgroundVisible ? 1f : 0f);

        if (sizeSlider != null)
            sizeSlider.SetValueWithoutNotify(stats.joyStickScale[0]);

        if (opacitySlider != null)
            opacitySlider.SetValueWithoutNotify(stats.joystickOpacity);
    }

    // --- UI EVENTLERİ ---

    // Bu metod Slider'ın OnValueChanged olayına bağlanacak
    public void OnLockSliderChanged(float value)
    {
        // 0.5'ten büyükse true (On), küçükse false (Off)
        bool isLocked = value > 0.5f;
        StatsManager.Instance.isJoystickPositionLocked = isLocked;
        StatsManager.Instance.UpdateJoystickSettings();
    }

    public void OnBackgroundSliderChanged(float value)
    {
        bool isVisible = value > 0.5f;
        StatsManager.Instance.isJoystickBackgroundVisible = isVisible;
        StatsManager.Instance.UpdateJoystickSettings();
    }

    public void OnSizeSliderChanged(float value)
    {
        StatsManager.Instance.joyStickScale = new float[] { value, value, value };
        StatsManager.Instance.UpdateJoystickSettings();
    }

    public void OnOpacitySliderChanged(float value)
    {
        StatsManager.Instance.joystickOpacity = value;
        StatsManager.Instance.UpdateJoystickSettings();
    }
}