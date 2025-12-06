using System.Collections.Generic;
using System.Threading.Tasks;
using CardWar.Interfaces;
using CardWar_v2.Enums;
using CardWar_v2.Factories;
using CardWar_v2.GameControl;
using UnityEngine;
using UnityEngine.Events;
using static CardWar_v2.Factories.EffectViewFactory;

namespace CardWar_v2.Entities
{
    public abstract class SkillEffect
    {
        protected CharacterCard _caster;
        protected CharacterCard _target;
        protected ParticleSystem _onApplyFx;
        protected ParticleSystem _onActiveFx;
        protected string _effectColor = "FFFFFF";
        protected AudioClip _sfx;

        public ESkillEffect EffectType { get; protected set; }
        public int Duration { get; protected set; }

        private Dictionary<ESkillEffect, EffectSetting> EffectDict => EffectViewFactory.Instance.EffectDict;

        public UnityEvent OnEffectUpdated;
        public UnityEvent OnDoEffect;

        public SkillEffect(ESkillEffect type, CharacterCard caster, CharacterCard target, int duration, Transform targetTranform)
        {
            _caster = caster;
            _target = target;
            Duration = duration;
            EffectType = type;

            if (EffectType != ESkillEffect.None)
            {
                _effectColor = ColorUtility.ToHtmlStringRGB(EffectDict[EffectType].TextColor);
                _sfx = EffectDict[EffectType].SFX;
                var offset = 3f * Vector3.up;

                var onApplyFx = EffectDict[EffectType].OnApplyFX;
                if(onApplyFx != null && _onApplyFx == null)
                {
                    _onApplyFx = Object.Instantiate(onApplyFx, targetTranform.position + offset, Quaternion.identity, targetTranform);
                    foreach (Transform transform in _onApplyFx.gameObject.transform)
                    {
                        transform.gameObject.layer = LayerMask.NameToLayer("FX");
                    }
                    _onApplyFx.gameObject.SetActive(false);
                }

                var onActiveFx = EffectDict[EffectType].OnActiveFX;
                if(onActiveFx != null && _onActiveFx == null)
                {
                    _onActiveFx = Object.Instantiate(onActiveFx, targetTranform.position + offset, Quaternion.identity, targetTranform);
                    foreach (Transform transform in _onActiveFx.gameObject.transform)
                    {
                        transform.gameObject.layer = LayerMask.NameToLayer("FX");
                    }
                    _onActiveFx.gameObject.SetActive(false);
                }
            }

            OnEffectUpdated = new();
            OnDoEffect = new();

            GameplayManager.Instance.OnTurnChanged.AddListener(() =>
            {
                Duration--;
                OnEffectUpdated?.Invoke();
            });
        }

        private async Task PlayFX(ParticleSystem fx)
        {
            if (fx == null) return;
            fx.gameObject.SetActive(true);
            while (fx != null && fx.isPlaying)
            {
                await Task.Yield();
            }
            if (fx != null) fx.gameObject.SetActive(false);
        }

        public virtual async Task ApplyEffect()
        {
            GameAudioManager.Instance.PlaySFX(GameAudioManager.ESfx.PopEffect, restart: true);
            await PlayFX(_onApplyFx);
        }

        public virtual void OverrideEffect(SkillEffect newEffect)
        {
            OnEffectUpdated?.Invoke();
        }

        public virtual async Task DoEffect()
        {
            if (_sfx != null) GameAudioManager.Instance.PlaySFX(_sfx, false);
            await PlayFX(_onActiveFx);

            OnDoEffect?.Invoke();
            // Duration--;
            // OnEffectUpdated?.Invoke();
        }

        public virtual void RemoveEffect()
        {
            // if (_onApplyFx != null) Object.Destroy(_onApplyFx.gameObject);
            // if (_onActiveFx != null) Object.Destroy(_onActiveFx.gameObject);
        }

        public virtual string GetDescription() => $" ({Duration} turn{(Duration > 1 ? "s" : "")}).";
    }

    #region Regen
    public class RegenEffect : SkillEffect, ICanDoDamage
    {
        private float _mult = .02f;

        public float HealAmount => _mult * _target.CurHp;

        public RegenEffect(ESkillEffect type, CharacterCard caster, CharacterCard target, int duration, Transform targetTranform) : base(type, caster, target, duration,  targetTranform)
        {
        }

        public override void OverrideEffect(SkillEffect newEffect)
        {
            Duration += newEffect.Duration;

            base.OverrideEffect(newEffect);
        }

        public override async Task DoEffect()
        {
            await base.DoEffect();

            _target.TakeDamage(this, -HealAmount, EDamageType.Pure);
        }

        public override string GetDescription()
        {
            return $"Regen equals to <color=#{_effectColor}>{_mult * 100}</color>% of your current HP "
                + $"(<color=#{_effectColor}>{(int)HealAmount}</color>) every turn" 
                + base.GetDescription();
        }
    }
    #endregion

    #region  Poison
    public class PoisonEffect : SkillEffect, ICanDoDamage
    {
        private float _mult = .45f;

        // Use in battle
        public float DamageAmount => _mult * _caster.CurAtk;

        public PoisonEffect(ESkillEffect type, CharacterCard caster, CharacterCard target, int duration, Transform targetTranform) : base(type, caster, target, duration,  targetTranform)
        {
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
            await base.DoEffect();

            _target.TakeDamage(this, DamageAmount, EDamageType.Pure);
        }

        public override string GetDescription()
        {
            return $"Take damage equals to <color=#{_effectColor}>{_mult * 100}</color>% of the caster's ATK "
                + $"(<color=#{_effectColor}>{(int)DamageAmount}</color>) every turn - Caster: {_caster.Name} -" 
                + base.GetDescription();
        }
    }
    #endregion

    #region  Vulnerable
    public class VulnerableEffect : SkillEffect
    {
        private float _mult;
        public float VulAmount => _mult;

        public VulnerableEffect(ESkillEffect type, CharacterCard caster, CharacterCard target, int duration, Transform targetTranform, float mult) : base(type, caster, target, duration,  targetTranform)
        {
            _mult = mult;
        }

        public override async Task ApplyEffect()
        {
            _target.OnTakingDamage.AddListener(async (_, _) => await DoEffect());

            await base.ApplyEffect();
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
            return $"Increase incoming damage taken by <color=#{_effectColor}>{VulAmount * 100}</color>%" 
                + base.GetDescription();
        }
    }
    #endregion

    #region Strengthen
    public class StrengthenEffect : SkillEffect
    {
        private float _mult;
        public float StrengthAmount => _mult;

        public StrengthenEffect(ESkillEffect type, CharacterCard caster, CharacterCard target, int duration, Transform targetTranform, float mult) : base(type, caster, target, duration,  targetTranform)
        {
            _mult = mult;
        }

        public override async Task ApplyEffect()
        {
            _target.OnTakingDamage.AddListener(async (_, _) => await DoEffect());

            await base.ApplyEffect();
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
            return $"Decrease incoming damage taken by <color=#{_effectColor}>{StrengthAmount * 100}</color>%" 
                + base.GetDescription();
        }
    }
    #endregion

    #region Silence
    public class SilenceEffect : SkillEffect
    {
        public SilenceEffect(ESkillEffect type, CharacterCard caster, CharacterCard target, int duration, Transform targetTranform) : base(type, caster, target, duration,  targetTranform)
        {
        }

        public override async Task ApplyEffect()
        {
            _target.OnUseSkill.AddListener(async (_) => await DoEffect());

            await base.ApplyEffect();
        }

        public override void OverrideEffect(SkillEffect newEffect)
        {
            Duration = newEffect.Duration;

            base.OverrideEffect(newEffect);
        }

        public override string GetDescription()
        {
            return $"Cannot use skills this turn" + base.GetDescription();
        }
    }
    #endregion

    #region Burn
    public class BurnEffect : SkillEffect, ICanDoDamage
    {
        private float _mult = .15f;

        // Use in battle
        public float DamageAmount => _mult * _target.CurHp;

        public BurnEffect(ESkillEffect type, CharacterCard caster, CharacterCard target, int duration, Transform targetTranform) : base(type, caster, target, duration,  targetTranform)
        {
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
            await base.DoEffect();

            _target.TakeDamage(this, DamageAmount, EDamageType.Pure);
        }

        public override string GetDescription()
        {
            return $"Take damage equals to <color=#{_effectColor}>{_mult * 100}</color>% of the your current HP "
                + $"(<color=#{_effectColor}>{(int)DamageAmount}</color>) every turn" 
                + base.GetDescription();
        }
    }
    #endregion

    #region Chill
    public class ChillEffect : SkillEffect
    {
        private float _mult = .15f;
        public float ChillAmount => _mult * Duration;

        public ChillEffect(ESkillEffect type, CharacterCard caster, CharacterCard target, int duration, Transform targetTranform) : base(type, caster, target, duration,  targetTranform)
        {
        }

        public override async Task ApplyEffect()
        {
            _target.OnDealDamage.AddListener(async (_) => await DoEffect());

            await base.ApplyEffect();
        }

        public override void OverrideEffect(SkillEffect newEffect)
        {
            var chillEffect = (ChillEffect)newEffect;
            // _mult += chillEffect._mult;
            Duration = chillEffect.Duration;

            base.OverrideEffect(newEffect);
        }

        public override string GetDescription()
        {
            return $"Decrease your ATK by <color=#{_effectColor}>{ChillAmount * 100}</color>%" 
                + base.GetDescription();
        }
    }
    #endregion

    #region Frostbite
    public class FrostbiteEffect : SkillEffect, ICanDoDamage
    {
        private CharStat _targetBaseStat => _target.GetStatAtLevel(_target.Level);
        public float FrostbiteAmount => Mathf.Max(0, (_targetBaseStat.Atk - _target.CurAtk) / _targetBaseStat.Atk) * _targetBaseStat.Hp;

        public FrostbiteEffect(ESkillEffect type, CharacterCard caster, CharacterCard target, int duration, Transform targetTranform) : base(type, caster, target, duration,  targetTranform)
        {
        }

        public override void OverrideEffect(SkillEffect newEffect)
        {
            Duration = newEffect.Duration;

            base.OverrideEffect(newEffect);
        }

        public override async Task DoEffect()
        {
            await base.DoEffect();

            _target.TakeDamage(this, FrostbiteAmount, EDamageType.Pure);
        }

        public override string GetDescription()
        {
            return $"Take damage equals to the difference percentages between your base ATK and current ATK * your Max HP "
                + $"(<color=#{_effectColor}>{(int)FrostbiteAmount}</color>)" 
                + base.GetDescription();
        }
    }
    #endregion

    #region Omnivamp
    public class OmnivampEffect : SkillEffect
    {
        private float _mult;
        public float OmnivampAmount = 0;

        public OmnivampEffect(ESkillEffect type, CharacterCard caster, CharacterCard target, int duration, Transform targetTranform, float mult) : base(type, caster, target, duration,  targetTranform)
        {
            _mult = mult;
        }

        public override void OverrideEffect(SkillEffect newEffect)
        {
            var omnivampEffect = (OmnivampEffect)newEffect;
            _mult += omnivampEffect._mult;
            Duration = omnivampEffect.Duration;

            base.OverrideEffect(newEffect);
        }

        public override async Task ApplyEffect()
        {
            Debug.Log("Apply Omnivamp");
            await base.ApplyEffect();

            _caster.OnDealDamage.AddListener(DoEffect);
        }

        public async void DoEffect(float damageDealt)
        {
            // Debug.Log($"Caster {_caster.Name} - Omnivamp: {damageDealt * _mult} Hp");
            await base.DoEffect();

            _caster.ChangeHp(damageDealt * _mult);
        }

        public override void RemoveEffect()
        {
            Debug.Log("Remove Omnivamp");
            base.RemoveEffect();
            
            _caster.OnDealDamage.RemoveListener(DoEffect);
        }

        public override string GetDescription()
        {
            return $"Heal for <color=#{_effectColor}>{_mult * 100}</color>% of damage dealt" 
                + base.GetDescription();
        }
    }
    #endregion
}

