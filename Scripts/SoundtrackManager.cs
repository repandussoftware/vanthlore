using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
using System.Collections.Generic;

public class SoundtrackManager : MonoBehaviour
{
    public static SoundtrackManager Instance { get; private set; }

    [Header("--- ONLY AUDIO SOURCES (HARDWARE) ---")]
    public AudioSource musicSource;
    public AudioSource ambienceSource;
    public AudioSource uiAudioSource;

    // RAM tahliyeleri için asenkron kilit kolları yerli yerinde duruyor canım
    private AsyncOperationHandle<AudioClip> musicHandle;
    private AsyncOperationHandle<AudioClip> ambienceHandle;
    private AsyncOperationHandle<AudioClip> uiClickHandle;

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

    private void Start()
    {
        // 🎯 TIKLAMA SESİ ÖN-YÜKLEME: Oyun açıldığı salisede tıklama sesi RAM'e hazır insin canım benim
        _ = InitializeClickSound("sfx_ui_click");

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "InitialMenu")
        {
            _ = PlayMusicByKey("music_menu_theme");
        }
    }

    // 🚀 GLOBAL MÜZİK MOTORU: Bootstrapper sunucudan aldığı string key'i buraya üfleyecek!
    public async Task PlayMusicByKey(string musicAddressableKey)
    {
        if (string.IsNullOrEmpty(musicAddressableKey)) return;

        // 1. AYNI MÜZİK ZATEN ÇALIYORSA BOŞUNA YENİDEN İNDİRİP RAM'E ÇAĞIRMA
        if (musicHandle.IsValid() && musicSource.isPlaying && musicSource.clip != null && musicHandle.Result != null && musicHandle.Result.name == musicAddressableKey)
            return;

        // 2. RAM TEMİZLİĞİ: Eski müziği hafızadan tamamen söküp atıyoruz
        if (musicHandle.IsValid())
        {
            Addressables.Release(musicHandle);
        }

        Debug.Log($"<color=green>Aritheon Audio:</color> '{musicAddressableKey}' müziği buluttan RAM'e çağrılıyor...");

        // 3. YENİ MÜZİĞİ ADRESİNDEN YÜKLE
        musicHandle = Addressables.LoadAssetAsync<AudioClip>(musicAddressableKey);
        await musicHandle.Task;

        if (musicHandle.Status == AsyncOperationStatus.Succeeded)
        {
            musicSource.clip = musicHandle.Result;
            musicSource.loop = true;
            musicSource.Play();
            Debug.Log($"<color=lime>Aritheon Audio:</color> Soundtrack mühürlendi: {musicHandle.Result.name}");
        }
        else
        {
            Debug.LogWarning($"⚠️ <color=yellow>Ses Eksik:</color> Addressables içinde '{musicAddressableKey}' anahtarı bulunamadı canım!");
        }
    }

    // 🚀 GLOBAL AMBİYANS MOTORU: Sunucudan gelen ortam sesini çalar
    public async Task PlayAmbienceByKey(string ambienceAddressableKey)
    {
        if (string.IsNullOrEmpty(ambienceAddressableKey)) return;

        if (ambienceHandle.IsValid() && ambienceSource.isPlaying && ambienceSource.clip != null && ambienceHandle.Result != null && ambienceHandle.Result.name == ambienceAddressableKey)
            return;

        if (ambienceHandle.IsValid())
        {
            Addressables.Release(ambienceHandle);
        }

        ambienceHandle = Addressables.LoadAssetAsync<AudioClip>(ambienceAddressableKey);
        await ambienceHandle.Task;

        if (ambienceHandle.Status == AsyncOperationStatus.Succeeded)
        {
            ambienceSource.clip = ambienceHandle.Result;
            ambienceSource.loop = true;
            ambienceSource.Play();
            Debug.Log($"<color=cyan>Aritheon Audio:</color> Yeni ambiyans mühürlendi: {ambienceHandle.Result.name}");
        }
    }

    // 🚀 KUTSAL SÖKÜM: UI Tıklama sesini de Addressables üzerinden dinamik yükleyelim canım
    public async Task InitializeClickSound(string clickSoundKey = "sfx_ui_click")
    {
        if (uiClickHandle.IsValid()) Addressables.Release(uiClickHandle);

        uiClickHandle = Addressables.LoadAssetAsync<AudioClip>(clickSoundKey);
        await uiClickHandle.Task;
    }

    // 🎯 GLOBAL UI KLİK TETİKLEYİCİSİ
    public void PlayClickSound()
    {
        if (uiAudioSource != null && uiClickHandle.IsValid() && uiClickHandle.Status == AsyncOperationStatus.Succeeded)
        {
            uiAudioSource.PlayOneShot(uiClickHandle.Result);
        }
    }
}