using System;
using System.Collections.Generic;
using UnityEngine;

namespace CardWar_v2.ComponentViews
{
    public class CharacterAudio : MonoBehaviour
    {
        public enum EVoiceLine
        {
            Idle,
            Skill1,
            Skill2,
            Skill3,
            Die,
            Upgrade,
            Win,
            Lose,
        }

        [Serializable]
        class VoiceLine
        {
            public EVoiceLine Key;
            public List<AudioClip> Clips;
        }

        [SerializeField] private AudioSource _audio;
        [SerializeField] private List<VoiceLine> _voiceLineList;
        private Dictionary<EVoiceLine, List<AudioClip>> _voiceLineDict = new();

        public void PlaySFX(EVoiceLine key)
        {
            if(!_voiceLineDict.ContainsKey(key)) return;

            var clips = _voiceLineDict[key];
            var randomClip = clips[UnityEngine.Random.Range(0, clips.Count)];
            _audio.PlayOneShot(randomClip);
        }

        void Awake()
        {
            _voiceLineList.ForEach(v => _voiceLineDict[v.Key] = v.Clips);
        }
    }
}

