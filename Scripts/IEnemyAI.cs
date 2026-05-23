public interface IEnemyAI
{
    void Attack();
    void TakeDamage();
    void Die();
    void Move(bool canMove); // Yürümeyi durdurup başlatmak için
}