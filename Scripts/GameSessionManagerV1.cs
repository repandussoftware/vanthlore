using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSessionManager : MonoBehaviour
{
    public static GameSessionManager Instance;

    [Header("--- HUD PANELS ---")]
    [SerializeField] private GameObject gameplayHUDPanel;

    [Header("--- SCENE GAMEPLAY OBJECTS ---")]
    [Tooltip("Sahnede oyun başladığında açılacak canavarlar, tetikleyiciler veya harita elemanları canım")]
    [SerializeField] private GameObject[] gameplayObjects;

    [HideInInspector] public bool isFirstLoad = true; // 🎯 İlk yükleme kilidi

    private void Awake()
    {
        // Kurumsal Singleton omurgası sahneler arası yaşıyor canım
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

    private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    // 🚀 HARİTA DEĞİŞTİĞİ AN TETİKLENEN KUTSAL YAŞAM DÖNGÜSÜ
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Eğer oyuncu hala giriş yapılan ilk menüde değilse, gerçek oyuna sızmış demektir!
        if (scene.name != "InitialMenu")
        {
            if (gameplayHUDPanel != null) gameplayHUDPanel.SetActive(true);

            // Sahnede canavar ve etkileşim objelerini ayağa kaldır canım
            ToggleGameplayObjects(true);

            // 🎯 KRİTİK KONTROL: Oyuncu buluttan ilk kez giriş yaptığında Darion'u uyandırıyoruz
            if (isFirstLoad && DarionController.Instance != null)
            {
                Debug.Log("<color=cyan>GameSessionManager:</color> İlk bulut oturumu mühürleniyor, Darion uykudan uyandırılıyor...");
                DarionController.Instance.gameObject.SetActive(true);
                //DarionController.Instance.ApplySceneSpecificSettings(scene.name);
                
                isFirstLoad = false; // Kapıyı kapatıyoruz, harita geçişlerinde bir daha tetiklenmez
            }

            // Oyuncuyu buluttan gelen koordinatlara milimetrik yerleştir canım
            PositionPlayerFromCloud();

            // Ekipmanların yüklenmesi bitince can barlarını fulle ve HUD'ı tazele
            StartCoroutine(InitializeHUDAndStatsRoutine());
        }
        else
        {
            // Eğer InitialMenu'ye geri dönüldüyse (Logout vb.) her şeyi sustur canım
            ToggleGameplayObjects(false);
            if (gameplayHUDPanel != null) gameplayHUDPanel.SetActive(false);
        }
    }

    // 🌍 EN KRİTİK KÖPRÜ: Oyuncuyu dünyadaki bulut koordinatlarına ışınlayan sihirbaz
    private void PositionPlayerFromCloud()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        // Sahnedeki AuthManager'a uzanıp buluttan gelen o anlık canlı save verisini okuyoruz!
        AuthManager auth = FindObjectOfType<AuthManager>();
        if (auth != null && auth.currentUser != null && auth.currentUser.save_data != null)
        {
            var save = auth.currentUser.save_data;

            if (save.playerPosition != null && save.playerPosition.Length >= 3)
            {
                player.transform.position = new Vector3(save.playerPosition[0], save.playerPosition[1], save.playerPosition[2]);
                player.transform.eulerAngles = new Vector3(save.playerRotation[0], save.playerRotation[1], save.playerRotation[2]);
                player.transform.localScale = new Vector3(save.playerScale[0], save.playerScale[1], save.playerScale[2]);
                
                Debug.Log($"<color=lime>GameSessionManager:</color> Kahraman buluttaki koordinatlarına başarıyla yerleştirildi: {player.transform.position}");
            }
        }
    }

    private IEnumerator InitializeHUDAndStatsRoutine()
    {
        // Diğer objelerin Awake/Start işlemlerini bitirmesi için bir kare bekliyoruz canım
        yield return new WaitForEndOfFrame();

        // 1. Yetenek slotlarını tazele
        if (SkillBarManager.Instance != null)
        {
            SkillBarManager.Instance.RefreshAllSkillSlots();
        }

        // 2. Ekipman bonusları RAM'e tam işlensin diye bir tık daha bekleyip canları eşle canım
        if (StatsManager.Instance != null)
        {
            StatsManager.Instance.currentHealth = StatsManager.Instance.maxHealth;
            StatsManager.Instance.currentMana = StatsManager.Instance.maxMana;
            
            // Eğer HUD UI bar yenileme metodun varsa buraya tetik atabilirsin canım:
            // UIManager.Instance.RefreshHUD();
        }

        Debug.Log("<color=magenta>GameSessionManager:</color> Sahne içi arayüz ve stat senkronizasyonu tamamlandı.");
    }

    private void ToggleGameplayObjects(bool state)
    {
        if (gameplayObjects == null) return;
        foreach (GameObject obj in gameplayObjects)
        {
            if (obj != null) obj.SetActive(state);
        }
    }

    // --- SİSTEMDEN GÜVENLİ ÇIKIŞ ---
    public void QuitGame()
    {
        Debug.Log("<color=red>GameSessionManager:</color> Sunucu oturumu kapatılıyor, oyun sonlandırılıyor...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}