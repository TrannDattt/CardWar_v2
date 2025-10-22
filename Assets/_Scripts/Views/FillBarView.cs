using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CardWar_v2.Views
{
    public class FillBarView : MonoBehaviour
    {
        [SerializeField] private Image _bar;
        [SerializeField] private Image _barOverlay;

        private float _maxValue;

        public void SetMaxValue(float value)
        {
            _maxValue = value;
        }

        public async Task UpdateBar(float curValue)
        {
            var fillAmount = Mathf.Clamp(curValue / _maxValue, 0, 1);

            if (fillAmount >= _bar.fillAmount)
            {
                await IncreaseFill(fillAmount);
            }
            else
            {
                await DecreaseFill(fillAmount);
            }
        }

        private async Task IncreaseFill(float fillAmount)
        {
            var barFillAmount = _bar.fillAmount;
            var overlayFillAmount = _barOverlay.fillAmount;

            var sequence = DOTween.Sequence();
            sequence.Append(DOTween.To(() => overlayFillAmount, x => _barOverlay.fillAmount = x, fillAmount, .1f).SetEase(Ease.InQuart));
            sequence.AppendInterval(.5f);
            sequence.Append(DOTween.To(() => barFillAmount, x => _bar.fillAmount = x, fillAmount, .3f).SetEase(Ease.InQuad));

            await sequence.AsyncWaitForCompletion();
        }

        private async Task DecreaseFill(float fillAmount)
        {
            var barFillAmount = _bar.fillAmount;
            var overlayFillAmount = _barOverlay.fillAmount;

            var sequence = DOTween.Sequence();
            sequence.Append(DOTween.To(() => barFillAmount, x => _bar.fillAmount = x, fillAmount, .1f).SetEase(Ease.InQuart));
            sequence.AppendInterval(.5f);
            sequence.Append(DOTween.To(() => overlayFillAmount, x => _barOverlay.fillAmount = x, fillAmount, .3f).SetEase(Ease.InQuad));

            await sequence.AsyncWaitForCompletion();
        }
    }
}

