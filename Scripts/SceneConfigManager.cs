using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;
using System.Threading.Tasks; // Task kullanımı için
using System;

public class SceneConfigManager : MonoBehaviour
{
    public static SceneConfigManager Instance { get; private set; }

    [Header("Scene Configurations")]
    [SerializeField] private List<SceneSettings> allSettings;
    [SerializeField] private SceneSettings defaultSettings;

    // Birden fazla handle'ı takip etmek için liste (Parallax, Floor, Houses)
    private List<AsyncOperationHandle<Sprite>> activeHandles = new List<AsyncOperationHandle<Sprite>>();
    private bool isAssetLoading = false;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name.Equals("InitialMenu", StringComparison.OrdinalIgnoreCase)) return;
        StartCoroutine(ApplySettingsWithDelay(scene.name));
    }

    private IEnumerator ApplySettingsWithDelay(string sceneName)
    {
        yield return new WaitForEndOfFrame();
        ApplySceneSpecificSettings(sceneName);
    }

    public void ApplySceneSpecificSettings(string sceneName)
    {
        var settings = allSettings.FirstOrDefault(s => s.sceneName.Equals(sceneName, StringComparison.OrdinalIgnoreCase)) ?? defaultSettings;

        if (DarionController.Instance != null)
        {
            UpdateStatsAndPhysics(DarionController.Instance.gameObject, settings);
            //DarionController.Instance.UpdateUIBars();
        }

        // 🎯 TÜM VARLIKLARI SIRAYLA YÜKLE
        _ = LoadAllSceneAssetsSequential(settings);
    }

    private async Task LoadAllSceneAssetsSequential(SceneSettings s)
    {
        if (isAssetLoading) return;
        isAssetLoading = true;

        try
        {
            // 1. ADIM: ESKİLERİ TAHLİYE ET (RAM Temizliği)
            if (activeHandles.Count > 0)
            {
                Debug.Log("<color=orange>Aritheon [RAM]:</color> Eski sahne varlıkları tahliye ediliyor...");
                foreach (var handle in activeHandles)
                {
                    if (handle.IsValid()) Addressables.Release(handle);
                }
                activeHandles.Clear();
                await Task.Delay(100); // Bellek nefesi
            }

            // 2. ADIM: ANA GÖRSEL (Varsa - İç mekanlar için)
            await LoadAndApply(s.environmentTextureReference, "EnvironmentBG");

            // 3. ADIM: NEHALENGRAD ZEMİN (Floor)
            await LoadAndApply(s.floorTextureReference, "FloorBG");

            // 4. ADIM: PARALLAX KATMANLARI (Sırayla)
            for (int i = 0; i < s.parallaxLayers.Count; i++)
            {
                // Parallax_0, Parallax_1... isimli objeleri arar
                await LoadAndApply(s.parallaxLayers[i], $"Parallax_{i}");
            }

            // 5. ADIM: NPC EVLERİ / YAKIN ÇEVRE (Sırayla)
            for (int i = 0; i < s.npcHouseAssets.Count; i++)
            {
                // NpcHouse_0, NpcHouse_1... isimli objeleri arar
                await LoadAndApply(s.npcHouseAssets[i], $"env_{i}");
            }
        }
        finally
        {
            isAssetLoading = false;
            Debug.Log("<color=green>Aritheon:</color> Sahne varlıkları 'Sequential' olarak mühürlendi.");
        }
    }

    // Yardımcı Metod: Tek tek yükler ve sahnede ilgili objeye giydirir
    private async Task LoadAndApply(AssetReference reference, string objectSelector)
    {
        if (reference == null || !reference.RuntimeKeyIsValid()) return;

        var handle = reference.LoadAssetAsync<Sprite>();
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            activeHandles.Add(handle);

            // 🎯 GÜVENLİ ARAMA: Önce objeyi ismiyle (Name) bulmayı deneriz.
            // GameObject.Find hata fırlatmaz, bulamazsa sadece null döner.
            GameObject targetObj = GameObject.Find(objectSelector);

            // Eğer isimle bulamazsak, bir ihtimal Tag olarak atanmıştır diye bakarız
            if (targetObj == null)
            {
                try
                {
                    targetObj = GameObject.FindGameObjectWithTag(objectSelector);
                }
                catch
                {
                    // Tag tanımlı değilse hata fırlatmasını bu şekilde engelliyoruz.
                }
            }

            if (targetObj != null && targetObj.TryGetComponent<SpriteRenderer>(out var sr))
            {
                sr.sprite = handle.Result;
                Debug.Log($"<color=cyan>Aritheon:</color> {objectSelector} başarıyla giydirildi.");
            }
            else
            {
                Debug.LogWarning($"<color=yellow>Aritheon:</color> '{objectSelector}' isimli obje veya SpriteRenderer sahnede bulunamadı!");
            }
        }
    }

    private void UpdateStatsAndPhysics(GameObject player, SceneSettings s)
    {
        if (StatsManager.Instance != null)
        {
            StatsManager.Instance.walkSpeed = s.walkSpeed;
            StatsManager.Instance.runSpeed = s.runSpeed;
            StatsManager.Instance.jumpForce = s.jumpForce;
            StatsManager.Instance.forcedScale = s.forcedScale;
        }

        Vector3 finalPosition = s.spawnPosition;
        if (MenuController.Instance != null && MenuController.Instance.activeSaveData != null)
        {
            var save = MenuController.Instance.activeSaveData;
            finalPosition = new Vector3(save.playerPosition[0], save.playerPosition[1], save.playerPosition[2]);
        }

        player.transform.position = finalPosition;
        player.transform.localScale = s.scale;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = true;
            rb.gravityScale = s.gravityScale;
            rb.linearVelocity = Vector2.zero;
        }
    }
}