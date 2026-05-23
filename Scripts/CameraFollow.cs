using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Collider2D boundary;
    public float smoothSpeed = 0.125f;

    // --- YENİ: EL İLE AYARLANABİLİR PAY ---
    [Header("Takip Ayarları")]
    public Vector3 offset; // Buradan yukarı-aşağı ve sağ-sol payı verebilirsin

    private Camera cam;
    private float minX, maxX, minY, maxY;

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        cam = GetComponent<Camera>();
        FindPlayer();
        UpdateCameraBounds();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindPlayer();
        UpdateCameraBounds();
    }

    void FindPlayer()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }

    public void UpdateCameraBounds()
    {
        

        GameObject boundsObj = GameObject.Find("CameraBoundary");
        if (boundsObj != null)
        {
            boundary = boundsObj.GetComponent<Collider2D>();
        }

        if (boundary == null || cam == null) return;

        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;
        Bounds b = boundary.bounds;

        minX = b.min.x + camWidth;
        maxX = b.max.x - camWidth;
        minY = b.min.y + camHeight;
        maxY = b.max.y - camHeight;
    

    }

    void LateUpdate()
    {
        if (target == null) 
        {
            FindPlayer();
            return;
        }

        // --- GÜNCELLEME: OFFSET DEĞERİNİ EKLEDİK ---
        // Hedef pozisyona senin verdiğin kayma payını ekliyoruz
        Vector3 desiredPosition = target.position + offset;
        desiredPosition.z = transform.position.z; // Kameranın derinliğini koruyoruz

        if (boundary != null)
        {
            float clampedX = Mathf.Clamp(desiredPosition.x, minX, maxX);
            float clampedY = Mathf.Clamp(desiredPosition.y, minY, maxY);
            desiredPosition = new Vector3(clampedX, clampedY, transform.position.z);
        }

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}