using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement; // 🚀 Kutsal sahne geçiş motoru için şart canım!
using TMPro;
using Vanthlore.Auth;

namespace Vanthlore.Auth
{
    // --- DTO (AĞ VERİ PAKETLERİ) ---

    [System.Serializable]
    public class LoginRequest
    {
        public string usernameOrEmail;
        public string password;
    }

    [System.Serializable]
    public class RegisterRequest
    {
        public string username;
        public string email;
        public string password;
    }

    [System.Serializable]
    public class GuestLoginRequest
    {
        public string device_id;
    }

    [System.Serializable]
    public class CloudSavePostPack
    {
        public int user_id;
        public CloudSaveData save_data;
    }

    // ⚔️ Sunucu ile birebir eşleşen ve hafızaya kaydedilecek olan kutsal oyuncu profili sınıfı
    [System.Serializable]
    public class LocalUserSession
    {
        public int id;
        public string username;
        public string email;
        public int gold;
        public int premium_currency;
        public bool is_guest;
        public CloudSaveData save_data; // 🎯 Dev bulut save paketi buraya iniyor canım!
        public string created_at;
    }

    [System.Serializable]
    public class AuthResponse
    {
        public string status;
        public string message;
        public LocalUserSession user;
    }
}

public class AuthManager : MonoBehaviour
{
    [Header("--- UI PANELS ---")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject registerPanel;
    [SerializeField] private GameObject guestPanel;
    [SerializeField] private GameObject loadingPanel; // ⏳ İlk açılışta kontrol yapılırken görünecek şık panel

    [Header("--- LOGIN INPUTS ---")]
    [SerializeField] private TMP_InputField loginUsernameInput;
    [SerializeField] private TMP_InputField loginPasswordInput;

    [Header("--- CURRENT ACTIVE USER SESSION ---")]
    public LocalUserSession currentUser;

    private const string BASE_URL = "https://vanthlore.repandus.com/api/auth";
    private const string CACHE_KEY = "LUNARA_USER_SESSION"; // Diske kilitlenecek yerel hafıza anahtarımız

    private void Awake()
    {
        if (loadingPanel != null) loadingPanel.SetActive(true);
    }

    private void Start()
    {
        CheckLocalSessionOnBoot();
    }

    // 🚀 OYUN AÇILDIĞI AN ÇALIŞAN OTOMATİK OTURUM SÜZGECİ
    private void CheckLocalSessionOnBoot()
    {
        // 1. Oyuncu cihazda daha önce başarılı bir tam giriş yapmış mı?
        if (PlayerPrefs.HasKey(CACHE_KEY))
        {
            string cachedJson = PlayerPrefs.GetString(CACHE_KEY);
            currentUser = JsonUtility.FromJson<LocalUserSession>(cachedJson);

            // Eğer misafirliği bitmiş gerçek, şifreli bir hesapsa hiç sunucuya sormadan direkt oyuna uçuruyoruz!
            if (currentUser != null && !currentUser.is_guest)
            {
                Debug.Log($"<color=cyan>Yerel Hafıza Onaylandı!</color> Gerçek hesap otomatik içeri alınıyor: {currentUser.username}");
                StartCoroutine(InitializeGameFlow(currentUser));
                return;
            }
        }

        // Eğer cihazda aktif şifreli bir oturum yoksa, sunucuya sormadan doğrudan misafir panelini açıp bekliyoruz!
        ShowGuestPanel();
    }

    // 📱 MİSAFİR (GUEST) GİRİŞ BUTONUNA BAĞLANACAK KUTSAL FONKSİYON
    public void OnClick_SubmitGuestLogin()
    {
        Debug.Log("<color=yellow>Misafir Girişi Butonuna Basıldı Canım!</color> Sunucu ağ trafiği başlatılıyor...");

        if (loadingPanel != null) loadingPanel.SetActive(true);
        StartCoroutine(AutoGuestLoginCoroutine());
    }

    private IEnumerator AutoGuestLoginCoroutine()
    {
        string url = $"{BASE_URL}/guest-login";
        GuestLoginRequest requestData = new GuestLoginRequest { device_id = SystemInfo.deviceUniqueIdentifier };
        string jsonBody = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = CreateJsonPostRequest(url, jsonBody))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                AuthResponse response = JsonUtility.FromJson<AuthResponse>(request.downloadHandler.text);
                SaveSessionLocally(response.user); // Oturumu diske kilitliyoruz canım
                StartCoroutine(InitializeGameFlow(response.user));
            }
            else
            {
                Debug.LogWarning("Misafir Girişi Reddedildi (Hesap yükseltilmiş olabilir): " + request.downloadHandler.text);
                ShowLoginPanel();
            }
        }
    }

    // --- NORMAL KULLANICI GİRİŞ MOTORU ---
    public void OnClick_SubmitLogin()
    {
        string usernameOrEmail = loginUsernameInput.text.Trim();
        string password = loginPasswordInput.text;

        if (string.IsNullOrEmpty(usernameOrEmail) || string.IsNullOrEmpty(password)) return;

        if (loadingPanel != null) loadingPanel.SetActive(true);
        StartCoroutine(LoginCoroutine(usernameOrEmail, password));
    }

    private IEnumerator LoginCoroutine(string usernameOrEmail, string password)
    {
        string url = $"{BASE_URL}/login";
        LoginRequest requestData = new LoginRequest { usernameOrEmail = usernameOrEmail, password = password };
        string jsonBody = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = CreateJsonPostRequest(url, jsonBody))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // ❌ ESKİ HATALI SATIR:

                //  YENİ DOĞRU SATIR:
                AuthResponse response = JsonUtility.FromJson<AuthResponse>(request.downloadHandler.text);
                SaveSessionLocally(response.user);
                StartCoroutine(InitializeGameFlow(response.user));
            }
            else
            {
                Debug.LogError("Giriş Hatası: " + request.downloadHandler.text);
                if (loadingPanel != null) loadingPanel.SetActive(false);
            }
        }
    }

    // --- DÜMDÜZ NATIVE KAYIT MOTORU ---
    public void ExecuteRegister(string username, string email, string password)
    {
        if (loadingPanel != null) loadingPanel.SetActive(true);
        StartCoroutine(RegisterCoroutine(username, email, password));
    }

    private IEnumerator RegisterCoroutine(string username, string email, string password)
    {
        string url = $"{BASE_URL}/register";
        RegisterRequest requestData = new RegisterRequest { username = username, email = email, password = password };
        string jsonBody = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = CreateJsonPostRequest(url, jsonBody))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                AuthResponse response = JsonUtility.FromJson<AuthResponse>(request.downloadHandler.text);
                SaveSessionLocally(response.user);
                StartCoroutine(InitializeGameFlow(response.user));
            }
            else
            {
                Debug.LogError("AWS Sunucu Kayıt Hatası: " + request.downloadHandler.text);
                if (loadingPanel != null) loadingPanel.SetActive(false);
            }
        }
    }

    // 💾 OYUN İÇİNDEN BULUTA ANLIK SAVE GÖNDERME MOTORU (SaveManager Burayı Tetikler Canım)
    public void SaveCurrentGameProgress(CloudSaveData runtimeSaveData)
    {
        if (currentUser == null) return;
        currentUser.save_data = runtimeSaveData;
        StartCoroutine(UploadSaveDataCoroutine(runtimeSaveData));
    }

    private IEnumerator UploadSaveDataCoroutine(CloudSaveData dataToUpload)
    {
        string url = $"{BASE_URL}/save";
        CloudSavePostPack postPack = new CloudSavePostPack { user_id = currentUser.id, save_data = dataToUpload };
        string jsonBody = JsonUtility.ToJson(postPack);

        using (UnityWebRequest request = CreateJsonPostRequest(url, jsonBody))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("<color=green>Bulut Save:</color> İlerleme başarıyla AWS veritabanına mühürlendi canım.");
                SaveSessionLocally(currentUser);
            }
            else
            {
                Debug.LogError("Bulut Save Hatası: " + request.downloadHandler.text);
            }
        }
    }

    // ⚔️ BULUT VERİSİNİ STATSMANAGER'A ENJEKTE EDEN KUTSAL KÖPRÜ MOTORU
    private IEnumerator InitializeGameFlow(LocalUserSession sessionData)
    {
        if (loadingPanel != null) loadingPanel.SetActive(true);

        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        if (guestPanel != null) guestPanel.SetActive(false);

        // 1. ADIM: Çizim tarzımızın temeli olan Darion global atlasını arka planda asenkron yüklüyoruz canım
        if (EnvironmentTextureManager.Instance != null)
        {
            var textureTask = EnvironmentTextureManager.Instance.LoadProtagonistGlobal();
            yield return new WaitUntil(() => textureTask.IsCompleted);
        }

        // 2. ADIM: Sunucudan az önce inen taze CloudSaveData paketini, 
        // StatsManager'ın beklediği eski SaveData kutusuna (Wrapper) 1:1 kopyalayıp enjekte ediyoruz canım!
        if (sessionData.save_data != null)
        {
            Debug.Log($"<color=green>Veri Köprüsü:</color> {sessionData.username} verileri yerel motor formatına çevriliyor...");

            SaveData bridgeContainer = new SaveData();

            // Milimetrik 1:1 Değişken Eşleme Haritası canım:
            bridgeContainer.saveName = sessionData.save_data.saveName;
            bridgeContainer.lastScene = sessionData.save_data.lastScene;
            bridgeContainer.playerPosition = sessionData.save_data.playerPosition;
            bridgeContainer.playerRotation = sessionData.save_data.playerRotation;
            bridgeContainer.playerScale = sessionData.save_data.playerScale;
            bridgeContainer.currentLevel = sessionData.save_data.currentLevel;
            bridgeContainer.totalCoins = sessionData.save_data.totalCoins;
            bridgeContainer.currentDiamonds = sessionData.save_data.currentDiamonds;
            bridgeContainer.exchangeRate = sessionData.save_data.exchangeRate;
            bridgeContainer.currentHealth = sessionData.save_data.currentHealth;
            bridgeContainer.maxHealth = sessionData.save_data.maxHealth;
            bridgeContainer.currentMana = sessionData.save_data.currentMana;
            bridgeContainer.maxMana = sessionData.save_data.maxMana;
            bridgeContainer.currentExp = sessionData.save_data.currentExp;
            bridgeContainer.maxExp = sessionData.save_data.maxExp;

            bridgeContainer.isArmed = sessionData.save_data.isArmed;
            bridgeContainer.isHelmetEquipped = sessionData.save_data.isHelmetEquipped;
            bridgeContainer.isGauntletEquipped = sessionData.save_data.isGauntletEquipped;
            bridgeContainer.isBootEquipped = sessionData.save_data.isBootEquipped;
            bridgeContainer.isPadEquipped = sessionData.save_data.isPadEquipped;
            bridgeContainer.isPauldronEquipped = sessionData.save_data.isPauldronEquipped;

            bridgeContainer.startingItemsIDs = sessionData.save_data.startingItemsIDs;
            bridgeContainer.startingWearedItemsIDs = sessionData.save_data.startingWearedItemsIDs;
            bridgeContainer.isDayTime = sessionData.save_data.isDayTime;
            bridgeContainer.currentLanguage = sessionData.save_data.currentLanguage;

            bridgeContainer.masterVolume = sessionData.save_data.masterVolume;
            bridgeContainer.musicVolume = sessionData.save_data.musicVolume;
            bridgeContainer.sfxVolume = sessionData.save_data.sfxVolume;
            bridgeContainer.usingSkillsIDs = sessionData.save_data.usingSkillsIDs;
            bridgeContainer.unlockedSkillsIDs = sessionData.save_data.unlockedSkillsIDs;
            bridgeContainer.openedSkillSlots = sessionData.save_data.openedSkillSlots;
            bridgeContainer.quickBarPotionIDs = sessionData.save_data.quickBarPotionIDs;

            bridgeContainer.joyStickPosition = sessionData.save_data.joyStickPosition;
            bridgeContainer.joyStickScale = sessionData.save_data.joyStickScale;
            bridgeContainer.joyStickRotation = sessionData.save_data.joyStickRotation;
            bridgeContainer.joystickOpacity = sessionData.save_data.joystickOpacity;
            bridgeContainer.isJoystickPositionLocked = sessionData.save_data.isJoystickPositionLocked;
            bridgeContainer.isJoystickBackgroundVisible = sessionData.save_data.isJoystickBackgroundVisible;

            bridgeContainer.skillHUDScale = sessionData.save_data.skillHUDScale;
            bridgeContainer.skillHUDOpacity = sessionData.save_data.skillHUDOpacity;
            bridgeContainer.isSkillHUDLocked = sessionData.save_data.isSkillHUDLocked;
            bridgeContainer.savedSkillPositions = sessionData.save_data.savedSkillPositions;

            // Kutsal veriyi StatsManager mekanizmasına üflüyoruz!
            if (StatsManager.Instance != null)
            {
                StatsManager.Instance.ImportFromSaveData(bridgeContainer);
            }

            // 3. ADIM: Yükleme ekranını kapatıp oyuncuyu son kaldığı haritaya fırlatıyoruz canım!
            if (loadingPanel != null) loadingPanel.SetActive(false);

            // 🚀 BÜYÜK ŞAMPİYONLUK DÜZELTMESİ:
            // Önce master GameWorld (Kalıcı iskelet) sahnemizi düz açıyoruz canım
            SceneManager.LoadScene("GameWorld", LoadSceneMode.Single);

            // Sahne yüklendiği salisede bizim o akıllı Live-Ops Bootstrapper'ı uyandırıp 
            // oyuncunun save dosyasından gelen o son harita adını ("darions_room") üflüyoruz!
            StartCoroutine(TriggerCloudLoadPostScene(sessionData.save_data.lastScene));

            // Düzenli akış için çıtır bir bekleme coroutine'i canım beni
        }
    }

    private IEnumerator TriggerCloudLoadPostScene(string targetMapName)
    {
        yield return new WaitForEndOfFrame(); // GameWorld'ün kendine gelmesini bekliyoruz
        if (CloudSceneBootstrapper.Instance != null)
        {
            // Sistem Frankfurt'a uçuyor, Postgres'i okuyor ve her şeyi havada lego gibi birleştiriyor!
            CloudSceneBootstrapper.Instance.InitiateCloudSceneLoad(targetMapName);
        }
    }

    private void SaveSessionLocally(LocalUserSession userSession)
    {
        currentUser = userSession;
        string json = JsonUtility.ToJson(userSession);
        PlayerPrefs.SetString(CACHE_KEY, json);
        PlayerPrefs.Save(); // Mac/Cihaz diskine oturumu mühürle canım
    }

    public void Logout()
    {
        PlayerPrefs.DeleteKey(CACHE_KEY);
        currentUser = null;
        ShowGuestPanel();
    }

    public void ShowLoginPanel()
    {
        if (loadingPanel != null) loadingPanel.SetActive(false);
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
        guestPanel.SetActive(false);
    }

    public void ShowRegisterPanel()
    {
        if (loadingPanel != null) loadingPanel.SetActive(false);
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
        guestPanel.SetActive(false);
    }

    public void ShowGuestPanel()
    {
        if (loadingPanel != null) loadingPanel.SetActive(false);
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        guestPanel.SetActive(true);
    }

    private UnityWebRequest CreateJsonPostRequest(string url, string jsonBody)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        return request;
    }
}