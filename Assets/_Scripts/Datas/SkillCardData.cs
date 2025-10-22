using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardWar.Enums;
using CardWar.Interfaces;
using CardWar_v2.Entities;
using CardWar_v2.Enums;
using CardWar_v2.Views;
using Demo;
using UnityEngine;

namespace CardWar_v2.Datas
{
    #region Skill Data 
    [CreateAssetMenu(menuName = "SO/Card Data/Skill")]
    public class SkillCardData : ScriptableObject
    {
        // Info
        public string Name;
        public Sprite Image;

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
        public ESkillType Type;
        public AnimationClip Clip;
        public float DelayToSkill;
        public List<EPositionTarget> PosTargets;

        public abstract Task DoSkill(CharacterModelView caster, CharacterModelView target);
        public abstract string GenerateDescription(CharacterCard owner);
    }
    #endregion

    #region  Range Attack
    [Serializable]
    public class DoRangeAttack : SubSkill
    {
        public float DamageMult;
        public ProjectileView Projectile;
        public Vector3 OffsetToCaster;
        public EDamageType DamageType;

        public override string GenerateDescription(CharacterCard owner)
        {
            var randomTargets = PosTargets.Where(p => p == EPositionTarget.Random).ToList();
            var normalTargets = PosTargets.Where(p => p != EPositionTarget.Random).ToList();

            string targetList = "";

            if (normalTargets.Count == 1)
            {
                targetList = normalTargets[0].ToString();
            }
            else if (normalTargets.Count > 1)
            {
                targetList = string.Join(", ", $"the {normalTargets.Take(normalTargets.Count - 1)}")
                            + $" and the {normalTargets[^1]}";
            }

            if (randomTargets.Count > 0)
            {
                string randomDesc = $"{randomTargets.Count} random target(s)";
                if (!string.IsNullOrEmpty(targetList))
                    targetList += $" and {randomDesc}";
                else
                    targetList = randomDesc;
            }

            if (string.IsNullOrEmpty(targetList))
                targetList = "unknown positions";

            return $"Fire projectile(s) to targets at {targetList}, " +
                $"dealing {DamageMult * owner.Atk} {DamageType} damage to each target.";
        }

        public override async Task DoSkill(CharacterModelView caster, CharacterModelView target)
        {
            // TODO: Spawn projectile with factory
            var projectile = UnityEngine.Object.Instantiate(Projectile, caster.transform);
            await projectile.FlyToTarget(caster.transform.position, OffsetToCaster,
                                        target.transform.position + OffsetToCaster.y * Vector3.up,
                                        () => DoDamage(caster, target.BaseCard));
        }

        private void DoDamage(CharacterModelView caster, IDamagable target)
        {
            var damage = caster.Atk * DamageMult;

            target.TakeDamage(damage, DamageType);
            // Debug.Log($"Target '{target}' took {damage} damage from Caster '{caster}'");
        }
    }
    #endregion

    #region Apply Effect
    [Serializable]
    public class ApplyEffect : SubSkill
    {
        public ESkillEffect EffectType;
        public float DamageMult;
        public int Duration;
        public EDamageType DamageType;

        public override async Task DoSkill(CharacterModelView caster, CharacterModelView target)
        {
            SkillEffect effect = EffectType switch
            {
                ESkillEffect.Poison => new PoisonEffect(caster.BaseCard, target.BaseCard, Duration, caster.Atk * DamageMult),
                ESkillEffect.Regen => new RegenEffect(caster.BaseCard, target.BaseCard, Duration, caster.Atk * DamageMult),
                _ => null,
            };

            target.BaseCard.ApplyEffect(effect);
        }

        public override string GenerateDescription(CharacterCard owner)
        {
            var randomTargets = PosTargets.Where(p => p == EPositionTarget.Random).ToList();
            var normalTargets = PosTargets.Where(p => p != EPositionTarget.Random).ToList();

            string targetList = "";

            if (normalTargets.Count == 1)
            {
                targetList = normalTargets[0].ToString();
            }
            else if (normalTargets.Count > 1)
            {
                targetList = string.Join(", ", $"the {normalTargets.Take(normalTargets.Count - 1)}")
                            + $" and the {normalTargets[^1]}";
            }

            if (randomTargets.Count > 0)
            {
                string randomDesc = $"{randomTargets.Count} random target(s)";
                if (!string.IsNullOrEmpty(targetList))
                    targetList += $" and {randomDesc}";
                else
                    targetList = randomDesc;
            }

            if (string.IsNullOrEmpty(targetList))
                targetList = "unknown positions";

            return $"Apply {EffectType} to targets at {targetList}, " +
                $"dealing {DamageMult * owner.Atk} {DamageType} damage for {Duration} turn(s).";
        }
    }
    #endregion
}