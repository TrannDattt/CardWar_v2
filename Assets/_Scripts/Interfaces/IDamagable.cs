using CardWar_v2.Enums;
using UnityEngine.Events;
using static CardWar_v2.ComponentViews.FXPlayer;

namespace CardWar.Interfaces
{
    public interface IDamagable
    {
        public float CurHp { get; }
        public UnityEvent OnChangeHp { get; set; }
        public UnityEvent OnDeath { get; set; }
        public void TakeDamage(ICanDoDamage attacker, float amount, EDamageType type);
        public void Die();
    }

    public interface ICanDoDamage
    {
    }
}