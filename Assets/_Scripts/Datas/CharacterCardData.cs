using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace CardWar_v2.Datas
{
    [CreateAssetMenu(menuName = "SO/Card Data/Character")]
    public class CharacterCardData : ScriptableObject
    {
        // Info
        public string Name;
        public Sprite Image;
        public GameObject Model;

        // Animation
        public AnimatorController AnimController;
        
        // Audio

        // Stat
        public float Hp;
        public float Atk;
        public float Armor;
        public float Resist;

        // Cards
        public List<SkillCardData> SkillCardDatas;
    }
}