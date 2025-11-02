// using System;
// using System.Collections.Generic;
// using CardWar.Enums;
// using UnityEditor.Animations;
// using UnityEngine;

// namespace CardWar.Datas
// {
//     public abstract class CardData : ScriptableObject
//     {
//         public string Id { get; private set; } = Guid.NewGuid().ToString();
//         public ECardType CardType
//         {
//             get
//             {
//                 if (this is MonsterCardData) return ECardType.Monster;
//                 if (this is SpellCardData) return ECardType.Spell;
//                 if (this is ConstructCardData) return ECardType.Construct;

//                 throw new NotImplementedException($"Unknown CardData type: {GetType().Name}");
//             }
//         }
//         public string Name;
//         public Sprite Image;
//         public Mesh Mesh;
//         public AnimatorController AnimController;
//         public ETerrain DefaultTerrain = ETerrain.Normal;
//         public List<CardVariant> Variants;
//     }

//     [Serializable]
//     public class CardVariant
//     {
//         public ETerrain TerrainType;
//         public Material Material;
//         // public CardSkill[] Skills;
//         public ETerrain[] RelativeTerrains;
//     }
// }