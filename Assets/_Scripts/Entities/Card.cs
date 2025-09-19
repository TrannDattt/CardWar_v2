using System.Collections.Generic;
using System.Linq;
using CardWar.Datas;
using CardWar.Enums;
using CardWar.Interfaces;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Events;

namespace CardWar.Entities
{
    public abstract class Card
    {
        protected CardData _data;
        public ECardType CardType => _data.CardType;
        public string Name => _data.Name;
        public Sprite Image => _data.Image;
        public Mesh Mesh => _data.Mesh;
        public AnimatorController AnimController => _data.AnimController;
        public List<CardVariant> Variants => _data.Variants;
        private CardVariant _curVariant;
        public ETerrain TerrainType => _curVariant.TerrainType;
        public Material Material => _curVariant.Material;
        // public CardSkill[] Skills;
        public ETerrain[] RelativeTerrains => _curVariant.RelativeTerrains;

        private Dictionary<ETerrain, CardVariant> _variantDict = new();

        public UnityEvent OnCardUpdated = new();

        public Card(CardData data = null)
        {
            if (data == null) return;

            _data = data;
            Variants.ForEach(v => _variantDict.Add(v.TerrainType, v));
            _curVariant = _variantDict[_data.DefaultTerrain];
        }

        public void ChangeVariant(ETerrain key)
        {
            var nextVariant = _variantDict.ContainsKey(key) ? _variantDict[key] : _variantDict[_data.DefaultTerrain];

            _curVariant = nextVariant;
            OnCardUpdated?.Invoke();
        }
    }

    public class MonsterCard : Card, IDamagable
    {
        private MonsterCardData _mData => _data as MonsterCardData;
        public int Atk => _mData.Atk + _bonusAtk;
        public int Hp => _mData.Hp + _bonusHp;
        public EMonsterTier Tier => _mData.Tier;
        public SummonCondiction SummonCondiction => _mData.SummonCondiction;

        private int _bonusAtk;
        private int _bonusHp;

        public UnityEvent OnTakenDamage { get; set; } = new();


        public MonsterCard(CardData data) : base(data)
        {
            _bonusAtk = 0;
            _bonusHp = 0;
        }

        public void TakeDamage(int amount)
        {
            _bonusHp = Mathf.Clamp(_bonusHp - amount, -_mData.Hp, int.MaxValue);
            //TODO: Do thing if Hp falls to 0

            OnTakenDamage?.Invoke();
        }
    }

    public class SpellCard : Card
    {
        private SpellCardData _sData => _data as SpellCardData;

        public SpellCard(CardData data) : base(data)
        {
        }
    }

    public class ConstructCard : Card, IDamagable
    {
        private ConstructCardData _cData => _data as ConstructCardData;
        public int Hp => _cData.Hp + _bonusHp;

        private int _bonusHp;

        public UnityEvent OnTakenDamage { get; set; } = new();

        public ConstructCard(CardData data) : base(data)
        {
            _bonusHp = 0;
        }

        public void TakeDamage(int amount)
        {
            _bonusHp = Mathf.Clamp(_bonusHp - amount, -_cData.Hp, int.MaxValue);
            //TODO: Do thing if Hp falls to 0

            OnTakenDamage?.Invoke();
        }
    }
}

