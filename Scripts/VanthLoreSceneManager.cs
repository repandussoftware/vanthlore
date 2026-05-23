using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using TMPro;
using UnityEngine.Networking;

[System.Serializable]
public class VanthLoreLoadingTip
{
    public string type;         // "LORE" (Evren Hikayesi) veya "GUIDE" (Oynanış Rehberi)
    public string image_key;    // Addressables içindeki görselin string anahtarı (Örn: "img_loading_darion_lore")
    public string title_key;    // LocalizationManager için metin anahtarı (Örn: "UI.loading.tip_title_01")
    public string desc_key;     // LocalizationManager için açıklama anahtarı (Örn: "UI.loading.tip_desc_01")
}
public class VanthLoreSceneManager : MonoBehaviour
{
    public static VanthLoreSceneManager Instance { get; private set; }

    [Header("--- LOADING UI REFERENCES ---")]
    public GameObject loadingPanel;
    public UnityEngine.UI.Slider loadingBar;
    public CanvasGroup loadingCanvasGroup;

    [Header("--- DYNAMIC LORE & GUIDE PANEL ---")]
    public UnityEngine.UI.Image loadingTipImage;   // Ekranda lore resminin basılacağı Image bileşeni 🎯
    public TextMeshProUGUI loadingTipTitleText;   // "LORE" veya "REHBER" başlığı 🎯
    public TextMeshProUGUI loadingTipDescText;    // Hikaye ya da rehber açıklaması 🎯

    [Header("--- TRANSITION SETTINGS ---")]
    public CanvasGroup fadeGroup;
    public float fadeDuration = 1.0f;
    public float sceneFadeDuration = 0.8f;
    public float waitOnLastFrame = 2.0f;

    // Sunucudan (PostgreSQL) çekip dolduracağımız o şanlı liste 📡
    [HideInInspector] public List<VanthLoreLoadingTip> serverLoadingTips = new List<VanthLoreLoadingTip>();

    private AsyncOperationHandle<Sprite> _currentTipImageHandle; // RAM temizliği için takip kolu

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

    public void ChangeScene(string targetSceneName, bool shouldSave = true)
    {
        // 🚀 SAHNE DEĞİŞİRKEN RASTGELE BİR LORE/GUIDE GÖRSELİ SEÇİP HAZIRLIYORUZ
        PrepareRandomLoadingTip();

        ShowLoadingScreen(0.05f);
        StartCoroutine(SceneTransitionRoutine(targetSceneName, shouldSave));
    }

    private void PrepareRandomLoadingTip()
    {
        // Savunma hattı: Eğer sunucudan liste henüz inmediyse veya boşsa bodoslama çık çökmesin
        if (serverLoadingTips == null || serverLoadingTips.Count == 0) return;

        // 🎲 Listeden rastgele bir indeks seçiyoruz
        int randomIndex = Random.Range(0, serverLoadingTips.Count);
        VanthLoreLoadingTip selectedTip = serverLoadingTips[randomIndex];

        // 1. Metinleri Dil Sözlüğünden (Localization) geçirerek basıyoruz canım benim
        if (loadingTipTitleText != null && LocalizationManager.Instance != null)
            loadingTipTitleText.text = LocalizationManager.Instance.GetText(selectedTip.title_key);

        if (loadingTipDescText != null && LocalizationManager.Instance != null)
            loadingTipDescText.text = LocalizationManager.Instance.GetText(selectedTip.desc_key);

        // 2. RAM TAHLİYESİ: Eğer bir önceki sahne geçişinden kalan bir görsel yükü varsa RAM'den söküyoruz
        if (_currentTipImageHandle.IsValid())
        {
            Addressables.Release(_currentTipImageHandle);
        }

        // 3. ASENKRON GÖRSEL YÜKLEME: Resim dosyasını Addressables ile buluttan/diskten çağırıyoruz
        if (loadingTipImage != null && !string.IsNullOrEmpty(selectedTip.image_key))
        {
            _currentTipImageHandle = Addressables.LoadAssetAsync<Sprite>(selectedTip.image_key);
            _currentTipImageHandle.Completed += (handle) =>
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            loadingTipImage.sprite = handle.Result;
            Debug.Log($"<color=lime>[VanthLore Loader]</color> Yükleme ekranı görseli başarıyla giydirildi: {selectedTip.image_key}");
        }
        else
        {
            // 🛡️ GÜVENLİK YEDEĞİ: Görsel indirilemezse paneli boş bırakma, varsayılan bir logo koy canım
            // loadingTipImage.sprite = defaultLoadingLogo; 
            Debug.LogWarning($"[VanthLore Loader] İpucu görseli yüklenemedi: {selectedTip.image_key}");
        }
    };
        }
    }

    private IEnumerator SceneTransitionRoutine(string targetSceneName, bool shouldSave)
    {
        Debug.Log($"<color=orange>[VanthLore Loader]</color> '{targetSceneName}' sahnesine geçiş lojistiği başladı...");

        if (fadeGroup != null)
        {
            fadeGroup.gameObject.SetActive(true);
            yield return StartCoroutine(FadeCanvas(fadeGroup, 0, 1));
        }

        if (shouldSave) yield return StartCoroutine(AutoSaveBeforeTransition(targetSceneName));

        var handle = Addressables.LoadSceneAsync(targetSceneName);

        while (!handle.IsDone)
        {
            float progress = Mathf.Clamp01(handle.PercentComplete);
            UpdateLoadingBar(progress);
            yield return null;
        }

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log($"<color=green>[VanthLore Loader]</color> {targetSceneName} RAM'e indi. Vejetasyon havuzlaması tetikleniyor...");
            HandleVegetationPooling();
        }

        yield return new WaitForEndOfFrame();

        if (targetSceneName == "InitialMenu")
        {
            if (SoundtrackManager.Instance != null)
                _ = SoundtrackManager.Instance.PlayMusicByKey("music_menu_theme");
        }

        // Oyuncu o şanlı gotik çizimlerini rahat rahat okusun diye beklettiğimiz o tatlı nefes payı 🎯
        yield return new WaitForSeconds(waitOnLastFrame);

        if (fadeGroup != null)
        {
            yield return StartCoroutine(FadeCanvas(fadeGroup, 1, 0));
            fadeGroup.gameObject.SetActive(false);
        }

        HideLoadingScreen();
    }

    private void HandleVegetationPooling()
    {
        VegetationManager vegManager = GameObject.FindAnyObjectByType<VegetationManager>();
        if (vegManager != null && vegManager.sceneData != null)
        {
            HashSet<string> uniquePrefabs = new HashSet<string>();
            foreach (var point in vegManager.sceneData.vegetationPoints)
            {
                if (!string.IsNullOrEmpty(point.prefabID)) uniquePrefabs.Add(point.prefabID);
            }

            foreach (string prefabID in uniquePrefabs)
            {
                ObjectPooler.Instance.PreWarm(prefabID, 12);
            }
        }
    }

    private IEnumerator AutoSaveBeforeTransition(string targetSceneName)
    {
        if (StatsManager.Instance != null && SaveManager.instance != null)
        {
            SaveData currentData = new SaveData();
            StatsManager.Instance.ExportToSaveData(currentData);
            currentData.lastScene = targetSceneName;

            var saveTask = SaveManager.instance.SaveGame(currentData, "VanthLore_QuickSave");
            while (!saveTask.IsCompleted) yield return null;
        }
    }

    public void ShowLoadingScreen(float initialProgress = 0f)
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
            if (loadingBar != null) loadingBar.value = initialProgress;
        }
    }

    public void HideLoadingScreen()
    {
        if (loadingPanel != null) loadingPanel.SetActive(false);

        // 🎯 GÜVENLİK TEMİZLİĞİ: Yükleme ekranı kapandığı saniye 
        // o yüklenen lore görselini RAM'den uçuruyoruz ki boşuna yer kaplamasın!
        if (_currentTipImageHandle.IsValid())
        {
            Addressables.Release(_currentTipImageHandle);
            if (loadingTipImage != null) loadingTipImage.sprite = null;
        }
    }

    public void UpdateLoadingBar(float progress)
    {
        if (loadingBar != null) loadingBar.value = progress;
    }

    // VanthLoreSceneLoader.cs içindeki o şanlı metod artık public! 👑
    public IEnumerator FadeCanvas(CanvasGroup cg, float start, float end)
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, t / fadeDuration);
            yield return null;
        }
        cg.alpha = end;
    }

    // Sunucudan gelen ham JSON dizisini Unity'nin çözebilmesi için sarmalayan minik yardımcı sınıf
    [System.Serializable]
    public class VanthLoreLoadingTipWrapper
    {
        public List<VanthLoreLoadingTip> tips;
    }

    // 🛡️ 1. ODA BAZLI FİZİK VE KORUMA MODELİ
    [System.Serializable]
    public class VanthLoreRoomPhysicsDTO
    {
        public float spawn_x;
        public float spawn_y;
        public float walk_speed;
        public float run_speed;
        public float gravity_scale;
        public float forced_scale;
        public float jump_force;
    }

    // ⚔️ 2. DOĞRULANMIŞ OYUNCU STAT MODELİ
    [System.Serializable]
    public class VanthLorePlayerProfileDTO
    {
        public int current_level;
        public int current_exp;
        public int max_exp;
        public float current_health;
        public float max_health;
        public float current_mana;
        public float max_mana;
        public bool is_armed;

        // Sinsi Kozmetik Şalterleri (Görsel Giydirme İçin)
        public bool is_helmet_equipped;
        public bool is_boot_equipped;
        public bool is_gauntlet_equipped;
        public bool is_pad_equipped;
        public bool is_pauldron_equipped;
    }

    public class VanthLoreNetworkManager : MonoBehaviour
    {
        // Frankfurt AWS Lightsail sunucunun o şanlı endpoint adresi 🎯
        private string serverUrl = "https://vanthlore.repandus.com/api/loading-tips";

        void Start()
        {
            // Oyun açıldığında bodoslama sunucudan taptaze lore listesini çekiyoruz
            StartCoroutine(FetchLoadingTipsRoutine());
        }

        private IEnumerator FetchLoadingTipsRoutine()
        {
            Debug.Log("<color=cyan>[VanthLore Network]</color> Yükleme ekranı ipuçları sunucudan talep ediliyor...");

            using (UnityWebRequest webRequest = UnityWebRequest.Get(serverUrl))
            {
                // İsteği Frankfurt semalarına fırlatıyoruz
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogWarning($"⚠️ <color=yellow>[VanthLore Network]</color> Sunucudan yükleme ipuçları çekilemedi, lokal yedekler kullanılabilir. Hata: {webRequest.error}");
                }
                else
                {
                    // Sunucudan gelen veri örn: {"tips": [{"type":"LORE", "image_key":"...", "title_key":"...", "desc_key":"..."}]}
                    string jsonResponse = webRequest.downloadHandler.text;

                    // Unity'nin JsonUtility mekanizmasıyla ham metni jilet gibi nesne listesine çeviriyoruz
                    VanthLoreLoadingTipWrapper wrapper = JsonUtility.FromJson<VanthLoreLoadingTipWrapper>(jsonResponse);

                    if (wrapper != null && wrapper.tips != null && VanthLoreSceneManager.Instance != null)
                    {
                        // 🎯 KUTSAL AKTARIM: Sunucudan akan veriyi küresel sahne yükleyicimizin damarlarına basıyoruz!
                        VanthLoreSceneManager.Instance.serverLoadingTips = wrapper.tips;
                        Debug.Log($"<color=lime>[VanthLore Live-Ops]</color> {wrapper.tips.Count} adet Lore/Guide verisi PostgreSQL'den çekildi ve mühürlendi canım!");
                    }
                }
            }
        }
    }
}