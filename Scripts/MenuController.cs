using UnityEngine;
using System.Collections;

public class MenuController : MonoBehaviour
{
    public static MenuController Instance;

    [Header("--- MENU UI PANELS ---")]
    [Tooltip("InitialMenu sahnesindeki ana menü paneli (Oyna, Ayarlar butonlarının olduğu arayüz)")]
    public GameObject mainMenuPanel; 

    [HideInInspector] public bool isFirstLoad = true; 

    // 🎯 GERİYE DÖNÜK UYUMLULUK KÖPRÜSÜ (Kusursuz Korundu Canım!)
    public SaveData activeSaveData
    {
        get
        {
            SaveData bridgeContainer = new SaveData();
            if (StatsManager.Instance != null)
            {
                StatsManager.Instance.ExportToSaveData(bridgeContainer);
            }
            return bridgeContainer;
        }
        set
        {
            if (value != null && StatsManager.Instance != null)
            {
                StatsManager.Instance.ImportFromSaveData(value);
                Debug.Log("<color=cyan>MenuController [Setter Köprüsü]:</color> Eski atama çağrısı başarıyla StatsManager RAM'ine yönlendirildi canım.");
            }
        }
    }

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
            return;
        }
    }

    // 🎯 INITIALMENU BUTON TETİKLERİ (Saf Arayüz Görevi)

    public void OnClick_StartGame()
    {
        // Küresel ses motorumuzdan o tatlı tıkırtıyı çalıyoruz canım 🎵
        if (SoundtrackManager.Instance != null)
        {
            SoundtrackManager.Instance.PlayClickSound();
        }

        // 🚀 BÜYÜK GEÇİŞ: Oyuncuyu yükleme ekranı eşliğinde oyun dünyasına fırlatıyoruz!
        if (VanthLoreSceneManager.Instance != null)
        {
            // 'isFirstLoad' kilidini koruyoruz ki bootstrapper ilk girişi anlasın
            isFirstLoad = true; 
            
            // VanthLoreSceneManager bizim için rastgele lore/guide resmini sunucudan seçip yükleyecek!
            VanthLoreSceneManager.Instance.ChangeScene("gameworld", shouldSave: false);
            
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("[VanthLore] VanthLoreSceneManager bulunamadı! InitialMenu sahnesine objeyi yerleştirdin mi canım?");
        }
    }

    public void SaveOnlySettings()
    {
        if (SaveManager.instance != null) SaveManager.instance.SaveGameProgressOnline();
    }

    public void SaveCurrentGameProgress()
    {
        if (SaveManager.instance != null) SaveManager.instance.SaveGameProgressOnline();
    }

    public void QuitGame()
    {
        Debug.Log("<color=red>[VanthLore]</color> Sunucu oturumu kapatılıyor, uygulamadan çıkılıyor...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}