using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundSettingsController : MonoBehaviour
{
    public static SoundSettingsController Instance { get; private set; }

    [Header("Mixer References")]
    [SerializeField] private AudioMixer gameMasterMixer;

    [Header("UI References")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        // Sinyal kaçmış olsa bile, uyanınca bir kez StatsManager'daki mevcut değeri bas
        if (StatsManager.Instance != null)
        {
            HandleDataImported();
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
        }
    }

    // --- EVENT ABONELİĞİ ---
    // StatsManager'daki OnDataImported sinyalini dinlemeye başlıyoruz
    private void OnEnable()
    {
        StatsManager.OnDataImported += HandleDataImported;
    }

    private void OnDisable()
    {
        StatsManager.OnDataImported -= HandleDataImported;
    }

    // Sinyal geldiğinde çalışan ana metod
    private void HandleDataImported()
    {
        Debug.Log("<color=cyan>SoundSettingsController:</color> StatsManager verileri yükledi, sesler güncelleniyor.");

        // 2. UI Slider'larını yeni değerlere göre kaydır
        RefreshUISliders();

        // 3. AudioMixer'ı gerçek değerlerle güncelle (Sesin değiştiği an burası)
        ApplyAllVolumes();
    }

    // --- UI VE MIXER GÜNCELLEME ---

    public void RefreshUISliders()
    {
        if (StatsManager.Instance == null) return;

        // value = ... yerine SetValueWithoutNotify kullanarak eventleri susturuyoruz
        if (masterSlider != null)
            masterSlider.SetValueWithoutNotify(StatsManager.Instance.masterVolume);

        if (musicSlider != null)
            musicSlider.SetValueWithoutNotify(StatsManager.Instance.musicVolume);

        if (sfxSlider != null)
            sfxSlider.SetValueWithoutNotify(StatsManager.Instance.sfxVolume);

        // Sliderlar sustuğu için Mixer'i manuel olarak bir kez güncelliyoruz
        ApplyAllVolumes();
    }

    private void ApplyAllVolumes()
    {
        ApplyVolume("MasterVol", StatsManager.Instance.masterVolume);
        ApplyVolume("MusicVol", StatsManager.Instance.musicVolume);
        ApplyVolume("SFXVol", StatsManager.Instance.sfxVolume);
    }

    // --- SES GÜNCELLEME METODLARI (Slider'lar buna bağlı kalmalı) ---

    public void UpdateMasterVolume(float level)
    {
        StatsManager.Instance.masterVolume = level;
        ApplyVolume("MasterVol", level);

    }

    public void UpdateMusicVolume(float level)
    {
        StatsManager.Instance.musicVolume = level;
        ApplyVolume("MusicVol", level);

    }

    public void UpdateSFXVolume(float level)
    {
        StatsManager.Instance.sfxVolume = level;
        ApplyVolume("SFXVol", level);

    }

    private void ApplyVolume(string parameterName, float level)
    {
        Debug.Log($"<color=orange>Mixer Güncelleme:</color> Parametre: {parameterName}, Değer: {level}");
        if (gameMasterMixer != null)
        {
            // Logaritmik ölçek: 0.0001 (sessiz) -> 1.0 (full ses)
            float dbValue = Mathf.Log10(Mathf.Clamp(level, 0.0001f, 1f)) * 20;
            gameMasterMixer.SetFloat(parameterName, dbValue);
        }
    }

    // --- KAYIT VE YÜKLEME ---
}