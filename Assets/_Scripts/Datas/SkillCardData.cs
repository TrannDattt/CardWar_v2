using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardWar.Enums;
using CardWar.Interfaces;
using CardWar_v2.Entities;
using CardWar_v2.Enums;
using CardWar_v2.ComponentViews;
using Demo;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using CardWar_v2.Factories;
using CardWar_v2.GameControl;

namespace CardWar_v2.Datas
{
    #region Skill Data 
    [CreateAssetMenu(menuName = "SO/Card Data/Skill")]
    public class SkillCardData : ScriptableObject
    {
        // Info
        public string Name;
        public Sprite Image;
        [TextArea(5, 10)] public string Description;

        // Skill
        [SerializeReference]
        [SRDemo(typeof(SubSkill))]
        public List<SubSkill> Skill;
    }
    #endregion

    #region  Abstract sub-skill
    [Serializable]
    public abstract class SubSkill
    {
        public AnimationClip Clip;
        public float DelayToSkill;
        public EPlayerTarget TargetSide;
        public List<EPositionTarget> PositionTargets;

        public abstract Task DoSkill(CharacterModelView casterModel, CharacterModelView targetModel);
        // public abstract string GenerateDescription(CharacterCard owner);
    }
    #endregion

    #region  Range Attack
    [Serializable]
    public class DoRangeAttack : SubSkill
    {
        public CharStat CasterStatMult;
        public CharStat TargetStatMult;
        public ProjectileView Projectile;
        public Vector3 OffsetToCaster;
        public EDamageType DamageType;

        public override async Task DoSkill(CharacterModelView casterModel, CharacterModelView targetModel)
        {
            if (targetModel == null || Projectile == null) return;
            var projectile = UnityEngine.Object.Instantiate(Projectile, casterModel.transform);
            await projectile.FlyToTarget(casterModel.transform.position, OffsetToCaster,
                                        targetModel.transform.position + 3 * Vector3.up,
                                        () => DoDamage(casterModel, targetModel.BaseCard));
        }

        private void DoDamage(CharacterModelView casterModel, IDamagable target)
        {
            var caster = casterModel.BaseCard;
            var damage = (caster.GetCurStat() * CasterStatMult + (target as CharacterCard).GetCurStat() * TargetStatMult).Total;

            target.TakeDamage(caster, damage, DamageType);
            caster.OnDealDamage?.Invoke(damage);
            // Debug.Log($"Target '{target}' took {damage} projectile damage from Caster '{caster}'");
        }
    }
    #endregion

    #region Apply Effect
    [Serializable]
    public class ApplyEffect : SubSkill
    {
        public List<AppliedEffect> Effects;

        private List<SkillEffect> GetSkillEffect(CharacterModelView casterModel, CharacterModelView targetModel)
        {
            var result = new List<SkillEffect>();
            var caster = casterModel.BaseCard;
            var target = targetModel.BaseCard;

            foreach (var e in Effects)
            {
                var effect = e.EffectType switch
                {
                    ESkillEffect.Poison => new PoisonEffect(e.EffectType, caster, target, e.Duration, targetModel.transform),
                    ESkillEffect.Regen => new RegenEffect(e.EffectType, caster, target, e.Duration, targetModel.transform),
                    ESkillEffect.Vulnerable => new VulnerableEffect(e.EffectType, caster, target, e.Duration, targetModel.transform, e.Amount),
                    ESkillEffect.Strengthen => new StrengthenEffect(e.EffectType, caster, target, e.Duration, targetModel.transform, e.Amount),
                    ESkillEffect.Silence => new SilenceEffect(e.EffectType, caster, target, e.Duration, targetModel.transform),
                    ESkillEffect.Burn => new BurnEffect(e.EffectType, caster, target, e.Duration, targetModel.transform),
                    ESkillEffect.Chill => new ChillEffect(e.EffectType, caster, target, e.Duration, targetModel.transform),
                    ESkillEffect.Omnivamp => new OmnivampEffect(e.EffectType, caster, target, e.Duration, targetModel.transform, e.Amount),
                    ESkillEffect.Frostbite => new FrostbiteEffect(e.EffectType, caster, target, e.Duration, targetModel.transform),
                    _ => (SkillEffect)null,
                };

                if (effect == null)
                {
                    Debug.LogError($"Effect type {e.EffectType} not found");
                    return null;
                }

                result.Add(effect);
            }
            return result;
        }

        public override async Task DoSkill(CharacterModelView caster, CharacterModelView target)
        {
            if (target == null) return;
            var effects = GetSkillEffect(caster, target);
            foreach (var e in effects)
            {
                await target.BaseCard.ApplyEffect(e);
            }
        }
    }

    [Serializable]
    public struct AppliedEffect
    {
        public ESkillEffect EffectType;
        public float Amount;
        public int Duration;
    }
    #endregion

    #region Close Attack
    [Serializable]
    public class DoCloseAttack : SubSkill, ICanDoDamage
    {
        public CharStat CasterStatMult;
        public CharStat TargetStatMult;
        public EDamageType DamageType;

        public UnityEvent<float> OnDealDamage { get; set; } = new();

        public override async Task DoSkill(CharacterModelView casterModel, CharacterModelView targetModel)
        {
            if (targetModel == null) return;

            var startPos = casterModel.transform.position;
            var dir = targetModel.transform.position - startPos;
            var offset = new Vector3(4f * Mathf.Sign(dir.x), 0, 0);
            var targetPos = targetModel.transform.position - offset;
            await casterModel.transform.DOMove(targetPos, 1).SetEase(Ease.InCubic).AsyncWaitForCompletion();

            var casterStat = casterModel.BaseCard.GetCurStat();
            var targetStat = targetModel.BaseCard.GetCurStat();
            var damage = (casterStat * CasterStatMult + targetStat * TargetStatMult).Total;

            GameAudioManager.Instance.PlaySFX(GameAudioManager.ESfx.HitEffect, restart: true);
            targetModel.BaseCard.TakeDamage(casterModel.BaseCard, damage, DamageType);
            await Task.Delay(150);
            
            casterModel.BaseCard.OnDealDamage?.Invoke(damage);
            await casterModel.transform.DOMove(startPos, 1).SetEase(Ease.InCubic).AsyncWaitForCompletion();

            // Debug.Log($"Target '{targetModel.BaseCard.Name}' took {damage} damage from Caster '{casterModel.BaseCard.Name}'");
        }
    }
    #endregion

    #region Change Stat
    public class ChangeStat : SubSkill
    {
        public StatMult HpMult;
        public StatMult AtkMult;
        public StatMult AmrMult;
        public StatMult ResMult;

        public override async Task DoSkill(CharacterModelView casterModel, CharacterModelView targetModel)
        {
            if (targetModel == null) return;

            var casterStat = casterModel.BaseCard.GetCurStat();
            var targetStat = targetModel.BaseCard.GetCurStat();

            var hpChange = (casterStat * HpMult.CasterStatMult + targetStat * HpMult.TargetStatMult).Total;
            var atkChange = (casterStat * AtkMult.CasterStatMult + targetStat * AtkMult.TargetStatMult).Total;
            var amrChange = (casterStat * AmrMult.CasterStatMult + targetStat * AmrMult.TargetStatMult).Total;
            var resChange = (casterStat * ResMult.CasterStatMult + targetStat * ResMult.TargetStatMult).Total;

            CharStat statChange = new(hpChange, atkChange, amrChange, resChange);
            // Debug.Log($"ATK: {statChange.Atk} - HP: {statChange.Hp} - AMR: {statChange.Armor} - RES: {statChange.Resist}");
            casterModel.BaseCard.ChangeStat(statChange);
            GameAudioManager.Instance.PlaySFX(GameAudioManager.ESfx.MagicEffect, restart: true);
            await PlayFXs(statChange, targetModel.transform);
        }

        private async Task PlayFXs(CharStat statChange, Transform target)
        {
            async Task PlayFX(ParticleSystem fxRef)
            {
                var fx = UnityEngine.Object.Instantiate(fxRef, target);
                fx.Play();
                while (fx.isPlaying)
                {
                    await Task.Yield();
                }
                UnityEngine.Object.Destroy(fx.gameObject);
            }

            List<Task> tasks = new();
            if (statChange.Atk > 0) tasks.Add(PlayFX(EffectViewFactory.Instance.AtkBuffFX)); 
            if (statChange.Hp > 0) tasks.Add(PlayFX(EffectViewFactory.Instance.HpBuffFX)); 
            if (statChange.Armor > 0) tasks.Add(PlayFX(EffectViewFactory.Instance.AmrBuffFX)); 
            if (statChange.Resist > 0) tasks.Add(PlayFX(EffectViewFactory.Instance.ResBuffFX));

            await Task.WhenAll(tasks); 
        }
    }

    [Serializable]
    public struct StatMult
    {
        public CharStat CasterStatMult;
        public CharStat TargetStatMult;
    }
    #endregion

    #region Conditional Skill
    public class ConditionalSkill : SubSkill
    {
        [SerializeReference]
        [SRDemo(typeof(ConditionCheck))]
        public List<ConditionCheck> Conditions;

        [SerializeReference]
        [SRDemo(typeof(SubSkill))]
        public List<SubSkill> TrueSkills;

        public bool Checked { get; private set; }

        public bool CheckCanDoSkill(CharacterModelView casterModel, CharacterModelView targetModel)
        {
            // Debug.Log($"Check all conditions: {Conditions.All(c => c.CheckCondition(casterModel.BaseCard, targetModel.BaseCard))}");
            return Conditions.All(c => c.CheckCondition(casterModel.BaseCard, targetModel.BaseCard));
        }

        public override async Task DoSkill(CharacterModelView casterModel, CharacterModelView targetModel)
        {
            Checked = CheckCanDoSkill(casterModel, targetModel);
            // foreach(var s in TrueSkills)
            // {
            //     Debug.Log($"Do skill: {s.GetType()}");
            //     await s.DoSkill(casterModel, targetModel);
            // }
        }
    }

    [Serializable]
    public abstract class ConditionCheck
    {
        public abstract bool CheckCondition(CharacterCard caster, CharacterCard target);
    }

    [Serializable]
    public class KillLastTargetCheck : ConditionCheck
    {
        public override bool CheckCondition(CharacterCard caster, CharacterCard target)
        {
            // Debug.Log($"Target: {target?.Name}");
            // Debug.Log($"Target's Hp: {target?.GetCurStat().Hp}");
            // Debug.Log($"Condition check: {target == null || target.GetCurStat().Hp == 0}");
            return target == null || target.GetCurStat().Hp == 0;
        }
    }

    [Serializable]
    public class CharInSlotCheck : ConditionCheck
    {
        public EPlayerTarget Region;
        public EPositionTarget Position;
        public bool checkExist;

        public override bool CheckCondition(CharacterCard caster, CharacterCard target)
        {
            var board = UnityEngine.Object.FindFirstObjectByType<BoardView>();
            if (board == null) return false;
            return board.GetCharacterByPos(Region, Position, false) != null == checkExist;
        }
    }

    [Serializable]
    public class StatCheck : ConditionCheck
    {
        [Serializable]
        public class SC
        {
            [Range(0, 1)] public float StatPercentThreshold;
            public List<ECompareOperator> CheckTypes;
        }

        public List<SC> CasterHpCheck;
        public List<SC> CasterAtkCheck;
        public List<SC> CasterAmrCheck;
        public List<SC> CasterResCheck;

        public List<SC> TargetHpCheck;
        public List<SC> TargetAtkCheck;
        public List<SC> TargetAmrCheck;
        public List<SC> TargetResCheck;

        private bool CompareStat(float curStat, float baseStat, SC check)
        {
            bool result = false;
            foreach (var cmp in check.CheckTypes)
            {
                result |= cmp switch
                {
                    ECompareOperator.Greater => (curStat / baseStat) > check.StatPercentThreshold,
                    ECompareOperator.Equal => (curStat / baseStat) == check.StatPercentThreshold,
                    ECompareOperator.Less => (curStat / baseStat) < check.StatPercentThreshold,
                    ECompareOperator.Any => true,
                    _ => false,
                };
            }

            return result;
        }

        public override bool CheckCondition(CharacterCard caster, CharacterCard target)
        {
            var casterBaseStat = caster.GetStatAtLevel(caster.Level);
            var casterCurStat = caster.GetCurStat();
            var targetBaseStat = target != null ? target.GetStatAtLevel(target.Level) : new();
            var targetCurStat = target != null ? target.GetCurStat() : new();

            return CasterHpCheck.All(sc => CompareStat(casterCurStat.Hp, casterBaseStat.Hp, sc))
                   && CasterAtkCheck.All(sc => CompareStat(casterCurStat.Atk, casterBaseStat.Atk, sc))
                   && CasterAmrCheck.All(sc => CompareStat(casterCurStat.Armor, casterBaseStat.Armor, sc))
                   && CasterResCheck.All(sc => CompareStat(casterCurStat.Resist, casterBaseStat.Resist, sc))
                   && TargetHpCheck.All(sc => CompareStat(targetCurStat.Hp, targetBaseStat.Hp, sc))
                   && TargetAtkCheck.All(sc => CompareStat(targetCurStat.Atk, targetBaseStat.Atk, sc))
                   && TargetAmrCheck.All(sc => CompareStat(targetCurStat.Armor, targetBaseStat.Armor, sc))
                   && TargetResCheck.All(sc => CompareStat(targetCurStat.Resist, targetBaseStat.Resist, sc));
        }
    }
    #endregion

    #region Event Tracking
    public class EventTrackingSkill : SubSkill
    {
        public bool IsAttacking;

        [SerializeReference]
        [SRDemo(typeof(SubSkill))]
        public List<SubSkill> TrueSkills;

        public bool Checked { get; private set; }

        private void CheckEvent() => Checked = true;

        public override async Task DoSkill(CharacterModelView casterModel, CharacterModelView targetModel)
        {
            Checked = false;

            if (IsAttacking)
            {
                void OnDealDamageListerner(float damage)
                {
                    CheckEvent();
                    targetModel.BaseCard.OnDealDamage.RemoveListener(OnDealDamageListerner);
                }
                targetModel.BaseCard.OnDealDamage.AddListener(OnDealDamageListerner);
            }
        }
    }
    #endregion
}