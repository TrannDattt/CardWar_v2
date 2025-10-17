using System;
using System.Collections.Generic;
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

    [Serializable]
    public abstract class SubSkill
    {
        public ESkillType Type;
        public AnimationClip Clip;
        public float DelayToSkill;
        public List<EPositionTarget> PosTargets;

        public abstract Task DoSkill(CharacterModelView caster, CharacterModelView target);
    }

    [Serializable]
    public class DoRangeAttack : SubSkill
    {
        public float DamageMult;
        public ProjectileView Projectile;
        public Vector3 OffsetToCaster;
        public EDamageType DamageType;

        public override async Task DoSkill(CharacterModelView caster, CharacterModelView target)
        {
            // TODO: Spawn projectile with factory
            var projectile = UnityEngine.Object.Instantiate(Projectile, caster.transform);
            await projectile.FlyToTarget(caster.transform.position + OffsetToCaster,
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
}