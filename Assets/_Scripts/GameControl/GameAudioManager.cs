using System;
using System.Collections.Generic;
using System.Linq;
using CardWar_v2.Entities;
using CardWar_v2.Untils;
using UnityEngine;

namespace CardWar_v2.GameControl
{
    public class GameAudioManager : Singleton<GameAudioManager>
    {
        public enum EAudio
        {
            BGM,
            SFX,
            Voice,
        }

        public enum EBgm
        {
            Home,
            Ingame,
            None,
        }

        public enum ESfx
        {
            ButtonClick,
            IconClick,
            UpgradeCharacter,
            BuyItem,
            WinMatch,
            LoseMatch,
            None,
        }

        public enum EVoice
        {
            Idle,
            Skill1,
            Skill2,
            Skill3,
            Die,
            Selected,
            Upgrade,
            None,
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
        private EBgm _curBGM = EBgm.None;
        public float BgmVolumn => _bgm.volume;

        [Header("SFX")]
        [SerializeField] private AudioSource _sfx;
        [SerializeField] private List<SFX> _sfxList;
        private Dictionary<ESfx, List<AudioClip>> _sfxDict = new();
        private ESfx _curSFX = ESfx.None;
        public float SfxVolumn => _sfx.volume;

        [Header("Voice")]
        [SerializeField] private AudioSource _voice;
        private EVoice _curVoice = EVoice.None;
        private CharacterCard _curChar = null;
        public float VoiceVolumn => _voice.volume;

        #region BGM
        private void PlayBackgroundMusic(AudioClip clip)
        {
            if (clip == null) return;
            
            _bgm.clip = clip;
            _bgm.Play();
        }

        public void PlayBackgroundMusic(EBgm key, bool restart = false)
        {
            if(!_bgmDict.ContainsKey(key) || (_curBGM == key && !restart)) return;

            _curBGM = key;
            var clips = _bgmDict[key];
            var randomClip = clips[UnityEngine.Random.Range(0, clips.Count)];
            PlayBackgroundMusic(randomClip);
        }

        public void ChangeBSMVolumn(float value)
        {
            _bgm.volume = value;
        }
        #endregion

        #region SFX
        public void PlaySFX(AudioClip clip, bool isLoop)
        {
            if (clip == null) return;

            _sfx.loop = isLoop;

            if (!isLoop)
            {
                _sfx.PlayOneShot(clip);
                return;
            }

            _sfx.clip = clip;
            _sfx.Play();
        }

        public void StopSFX()
        {
            _sfx.loop = false;
            _sfx.Stop();
        }

        public void PlaySFX(ESfx key, bool isLoop = false, bool restart = false)
        {
            if(!_sfxDict.ContainsKey(key) || (_curSFX == key && !restart)) return;

            _curSFX = key;
            var clips = _sfxDict[key];
            var randomClip = clips[UnityEngine.Random.Range(0, clips.Count)];
            PlaySFX(randomClip, isLoop);
        }
        public void ChangeSFXVolumn(float value)
        {
            _sfx.volume = value;
        }
        #endregion

        #region Voice
        public void PlayVoice(AudioClip clip)
        {
            if (clip == null) return;
            
            _voice.PlayOneShot(clip);
        }

        public void PlayVoice(CharacterCard character, EVoice key, bool restart = false)
        {
            var voiceLine = character.VoiceLines.FirstOrDefault(v => v.Key == key);
            if(voiceLine == null || (_curChar == character && _curVoice == key && !restart)) return;

            _curVoice = key;
            _curChar = character;
            var randomClip = voiceLine.Clips[UnityEngine.Random.Range(0, voiceLine.Clips.Count)];
            PlayVoice(randomClip);
        }

        public void ChangeVoiceVolumn(float value)
        {
            _voice.volume = value;
        }
        #endregion

        protected override void Awake()
        {
            base.Awake();

            _bgmList.ForEach(b => _bgmDict[b.Key] = b.Clips);
            _sfxList.ForEach(s => _sfxDict[s.Key] = s.Clips);
            // _voiceList.ForEach(v => _voiceDict[v.Key] = v.Clips);
        }
    }
}