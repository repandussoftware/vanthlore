using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.U2D;
using System.Text.RegularExpressions;

public class EnvironmentTextureManager : MonoBehaviour
{
    public static EnvironmentTextureManager Instance { get; private set; }

    [Header("Protagonist Assets")]
    public AssetReference darionAtlasReference;
    private AsyncOperationHandle darionHandle;

    [Header("Menu Assets")]
    public AssetReference openingAtlasReference; // 🎯 Inspector'dan OpeningElements atlasını sürükle

    private List<AsyncOperationHandle> activeHandles = new List<AsyncOperationHandle>();
    private bool isInternalLoading = false;

    public bool IsLoading => isInternalLoading;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    public async Task ChangeEnvironmentTexture(SceneSettings settings)
    {
        if (settings == null || isInternalLoading) return;
        isInternalLoading = true;

        try
        {
            // 🎯 CERRAHİ TEMİZLİK: Hayalet sahneleri önlemek için eski sprite'ları sıfırla
            var allRenderers = GameObject.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var sr in allRenderers)
            {
                if (IsTargetMatch(sr.gameObject, "EnvironmentBG") || sr.gameObject.name.Contains("Parallax"))
                {
                    sr.sprite = null;
                }
            }

            // RAM Tahliyesi
            if (activeHandles.Count > 0)
            {
                foreach (var handle in activeHandles)
                    if (handle.IsValid()) Addressables.Release(handle);

                activeHandles.Clear();
                var unloadTask = Resources.UnloadUnusedAssets();
                while (!unloadTask.isDone) await Task.Delay(10);
                await Task.Delay(100);
            }

            // 🚀 SIRALI VE ZIRHLI YÜKLEME SİSTEMİ
            // Her yüklemeden sonra asset'i sahnede ilgili objelere dağıtıyoruz
            await LoadAndDistribute(settings.environmentTextureReference, "EnvironmentBG");
            await LoadAndDistribute(settings.floorTextureReference, "FloorBG");

            for (int i = 0; i < settings.parallaxLayers.Count; i++)
                await LoadAndDistribute(settings.parallaxLayers[i], $"Parallax_{i}");

            for (int i = 0; i < settings.npcHouseAssets.Count; i++)
                await LoadAndDistribute(settings.npcHouseAssets[i], $"env_{i}");

            for (int i = 0; i < settings.ambientAnimationAssets.Count; i++)
                await LoadAndDistribute(settings.ambientAnimationAssets[i], $"Ambient_{i}");
        }
        catch (System.Exception e) { Debug.LogError($"<color=red>Aritheon [Hata]:</color> {e.Message}"); }
        finally
        {
            isInternalLoading = false;
            Debug.Log("<color=green>Aritheon:</color> Tüm sahne varlıkları RAM'e mühürlendi ve dağıtıldı.");
        }
    }

    // 🛡️ AKILLI YÜKLEME VE DAĞITIM (InvalidKey ve Type uyuşmazlığını çözer)
    private async Task LoadAndDistribute(AssetReference reference, string label)
    {
        // 🎯 KRİTİK DÜZELTME: Sadece null kontrolü yetmez, anahtarın geçerli olduğuna bakmalıyız
        if (reference == null || !reference.RuntimeKeyIsValid())
        {
            // Boş bir referans gelirse sessizce dön veya log bas canım
            // Debug.Log($"<color=white>Aritheon:</color> {label} referansı boş, yükleme atlandı.");
            return;
        }

        // Çift yükleme koruması (Aynen kalıyor)
        if (reference.OperationHandle.IsValid() && reference.OperationHandle.Status == AsyncOperationStatus.Succeeded)
        {
            ApplyLoadedAsset(reference.OperationHandle, label);
            return;
        }

        try
        {
            // 95. satırdaki hata buradaydı, artık RuntimeKeyIsValid kontrolü sayesinde korunuyoruz
            var handle = reference.LoadAssetAsync<Object>();
            activeHandles.Add(handle);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                ApplyLoadedAsset(handle, label);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"<color=red>Aritheon [Hata]:</color> {label} yüklenemedi: {e.Message}");
        }
    }

    // Asset yüklendiğinde tipine göre dağıtım yapar
    private void ApplyLoadedAsset(AsyncOperationHandle handle, string label)
    {
        if (handle.Result is SpriteAtlas atlas)
        {
            HandleAtlasDistribution(atlas, label);
            Debug.Log($"<color=gold>Aritheon:</color> {label} (Atlas) mühürlendi.");
        }
        else if (handle.Result is Sprite spr)
        {
            ApplySpriteToTargets(spr, label);
            Debug.Log($"<color=cyan>Aritheon:</color> {label} (Sprite) mühürlendi.");
        }
        else if (handle.Result is Texture2D tex)
        {
            // Eğer texture ise sprite oluşturup basıyoruz
            Sprite generatedSpr = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            ApplySpriteToTargets(generatedSpr, label);
        }
    }

    private void ApplySpriteToTargets(Sprite spr, string selector)
    {
        var allRenderers = GameObject.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var sr in allRenderers)
        {
            if (IsTargetMatch(sr.gameObject, selector)) sr.sprite = spr;
        }
    }

    private void HandleAtlasDistribution(SpriteAtlas atlas, string selector)
    {
        var allRenderers = GameObject.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var sr in allRenderers)
        {
            if (!IsTargetMatch(sr.gameObject, selector)) continue;

            string cleanedName = Regex.Replace(sr.gameObject.name, @"\s\(\d+\)", "").Trim();
            Sprite foundInAtlas = atlas.GetSprite(cleanedName);
            if (foundInAtlas != null) sr.sprite = foundInAtlas;
        }
    }

    private bool IsTargetMatch(GameObject obj, string selector)
    {
        if (obj.name.Equals(selector, System.StringComparison.OrdinalIgnoreCase)) return true;
        if (obj.name.Contains(selector, System.StringComparison.OrdinalIgnoreCase)) return true; // Parallax_0 gibi eşleşmeler için
        try { if (obj.tag.Equals(selector, System.StringComparison.OrdinalIgnoreCase)) return true; } catch { }
        return false;
    }

    // Darion Global Yükleme (Aynen kalıyor)
    public async Task LoadProtagonistGlobal()
    {
        if (darionAtlasReference == null || (darionHandle.IsValid() && darionHandle.Status == AsyncOperationStatus.Succeeded)) return;

        try
        {
            darionHandle = darionAtlasReference.LoadAssetAsync<SpriteAtlas>();
            await darionHandle.Task;
            if (darionHandle.Status == AsyncOperationStatus.Succeeded)
                Debug.Log("<color=gold>Aritheon [BAŞARI]:</color> Darion global RAM'e mühürlendi.");
        }
        catch (System.Exception e) { Debug.LogError($"Darion İstisnası: {e.Message}"); }
    }

    public async Task LoadOpeningElements()
    {
        if (openingAtlasReference == null || !openingAtlasReference.RuntimeKeyIsValid()) return;

        // Eğer zaten yüklüyse tekrar yükleme canım
        if (openingAtlasReference.OperationHandle.IsValid() && openingAtlasReference.OperationHandle.Status == AsyncOperationStatus.Succeeded)
        {
            HandleAtlasDistribution(openingAtlasReference.OperationHandle.Result as SpriteAtlas, "OpeningBG");
            return;
        }

        try
        {
            isInternalLoading = true;
            var handle = openingAtlasReference.LoadAssetAsync<SpriteAtlas>();
            activeHandles.Add(handle);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                // Atlas içindeki spriteları "OpeningBG" tag'li veya isimli objelere dağıt
                HandleAtlasDistribution(handle.Result as SpriteAtlas, "OpeningBG");
                Debug.Log("<color=cyan>Aritheon:</color> Menü arka plan elementleri mühürlendi.");
            }
        }
        catch (System.Exception e) { Debug.LogError($"Menü Atlas Hatası: {e.Message}"); }
        finally { isInternalLoading = false; }
    }

    public void RefreshDynamicObject(GameObject obj)
    {
        // Objeyi tagine göre (Ambient_3 gibi) tarar ve doğru sprite'ı basar
        // activeHandles içindeki yüklü atlasları kullanır
        foreach (var handle in activeHandles)
        {
            if (handle.IsValid() && handle.Result is SpriteAtlas atlas)
            {
                // IsTargetMatch mantığını burada kullanıyoruz
                if (obj.tag.StartsWith("Ambient") || obj.tag.Contains("Environment"))
                {
                    HandleAtlasDistribution(atlas, obj.tag);
                }
            }
        }
    }
}