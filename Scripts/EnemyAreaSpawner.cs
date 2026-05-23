using UnityEngine;
using System.Collections.Generic;

public class EnemyAreaSpawner : MonoBehaviour
{
    [Header("Doğum Ayarları")]
    public GameObject enemyPrefab; 
    public int maxEnemies = 3; 
    public float spawnCheckInterval = 5f;

    private BoxCollider2D spawnArea;
    private List<GameObject> activeEnemies = new List<GameObject>();

    void Awake()
    {
        spawnArea = GetComponent<BoxCollider2D>();
    }

    void Start()
    {
        // İlk başta bölgeyi doldur
        for (int i = 0; i < maxEnemies; i++) 
        { 
            SpawnEnemy(); 
        }

        InvokeRepeating("CheckAndSpawn", spawnCheckInterval, spawnCheckInterval);
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null) return;

        Vector2 spawnPoint = GetRandomPointInBounds();
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint, Quaternion.identity);
        
        // --- KRİTİK GÜNCELLEME: DoomedWolfAI yerine BaseEnemyAI kullanıyoruz ---
        var ai = enemy.GetComponent<BaseEnemyAI>();
        if (ai != null) 
        {
            ai.patrolArea = spawnArea; // Devriye alanını spawner'dan atıyoruz
        }
        
        activeEnemies.Add(enemy);
    }

    public Vector2 GetRandomPointInBounds()
    {
        Bounds bounds = spawnArea.bounds;
        return new Vector2(
            Random.Range(bounds.min.x, bounds.max.x),
            transform.position.y // Mevcut spawner yüksekliğini baz alır
        );
    }

    void CheckAndSpawn()
    {
        activeEnemies.RemoveAll(item => item == null);

        if (activeEnemies.Count < maxEnemies)
        {
            int diff = maxEnemies - activeEnemies.Count;
            for (int i = 0; i < diff; i++)
            {
                SpawnEnemy();
            }
            Debug.Log($"<color=orange>Aritheon Spawner:</color> {diff} yeni varlık bölgeye salındı.");
        }
    }
}