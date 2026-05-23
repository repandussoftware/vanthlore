using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class VegetationManager : MonoBehaviour
{
    public SceneVegetationSO sceneData;
    public Transform playerTransform;

    [Header("Grid Ayarları")]
    public float gridSize = 30f; // Kare boyutu (Kamera x2 idealdir)
    public float moveThreshold = 2.0f; // Tarama hassasiyeti

    // Kare koordinatına göre içindeki objeleri tutan veri deposu
    private Dictionary<Vector2Int, List<VegetationPoint>> gridData = new Dictionary<Vector2Int, List<VegetationPoint>>();

    // Şu an sahnede fiziksel olarak var olan objeleri kare bazlı takip ederiz
    private Dictionary<Vector2Int, List<GameObject>> activeGridObjects = new Dictionary<Vector2Int, List<GameObject>>();

    private Vector2 lastCheckPos;
    private Vector2Int lastGridCoord = new Vector2Int(-999, -999);

    private bool isSpawning = false;
    private Queue<Vector2Int> spawnQueue = new Queue<Vector2Int>();

    void Start()
    {
        if (playerTransform == null)
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        // 1. ADIM: Tüm veriyi bir kez ızgaraya mühürle
        InitializeGrid();

        // 2. ADIM: Periyodik kontrolü başlat
        StartCoroutine(CullingRoutine());
    }

    private void InitializeGrid()
    {
        if (sceneData == null) return;
        gridData.Clear();

        foreach (var point in sceneData.vegetationPoints)
        {
            Vector2Int coord = GetGridCoord(point.position);
            if (!gridData.ContainsKey(coord))
                gridData[coord] = new List<VegetationPoint>();

            gridData[coord].Add(point);
        }
        Debug.Log($"<color=cyan>Aritheon:</color> {sceneData.vegetationPoints.Count} obje ızgaraya dizildi.");
    }

    private IEnumerator CullingRoutine()
    {
        while (true)
        {
            // Sadece Darion yeterince hareket ettiyse işlem yap canım
            if (playerTransform != null && Vector2.Distance(playerTransform.position, lastCheckPos) > moveThreshold)
            {
                lastCheckPos = playerTransform.position;
                UpdateChunks();
            }
            yield return new WaitForSeconds(0.15f); // 1 GB RAM için ideal nefes payı
        }
    }

    private void UpdateChunks()
    {
        if (ObjectPooler.Instance == null) return;

        Vector2Int currentGrid = GetGridCoord(playerTransform.position);
        if (currentGrid == lastGridCoord) return;
        lastGridCoord = currentGrid;

        HashSet<Vector2Int> targetGrids = new HashSet<Vector2Int>();
        for (int x = -1; x <= 1; x++)
            for (int y = -1; y <= 1; y++)
                targetGrids.Add(currentGrid + new Vector2Int(x, y));

        // 🎯 YENİ: Kareleri direkt yüklemek yerine sıraya (Queue) alıyoruz
        foreach (var coord in targetGrids)
        {
            if (!activeGridObjects.ContainsKey(coord) && gridData.ContainsKey(coord))
            {
                if (!spawnQueue.Contains(coord)) spawnQueue.Enqueue(coord);
            }
        }

        // Sıradaki işleri yapması için Coroutine'i tetikle
        if (!isSpawning) StartCoroutine(ProcessSpawnQueue());

        // Toplu silme anlık olabilir, o CPU'yu çok yormaz
        List<Vector2Int> toRelease = new List<Vector2Int>();
        foreach (var activeCoord in activeGridObjects.Keys)
        {
            if (!targetGrids.Contains(activeCoord)) toRelease.Add(activeCoord);
        }
        foreach (var coord in toRelease) DespawnGridChunk(coord);
    }

    private IEnumerator ProcessSpawnQueue()
    {
        isSpawning = true;
        while (spawnQueue.Count > 0)
        {
            Vector2Int coord = spawnQueue.Dequeue();
            List<GameObject> spawnedInChunk = new List<GameObject>();

            if (gridData.ContainsKey(coord))
            {
                foreach (var point in gridData[coord])
                {
                    GameObject obj = ObjectPooler.Instance.GetFromPool(point.prefabID, point.position, point.tag);
                    if (obj != null)
                    {
                        ApplyPointSettings(obj, point);
                        spawnedInChunk.Add(obj);
                    }

                    // 🎯 KRİTİK: Her 2-3 objede bir kare (frame) atla. 
                    // 5 biraz fazla olabilir, 2 yaparak MacBook'un nefes almasını sağlarız.
                    yield return null;
                }
            }

            if (!activeGridObjects.ContainsKey(coord))
                activeGridObjects.Add(coord, spawnedInChunk);
        }
        isSpawning = false;
    }
    private void SpawnGridChunk(Vector2Int coord)
    {
        List<GameObject> spawnedInChunk = new List<GameObject>();
        foreach (var point in gridData[coord])
        {
            GameObject obj = ObjectPooler.Instance.GetFromPool(point.prefabID, point.position, point.tag);
            if (obj != null)
            {
                ApplyPointSettings(obj, point);
                spawnedInChunk.Add(obj);
            }
        }
        activeGridObjects.Add(coord, spawnedInChunk);
    }

    private void DespawnGridChunk(Vector2Int coord)
    {
        if (activeGridObjects.TryGetValue(coord, out List<GameObject> objects))
        {
            foreach (var obj in objects)
            {
                ObjectPooler.Instance.ReturnToPool(obj);
            }
            activeGridObjects.Remove(coord);
        }
    }

    private Vector2Int GetGridCoord(Vector2 pos)
    {
        return new Vector2Int(Mathf.FloorToInt(pos.x / gridSize), Mathf.FloorToInt(pos.y / gridSize));
    }

    private void ApplyPointSettings(GameObject obj, VegetationPoint data)
    {
        obj.transform.localScale = Vector3.one * data.scale;
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = data.sortingLayerName;
            sr.sortingOrder = data.sortingOrderOffset; // Senin el emeği değerlerin
        }
    }
}