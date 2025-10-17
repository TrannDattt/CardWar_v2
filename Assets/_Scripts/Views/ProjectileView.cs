using System;
using System.Threading.Tasks;
using CardWar.Interfaces;
using DG.Tweening;
using UnityEngine;

namespace CardWar_v2.Views
{
    public class ProjectileView : MonoBehaviour
    {
        [SerializeField] private GameObject _modelBase;
        [SerializeField] private float _flyDuration;

        public async Task FlyToTarget(Vector3 spawnPos, Vector3 targetPos, Action callback = null)
        {
            transform.SetPositionAndRotation(spawnPos, Quaternion.LookRotation(targetPos - spawnPos));

            var sequence = DOTween.Sequence();
            sequence.Append(transform.DOMove(targetPos, _flyDuration).SetEase(Ease.InOutQuad));
            sequence.OnComplete(() =>
            {
                callback?.Invoke();
                //TODO: Return project to pool
                Destroy(gameObject);
            });

            await sequence.AsyncWaitForCompletion();
        }
    }
}

