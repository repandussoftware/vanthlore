using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Darion/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("Temel Bilgiler")]
    public string skillID;              // Benzersiz ID (Senin skillID değişkenin asdas)
    public string skillName;            // İngilizce Ad (NameENG yerine bunu kullanıyoruz)
    public string skillNameTR;          // Türkçe Ad (NameTR)
    public SkillType skillType;         // Aktif mi, Pasif mi? (IsActive yerine geçer)
    public SlotType slotType;           // Hangi slot tipine takılabilir? (SlotType)
    public SkillElement element;        // Element gereksinimi (ElementReq)

    [Header("Görsel ve Ses Efektleri (Referanslar)")]
    public Sprite skillIcon;            
    public GameObject vfxPrefab;
    public String vfxFunctionName; // VFX'i tetikleyecek fonksiyonun adı (örneğin "PlayVFXWorld" veya "PlayVFXAttached")
    public GameObject vfxOnEnemyPrefab; // Düşmana isabet ettiğinde çıkan efekt        
    public AudioClip castSound;         
    public RuntimeAnimatorController skillOverrideController;

    [Header("Adresleme (Addressables/AssetBundle Yolları)")]
    public string VFX_Address;          // VFX_Address asdas
    public string Icon_Address;         // Icon_Address
    public string SFX_Address;          // SFX_Address

    [Header("Maliyet ve Ekonomi")]
    public float manaCost;              
    public int Cost_Gold;               // Altın maliyeti
    public int Cost_Diamond;            // Elmas maliyeti
    public float cooldown;              
    public float castTime;              

    [Header("Güç, Etki ve Ölçeklendirme")]
    public int damage;
    public int fireDamage;
    public int iceDamage;                  
    public int defence;
    public int fireDefence;
    public int iceDefence;                 
    public float range;                 
    public float effectDuration;        
    public float knockbackForce;        
    public float Reinforcement_Bonus;   // Takviye/Güçlendirme bonusu asdas
    public string ScalingID;            // Veritabanı için Scaling ID
    public ScaleType scaleType;         // Ölçeklendirme mantığı (Linear, Exp vb.)
    public float ScaleFactor;           // Ölçeklendirme çarpanı
    public Vector3 BaseOffset;          // Efektin çıkış ofseti (BaseOffset)

    [Header("Açıklama")]
    [TextArea(3, 10)]
    public string skillDescription;     // Description_ENG
    public string skillDescriptionTR;   // Description_TR

    [Header("Gereksinimler ve Hiyerarşi")]
    public int requiredLevel;           
    public SkillData requiredPreSkill;  // Önceki tekil yetenek
    public List<string> LevelParentIDs; // Birden fazla üst yetenek gereksinimi asdas

    [Header("Animasyon ve Durum")]
    public string animationTriggerName;
    public bool IsActive = true;        // Yeteneğin aktif/kilitli durumu
}

// --- Enum Tanımlamaları cam gibi! ---
public enum SkillType { Active, Passive, Buff }
public enum SkillElement { Physical, Fire, Ice }
public enum SlotType { Main, Secondary, Utility, Passive }
public enum ScaleType { Constant, Linear, Exponential }