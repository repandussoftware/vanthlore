using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets; 
using UnityEngine.U2D;
using System.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations; 

public partial class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Addressables UI")]
    public AssetReference uiAtlasReference; 
    private SpriteAtlas cachedUIAtlas;

    [Header("Metin Efekti Ayarları")]
    public CanvasGroup locationTextGroup;
    public TextMeshProUGUI locationNameText;

    [Header("Global UI Panelleri")]
    public GameObject inventoryPopup;
    public GameObject mapPopup;
    public GameObject dutiesPopup;
    public GameObject parchementPopup;
    public GameObject lootPopup;
    public GameObject wardrobePopup;
    public GameObject settingsPopup;
    public GameObject SkillPopup;
    public GameObject infoPaperPopup;
    public GameObject dialogPanel;
    
    [Header("HUD Elements")]
    public TextMeshProUGUI timeText;
    public GameObject[] timeImage;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI diamondText;
    public TextMeshProUGUI expText;
    public TextMeshProUGUI levelText;

    [Header("HUD Bars (HP & MP)")]
    public UnityEngine.UI.Slider healthSlider;
    public TMPro.TextMeshProUGUI healthText;
    public UnityEngine.UI.Slider manaSlider;
    public TMPro.TextMeshProUGUI manaText;

    [Header("Uyarı Toast Prefabı")]
    public GameObject warningToastPrefab;
    public Transform canvasTransform;

    private bool isResting = false;

    void Awake()
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

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "InitialMenu")
        {
            _ = EnvironmentTextureManager.Instance.LoadOpeningElements();
            ApplyUIAtlasToPopups(); 
        }
    }

    void Update()
    {
        if (StatsManager.Instance != null)
        {
            if (coinText != null) coinText.text = StatsManager.Instance.totalCoins.ToString();
            if (diamondText != null) diamondText.text = StatsManager.Instance.currentDiamonds.ToString();
        }
    }

    public void UpdateHUD(float currentHp, float maxHp, float currentMp, float maxMp)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHp;
            healthSlider.value = currentHp;
        }

        if (healthText != null)
        {
            healthText.text = $"{(int)currentHp}/{(int)maxHp}";
        }

        if (manaSlider != null)
        {
            manaSlider.maxValue = maxMp;
            manaSlider.value = currentMp;
        }

        if (manaText != null)
        {
            manaText.text = $"{(int)currentMp}/{(int)maxMp}";
        }
    }

    public async Task InitializeUIAtlas()
    {
        if (uiAtlasReference == null || !uiAtlasReference.RuntimeKeyIsValid()) return;

        var handle = uiAtlasReference.LoadAssetAsync<SpriteAtlas>();
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            cachedUIAtlas = handle.Result;
            Debug.Log("<color=gold>VanthLore:</color> Global UI Atlası başarıyla yüklendi.");
            ApplyUIAtlasToPopups();
        }
    }

    private void ApplyUIAtlasToPopups()
    {
        if (cachedUIAtlas == null) return;

        var allImages = GameObject.FindObjectsByType<UnityEngine.UI.Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var img in allImages)
        {
            Sprite s = cachedUIAtlas.GetSprite(img.gameObject.name);
            if (s != null) img.sprite = s;
        }
    }

    // 🎯 ATMOSFER MANAGER'IN TETİKLEYECEĞİ SAF UI FONKSİYONU
    public void UpdateZamanMetniAndImages(bool isDayTime)
    {
        if (timeText != null && LocalizationManager.Instance != null)
        {
            string dayText = LocalizationManager.Instance.GetText("UI.gameplay_hud_panel.stats_bar.daytime_morning");
            string nightText = LocalizationManager.Instance.GetText("UI.gameplay_hud_panel.stats_bar.daytime_night");
            timeText.text = isDayTime ? dayText : nightText;
        }

        if (timeImage != null && timeImage.Length >= 2)
        {
            if (timeImage[0] != null) timeImage[0].SetActive(isDayTime);
            if (timeImage[1] != null) timeImage[1].SetActive(!isDayTime);
        }
    }

    public void UpdateExperienceUI(int level, int exp, int maxExp)
    {
        if (levelText != null) levelText.text = "LVL " + level;
        if (expText != null) expText.text = $"{exp} / {maxExp}";
    }

    public void ShowWarning(string message)
    {
        if (warningToastPrefab == null || canvasTransform == null) return;

        GameObject toastObj = Instantiate(warningToastPrefab, canvasTransform);
        toastObj.transform.SetAsLastSibling();
        toastObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 300);

        WarningToast toastScript = toastObj.GetComponent<WarningToast>();
        if (toastScript != null) toastScript.Initialize(message);
    }

    // --- Panel Kontrolleri ---
    public void ToggleInventory() { if (!isResting && inventoryPopup != null) inventoryPopup.SetActive(!inventoryPopup.activeSelf); }
    public void ToggleMap() { if (!isResting && mapPopup != null) mapPopup.SetActive(!mapPopup.activeSelf); }
    public void ToggleDuties() { if (!isResting && dutiesPopup != null) dutiesPopup.SetActive(!dutiesPopup.activeSelf); }
    public void ToggleParchement() { if (!isResting && parchementPopup != null) parchementPopup.SetActive(!parchementPopup.activeSelf); }
    public void ToggleLootPopup() { if (!isResting && lootPopup != null) lootPopup.SetActive(!lootPopup.activeSelf); }
    public void ToggleSkillPopup() { if (!isResting && SkillPopup != null) SkillPopup.SetActive(!SkillPopup.activeSelf); }
    public void ToggleWardrobePopup() { if (!isResting && wardrobePopup != null) wardrobePopup.SetActive(!wardrobePopup.activeSelf); }

    public void CloseInfoPaper() { if (infoPaperPopup != null) { infoPaperPopup.SetActive(false); PlayClickSound(); } }
    public void ShowSettingsPopup() { if (settingsPopup != null) { settingsPopup.SetActive(true); PlayClickSound(); } }

    public void ShowLootPopup(int amount)
    {
        ToggleLootPopup();
        if (LootPopupManager.Instance != null) LootPopupManager.Instance.FillLoot(amount);
    }

    public void AddCoins(int amount)
    {
        StatsManager.Instance.totalCoins += amount;
        if (coinText != null) coinText.text = StatsManager.Instance.totalCoins.ToString();
    }

    public void AddDiamonds(int amount)
    {
        StatsManager.Instance.currentDiamonds += amount;
        if (diamondText != null) diamondText.text = StatsManager.Instance.currentDiamonds.ToString();
    }

    public void CloseAllPanels()
    {
        if (inventoryPopup != null) inventoryPopup.SetActive(false);
        if (mapPopup != null) mapPopup.SetActive(false);
        if (dutiesPopup != null) dutiesPopup.SetActive(false);
        if (parchementPopup != null) parchementPopup.SetActive(false);
        if (lootPopup != null) lootPopup.SetActive(false);
        if (wardrobePopup != null) wardrobePopup.SetActive(false);
        if (dialogPanel != null) dialogPanel.SetActive(false);
        if (SkillPopup != null) SkillPopup.SetActive(false);
        if (settingsPopup != null)
        {
            settingsPopup.SetActive(false);
            if (SettingsManager.Instance != null) SettingsManager.Instance.saveSoundSettings();
            if (MenuController.Instance != null) MenuController.Instance.SaveOnlySettings();
        }
    }

    public void PlayClickSound() { if (SoundtrackManager.Instance != null) SoundtrackManager.Instance.PlayClickSound(); }

    public void RestAtDarionsRoom() { if (!isResting) StartCoroutine(RestRoutine()); }

    private IEnumerator RestRoutine()
    {
        isResting = true;
        CloseAllPanels();

        Debug.Log("<color=orange>VanthLore:</color> Dinlenme süreci başladı, ekran karartılıyor...");

        if (VanthLoreSceneManager.Instance != null && VanthLoreSceneManager.Instance.fadeGroup != null)
        {
            VanthLoreSceneManager.Instance.fadeGroup.gameObject.SetActive(true);
            yield return StartCoroutine(VanthLoreSceneManager.Instance.FadeCanvas(VanthLoreSceneManager.Instance.fadeGroup, 0f, 1f));
        }

        yield return new WaitForSeconds(0.8f);

        // 🎯 YENİ ATMANA PASLIYORUZ: Zaman değiştiğinde ışıkları yeni manager eziyor
        StatsManager.Instance.isDayTime = !StatsManager.Instance.isDayTime;
        if (VanthLoreAtmosManager.Instance != null) VanthLoreAtmosManager.Instance.ApplyCurrentEnvironment();

        yield return StartCoroutine(SaveAfterRest());

        if (locationTextGroup != null && VanthLoreSceneManager.Instance != null)
            yield return StartCoroutine(VanthLoreSceneManager.Instance.FadeCanvas(locationTextGroup, 0f, 1f));

        yield return new WaitForSeconds(1.5f);

        if (VanthLoreSceneManager.Instance != null && VanthLoreSceneManager.Instance.fadeGroup != null)
        {
            yield return StartCoroutine(VanthLoreSceneManager.Instance.FadeCanvas(VanthLoreSceneManager.Instance.fadeGroup, 1f, 0f));
            VanthLoreSceneManager.Instance.fadeGroup.gameObject.SetActive(false);
        }

        isResting = false;
    }

    private IEnumerator SaveAfterRest()
    {
        if (StatsManager.Instance != null && SaveManager.instance != null)
        {
            SaveData currentData = new SaveData();
            StatsManager.Instance.ExportToSaveData(currentData);
            currentData.lastScene = SceneManager.GetActiveScene().name;

            var saveTask = SaveManager.instance.SaveGame(currentData, "VanthLore_QuickSave");
            while (!saveTask.IsCompleted) yield return null;

            if (MenuController.Instance != null) MenuController.Instance.activeSaveData = currentData;
        }
    }

private void FindSceneSpecificUI()
    {
        Canvas[] allCanvases = GameObject.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (allCanvases == null || allCanvases.Length == 0) return;

        if (inventoryPopup == null) inventoryPopup = SearchInAllCanvases("InventoryPopup", allCanvases);
        if (mapPopup == null) mapPopup = SearchInAllCanvases("MapPopup", allCanvases);
        if (dutiesPopup == null) dutiesPopup = SearchInAllCanvases("DutiesPopup", allCanvases);
        if (parchementPopup == null) parchementPopup = SearchInAllCanvases("ParchementPopup", allCanvases);
        if (lootPopup == null) lootPopup = SearchInAllCanvases("LootPopup", allCanvases);
        if (wardrobePopup == null) wardrobePopup = SearchInAllCanvases("WardrobePopup", allCanvases);

        // 🎯 RADAR ENTEGRASYONU: Eğer barlar DontDestroyOnLoad değilse sahne başında isimleriyle şak diye bulur:
        if (healthSlider == null) 
        {
            GameObject hpGo = SearchInAllCanvases("HealthSlider", allCanvases);
            if (hpGo != null) healthSlider = hpGo.GetComponent<UnityEngine.UI.Slider>();
        }
        if (manaSlider == null) 
        {
            GameObject mpGo = SearchInAllCanvases("ManaSlider", allCanvases);
            if (mpGo != null) manaSlider = mpGo.GetComponent<UnityEngine.UI.Slider>();
        }
    }

    private GameObject SearchInAllCanvases(string targetName, Canvas[] canvases)
    {
        foreach (Canvas canvas in canvases)
        {
            Transform found = canvas.transform.Find(targetName);
            if (found != null) return found.gameObject;
        }
        return null;
    }
}