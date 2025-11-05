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

        public abstract string GetDescription(string targetList, bool isShowNextLevel);
    }

    public class RegenEffect : SkillEffect
    {
        private float _healMult;
        private float _bonusHealMultPerLevel;

        // Use in battle
       public float GetCurAmount() => (_healMult + _caster.Level * _bonusHealMultPerLevel) * _caster.CurAtk; 
       public float GetCurAmount(int level) => (_healMult + level * _bonusHealMultPerLevel) * _caster.CurAtk; 
        
        // Use outside battle
        public float GetBaseAmount(int level) => (_healMult + level * _bonusHealMultPerLevel) * _caster.GetStatAtLevel(level).Atk; 

        public RegenEffect(CharacterCard caster,
                           CharacterCard target,
                           int duration,
                           float damageMult,
                           float bonusDamageMultPerLevel) : base(caster, target, duration)
        {
            _healMult = damageMult;
            _bonusHealMultPerLevel = bonusDamageMultPerLevel;
            EffectType = ESkillEffect.Regen;
        }

        public override void OverrideEffect(SkillEffect newEffect)
        {
            var regenEffect = (RegenEffect)newEffect;
            _healMult = regenEffect._healMult;
            _bonusHealMultPerLevel = regenEffect._bonusHealMultPerLevel;

            base.OverrideEffect(newEffect);
        }

        public override async Task DoEffect()
        {
            _target.ChangeHp(GetCurAmount(_caster.Level));

            await base.DoEffect();
        }

        public override string GetDescription(string targetList, bool isShowNextLevel)
        {
            //TODO: Control the stat to scale healing with
            return $"Apply {EffectType} to targets at {targetList}, " +
                $"heal them for {GetCurAmount(_caster.Level)} " +
                $"{(isShowNextLevel ? $"=> {GetCurAmount(_caster.Level + 1)} " : "")}" +
                $"for {Duration} turn(s).";
        }
    }

    public class PoisonEffect : SkillEffect
    {
        private float _damageMult;
        private float _bonusDamageMultPerLevel;
        private EDamageType _damageType;

        // Use in battle
       public float GetCurAmount() => (_damageMult + _caster.Level * _bonusDamageMultPerLevel) * _caster.CurAtk; 
       public float GetCurAmount(int level) => (_damageMult + level * _bonusDamageMultPerLevel) * _caster.CurAtk; 
        
        // Use outside battle
        public float GetBaseAmount(int level) => (_damageMult + level * _bonusDamageMultPerLevel) * _caster.GetStatAtLevel(level).Atk; 

        public PoisonEffect(CharacterCard caster,
                            CharacterCard target,
                            int duration,
                            float damageMult,
                            float bonusDamageMultPerLevel,
                            EDamageType damageType) : base(caster, target, duration)
        {
            _damageMult = damageMult;
            _bonusDamageMultPerLevel = bonusDamageMultPerLevel;
            _damageType = damageType;
            EffectType = ESkillEffect.Poison;
        }

        public override void OverrideEffect(SkillEffect newEffect)
        {
            var poisonEffect = (PoisonEffect)newEffect;
            _damageMult = poisonEffect._damageMult;
            _bonusDamageMultPerLevel = poisonEffect._bonusDamageMultPerLevel;
            _damageType = poisonEffect._damageType;
            EffectType = ESkillEffect.Poison;

            base.OverrideEffect(newEffect);
        }

        public override async Task DoEffect()
        {
            _target.ChangeHp(-GetCurAmount(_caster.Level));

            await base.DoEffect();
        }

        public override string GetDescription(string targetList, bool isShowNextLevel)
        {
            return $"Apply {EffectType} to targets at {targetList}, " +
                $"dealing {GetCurAmount(_caster.Level)} " +
                $"{(isShowNextLevel ? $"=> {GetCurAmount(_caster.Level + 1)} " : "")}" +
                $"{(_damageType != EDamageType.None ? _damageType : "")} damage " +
                $"for {Duration} turn(s).";
        }
    }

    public class VulnerableEffect : SkillEffect
    {
        private float _damageMult;
        private float _bonusDamageMultPerLevel;

        public float GetCurAmount() => _damageMult + _caster.Level * _bonusDamageMultPerLevel;
        public float GetCurAmount(int level) => _damageMult + level * _bonusDamageMultPerLevel;

        public VulnerableEffect(CharacterCard caster,
                                CharacterCard target,
                                int duration,
                                float damageMult,
                                float bonusDamageMultPerLevel) : base(caster, target, duration)
        {
            _damageMult = damageMult;
            _bonusDamageMultPerLevel = bonusDamageMultPerLevel;
            EffectType = ESkillEffect.Vulnerable;
        }

        public override void OverrideEffect(SkillEffect newEffect)
        {
            var vulEffect = (VulnerableEffect)newEffect;
            _damageMult = vulEffect._damageMult;
            _bonusDamageMultPerLevel = vulEffect._bonusDamageMultPerLevel;

            base.OverrideEffect(newEffect);
        }

        public override string GetDescription(string targetList, bool isShowNextLevel)
        {
            return $"Apply {EffectType} to targets at {targetList}, " +
                $"increase damage receive by {GetCurAmount(_caster.Level)}% " +
                $"{(isShowNextLevel ? $"=> {GetCurAmount(_caster.Level + 1)}% " : "")}" +
                $"for {Duration} turn(s).";
        }
    }
}

