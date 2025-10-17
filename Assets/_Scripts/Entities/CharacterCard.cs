using System.Collections.Generic;
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

        public UnityEvent OnCardUpdated = new();

        public UnityEvent<SubSkill> OnUseSkill { get; set; } = new();
        public UnityEvent OnTakenDamage { get; set; } = new();
        public UnityEvent OnDeath { get; set; } = new();

        public CharacterCard(CharacterCardData data)
        {
            _data = data;

            _bonusHp = 0;
            _bonusAtk = 0;
            _bonusArmor = 0;
            _bonusResist = 0;

            SkillCards = new();
            _data.SkillCardDatas.ForEach(d => SkillCards.Add(new(d, this)));
        }

        public void Die()
        {
            OnDeath?.Invoke();

            OnCardUpdated.RemoveAllListeners();
            OnUseSkill.RemoveAllListeners();
            OnTakenDamage.RemoveAllListeners();
            OnDeath.RemoveAllListeners();
        }

        public void TakeDamage(float amount, EDamageType type)
        {
            var dmgReduce = type == EDamageType.Physical ? Armor : Resist;
            _bonusHp -= amount - dmgReduce;
            OnTakenDamage?.Invoke();
        }
    }
}

