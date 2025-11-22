using System;
using System.Collections.Generic;
using CardWar.Enums;
using UnityEditor.Animations;
using UnityEngine;

namespace CardWar_v2.Datas
{
    [CreateAssetMenu(menuName = "SO/Card Data/Character")]
    public class CharacterCardData : ScriptableObject
    {
        // Info
        public string Id => GetInstanceID().ToString();
        public ECharacter Character;
        public string Name;
        public Sprite Image;
        public Sprite SplashArt;
        public GameObject Model;
        public bool IsPlayable;

        // Animation
        public AnimatorController AnimController;
        
        // Audio

        // Stat
        public float Hp;
        public float Atk;
        public float Armor;
        public float Resist;

        // Level Scale
        public float HpPerLevel;
        public float AtkPerLevel;
        public float ArmorPerLevel;
        public float ResistPerLevel;

        // Cards
        public List<SkillCardData> SkillCardDatas;
    }
}