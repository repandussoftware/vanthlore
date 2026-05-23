using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using TMPro;
using System.Collections;

public class PatchManager : MonoBehaviour
{
    [Header("--- UI NESNELERİ ---")]
    [SerializeField] private Slider patchProgressBar; // Gotik yüklenme barın canım
    [SerializeField] private TextMeshProUGUI patchStatusText; // "İndiriliyor..." yazan metin

    [Header("--- SAHNE YÖNLENDİRMESİ ---")]
    [SerializeField] private string mainMenuSceneName = "InitialMenu"; // Yükleme bitince açılacak şanlı sahne

    private void Start()
    {
        // Oyun açılır açılmaz bulut boru hattını ateşliyoruz canım!
        StartCoroutine(StartPatchSequenceRoutine());
    }

    private IEnumerator StartPatchSequenceRoutine()
    {
        patchProgressBar.value = 0f;
        patchStatusText.text = "Connecting to Frankfurt Gates... (Sunucuya Bağlanılıyor...)";

        // 🎯 1. ADIM: Önce Addressables sistemini başlatıyoruz canım
        AsyncOperationHandle initHandle = Addressables.InitializeAsync();
        yield return initHandle;

        // 🎯 2. ADIM: Sunucudaki o şanlı güncel kataloğu denetliyoruz
        patchStatusText.text = "Checking for updates... (Güncellemeler Denetleniyor...)";
        AsyncOperationHandle<System.Collections.Generic.List<string>> checkHandle = Addressables.CheckForCatalogUpdates(false);
        yield return checkHandle;

    // Eğer yeni katalog varsa onu indirip güncelliyoruz canım
        if (checkHandle.Status == AsyncOperationStatus.Succeeded && checkHandle.Result != null && checkHandle.Result.Count > 0)
        {
            patchStatusText.text = "Updating world catalog... (Katalog Güncelleniyor...)";
            
            // 🎯 Senior Dokunuşu: updateHandle tipini 'var' yaparak derleyicinin tür eşleme sinsi hatasını tamamen kırıyoruz!
            // İkinci parametreye 'true' vererek işi biten kataloğu bellekten otomatik uçurmasını söylüyoruz canım.
            var updateHandle = Addressables.UpdateCatalogs(checkHandle.Result, true);
            yield return updateHandle;
        }
        // 🎯 3. ADIM: "VanthLore_Remote_Group" için indirilecek veri boyutunu hesaplıyoruz canım
        // Buradaki anahtar kelime senin o Addressables penceresindeki grup adın veya etiketindir
        string targetKey = "Default Local Group"; // Grubun adını ne yaptıysan o canım (Örn: VanthLore_Remote_Group)
        var sizeHandle = Addressables.GetDownloadSizeAsync(targetKey);
        yield return sizeHandle;

        long downloadSize = sizeHandle.Result;

        // Eğer indirilecek boyut 0'dan büyükse, demek ki oyuncunun telefonunda bu assetler yok veya güncel değil!
        if (downloadSize > 0)
        {
            // Bayt cinsinden gelen boyutu MB'a çevirip ekrana basıyoruz, tam bir Senior şovu!
            double sizeInMB = (double)downloadSize / (1024 * 1024);
            patchStatusText.text = $"New assets found! Size: {sizeInMB:F2} MB (Yeni assetler indiriliyor...)";

            // 🎯 4. ADIM: İndirme işlemini canlı bar eşliğinde başlatıyoruz canım!
            var downloadHandle = Addressables.DownloadDependenciesAsync(targetKey, true);

            while (!downloadHandle.IsDone)
            {
                // downloadHandle.PercentComplete bize 0 ile 1 arasında canlı ilerlemeyi verir canım
                patchProgressBar.value = downloadHandle.PercentComplete;
                
                // İlerlemeyi yüzde olarak da metne yazalım canım benim
                patchStatusText.text = $"Downloading assets... %{(downloadHandle.PercentComplete * 100f):F0} ({sizeInMB:F2} MB)";
                yield return null;
            }

            // İndirme bittiğinde belleği temizle canım
            Addressables.Release(downloadHandle);
        }
        else
        {
            patchStatusText.text = "World assets are up to date! Entering Nehalengrad... (VanthLore Güncel!)";
            patchProgressBar.value = 1f;
            yield return new WaitForSeconds(1f); // O şanlı yazıyı oyuncu 1 saniyecik görsün canım
        }

        // Bellekleri serbest bırakıp o canım giriş ekranı sahnemize zıplıyoruz!
        Addressables.Release(sizeHandle);
        Addressables.Release(checkHandle);
        
        SceneManager.LoadScene(mainMenuSceneName);
    }
}