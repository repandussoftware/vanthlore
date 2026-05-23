using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Aritheon/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Görsel ve Kimlik")]
    public string enemyName;
    public RuntimeAnimatorController animatorController;

    [Header("Hareket Ayarları")]
    public float moveSpeed = 2f;
    public float chaseSpeed = 3.5f;
    public float patrolRange = 5f;

    [Header("Savaş Ayarları")]
    public float maxHealth = 100f;
    public float normalAttackPower = 10f;
    public float fireAttackPower = 10f;
    public float iceAttackPower = 10f;
    public float normalDefencePower = 5f;
    public float fireDefencePower = 5f;
    public float iceDefencePower = 5f;
    public float chaseRange = 5f;
    public float attackRange = 1.3f;
    public float attackCooldown = 1.5f;
    public float attackImpactRange = 1.5f;

    [Header("Ses Efektleri")]
    public AudioClip attackClip;
    public AudioClip hitClip;
    public AudioClip deathClip;
}