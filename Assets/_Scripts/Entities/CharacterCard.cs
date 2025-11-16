using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardWar.Enums;
using CardWar.Interfaces;
using CardWar_v2.Datas;
using CardWar_v2.Enums;
using CardWar_v2.GameControl;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Events;

namespace CardWar_v2.Entities
{
    [Serializable]
    public struct CharStat
    {
        public float Hp;
        public float Atk;
        public float Armor;
        public float Resist;

        public readonly float Total => Hp + Atk + Armor + Resist;

        public CharStat(float hp, float atk, float armor, float resist)
        {
            Hp = hp;
            Atk = atk;
            Armor = armor;
            Resist = resist;
        }

        public static CharStat operator +(CharStat a, CharStat b)
        {
            if (a.Equals(null) || b.Equals(null))
                throw new ArgumentNullException("Cannot multiply null CharStat");

            return new CharStat(
                a.Hp + b.Hp,
                a.Atk + b.Atk,
                a.Armor + b.Armor,
                a.Resist + b.Resist
            );
        }

        public static CharStat operator *(CharStat a, CharStat b)
        {
            if (a.Equals(null) || b.Equals(null))
                throw new ArgumentNullException("Cannot multiply null CharStat");

            return new CharStat(
                a.Hp * b.Hp,
                a.Atk * b.Atk,
                a.Armor * b.Armor,
                a.Resist * b.Resist
            );
        }
    }

    public class CharacterCard : IDamagable
    {
        public CharacterCardData Data { get; private set; }

        public ECharacter Character => Data.Character;
        public string Name => Data.name;
        public Sprite Image => Data.Image;
        public AnimatorController AnimController => Data.AnimController;
        public GameObject Model => Data.Model;

        public bool IsUnlocked { get; private set; }
        public int Level { get; private set; }

        public float CurHp => Data.Hp + Data.HpPerLevel * Level + _bonusHp;
        private float _bonusHp;

        public float CurAtk => Data.Atk + Data.AtkPerLevel * Level + _bonusAtk;
        private float _bonusAtk;

        public float CurArmor => Data.Armor + Data.ArmorPerLevel * Level + _bonusArmor;
        private float _bonusArmor;

        public float CurResist => Data.Resist + Data.ResistPerLevel * Level + _bonusResist;
        private float _bonusResist;

        private bool _isDeath => CurHp <= 0;

        public List<SkillCard> SkillCards;
        public Dictionary<ESkillEffect, SkillEffect> ActiveEffects;

        public UnityEvent OnCardLevelUp = new();
        public UnityEvent OnCardUnlock = new();
        public UnityEvent<SubSkill> OnUseSkill { get; set; } = new();
        public UnityEvent<SkillEffect> OnApplyEffect { get; set; } = new();
        public UnityEvent OnChangeHp { get; set; } = new();
        public UnityEvent OnDeath { get; set; } = new();

        public CharacterCard(CharacterCardData data, int level = 1, bool isUnlock = false)
        {
            Data = data;
            IsUnlocked = isUnlock;

            if (!isUnlock)
            {
                Level = 1;
            }
            else
            {
                Level = level;
            }

            _bonusHp = 0;
            _bonusAtk = 0;
            _bonusArmor = 0;
            _bonusResist = 0;
            
            if(data == null) return;

            SkillCards = new();
            ActiveEffects = new();
            Data.SkillCardDatas?.ForEach(d => SkillCards.Add(new(d, this)));
        }

        public void LevelUp()
        {
            Level += 1;

            OnCardLevelUp?.Invoke();
        }

        public void UnlockCard()
        {
            IsUnlocked = true;

            OnCardUnlock?.Invoke();
        }

        public CharStat GetCurStat() => new(CurHp, CurAtk, CurArmor, CurResist);

        public CharStat GetStatAtLevel(int level) => new(Data.Hp + Data.HpPerLevel * level,
                                                         Data.Atk + Data.AtkPerLevel * level,
                                                         Data.Armor + Data.ArmorPerLevel * level,
                                                         Data.Resist + Data.ResistPerLevel * level);

        public void Die()
        {
            OnDeath?.Invoke();

            OnCardLevelUp.RemoveAllListeners();
            OnUseSkill.RemoveAllListeners();
            OnChangeHp.RemoveAllListeners();
            OnDeath.RemoveAllListeners();
        }

        public void TakeDamage(float amount, EDamageType type)
        {
            if (_isDeath) return;

            var dmgReduce = type switch
            {
                EDamageType.Physical => CurArmor,
                EDamageType.Magical => CurResist,
                EDamageType.Pure => 0,
                _ => throw new Exception("Invalid damage type")
            };
            
            float vulAmount = 0;
            foreach (var e in ActiveEffects)
            {
                if (e.Key == ESkillEffect.Vulnerable)
                    vulAmount += (e.Value as VulnerableEffect).VulAmount;
                else if (e.Key == ESkillEffect.Strengthen)
                    vulAmount -= (e.Value as StrengthenEffect).StrengthAmount;
            }
            
            ChangeHp((-amount + dmgReduce) * Mathf.Max(0, 1 + vulAmount));
        }

        public void ApplyEffect(SkillEffect effect)
        {
            if (_isDeath) return;

            Debug.Log(effect.EffectType);
            Debug.Log($"Applying {effect.EffectType} to {Name}");
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
            if (_isDeath || ActiveEffects.Count == 0) return;

            var activeEffects = new List<SkillEffect>(ActiveEffects.Values.ToList());
            activeEffects.Reverse();
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

        public void ChangeAtk(float amount)
        {
            _bonusAtk += amount;
        }

        public void ChangeAmr(float amount)
        {
            _bonusArmor += amount;
        }

        public void ChangeRes(float amount)
        {
            _bonusResist += amount;
        }

        public void ChangeStat(CharStat delta)
        {
            ChangeHp(delta.Hp);
            ChangeAtk(delta.Atk);
            ChangeAmr(delta.Armor);
            ChangeRes(delta.Resist);
        }
    }
}

