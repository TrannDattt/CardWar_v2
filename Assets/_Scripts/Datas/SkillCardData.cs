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

        // public override string GenerateDescription(CharacterCard owner)
        // {
        //     bool isSelfApply = PositionTargets.Any(t => t == EPositionTarget.Self);
        //     var randomTargets = PositionTargets.Where(t => t == EPositionTarget.Random).ToList();
        //     var normalTargets = PositionTargets.Where(t => t != EPositionTarget.Self && t != EPositionTarget.Random).ToList();

        //     string targetList = "";

        //     if (normalTargets.Count == 1)
        //     {
        //         targetList = normalTargets[0].ToString();
        //     }
        //     else if (normalTargets.Count > 1)
        //     {
        //         targetList = string.Join(", ", $"the {normalTargets.Take(normalTargets.Count - 1)}")
        //                     + $" and the {normalTargets[^1]}";
        //     }

        //     if (randomTargets.Count > 0)
        //     {
        //         string randomDesc = $"{randomTargets.Count} random target(s)";
        //         if (!string.IsNullOrEmpty(targetList))
        //             targetList += $" and {randomDesc}";
        //         else
        //             targetList = randomDesc;
        //     }

        //     if (string.IsNullOrEmpty(targetList))
        //         targetList = "unknown positions";

        //     return $"Fire projectile(s) to {(isSelfApply ? "yourself and " : "")}" +
        //         $"{TargetSide} targets at {targetList}, " +
        //         $"dealing {DamageMult * owner.GetStatAtLevel(owner.Level).Atk} " +
        //         $"{DamageType} damage to each target.";
        // }

        public override async Task DoSkill(CharacterModelView casterModel, CharacterModelView targetModel)
        {
            // TODO: Spawn projectile with factory
            if (Projectile == null) return;
            var projectile = UnityEngine.Object.Instantiate(Projectile, casterModel.transform);
            await projectile.FlyToTarget(casterModel.transform.position, OffsetToCaster,
                                        targetModel.transform.position + 3 * Vector3.up,
                                        () => DoDamage(casterModel, targetModel.BaseCard));
        }

        private void DoDamage(CharacterModelView casterModel, IDamagable target)
        {
            var caster = casterModel.BaseCard;
            var damage = (caster.GetCurStat() * CasterStatMult + (target as CharacterCard).GetCurStat() * TargetStatMult).Total;

            target.TakeDamage(caster, damage, DamageType, FXPlayer.EFXType.Hit);
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
            var effects = GetSkillEffect(caster, target);
            foreach (var e in effects)
            {
                await target.BaseCard.ApplyEffect(e);
            }
        }

    //     public override string GenerateDescription(CharacterCard owner)
    //     {
    //         var effect = GetSkillEffect(owner, null);
    //         bool isSelfApply = PositionTargets.Any(t => t == EPositionTarget.Self);
    //         var randomTargets = PositionTargets.Where(t => t == EPositionTarget.Random).ToList();
    //         var normalTargets = PositionTargets.Where(t => t != EPositionTarget.Self && t != EPositionTarget.Random).ToList();

    //         string targetList = "";

    //         if (normalTargets.Count == 1)
    //         {
    //             targetList = normalTargets[0].ToString();
    //         }
    //         else if (normalTargets.Count > 1)
    //         {
    //             targetList = string.Join(", ", $"the {normalTargets.Take(normalTargets.Count - 1)}")
    //                         + $" and the {normalTargets[^1]}";
    //         }

    //         if (randomTargets.Count > 0)
    //         {
    //             string randomDesc = $"{randomTargets.Count} random target(s)";
    //             if (!string.IsNullOrEmpty(targetList))
    //                 targetList += $" and {randomDesc}";
    //             else
    //                 targetList = randomDesc;
    //         }

    //         if (string.IsNullOrEmpty(targetList))
    //             targetList = "unknown positions";

    //         // return effect.GetDescription(isSelfApply, TargetSide, targetList, isShowNextLevel);
    //         return $"Apply {E} to {(isSelfApply ? "yourself and " : "")}" +
    //             $"{TargetSide} targets at {targetList}, " +
    //             $"dealing {DamageMult * owner.GetStatAtLevel(owner.Level).Atk} " +
    //             $"{DamageType} damage to each target.";
    //     }
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
    public class DoCloseAttack : SubSkill
    {
        public CharStat CasterStatMult;
        public CharStat TargetStatMult;
        public EDamageType DamageType;

        public override async Task DoSkill(CharacterModelView casterModel, CharacterModelView targetModel)
        {
            var startPos = casterModel.transform.position;
            var dir = targetModel.transform.position - startPos;
            var offset = new Vector3(4f * Mathf.Sign(dir.x), 0, 0);
            var targetPos = targetModel.transform.position - offset;
            await casterModel.transform.DOMove(targetPos, 1).SetEase(Ease.InCubic).AsyncWaitForCompletion();

            var casterStat = casterModel.BaseCard.GetCurStat();
            var targetStat = targetModel.BaseCard.GetCurStat();
            var damage = (casterStat * CasterStatMult + targetStat * TargetStatMult).Total;
            await Task.Delay(150);

            targetModel.BaseCard.TakeDamage(casterModel.BaseCard, damage, DamageType, FXPlayer.EFXType.Hit);
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
            var casterStat = casterModel.BaseCard.GetCurStat();
            var targetStat = targetModel.BaseCard.GetCurStat();

            var hpChange = (casterStat * HpMult.CasterStatMult + targetStat * HpMult.TargetStatMult).Total;
            var atkChange = (casterStat * AtkMult.CasterStatMult + targetStat * AtkMult.TargetStatMult).Total;
            var amrChange = (casterStat * AmrMult.CasterStatMult + targetStat * AmrMult.TargetStatMult).Total;
            var resChange = (casterStat * ResMult.CasterStatMult + targetStat * ResMult.TargetStatMult).Total;

            casterModel.BaseCard.ChangeStat(new(hpChange, atkChange, amrChange, resChange));

            //TODO: Add some animation or effect here
            await Task.CompletedTask;
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
        


        public bool TargetHpPercentCheck;
        [Range(0, 1)] public float TargetHpPercentThreshold;

        public List<SubSkill> TrueSkills;
        public UnityEvent OnConditionMet = new();

        // public bool CheckCanDoSkill(CharacterCard caster, CharacterCard target)
        // {
        //     OnConditionMet?.Invoke();
        //     // return true;
        // }

        public override Task DoSkill(CharacterModelView casterModel, CharacterModelView targetModel)
        {
            throw new NotImplementedException();
        }
    }

    [Serializable]
    public abstract class ConditionCheck
    {
        public abstract bool CheckCondition(CharacterCard caster, CharacterCard target);
    }

    // [Serializable]
    // public class EffectCheck : ConditionCheck
    // {
    //     public List<ESkillEffect> TargetHasEffects;
    // }

    // [Serializable]
    // public class StatCheck : ConditionCheck
    // {
    //     [Range(0, 1)] public CharStat StatPercentThreshold;
    // }
    #endregion
}