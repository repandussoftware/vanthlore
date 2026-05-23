using UnityEngine;

public class BaseEnemyAnimationBridge : MonoBehaviour
{
    public BaseEnemyAI enemyAI;
    public EnemyUI enemyUI;
    public EnemyStats enemyStats;

    public void PlaySound(AudioClip clip)
    {
        enemyAI.PlaySound(clip);
    }

    public void TriggerDamageEvent()
    {
        enemyAI.TriggerDamageEvent();
    }

    public void TakeDamage()
    {
        enemyAI.TakeDamage();
    }

    public void Die()
    {
        enemyAI.Die();
    }
    
}
