using System;
using System.Collections.Generic;
using CardWar.Untils;
using UnityEngine;

namespace CardWar_v2.GameControl
{
    public class GameAudioManager : Singleton<GameAudioManager>
    {
        public enum EBgm
        {
            Home,
            Ingame,
        }

        public enum ESfx
        {
            ButtonClick,
            IconClick,
            UpgradeCharacter,
            BuyItem,
            WinMatch,
            LoseMatch,
        }

        [Serializable]
        class BGM
        {
            public EBgm Key;
            public List<AudioClip> Clips;
        }

        [Serializable]
        class SFX
        {
            public ESfx Key;
            public List<AudioClip> Clips;
        }

        [Header("BGM")]
        [SerializeField] private AudioSource _bgm;
        [SerializeField] private List<BGM> _bgmList;
        private Dictionary<EBgm, List<AudioClip>> _bgmDict = new();

        [Header("SFX")]
        [SerializeField] private AudioSource _sfx;
        [SerializeField] private List<SFX> _sfxList;
        private Dictionary<ESfx, List<AudioClip>> _sfxDict = new();

        private void PlayBackgroundMusic(AudioClip clip)
        {
            _bgm.clip = clip;
            _bgm.Play();
        }

        public void PlayBackgroundMusic(EBgm key)
        {
            if(_bgmDict.ContainsKey(key)) return;

            var clips = _bgmDict[key];
            var randomClip = clips[UnityEngine.Random.Range(0, clips.Count)];
            PlayBackgroundMusic(randomClip);
        }

        private void PlaySFX(AudioClip clip)
        {
            _sfx.PlayOneShot(clip);
        }

        public void PlaySFX(ESfx key)
        {
            if(_sfxDict.ContainsKey(key)) return;

            var clips = _sfxDict[key];
            var randomClip = clips[UnityEngine.Random.Range(0, clips.Count)];
            PlaySFX(randomClip);
        }

        protected override void Awake()
        {
            base.Awake();

            _bgmList.ForEach(b => _bgmDict[b.Key] = b.Clips);
            _sfxList.ForEach(s => _sfxDict[s.Key] = s.Clips);
        }
    }
}