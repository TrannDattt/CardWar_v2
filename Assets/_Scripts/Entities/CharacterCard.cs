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
    public class CharacterCard : IDamagable
    {
        public class CharStat
        {
            public float Hp;
            public float Atk;
            public float Armor;
            public float Resist;

            public CharStat(float hp, float atk, float armor, float resist)
            {
                Hp = hp;
                Atk = atk;
                Armor = armor;
                Resist = resist;
            }
        }

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

        public int GoldCost => Data.GoldCost;
        public int GemCost => Data.GemCost;

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

            SkillCards = new();
            ActiveEffects = new();
            Data.SkillCardDatas.ForEach(d => SkillCards.Add(new(d, this)));
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
            var dmgReduce = type == EDamageType.Physical ? CurArmor : CurResist;
            float vulAmount = 0;
            foreach(var e in ActiveEffects)
            {
                if (e.Key != ESkillEffect.Vulnerable) continue;
                vulAmount += (e.Value as VulnerableEffect).GetCurAmount();
            }
            
            ChangeHp((-amount + dmgReduce) * vulAmount);
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

