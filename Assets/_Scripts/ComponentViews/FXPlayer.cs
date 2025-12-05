using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CardWar_v2.GameControl;
using Unity.Mathematics;
using UnityEngine;

namespace CardWar_v2.ComponentViews
{
    public class FXPlayer : MonoBehaviour
    {
        public enum EFXType
        {
            None,
            Explode,
            Attack,
            Hit,
            Effect,
            StatBuff,
        }

        [Serializable]
        public class FX
        {
            public EFXType Type;
            public ParticleSystem ParticleSystem;
            public AudioClip SFX;
            public bool LoopFX;
        }

        [SerializeField] private List<FX> _fxs;
        [SerializeField] private Transform _spawnParent;

        private ParticleSystem _curFX;

        private Dictionary<EFXType, FX> _fxDict = new();

        public async void PlayFXByKey(EFXType key)
        {
            if(!_fxDict.ContainsKey(key) || _fxDict[key] == null) return;

            await PlayFX(_fxDict[key]);
        }

        public async Task AsyncPlayFXByKey(EFXType key)
        {
            if(!_fxDict.ContainsKey(key) || _fxDict[key] == null) return;

            await PlayFX(_fxDict[key]);
        }

        public async void PlayFXByIndex(int index)
        {
            if(index < 0 || index >= _fxs.Count) return;

            await PlayFX(_fxs[index]);
        }

        public async Task PlayFX(FX fxRef)
        {
            _curFX = fxRef.ParticleSystem;
            if (_curFX == null) return;

            var (spawnPos, parent) = _spawnParent == null ? (transform.position, transform) : (_spawnParent.position, _spawnParent);
            _curFX.transform.SetParent(null);
            _curFX.transform.position = spawnPos;
            _curFX.gameObject.SetActive(true);
            _curFX.Play();

            GameAudioManager.Instance.PlaySFX(fxRef.SFX, fxRef.LoopFX);
            await Task.Delay((int)(_curFX.main.duration * 1000));
            if (fxRef.LoopFX) GameAudioManager.Instance.StopSFX();

            if (_spawnParent == null) 
            {
                Destroy(_curFX.gameObject);
                return;
            }

            _curFX.gameObject.SetActive(false);
            _curFX.transform.SetParent(parent);
        }

        public void StopCurrentFX()
        {
            if (_spawnParent == null) Destroy(_curFX.gameObject);
            else 
            {
                _curFX.Stop();
                _curFX.gameObject.SetActive(false);
            }
        }

        void Awake()
        {
            _fxs.ForEach(fx => 
            {
                _fxDict[fx.Type] = fx;
                fx.ParticleSystem.gameObject.SetActive(false);
            });
        }
    }
}

