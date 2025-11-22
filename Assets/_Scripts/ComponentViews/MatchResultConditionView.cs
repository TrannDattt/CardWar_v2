using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardWar_v2.ComponentViews
{
    public class MatchResultConditionView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _conditionText;
        [SerializeField] private Image _iconCheckbox;
        [SerializeField] private RectTransform _checkIconRt;
        [SerializeField] private RectTransform _checkIconMaskRt;

        [SerializeField] private Color _defaultColor;
        [SerializeField] private Color _metColor;

        private float _checkOffsetX;

        public async Task CheckCondiction(string text, bool isMet, bool doAnim)
        {
            HideCondition();
            _conditionText.SetText(text);

            if (doAnim)
            {
                var sequence = DOTween.Sequence();
                sequence.SetUpdate(true);

                sequence.Append(_canvasGroup.DOFade(1f, 0.2f).SetEase(Ease.InOutQuad));

                if (isMet)
                {
                    sequence.Append(_checkIconRt.DOAnchorPosX(0f, 0.2f).SetEase(Ease.InOutQuad));
                    sequence.Join(_checkIconMaskRt.DOAnchorPosX(0f, 0.2f).SetEase(Ease.InOutQuad));
                    sequence.Append(_conditionText.DOColor(_metColor, 0.2f).SetEase(Ease.InOutQuad));
                    sequence.Join(_iconCheckbox.DOColor(_metColor, 0.2f).SetEase(Ease.InOutQuad));
                }
                await sequence.AsyncWaitForCompletion();

                return;
            }

            _canvasGroup.alpha = 1f;
            _checkIconRt.anchoredPosition = new Vector2(0f, _checkIconRt.anchoredPosition.y);
            _checkIconMaskRt.anchoredPosition = new Vector2(0f, _checkIconMaskRt.anchoredPosition.y);
            if (!isMet) return;

            _conditionText.color = _metColor;
            _iconCheckbox.color = _metColor;
        }

        public void HideCondition()
        {
            _canvasGroup.alpha = 0f;
            _conditionText.color = _defaultColor;
            _iconCheckbox.color = _defaultColor;
            _checkIconRt.anchoredPosition = new Vector2(_checkOffsetX, _checkIconRt.anchoredPosition.y);
            _checkIconMaskRt.anchoredPosition = new Vector2(-_checkOffsetX, _checkIconMaskRt.anchoredPosition.y);
        }

        void Start()
        {
            _checkOffsetX = _checkIconRt.rect.width;

            HideCondition();
        }
    }
}

