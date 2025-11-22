using System.Threading.Tasks;
using CardWar_v2.Enums;
using UnityEngine;
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

        public abstract string GetDescription();
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

        public override string GetDescription()
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

        public override string GetDescription()
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

        public override string GetDescription()
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
            EffectType = ESkillEffect.Strengthen;
        }

        public override void OverrideEffect(SkillEffect newEffect)
        {
            var strengthenEffect = (StrengthenEffect)newEffect;
            _mult += strengthenEffect._mult;
            Duration = strengthenEffect.Duration;

            base.OverrideEffect(newEffect);
        }

        public override string GetDescription()
        {
            return $"Decrease incoming damage taken by {StrengthAmount * 100}%";
        }
    }
    #endregion

    #region Silence
    public class SilenceEffect : SkillEffect
    {
        public SilenceEffect(CharacterCard caster, CharacterCard target, int duration) : base(caster, target, duration)
        {
            EffectType = ESkillEffect.Silence;
        }

        public override void OverrideEffect(SkillEffect newEffect)
        {
            Duration = newEffect.Duration;

            base.OverrideEffect(newEffect);
        }

        public override string GetDescription()
        {
            return $"Cannot use skills this turn";
        }
    }
    #endregion

    #region Burn
    public class BurnEffect : SkillEffect
    {
        private float _mult = .2f;

        // Use in battle
        public float DamageAmount => _mult * _target.CurHp;

        public BurnEffect(CharacterCard caster, CharacterCard target, int duration) : base(caster, target, duration)
        {
            EffectType = ESkillEffect.Burn;
        }

        public override void OverrideEffect(SkillEffect newEffect)
        {
            var burnEffect = (BurnEffect)newEffect;
            Duration += newEffect.Duration;
            _caster = burnEffect._caster;

            base.OverrideEffect(newEffect);
        }

        public override async Task DoEffect()
        {
            _target.ChangeHp(-DamageAmount);

            await base.DoEffect();
        }

        public override string GetDescription()
        {
            return $"Take damage equals to {_mult * 100}% of the target's current HP ({DamageAmount}) every turn. - Target: {_target.Name} -";
        }
    }
    #endregion

    #region Chill
    public class ChillEffect : SkillEffect
    {
        private float _mult = .05f;
        public float ChillAmount => _mult;

        public ChillEffect(CharacterCard caster, CharacterCard target, int duration) : base(caster, target, duration)
        {
            EffectType = ESkillEffect.Chill;
        }

        public override void OverrideEffect(SkillEffect newEffect)
        {
            var chillEffect = (ChillEffect)newEffect;
            _mult += chillEffect._mult;
            Duration = chillEffect.Duration;

            base.OverrideEffect(newEffect);
        }

        public override string GetDescription()
        {
            return $"Decrease target's ATK by {ChillAmount * 100}%";
        }
    }
    #endregion

    #region Frostbite
    public class FrostbiteEffect : SkillEffect
    {
        private CharStat _targetBaseStat => _target.GetStatAtLevel(_target.Level);
        public float FrostbiteAmount => Mathf.Max(0, (_targetBaseStat.Atk - _target.CurAtk) / _targetBaseStat.Atk) * _targetBaseStat.Hp;

        public FrostbiteEffect(CharacterCard caster, CharacterCard target, int duration) : base(caster, target, duration)
        {
            EffectType = ESkillEffect.Frostbite;
        }

        public override void OverrideEffect(SkillEffect newEffect)
        {
            Duration = newEffect.Duration;

            base.OverrideEffect(newEffect);
        }

        public override async Task DoEffect()
        {
            _target.ChangeHp(-FrostbiteAmount);

            await base.DoEffect();
        }

        public override string GetDescription()
        {
            return $"Take damage equals to the difference percentages between target's base ATK and current ATK * target's Max HP ({FrostbiteAmount}) - Target: {_target.Name} -";
        }
    }
    #endregion

    #region Omnivamp
    public class OmnivampEffect : SkillEffect
    {
        private float _mult;
        public float OmnivampAmount = 0;

        public OmnivampEffect(CharacterCard caster, CharacterCard target, int duration, float mult) : base(caster, target, duration)
        {
            _mult = mult;
            EffectType = ESkillEffect.Omnivamp;
        }

        public override void OverrideEffect(SkillEffect newEffect)
        {
            var omnivampEffect = (OmnivampEffect)newEffect;
            _mult += omnivampEffect._mult;
            Duration = omnivampEffect.Duration;

            base.OverrideEffect(newEffect);
        }

        //TODO: Active omnivamp when dealing damage

        public override string GetDescription()
        {
            return $"Heal for {_mult * 100}% of damage dealt";
        }
    }
    #endregion
}

