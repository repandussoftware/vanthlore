using UnityEngine;
using System.Collections;

public class BurningSkyManager : MonoBehaviour
{
    public static BurningSkyManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    [Header("Meteor Ayarları")]
    public GameObject meteorPrefab;
    public float spawnRate = 0.5f; 
    public float skillDuration = 5f; 

    [Header("Sınırlar ve Randomizasyon")]
    public float spawnWidth = 10f; 
    public float spawnHeight = 6f; 
    
    public float minSpeed = 8f, maxSpeed = 15f;
    public float minScale = 0.5f, maxScale = 1.5f;

    // 🎯 YENİ: Dışarıdan hasar paketi alan tetikleyici
    public void StartMeteorRain(float[] damagePackage)
    {
        // Gelen hasarı Coroutine'e paslıyoruz asdas
        StartCoroutine(RainCoroutine(damagePackage));
    }

    // Editörden test yapmak istersen (Hasarsız test)
    [ContextMenu("Test Meteor Rain")]
    public void TestRain() => StartMeteorRain(new float[] { 0, 0, 0 });

    private IEnumerator RainCoroutine(float[] damage)
    {
        float timer = 0;
        while (timer < skillDuration)
        {
            // Hasarı SpawnMeteor'a gönderiyoruz
            SpawnMeteor(damage);
            
            float waitTime = Random.Range(spawnRate * 0.7f, spawnRate * 1.3f); 
            yield return new WaitForSeconds(waitTime);
            timer += waitTime;
        }
    }

    private void SpawnMeteor(float[] damage)
    {
        float randomX = Random.Range(transform.position.x - spawnWidth / 2, transform.position.x + spawnWidth / 2);
        Vector3 spawnPos = new Vector3(randomX, transform.position.y + spawnHeight, 0);

        GameObject meteor = Instantiate(meteorPrefab, spawnPos, Quaternion.identity);

        // 🎯 1. HASAR ENJEKSİYONU: Meteor doğduğu an hasarını mühürlüyoruz asdas
        DamageDealer dealer = meteor.GetComponent<DamageDealer>();
        if (dealer != null)
        {
            dealer.SetDamage(damage);
            // Debug.Log($"<color=orange>Meteor:</color> {damage[1]} ateş hasarı yüklendi!");
        }

        // 🎯 2. HAREKET ENTEGRASYONU
        MeteorMove moveScript = meteor.GetComponent<MeteorMove>();
        if (moveScript != null)
        {
            float randomSpeed = Random.Range(minSpeed, maxSpeed);
            float randomScale = Random.Range(minScale, maxScale);
            moveScript.Setup(randomSpeed, randomScale); 
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = transform.position + Vector3.up * spawnHeight;
        Gizmos.DrawWireCube(center, new Vector3(spawnWidth, 0.2f, 1));
    }
}