using System;
using System.Threading.Tasks;
using CardWar.Interfaces;
using DG.Tweening;
using UnityEngine;

namespace CardWar_v2.ComponentViews
{
    public class ProjectileView : MonoBehaviour
    {
        [SerializeField] private GameObject _modelBase;
        [SerializeField] private float _flyDuration;

        public async Task FlyToTarget(Vector3 casterPos, Vector3 offset, Vector3 targetPos, Action callback = null)
        {
            // transform.SetPositionAndRotation(spawnPos, Quaternion.LookRotation(targetPos - spawnPos));
            transform.position = casterPos;
            transform.localPosition += offset;
            transform.LookAt(targetPos);

            var sequence = DOTween.Sequence();
            sequence.Append(transform.DOMove(targetPos, _flyDuration).SetEase(Ease.InExpo));
            sequence.OnComplete(() =>
            {
                callback?.Invoke();
                Destroy(gameObject);
            });

            await sequence.AsyncWaitForCompletion();
        }
    }
}

