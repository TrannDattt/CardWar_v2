using System;
using CardWar.Enums;
using UnityEngine;

namespace CardWar.Datas
{
    public abstract class CardData : ScriptableObject
    {
        public string Id { get; private set; } = Guid.NewGuid().ToString();
        public string Name;
        public Sprite Image;
        public Mesh Model;
        // public CardSkill[] Skills;
        public ETerrain TerrainType;
        public ETerrain[] VariantsType;
    }
}