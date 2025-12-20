using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CardWar_v2.Entities;
using CardWar_v2.Factories;
using CardWar_v2.GameControl;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CardWar_v2.ComponentViews
{
    public class SkillCardView : MonoBehaviour, IPointerClickHandler
    {
        public SkillCard BaseCard { get; private set; }

        [SerializeField] private Canvas _canvas;
        [SerializeField] private Image _image;

        public UnityEvent<PointerEventData> OnCardClick = new();

        private List<Renderer> _rends = new();
        private HashSet<Material> _mats = new();

        public void SetBaseCard(SkillCard card)
        {
            BaseCard = card;

            _rends.AddRange(GetComponentsInChildren<MeshRenderer>());
            foreach (var r in _rends)
            {
                foreach (var m in r.materials)
                {
                    _mats.Add(m);
                }
            }

            var graphics = GetComponentsInChildren<Graphic>(true);
            foreach (var g in graphics)
            {
                var instancedMat = new Material(g.material);
                g.material = instancedMat;

                _mats.Add(instancedMat);
                // if (instancedMat.Equals(_imageMat))
                // {
                //     _imageMat.SetTexture("_BaseColorMap", BaseCard.Image.texture);
                //     Debug.Log("Change image");
                // }
            }

            _image.material.SetTexture("_BaseColorMap", BaseCard.Image != null ? BaseCard.Image.texture : null);
        }

        private void SetDissolve(float value)
        {
            foreach (var m in _mats)
                m.SetFloat("_Dissolve", value);
        }

        public IEnumerator DestroySkill(float duration)
        {
            var sequence = DOTween.Sequence();
            sequence.Append(transform.DORotate(new Vector3(0, 0, -30), .3f).SetEase(Ease.InOutQuad));
            sequence.Join(DOTween.To(() => 0f, x => SetDissolve(x), 1f, duration).SetEase(Ease.InOutQuad));
            sequence.OnComplete(() =>
            {
                SetDissolve(1f);
                CardFactory.Instance.RecycleCardView(this);
            });

            yield return sequence.AsyncWaitForCompletion();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            GameAudioManager.Instance.PlaySFX(GameAudioManager.ESfx.IconClick, restart: true);
            OnCardClick?.Invoke(eventData);
        }
    }
}

