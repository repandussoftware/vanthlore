using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // Sahne isimlerini kontrol etmek için şart!

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance;

    [System.Serializable]
    public struct SceneOverride
    {
        public string sceneName;      // Sahnenin tam adı (Örn: "Level1_Forest")
        public Vector3 spawnOffset;   // O sahneye özel ofset
        public Vector3 spawnRotation; // O sahneye özel rotasyon
        public Vector3 spawnScale;    // O sahneye özel ölçek
    }

    [System.Serializable]
    public struct VFXData
    {
        public string effectName;
        public GameObject prefab;
        public float duration;

        [Header("Varsayılan Ayarlar (Eğer listede sahne yoksa)")]
        public Vector3 spawnOffset;
        public Vector3 spawnRotation;
        public Vector3 spawnScale;

        [Header("Sahne Bazlı Özel Ayarlar")]
        public List<SceneOverride> sceneOverrides;
    }

    public List<VFXData> vfxList;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // --- YARDIMCI: SAHNE AYARLARINI GETİRİR ---
    private void GetFinalSettings(VFXData data, out Vector3 offset, out Vector3 rotation, out Vector3 scale)
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Önce varsayılanları ata
        offset = data.spawnOffset;
        rotation = data.spawnRotation;
        scale = data.spawnScale;

        // Eğer bu sahne için özel bir ayar varsa onu kullan asdas
        foreach (var over in data.sceneOverrides)
        {
            if (over.sceneName == currentScene)
            {
                offset = over.spawnOffset;
                rotation = over.spawnRotation;
                scale = over.spawnScale;
                break;
            }
        }
    }

    // 1. YÖNTEM: Sabit Dünyada Oluşturma (Örn: Düşman Ölme Efekti)
    public void PlayVFX(string name, Vector3 position, Quaternion rotation)
    {
        VFXData? data = GetVFXData(name);
        if (data.HasValue)
        {
            GetFinalSettings(data.Value, out Vector3 offset, out Vector3 rot, out Vector3 scale);
            GameObject instance = Instantiate(data.Value.prefab, position + offset, rotation);
            instance.transform.localScale = scale;
            Destroy(instance, data.Value.duration);
        }
    }

    // 2. YÖNTEM: Projectile/Mızrak İçin Bağımsız Dünya Oluşturma
    // 2. YÖNTEM: Projectile/Mızrak/Tornado İçin Bağımsız Dünya Oluşturma
    // Geriye 'GameObject' döndürüyoruz ki çağıran yer (Bridge veya Manager) ekstra ayar yapabilsin asdas
    // Parametreye 'float[] damage = null' ekledik
    // 1. DÜNYA ÜZERİNDE OLUŞTURMA (Mermiler, Meteorlar vb.)
    public GameObject PlayVFXWorld(string name, Vector3 origin, bool facingRight, float[] damage = null)
    {
        VFXData? data = GetVFXData(name);
        if (data.HasValue)
        {
            GetFinalSettings(data.Value, out Vector3 offset, out Vector3 rot, out Vector3 scale);

            // Ofset ve Aynalama
            if (!facingRight)
            {
                offset.x *= -1;
                scale.x *= -1;
            }

            // OLUŞTURMA
            Quaternion finalRotation = Quaternion.Euler(rot.x, rot.y, rot.z);
            GameObject instance = Instantiate(data.Value.prefab, origin + offset, finalRotation);
            instance.transform.localScale = scale;

            // --- KRİTİK: HASAR ENJEKSİYONU ---
            // Mermi daha ilk karesini (frame) yaşamadan hasarı içine koyuyoruz asdas
            if (damage != null)
            {
                DamageDealer dealer = instance.GetComponent<DamageDealer>();
                if (dealer != null)
                {
                    dealer.SetDamage(damage);
                }
            }

            // Mermi Hareketi Kontrolü
            ProjectileMove projScript = instance.GetComponent<ProjectileMove>();
            if (projScript != null)
            {
                projScript.Setup(facingRight);
            }

            // Temizlik
            Destroy(instance, data.Value.duration);
            return instance;
        }
        return null;
    }

    // 2. OBJEYE BAĞLI OLUŞTURMA (Kılıç İzleri, Aura, Kalkan vb.)
    // Buraya da 'float[] damage = null' parametresini ekledik cam gibi!
    public GameObject PlayVFXAttached(string name, Transform parent, float[] damage = null)
    {
        VFXData? data = GetVFXData(name);

        if (data.HasValue)
        {
            GetFinalSettings(data.Value, out Vector3 offset, out Vector3 rot, out Vector3 scale);

            // OLUŞTURMA (Parent'a bağlı)
            GameObject instance = Instantiate(data.Value.prefab, parent);

            // Yerel Ayarlar
            instance.transform.localPosition = offset;
            instance.transform.localEulerAngles = rot;
            instance.transform.localScale = scale;

            // --- KRİTİK: HASAR ENJEKSİYONU ---
            // Yakın dövüş efektlerinde de hasar paketi içerde mühürlensin asdas
            if (damage != null)
            {
                DamageDealer dealer = instance.GetComponent<DamageDealer>();
                if (dealer != null)
                {
                    dealer.SetDamage(damage);
                }
            }

            // Temizlik
            Destroy(instance, data.Value.duration);
            return instance;
        }

        return null;
    }
    // 4. YÖNTEM: Dinamik Viewport (Hala kullanmak istersen override desteğiyle)
    public void PlayVFXWorldDynamic(string name, Vector3 origin, bool facingRight)
    {
        VFXData? data = GetVFXData(name);
        if (!data.HasValue) return;

        float screenRatio = GetPlayerScreenHeightRatio();
        float cameraWorldHeight = Camera.main.orthographicSize * 2f;

        // Override kontrolü asdas
        GetFinalSettings(data.Value, out Vector3 offset, out Vector3 rot, out Vector3 scale);

        float finalSpearScale = (cameraWorldHeight * screenRatio) * 0.4f;

        GameObject instance = Instantiate(data.Value.prefab, origin + offset, Quaternion.Euler(0, 0, facingRight ? 0 : 180));
        instance.transform.localScale = new Vector3(finalSpearScale, finalSpearScale, 1f);

        Destroy(instance, data.Value.duration);
    }

    // --- YARDIMCI FONKSİYONLAR ---
    public float GetPlayerScreenHeightRatio()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return 0f;
        SpriteRenderer sr = player.GetComponentInChildren<SpriteRenderer>();
        if (sr == null) return 0f;

        Bounds bounds = sr.bounds;
        float top = Camera.main.WorldToViewportPoint(new Vector3(0, bounds.max.y, 0)).y;
        float bottom = Camera.main.WorldToViewportPoint(new Vector3(0, bounds.min.y, 0)).y;
        return Mathf.Abs(top - bottom);
    }

    private VFXData? GetVFXData(string name)
    {
        foreach (var vfx in vfxList)
        {
            if (vfx.effectName == name) return vfx;
        }
        Debug.LogWarning($"<color=orange>VFXManager:</color> {name} bulunamadı!");
        return null;
    }
}