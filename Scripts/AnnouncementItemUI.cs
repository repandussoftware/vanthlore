using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AnnouncementItemUI : MonoBehaviour
{
    [Header("--- ROW REFS ---")]
    [SerializeField] private TextMeshProUGUI titleText; // Satırdaki duyuru başlığı canım
    [SerializeField] private Image iconImage;           // Satırdaki mini ikon (info, book vb.)

    [Header("--- ICON SPRITES (INSPECTOR'DAN BAĞLA CANIM) ---")]
    [SerializeField] private Sprite infoSprite;
    [SerializeField] private Sprite bookSprite;
    [SerializeField] private Sprite scrollSprite;

    private AnnouncementManager.ServerAnnouncementData _myData;

    // 🎯 SUNUCUDAN GELEN JONSB VERİSİNE GÖRE SATIRI BAŞTAN YARATAN KUTSAL METOD
    public void InitializeRow(AnnouncementManager.ServerAnnouncementData data)
    {
        _myData = data;
        
        if (titleText != null) titleText.text = data.title;

        if (iconImage != null)
        {
            switch (data.icon_type.ToLower())
            {
                case "book":
                    iconImage.sprite = bookSprite;
                    break;
                case "scroll":
                    iconImage.sprite = scrollSprite;
                    break;
                default:
                    iconImage.sprite = infoSprite;
                    break;
            }
        }
    }

    // Satıra tıklandığında konsola detayları döker canım
    public void OnClick_OpenDetailedContent()
    {
        if (_myData == null) return;
        Debug.Log($"<color=yellow>Duyuru Detayı:</color> {_myData.content}");
    }
}