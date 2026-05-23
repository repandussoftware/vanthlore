using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;

public class VcamFollowTarget : MonoBehaviour
{
    [Header("Kamera Ayarları")]
    public float targetZoom = 7f; // Kameranın uzaklığı
    
    private CinemachineCamera vcam;

    void Awake()
    {
        vcam = GetComponent<CinemachineCamera>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        // İlk açılışta anında kilitlenmesi için true gönderiyoruz
        RefreshTarget(true); 
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Sahne değiştiğinde de anında kilitlenmesi için true
        RefreshTarget(true); 
    }

    public void RefreshTarget(bool snapImmediately = false)
    {
        GameObject player = GameObject.FindWithTag("Player");

        if (player != null && vcam != null)
        {
            vcam.Follow = player.transform;
            vcam.Lens.OrthographicSize = targetZoom;

            if (snapImmediately)
            {
                // 1. Kameranın kendi pozisyonunu karakterin olduğu yere çekiyoruz (Z'yi koruyarak)
                Vector3 targetPos = player.transform.position;
                targetPos.z = transform.position.z; 
                transform.position = targetPos;

                // 2. Cinemachine'e "Yumuşak geçiş yapma, direkt buraya kilitlen" diyoruz
                vcam.ForceCameraPosition(transform.position, Quaternion.identity);
            }
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}