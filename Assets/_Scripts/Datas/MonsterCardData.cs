// using System;
// using CardWar.Enums;
// using UnityEngine;

// namespace CardWar.Datas
// {
//     [CreateAssetMenu(menuName = "SO/Card Data/Monster")]
//     public class MonsterCardData : CardData
//     {
//         public EMonsterTier Tier;
//         public int Hp;
//         public int Atk;
//         public SummonCondiction SummonCondiction;
//     }

//     [Serializable]
//     public class SummonCondiction
//     {
//         // Check condictions fisrt
        

//         // Then consume sacrifices
//         public Sacrifce[] Sacrifces;
//     }

//     [Serializable]
//     public class Sacrifce
//     {
//         public EMonsterTier Tier;
//         public int Amount;
//     }
// }