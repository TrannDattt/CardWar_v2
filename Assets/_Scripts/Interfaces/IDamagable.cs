using UnityEngine.Events;

namespace CardWar.Interfaces
{
    public interface IDamagable
    {
        public UnityEvent OnTakenDamaged { get; set; }
        public void TakeDamage(int amount);
    }
}