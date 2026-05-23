using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets; // Addressables sistemini kullanabilmek için gerekli

[CreateAssetMenu(fileName = "Scene_Config_", menuName = "Aritheon/Scene Config")]
public class SceneSettings : ScriptableObject
{
    public string sceneName;

    [Header("Transform Settings")]
    public Vector3 spawnPosition;
    public Vector3 rotation;
    public Vector3 scale;
    public float forcedScale; // Darion'un görsel boyutu[cite: 1, 2]

    [Header("Movement Settings")]
    public float walkSpeed; //
    public float runSpeed; //[cite: 2]
    public float jumpForce; //[cite: 2]

    [Header("Physics Settings")]
    public float gravityScale; //[cite: 1]

    [Header("Lazy Load Assets (Cerrahi Optimizasyon)")]
    // İsmi 'environmentTextureReference' yaparak Manager ile tam uyumlu hale getirdik.
    public AssetReference environmentTextureReference;

    [Header("Nehalengrad & Parallax Assets (Complex)")]
    // Sahneye özel zemin dokusu (Yüksek çözünürlük gerektiren yer)
    public AssetReference floorTextureReference;

    // Arka plan katmanları (Sıralama: En uzak bulutlardan, oyuncuya en yakın binalara doğru)
    public List<AssetReference> parallaxLayers = new List<AssetReference>();

    [Header("Etkileşimli / Yakın Çevre")]
    public List<AssetReference> npcHouseAssets; // 🏠 Oyuncunun yanından geçtiği detaylı evler

    [Header("Ambient Animations (Sprite Sheets / Atlases)")]
    // Nehir, sallanan ağaçlar, fener ışıkları vb. için
    public List<AssetReference> ambientAnimationAssets = new List<AssetReference>();

    [Header("Audio Settings")]
    public AssetReferenceT<AudioClip> sceneMusicReference; // Sadece AudioClip kabul eder


}