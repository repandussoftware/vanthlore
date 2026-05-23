using UnityEngine;

public class VegetationSpawner : MonoBehaviour
{
    [Header("Ayarlar")]
    public GameObject[] bushPrefabs;
    public Transform bushContainer; // Prefab içinde boş bir obje oluşturup buraya sürükle!
    public int minBushes = 2;
    public int maxBushes = 5;

    [Header("Alan Ayarları")]
    public float spawnAreaWidth = 18f; 
    public float yMinOffset = -0.3f;
    public float yMaxOffset = 0.5f;

    public void SpawnBushes()
    {
        // 1. SADECE çalıların olduğu kutuyu temizle (Çok daha güvenli!)
        if (bushContainer != null)
        {
            foreach (Transform child in bushContainer)
            {
                Destroy(child.gameObject);
            }
        }
        else
        {
            Debug.LogWarning("Canım, bushContainer'ı sürüklemeyi unutmuşsun!");
            return;
        }

        // 2. Yeni çalıları oluştur
        int spawnCount = Random.Range(minBushes, maxBushes + 1);

        for (int i = 0; i < spawnCount; i++)
        {
            GameObject selectedPrefab = bushPrefabs[Random.Range(0, bushPrefabs.Length)];
            
            float randomX = Random.Range(-spawnAreaWidth / 2, spawnAreaWidth / 2);
            float randomY = Random.Range(yMinOffset, yMaxOffset);
            Vector3 spawnPos = new Vector3(randomX, randomY, 0);

            // Çalıyı bushContainer'ın içine oluşturuyoruz
            GameObject newBush = Instantiate(selectedPrefab, bushContainer);
            newBush.transform.localPosition = spawnPos;

            // Rastgele Flip
            if (Random.value > 0.5f)
            {
                newBush.transform.localScale = new Vector3(-newBush.transform.localScale.x, newBush.transform.localScale.y, 1);
            }
        }
    }
}