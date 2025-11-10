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
        public int Duration { get; protected set; }

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

        public abstract string GetDescription(bool isSelfApply, EPlayerTarget side, string targetList, bool isShowNextLevel);
    }

    #region Regen
    public class RegenEffect : SkillEffect
    {
        private float _mult = .01f;

        public float HealAmount => _mult * _target.CurHp;

        public RegenEffect(CharacterCard caster, CharacterCard target, int duration) : base(caster, target, duration)
        {
            EffectType = ESkillEffect.Regen;
        }

        public override void OverrideEffect(SkillEffect newEffect)
        {
            Duration += newEffect.Duration;

            base.OverrideEffect(newEffect);
        }

        public override async Task DoEffect()
        {
            _target.ChangeHp(HealAmount);

            await base.DoEffect();
        }

        public override string GetDescription(bool isSelfApply, EPlayerTarget side, string targetList, bool isShowNextLevel)
        {
            return $"Regen equals to {_mult * 100}% of your current HP ({HealAmount}) every turn";
        }
    }
    #endregion

    #region  Poison
    public class PoisonEffect : SkillEffect
    {
        private float _mult = .45f;

        // Use in battle
        public float DamageAmount => _mult * _caster.CurAtk;

        public PoisonEffect(CharacterCard caster, CharacterCard target, int duration) : base(caster, target, duration)
        {
            EffectType = ESkillEffect.Poison;
        }

        public override void OverrideEffect(SkillEffect newEffect)
        {
            var poisonEffect = (PoisonEffect)newEffect;
            Duration += newEffect.Duration;
            _caster = poisonEffect._caster;

            base.OverrideEffect(newEffect);
        }

        public override async Task DoEffect()
        {
            _target.ChangeHp(-DamageAmount);

            await base.DoEffect();
        }

        public override string GetDescription(bool isSelfApply, EPlayerTarget side, string targetList, bool isShowNextLevel)
        {
            return $"Take damage equals to {_mult * 100}% of the caster's ATK ({DamageAmount}) every turn. - Caster: {_caster.Name} -";
        }
    }
    #endregion

    #region  Vulnerable
    public class VulnerableEffect : SkillEffect
    {
        private float _mult;
        public float VulAmount => _mult;

        public VulnerableEffect(CharacterCard caster, CharacterCard target, int duration, float mult) : base(caster, target, duration)
        {
            _mult = mult;
            EffectType = ESkillEffect.Vulnerable;
        }

        public override void OverrideEffect(SkillEffect newEffect)
        {
            var vulEffect = (VulnerableEffect)newEffect;
            _mult += vulEffect._mult;
            Duration = vulEffect.Duration;

            base.OverrideEffect(newEffect);
        }

        public override string GetDescription(bool isSelfApply, EPlayerTarget side, string targetList, bool isShowNextLevel)
        {
            return $"Increase incoming damage taken by {VulAmount * 100}%";
        }
    }
    #endregion

    #region Strengthen
    public class StrengthenEffect : SkillEffect
    {
        private float _mult;
        public float StrengthAmount => _mult;

        public StrengthenEffect(CharacterCard caster, CharacterCard target, int duration, float mult) : base(caster, target, duration)
        {
            _mult = mult;
            EffectType = ESkillEffect.Vulnerable;
        }

        public override void OverrideEffect(SkillEffect newEffect)
        {
            var strengthenEffect = (StrengthenEffect)newEffect;
            _mult += strengthenEffect._mult;
            Duration = strengthenEffect.Duration;

            base.OverrideEffect(newEffect);
        }

        public override string GetDescription(bool isSelfApply, EPlayerTarget side, string targetList, bool isShowNextLevel)
        {
            return $"Decrease incoming damage taken by {StrengthAmount * 100}%";
        }
    }
    #endregion
}

