using UnityEngine.Events;

namespace CardWar.Interfaces
{
    public interface IDamagable
    {
        public int Hp { get; }
        public UnityEvent OnTakenDamage { get; set; }
        public UnityEvent OnDeath { get; set; }
        public void TakeDamage(int amount);
        public void Die();
    }
}