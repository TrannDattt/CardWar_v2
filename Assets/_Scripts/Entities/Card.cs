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
        public ECardType CardType { get; protected set; }
        public string Name { get; protected set; }
        public Sprite Image { get; protected set; }
        public GameObject Model { get; protected set; }
        // public CardSkill[] Skills;
        public ETerrain TerrainType { get; protected set; }
        public ETerrain[] VariantsType { get; protected set; }
        
        public Dictionary<ETerrain, CardData> _variantDict = new();
        public Dictionary<ETerrain, CardData> VariantDict
        {
            get => _variantDict;
        }

        public UnityEvent OnCardUpdated = new();

        public Card(CardData data = null)
        {
            if (data == null) return;

            CardType = data.CardType;
            Name = data.Name;
            Image = data.Image;
            Model = data.Model;
            TerrainType = data.TerrainType;
            VariantsType = data.VariantsType;
        }

        private CardData[] GetVariants(ETerrain[] keys)
        {
            // TODO: Get all variant in card dict from keys
            return null;
        }

        private void UpdateDict()
        {
            var keys = VariantsType;
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

            var newData = _variantDict[key];
            Name = newData.Name;
            Image = newData.Image;
            Model = newData.Model;
            TerrainType = newData.TerrainType;
            VariantsType = newData.VariantsType;

            UpdateDict();
            //////////////////

            OnCardUpdated?.Invoke();
        }
    }

    public class MonsterCard : Card, IDamagable
    {
        public int Atk { get; private set; }
        public int Hp { get; private set; }

        public UnityEvent OnTakenDamaged { get; set; } = new();


        public MonsterCard(CardData data) : base(data)
        {
            Atk = (data as MonsterCardData).Atk;
            Hp = (data as MonsterCardData).Hp;
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
        public SpellCard(CardData data) : base(data)
        {
        }
    }

    public class TerrainCard : Card
    {
        public TerrainCard(CardData data) : base(data)
        {
        }
    }
}

