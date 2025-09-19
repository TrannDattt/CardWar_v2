using UnityEngine.Events;

namespace CardWar.Interfaces
{
    public interface IDamagable
    {
        public UnityEvent OnTakenDamage { get; set; }
        public void TakeDamage(int amount);
    }
}