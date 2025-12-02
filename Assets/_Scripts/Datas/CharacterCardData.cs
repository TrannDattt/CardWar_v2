using System;
using System.Collections.Generic;
using CardWar.Enums;
using UnityEngine;
using static CardWar_v2.GameControl.GameAudioManager;

namespace CardWar_v2.Datas
{
    [CreateAssetMenu(menuName = "SO/Card Data/Character")]
    public class CharacterCardData : ScriptableObject
    {
        // Info
        [SerializeField] private string _id;
        public string Id => _id;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_id))
            {
                _id = Guid.NewGuid().ToString();
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
        }
        public string Name;
        public Sprite Image;
        public Sprite SplashArt;
        public GameObject Model;
        public bool IsPlayable;

        // Animation
        public RuntimeAnimatorController AnimController;
        
        // Audio

        [Serializable]
        public class Voice
        {
            public EVoice Key;
            public List<AudioClip> Clips;
        }
        public List<Voice> VoiceLines;

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