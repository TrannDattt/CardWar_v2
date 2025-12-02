using System;
using System.Threading.Tasks;
using CardWar.Interfaces;
using CardWar_v2.GameControl;
using DG.Tweening;
using UnityEngine;
using static CardWar_v2.ComponentViews.FXPlayer;

namespace CardWar_v2.ComponentViews
{
    public class ProjectileView : MonoBehaviour
    {
        [SerializeField] private GameObject _modelBase;
        [SerializeField] private float _flyDuration;
        [SerializeField] private float _waitBeforeDestroyDuration;
        [SerializeField] private FXPlayer _fxPlayer;
        [SerializeField] private AudioClip _explodeSound;

        public async Task FlyToTarget(Vector3 casterPos, Vector3 offset, Vector3 targetPos, Action callback = null)
        {
            // transform.SetPositionAndRotation(spawnPos, Quaternion.LookRotation(targetPos - spawnPos));
            transform.position = casterPos;
            transform.position += offset;
            transform.LookAt(targetPos);

            var sequence = DOTween.Sequence();
            sequence.Append(transform.DOMove(targetPos, _flyDuration).SetEase(Ease.InExpo));
            sequence.OnComplete(async () =>
            {
                GameAudioManager.Instance.PlaySFX(_explodeSound);
                if (_modelBase != null) _modelBase.SetActive(false);
                callback?.Invoke();

                await _fxPlayer.AsyncPlayFXByKey(EFXType.Explode);

                Destroy(gameObject);
            });

            await sequence.AsyncWaitForCompletion();
        }
    }
}

