using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapMovementController : MonoBehaviour
{
    [Header("Başlangıç Ayarları")]
    public MapNode currentNode; 
    public float moveSpeed = 1200f; 
    public float joystickThreshold = 0.5f; 
    
    private bool isMoving = false;
    private List<MapNode> allNodes = new List<MapNode>(); 

    void Awake()
    {
        // KRİTİK NOKTA: (true) ekleyerek kapalı olan düğümleri de listeye alıyoruz.
        allNodes.Clear();
        allNodes.AddRange(GetComponentsInChildren<MapNode>(true));
        Debug.Log($"<color=green>Toplam {allNodes.Count} düğüm hafızaya alındı.</color>");
    }

    void Start()
    {
        if (currentNode != null)
        {
            transform.position = currentNode.transform.position;
        }
    }

    void Update()
    {
        // Harita kapalıyken Update çalışmaz, bu normaldir. 
        // Hareket sadece harita açıkken (Update çalışırken) joystick ile yapılır.
        if (isMoving || DarionController.Instance == null || currentNode == null) return;

        float horizontalInput = DarionController.Instance.stickInput.x;

        if (horizontalInput > joystickThreshold && currentNode.rightNode != null)
            StartCoroutine(MoveToNode(currentNode.rightNode));
        else if (horizontalInput < -joystickThreshold && currentNode.leftNode != null)
            StartCoroutine(MoveToNode(currentNode.leftNode));
    }

    // --- DÜNYADAKİ TETİKLEYİCİDEN ÇAĞRILACAK ---
    public void SyncNodeFromWorld(string nodeName)
    {
        // Eğer liste boşsa (Awake henüz çalışmadıysa veya bir hata olduysa) tekrar doldur
        if (allNodes.Count == 0) allNodes.AddRange(GetComponentsInChildren<MapNode>(true));

        MapNode target = allNodes.Find(n => n.name == nodeName);

        if (target != null)
        {
            currentNode = target;
            // Obje kapalı olsa bile transform verisi güncellenebilir.
            transform.position = target.transform.position;
            Debug.Log($"<color=cyan>Dünya ile Senkronize:</color> {nodeName}");
        }
    }

    IEnumerator MoveToNode(MapNode targetNode)
    {
        isMoving = true;
        while (Vector3.Distance(transform.position, targetNode.transform.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetNode.transform.position, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetNode.transform.position;
        currentNode = targetNode;
        yield return new WaitUntil(() => Mathf.Abs(DarionController.Instance.stickInput.x) < 0.2f);
        isMoving = false;
    }
}