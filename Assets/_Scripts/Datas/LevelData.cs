using System;
using System.Collections.Generic;
using CardWar.Enums;
using CardWar_v2.Datas;
using UnityEngine;

namespace CardWar_v2.Datas
{
    [CreateAssetMenu(menuName = "SO/Level")]
    public class LevelData : ScriptableObject
    {
        public string Id => GetInstanceID().ToString();
        public int Chapter;
        public int Room;
        public List<LevelEnemy> Enemies;
        public Reward Rewards;
        public int TurnConditionCheck;
    }

    [Serializable]
    public struct LevelEnemy
    {
        public CharacterCardData Data;
        public int Level;
    }

    [Serializable]
    public struct Reward
    {
        public int Exp;
        public int Gold;
        public int Gem;
        //TODO: Add other item reward if need
    }
}