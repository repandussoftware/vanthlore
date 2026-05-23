using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LocalizedDropdown : MonoBehaviour
{
    [Header("Localization Keys")]
    public List<string> optionKeys; // Sırasıyla: UI.duties_popup.title_active vb.

    private TMP_Dropdown _dropdown;

    void Awake()
    {
        _dropdown = GetComponent<TMP_Dropdown>();
    }

    void OnEnable()
    {
        LocalizationManager.OnLanguageChanged += RefreshDropdown;
        RefreshDropdown();
    }

    void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= RefreshDropdown;
    }

    public void RefreshDropdown()
    {
        if (_dropdown == null || optionKeys == null || optionKeys.Count == 0) return;

        // Oyuncunun seçtiği mevcut sırayı (index) kaybetmemek için saklayalım
        int savedIndex = _dropdown.value;

        // Manuel girilen eski seçenekleri temizleyelim
        _dropdown.options.Clear();

        // Her bir anahtar için çeviriyi çekip dropdown'a ekleyelim
        foreach (string key in optionKeys)
        {
            string localizedText = LocalizationManager.Instance.GetText(key);
            _dropdown.options.Add(new TMP_Dropdown.OptionData(localizedText));
        }

        // Dropdown'ı görsel olarak tazele ve eski seçimini geri ata
        _dropdown.RefreshShownValue();
        _dropdown.value = savedIndex;
    }
}