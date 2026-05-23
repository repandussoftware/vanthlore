using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    // Prefab ismine göre pasif objeleri tutan havuz
    private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();

    private Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 🎯 Artık her sahnede seninle gelir!
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GameObject GetFromPool(string prefabID, Vector3 position, string tag)
    {
        if (!poolDictionary.ContainsKey(prefabID))
            poolDictionary.Add(prefabID, new Queue<GameObject>());

        GameObject obj;

        if (poolDictionary[prefabID].Count > 0)
        {
            obj = poolDictionary[prefabID].Dequeue();
            obj.transform.position = position;
            obj.SetActive(true);
        }
        else
        {
            // 🎯 ZIRHLI GÜNCELLEME: Önce sözlüğe (Cache) bakıyoruz
            GameObject prefab;
            if (!prefabCache.TryGetValue(prefabID, out prefab))
            {
                prefab = Resources.Load<GameObject>($"Prefabs/Vegetation/{prefabID}");
                if (prefab == null) { Debug.LogError($"{prefabID} bulunamadı!"); return null; }
                prefabCache.Add(prefabID, prefab); // Hafızaya mühürle, bir daha diskle uğraşma!
            }

            obj = Instantiate(prefab, position, Quaternion.identity);
            obj.name = prefabID;
        }

        obj.tag = tag;
        UpdateObjectTexture(obj);
        return obj;
    }

    public void ReturnToPool(GameObject obj)
    {
        string key = obj.name;
        obj.SetActive(false);
        poolDictionary[key].Enqueue(obj);
    }

    private void UpdateObjectTexture(GameObject obj)
    {
        // EnvironmentTextureManager zaten Addressables'ları RAM'e yükledi
        // Biz sadece objeyi o anki atlastan besleyeceğiz
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null && EnvironmentTextureManager.Instance != null)
        {
            // Manager'a "Bu tag'e uygun sprite'ı ver" diyeceğiz (Bir sonraki adımda ekleyeceğiz)
            EnvironmentTextureManager.Instance.RefreshDynamicObject(obj);
        }
    }

    public void PreWarm(string prefabID, int count)
    {
        if (!poolDictionary.ContainsKey(prefabID))
            poolDictionary.Add(prefabID, new Queue<GameObject>());

        for (int i = 0; i < count; i++)
        {
            // Prefab'ı yükle (Cache mekanizmasını kullanır)
            GameObject obj = GetFromPool(prefabID, Vector3.zero, "Untagged");
            ReturnToPool(obj); // Hemen havuza geri gönder
        }
        Debug.Log($"<color=orange>Aritheon:</color> {prefabID} havuzu {count} adet için önceden ısıtıldı.");
    }
}