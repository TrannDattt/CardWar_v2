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

namespace CardWar_v2.Datas
{
    #region Skill Data 
    [CreateAssetMenu(menuName = "SO/Card Data/Skill")]
    public class SkillCardData : ScriptableObject
    {
        // Info
        public string Name;
        public Sprite Image;
        public string Description;

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
            var projectile = UnityEngine.Object.Instantiate(Projectile, casterModel.transform);
            await projectile.FlyToTarget(casterModel.transform.position, OffsetToCaster,
                                        targetModel.transform.position + OffsetToCaster.y * Vector3.up,
                                        () => DoDamage(casterModel, targetModel.BaseCard));
        }

        private void DoDamage(CharacterModelView casterModel, IDamagable target)
        {
            var caster = casterModel.BaseCard;
            var damage = (caster.GetCurStat() * CasterStatMult + (target as CharacterCard).GetCurStat() + TargetStatMult).Total;

            target.TakeDamage(damage, DamageType);
            // Debug.Log($"Target '{target}' took {damage} damage from Caster '{caster}'");
        }
    }
    #endregion

    #region Apply Effect
    [Serializable]
    public class ApplyEffect : SubSkill
    {
        public List<AppliedEffect> Effects;

        private List<SkillEffect> GetSkillEffect(CharacterCard caster, CharacterCard target)
        {
            var result = new List<SkillEffect>();

            foreach (var e in Effects)
            {
                var effect = e.EffectType switch
                {
                    ESkillEffect.Poison => new PoisonEffect(caster, target, e.Duration),
                    ESkillEffect.Regen => new RegenEffect(caster, target, e.Duration),
                    ESkillEffect.Vulnerable => new VulnerableEffect(caster, target, e.Duration, e.Amount),
                    ESkillEffect.Strengthen => new StrengthenEffect(caster, target, e.Duration, e.Amount),
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
            var effects = GetSkillEffect(caster.BaseCard, target.BaseCard);

            effects.ForEach(e => target.BaseCard.ApplyEffect(e));
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

        private async Task MovePosition(float duration)
        {
            
        }

        public override async Task DoSkill(CharacterModelView casterModel, CharacterModelView targetModel)
        {
            var startPos = casterModel.transform.position;
            var dir = targetModel.transform.position - startPos;
            var offset = new Vector3(4f * Mathf.Sign(dir.x), 0, 0);
            var targetPos = targetModel.transform.position - offset;
            await casterModel.transform.DOMove(targetPos, 1).SetEase(Ease.InCubic).AsyncWaitForCompletion();

            var casterStat = casterModel.BaseCard.GetCurStat();
            var targetStat = targetModel.BaseCard.GetCurStat();
            var damage = (casterStat * CasterStatMult + targetStat + TargetStatMult).Total;

            targetModel.BaseCard.TakeDamage(damage, DamageType);
            await casterModel.transform.DOMove(startPos, 1).SetEase(Ease.InCubic).AsyncWaitForCompletion();
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

            var hpChange = (casterStat * HpMult.CasterStatMult + targetStat + HpMult.TargetStatMult).Total;
            var atkChange = (casterStat * AtkMult.CasterStatMult + targetStat + AtkMult.TargetStatMult).Total;
            var amrChange = (casterStat * AmrMult.CasterStatMult + targetStat + AmrMult.TargetStatMult).Total;
            var resChange = (casterStat * ResMult.CasterStatMult + targetStat + ResMult.TargetStatMult).Total;

            casterModel.BaseCard.ChangeStat(new(hpChange, atkChange, amrChange, resChange));
        }
    }

    [Serializable]
    public struct StatMult
    {
        public CharStat CasterStatMult;
        public CharStat TargetStatMult;
    }
    #endregion
}