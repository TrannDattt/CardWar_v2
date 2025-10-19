using System.Threading.Tasks;
using CardWar_v2.Enums;
using UnityEngine.Events;

namespace CardWar_v2.Entities
{
    public abstract class SkillEffect
    {
        protected CharacterCard _caster;
        protected CharacterCard _target;

        public ESkillEffect EffectType { get; protected set; }
        public int Duration { get; private set; }

        public UnityEvent OnEffectUpdated;

        public SkillEffect(CharacterCard caster, CharacterCard target, int duration)
        {
            _caster = caster;
            _target = target;
            Duration = duration;

            OnEffectUpdated = new();
        }

        public virtual void ApplyEffect()
        {
        }

        public virtual void OverrideEffect(SkillEffect newEffect)
        {
            Duration = newEffect.Duration;
            OnEffectUpdated?.Invoke();
        }

        public virtual async Task DoEffect()
        {
            Duration--;
            OnEffectUpdated?.Invoke();
        }
        
        public virtual void RemoveEffect()
        {
            // TODO: Use factory to remove
        }
    }

    public class RegenEffect : SkillEffect
    {
        private float _amount;

        public RegenEffect(CharacterCard caster, CharacterCard target, int duration, float amount) : base(caster, target, duration)
        {
            _amount = amount;
            EffectType = ESkillEffect.Regen;
        }

        public override void OverrideEffect(SkillEffect newEffect)
        {
            var regenEffect = (RegenEffect)newEffect;
            _amount = regenEffect._amount;

            base.OverrideEffect(newEffect);
        }

        public override async Task DoEffect()
        {
            _target.ChangeHp(_amount);

            await base.DoEffect();
        }
    }

    public class PoisonEffect : SkillEffect
    {
        private float _amount;

        public PoisonEffect(CharacterCard caster, CharacterCard target, int duration, float amount) : base(caster, target, duration)
        {
            _amount = amount;
            EffectType = ESkillEffect.Poison;
        }

        public override void OverrideEffect(SkillEffect newEffect)
        {
            var poisonEffect = (PoisonEffect)newEffect;
            _amount = poisonEffect._amount;

            base.OverrideEffect(newEffect);
        }

        public override async Task DoEffect()
        {
            _target.ChangeHp(-_amount);

            await base.DoEffect();
        }
    }
}

