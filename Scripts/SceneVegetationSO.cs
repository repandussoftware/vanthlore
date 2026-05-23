using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewSceneVegetation", menuName = "Aritheon/Vegetation Data")]
public class SceneVegetationSO : ScriptableObject
{
    [Tooltip("Bu sahnedeki tüm vejetasyon ve harabe verileri burada mühürlü.")]
    public List<VegetationPoint> vegetationPoints = new List<VegetationPoint>();
    
    // 🕵️ Cerrahi Taktik: Kaç obje olduğunu hızlıca görmek için
    public int ObjectCount => vegetationPoints.Count;
}