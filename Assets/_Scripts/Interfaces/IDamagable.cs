using CardWar_v2.Enums;
using UnityEngine.Events;

namespace CardWar.Interfaces
{
    public interface IDamagable
    {
        public float Hp { get; }
        public UnityEvent OnTakenDamage { get; set; }
        public UnityEvent OnDeath { get; set; }
        public void TakeDamage(float amount, EDamageType type);
        public void Die();
    }
}