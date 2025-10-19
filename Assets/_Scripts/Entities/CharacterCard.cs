using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardWar.Interfaces;
using CardWar_v2.Datas;
using CardWar_v2.Enums;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Events;

namespace CardWar_v2.Entities
{
    public class CharacterCard : IDamagable
    {
        private CharacterCardData _data;

        public string Name => _data.name;
        public Sprite Image => _data.Image;
        public AnimatorController AnimController => _data.AnimController;
        public GameObject Model => _data.Model;

        public float Hp => _data.Hp + _bonusHp;
        private float _bonusHp;

        public float Atk => _data.Atk + _bonusAtk;
        private float _bonusAtk;

        public float Armor => _data.Armor + _bonusArmor;
        private float _bonusArmor;

        public float Resist => _data.Resist + _bonusResist;
        private float _bonusResist;

        public List<SkillCard> SkillCards;
        public Dictionary<ESkillEffect, SkillEffect> ActiveEffects;

        public UnityEvent OnCardUpdated = new();

        public UnityEvent<SubSkill> OnUseSkill { get; set; } = new();
        public UnityEvent<SkillEffect> OnApplyEffect { get; set; } = new();
        public UnityEvent OnChangeHp { get; set; } = new();
        public UnityEvent OnDeath { get; set; } = new();

        public CharacterCard(CharacterCardData data)
        {
            _data = data;

            _bonusHp = 0;
            _bonusAtk = 0;
            _bonusArmor = 0;
            _bonusResist = 0;

            SkillCards = new();
            ActiveEffects = new();
            _data.SkillCardDatas.ForEach(d => SkillCards.Add(new(d, this)));
        }

        public void Die()
        {
            OnDeath?.Invoke();

            OnCardUpdated.RemoveAllListeners();
            OnUseSkill.RemoveAllListeners();
            OnChangeHp.RemoveAllListeners();
            OnDeath.RemoveAllListeners();
        }

        public void TakeDamage(float amount, EDamageType type)
        {
            var dmgReduce = type == EDamageType.Physical ? Armor : Resist;
            ChangeHp(-amount + dmgReduce);
        }

        public void ApplyEffect(SkillEffect effect)
        {
            Debug.Log($"Applying {effect.GetType().Name} to {Name}");
            if (ActiveEffects.ContainsKey(effect.EffectType))
            {
                ActiveEffects[effect.EffectType].OverrideEffect(effect);
            }
            else
            {
                ActiveEffects.Add(effect.EffectType, effect);
                OnApplyEffect?.Invoke(effect);
            }
            
            ActiveEffects[effect.EffectType].ApplyEffect();
        }

        public async Task DoEffects()
        {
            if (ActiveEffects.Count == 0) return;

            var activeEffects = new List<SkillEffect>(ActiveEffects.Values.ToList());
            foreach (var effect in activeEffects)
            {
                if (effect.Duration <= 0)
                {
                    RemoveEffect(effect.EffectType);
                    continue;
                }

                await effect.DoEffect();
            }
        }

        public void RemoveEffect(ESkillEffect effectType)
        {
            if (ActiveEffects.ContainsKey(effectType))
            {
                ActiveEffects[effectType].RemoveEffect();
                ActiveEffects.Remove(effectType);
            }
        }

        public void ChangeHp(float amount)
        {
            _bonusHp += amount;
            OnChangeHp?.Invoke();
        }
    }
}

