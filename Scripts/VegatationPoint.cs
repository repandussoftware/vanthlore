using UnityEngine;

[System.Serializable] // 🎯 Müfettişte (Inspector) görünebilmesi için şart!
public struct VegetationPoint
{
    public string prefabID;         // Hangi ağaç veya harabe? (Örn: "Tree_Oak_01")
    public Vector2 position;        // Sahnedeki dünya konumu
    public float scale;             // Çeşitlilik için ölçek (Örn: 0.8f ile 1.2f arası)

    public string tag;
    
    [Header("Sorting Ayarları")]
    public string sortingLayerName; // "Environment", "Foreground" vb.
    public int sortingOrderOffset;  // Manuel ince ayar gerekirse (Default: 0)
}