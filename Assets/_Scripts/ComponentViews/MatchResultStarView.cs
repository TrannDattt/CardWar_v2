using System;
using System.Collections;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CardWar_v2.ComponentViews
{
    public class MatchResultStarView : MonoBehaviour
    {
        [Serializable]
        struct Transform
        {
            public Vector2 Position;
            public Vector3 Scale;
        }

        [SerializeField] private Image _starImage;
        [SerializeField] private Transform _defaultTransform;
        [SerializeField] private Transform _disabledTransform;

        private RectTransform _imageRt => _starImage.rectTransform;

        public IEnumerator ShowStarResult(bool isEnable, bool doAnim)
        {
            _imageRt.anchoredPosition = _disabledTransform.Position;
            _imageRt.localScale = _disabledTransform.Scale;
            _starImage.color = Color.clear;

            if (!isEnable) yield break;

            if (doAnim)
            {
                var sequence = DOTween.Sequence();
                sequence.SetUpdate(true);
                
                sequence.Append(_imageRt.DOAnchorPos(_defaultTransform.Position, 0.3f).SetEase(Ease.InOutQuad));
                sequence.Join(_imageRt.DOScale(_defaultTransform.Scale, 0.5f).SetEase(Ease.InOutQuad));
                sequence.Join(_starImage.DOColor(Color.white, 0.5f).SetEase(Ease.InOutQuad));
                yield return sequence.WaitForCompletion();

                yield break;
            }

            _imageRt.anchoredPosition = _defaultTransform.Position;
            _imageRt.localScale = _defaultTransform.Scale;
            _starImage.color = Color.white;
        }

        void Start()
        {
            // _imageRt.anchoredPosition = _disabledTransform.Position;
            // _imageRt.localScale = _disabledTransform.Scale;
            // _starImage.color = Color.clear;
        }
    }
}

