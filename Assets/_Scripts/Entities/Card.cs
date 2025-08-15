using System.Collections.Generic;
using CardWar.Datas;
using CardWar.Enums;
using CardWar.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace CardWar.Entities
{
    public class Card : IHaveVariant<CardData>
    {
        public CardData Data { get; private set; }
        public Dictionary<ETerrain, CardData> _variantDict = new();
        
        public Dictionary<ETerrain, CardData> VariantDict
        {
            get => _variantDict;
        }

        public UnityEvent OnCardUpdated = new();

        public Card(CardData data = null)
        {
            Data = data;
        }

        private CardData[] GetVariants(ETerrain[] keys)
        {
            // TODO: Get all variant in card dict from keys
            return null;
        }

        private void UpdateDict()
        {
            var keys = Data.VariantsType;
            var values = GetVariants(keys);

            _variantDict.Clear();
            for (int i = 0; i < keys.Length; i++)
            {
                _variantDict.Add(keys[i], values[i]);
            }
        }

        public void ChangeVariant(ETerrain key)
        {
            if (!_variantDict.ContainsKey(key)) return;

            Data = _variantDict[key];
            UpdateDict();
            //////////////////

            OnCardUpdated?.Invoke();
        }
    }

    public class MonsterCard : Card, IDamagable
    {
        public MonsterCardData MonsterData => Data as MonsterCardData;

        public UnityEvent OnTakenDamaged { get; set; } = new();

        public int Atk;
        public int Hp;

        public MonsterCard(CardData data) : base(data)
        {
            Atk = MonsterData.Atk;
            Hp = MonsterData.Hp;
        }

        public void TakeDamage(int amount)
        {
            Hp = Mathf.Clamp(Hp - amount, 0, int.MaxValue);
            //TODO: Do thing if Hp falls to 0

            OnTakenDamaged?.Invoke();
        }
    }

    public class SpellCard : Card
    {
        public SpellCardData SpellData => Data as SpellCardData;

        public SpellCard(CardData data) : base(data)
        {
        }
    }

    public class TerrainCard : Card
    {
        public TerrainCardData TerrainData => Data as TerrainCardData;

        public TerrainCard(CardData data) : base(data)
        {
        }
    }
}

