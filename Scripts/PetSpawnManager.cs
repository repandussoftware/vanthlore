using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;

// ============================================================================
// 🌐 NETWORK / DATABASE VERİ TRANSFER MODELLERİ (DTO)
// ============================================================================

[System.Serializable]
public class DBPotionPoint
{
    public float x;
    public float y;
    public float z;
    public int idle_type;
    public Vector3 ToVector3() => new Vector3(x, y, z);
}

[System.Serializable]
public class DBScenePetData
{
    public string addressable_key;   // YENİ: Unity Addressable Adresi (Örn: "Pets/Helen")
    public bool is_static;
    public float movement_speed;
    public float min_wait_time;
    public float max_wait_time;
    public string moving_bool_name;
    public List<DBPotionPoint> spawn_points;
}

// ============================================================================
// 🏭 YENİ NESİL ADRESİLENEBİLİR DOĞURUCU (PET SPAWN MANAGER)
// ============================================================================

public class PetSpawnManager : MonoBehaviour
{
    public static PetSpawnManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// REVRUTU DEVRİMİ: DB'den gelen Addressable key'lerini kullanarak 
    /// prefabları hafızaya asenkron yükler ve sahnede var eder canım!
    /// </summary>
    public void InitializeAllPetsFromNetwork(List<DBScenePetData> networkData)
    {
        if (networkData == null || networkData.Count == 0) return;

        foreach (DBScenePetData dbPet in networkData)
        {
            // Her bir hayvanı asenkron (RAM'i yormadan) yüklemek için alt fonksiyona paslıyoruz
            LoadAndSpawnPetAddressable(dbPet);
        }
    }

    private void LoadAndSpawnPetAddressable(DBScenePetData petData)
    {
        if (string.IsNullOrEmpty(petData.addressable_key))
        {
            Debug.LogWarning("<color=red>Aritheon:</color> Gelen veride Addressable anahtarı boş!");
            return;
        }

        // 🎯 CRITICAL HAMLE: Addressables ile prefabı arka planda yüklemeye başlıyoruz
        Addressables.InstantiateAsync(petData.addressable_key, Vector3.zero, Quaternion.identity).Completed += (handle) =>
        {
            // Yükleme operasyonu başarıyla tamamlandı mı kontrolü
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject spawnedPet = handle.Result;
                spawnedPet.name = $"{petData.addressable_key.Replace("/", "_")}_DB_{spawnedPet.GetInstanceID()}";

                // Koordinatları ve modları ayıkla
                List<Vector3> positions = new List<Vector3>();
                List<int> idles = new List<int>();

                foreach (DBPotionPoint point in petData.spawn_points)
                {
                    positions.Add(point.ToVector3());
                    idles.Add(point.idle_type);
                }

                // Hayvanın üzerindeki evrensel zekayı bul ve verileri teslim et canım
                UniversalPetAI petAI = spawnedPet.GetComponent<UniversalPetAI>();
                if (petAI != null)
                {
                    petAI.InitializePetFromDatabase(
                        positions,
                        idles,
                        petData.is_static,
                        petData.movement_speed,
                        petData.min_wait_time,
                        petData.max_wait_time,
                        petData.moving_bool_name
                    );
                }
                else
                {
                    Debug.LogError($"<color=red>Aritheon:</color> {spawnedPet.name} prefabında UniversalPetAI scripti eksik!");
                    Addressables.ReleaseInstance(spawnedPet); // Hatalı yüklenen objeyi güvenle RAM'den temizle
                }
            }
            else
            {
                Debug.LogError($"<color=red>Aritheon Addressables Hatası:</color> '{petData.addressable_key}' anahtarına ait prefab yüklenemedi! Adres doğruluğunu Unity içinden kontrol et canım.");
            }
        };
    }
}