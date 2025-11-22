using System.Threading;
using System.Threading.Tasks;
using CardWar_v2.Enums;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CardWar_v2.ComponentViews
{
    public class FillBarView : MonoBehaviour
    {
        [SerializeField] private Image _bar;
        [SerializeField] private Image _barOverlay;

        public void Initialize(EPlayerTarget side)
        {
            var barColor = side switch
            {
                EPlayerTarget.Ally => Color.green,  
                EPlayerTarget.Enemy => Color.red,  
                EPlayerTarget.Neutral => Color.white,
                _ => Color.white  
            };

            _bar.color = barColor;
            _barOverlay.color = new(barColor.r, barColor.g, barColor.b, .8f);
        }

        public void SetFillValue(float value)
        {
            _bar.fillAmount = value;
            _barOverlay.fillAmount = value;
        }

        public async Task UpdateBarByValue(float curValue, float maxValue)
        {
            var fillAmount = Mathf.Clamp(curValue / maxValue, 0, 1);
            await UpdateBarByFillAmount(fillAmount);
        }
        
        public async Task UpdateBarByFillAmount(float fillAmount)
        {
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
            if (barFillAmount == fillAmount) return;
            var overlayFillAmount = _barOverlay.fillAmount;

            var sequence = DOTween.Sequence();
            sequence.Append(DOTween.To(() => overlayFillAmount, x => _barOverlay.fillAmount = x, fillAmount, .1f).SetEase(Ease.InQuart));
            sequence.AppendInterval(.2f);
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

