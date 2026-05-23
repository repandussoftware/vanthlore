using UnityEngine;
using TMPro;

public class MarketExchangeController : MonoBehaviour
{
    [Header("NumPad Bileşeni")]
    [SerializeField] private NumPadManager numPadManager;

    [Header("Market Üst Bilgi Yazıları (İsteğe Bağlı)")]
    // Eğer görseldeki o üst kısım oyuncunun toplam elmas ve coinini gösteriyorsa buraya bağlayabilirsin
    [SerializeField] public TextMeshProUGUI totalDiamondsText;
    [SerializeField] public TextMeshProUGUI totalCoinsText;

    private void OnEnable()
    {
        // Panel her açıldığında güncel cüzdan durumunu ekrana yansıt canım
        UpdateMarketUI();
    }

    // Exchange butonunun onClick() olayına bu metodu bağlayacaksın cam gibi!
    public void OnExchangeButtonClicked()
    {
        if (StatsManager.Instance == null || numPadManager == null) return;

        // 1. Numpad'den girilen takas edilmek istenen elmas miktarını alıyoruz
        int diamondsToExchange = numPadManager.GetCurrentValue();

        if (diamondsToExchange <= 0)
        {
            Debug.LogWarning("<color=yellow>VanthLore Market:</color> Takas etmek için lütfen 0'dan büyük bir miktar gir canım!");
            return;
        }

        // 2. Güvenlik Duvarı: Oyuncunun elinde satmak istediği kadar elmas var mı?
        if (StatsManager.Instance.currentDiamonds < diamondsToExchange)
        {
            Debug.LogError($"<color=red>Market Hatası:</color> Yetersiz Elmas! Mevcut: {StatsManager.Instance.currentDiamonds}, İstenen: {diamondsToExchange}");
            // Buraya ileride oyuncunun yüzüne çarpacak bir "Yetersiz Elmas" toast uyarısı ekleyebilirsin
            return;
        }

        // 3. Hesaplama: Kur değerini StatsManager'dan çekip çarpıyoruz
        // Eğer exchangeRate'i float yaptıysan Mathf.RoundToInt kullanıyoruz ki kur küsuratlıysa yuvarlasın
        int coinsGained = Mathf.RoundToInt(diamondsToExchange * StatsManager.Instance.exchangeRate);

        // 4. Cüzdanı Güncelle
        StatsManager.Instance.currentDiamonds -= diamondsToExchange;
        StatsManager.Instance.totalCoins += coinsGained;

        Debug.Log($"<color=green>Takas Başarılı!</color> {diamondsToExchange} Elmas satıldı. Kazanılan: {coinsGained} Gold.");

        // 5. İşlem başarılı olunca Numpad girişini sıfırla (Ekranda tekrar 0 yazsın)
        numPadManager.ResetPad();

        // 6. Market ekranındaki yazıları yeni paralarla tazele
        UpdateMarketUI();

        // 7. KRİTİK MÜHÜR: Para/Ekonomi işlerinde hile veya veri kaybı olmaması için anında ilerlemeyi kaydet!
        if (MenuController.Instance != null)
        {
            MenuController.Instance.SaveCurrentGameProgress();
        }
    }

    public void UpdateMarketUI()
    {
        if (StatsManager.Instance == null) return;

        // Eğer o üstteki elmas ve coin yazıları kalıcı bakiye göstergeleriyse günceller
        if (totalDiamondsText != null)
            totalDiamondsText.text = StatsManager.Instance.currentDiamonds.ToString();

        if (totalCoinsText != null)
            totalCoinsText.text = StatsManager.Instance.totalCoins.ToString();
    }
}