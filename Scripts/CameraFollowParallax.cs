using UnityEngine;
using UnityEngine.SceneManagement;

// Bu script artık sadece Parallax ve Görsel geçişleri yönetir.
public class CameraFollowParallax : MonoBehaviour
{
    [Header("Parallax Grupları")]
    public ParallaxLayer[] dayLayers;
    public ParallaxLayer[] nightLayers;

    [Header("Zaman Geçiş Ayarları")]
    public float transitionSpeed = 1f; 
    [Range(0, 1)] public float nightWeight = 0; 

    private Vector3 lastCameraPosition;
    private UIManager uiManager;

    [System.Serializable]
    public class ParallaxLayer
    {
        public SpriteRenderer spriteRenderer; 
        public float parallaxFactor; 
        [HideInInspector] public Transform transform;
        [HideInInspector] public GameObject gameObject; 
    }

    void Start()
    {
        lastCameraPosition = transform.position;
        uiManager = FindFirstObjectByType<UIManager>();
        
        InitializeLayerGroup(dayLayers);
        InitializeLayerGroup(nightLayers);
    }

    void InitializeLayerGroup(ParallaxLayer[] layers)
    {
        foreach (var layer in layers)
        {
            if (layer.spriteRenderer != null) 
            {
                layer.transform = layer.spriteRenderer.transform;
                layer.gameObject = layer.spriteRenderer.gameObject;
            }
        }
    }

    // Cinemachine kamerayı hareket ettirdikten sonra çalışması için LateUpdate kalmalı
    void LateUpdate()
    {
        // Gün/Gece Geçiş Mantığı (Aynen korundu)
        if (uiManager != null && StatsManager.Instance != null)
        {
            float targetWeight = StatsManager.Instance.isDayTime ? 0f : 1f;
            nightWeight = Mathf.MoveTowards(nightWeight, targetWeight, transitionSpeed * Time.deltaTime);
        }

        // Parallax Hareketi
        Vector3 deltaMovement = transform.position - lastCameraPosition;
        
        if (nightWeight < 1f) MoveLayers(dayLayers, deltaMovement);
        if (nightWeight > 0f) MoveLayers(nightLayers, deltaMovement);

        lastCameraPosition = transform.position;

        UpdateVisualTransition();
    }

    void MoveLayers(ParallaxLayer[] layers, Vector3 delta)
    {
        foreach (var layer in layers)
        {
            if (layer.transform != null)
                // Cinemachine kamerayı hareket ettirdikçe katmanlar faktöre göre kayar
                layer.transform.position += new Vector3(delta.x * layer.parallaxFactor, delta.y * layer.parallaxFactor, 0);
        }
    }

    void UpdateVisualTransition()
    {
        UpdateGroupStates(dayLayers, 1f - nightWeight);
        UpdateGroupStates(nightLayers, nightWeight);
    }

    void UpdateGroupStates(ParallaxLayer[] layers, float alpha)
    {
        foreach (var layer in layers)
        {
            if (layer.spriteRenderer != null)
            {
                bool shouldBeActive = alpha > 0.001f;
                if (layer.gameObject.activeSelf != shouldBeActive)
                    layer.gameObject.SetActive(shouldBeActive);

                if (shouldBeActive)
                {
                    Color c = layer.spriteRenderer.color;
                    c.a = alpha;
                    layer.spriteRenderer.color = c;
                }
            }
        }
    }
}