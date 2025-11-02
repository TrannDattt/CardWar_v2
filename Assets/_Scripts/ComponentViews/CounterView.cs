// using CardWar.Factories;
using DG.Tweening;
using TMPro;
using UnityEngine;

// using CardWar.Views;

namespace CardWar_v2.ComponentViews
{
    public class CounterView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _countText;

        private int _curValue = -1;

        public void SetCount(int newValue)
        {
            if (newValue == _curValue) return;

            DOTween.To(() => _curValue, x => 
            {
                _countText.SetText(x.ToString());
            }, newValue, 0.5f)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                _curValue = newValue;
                _countText.SetText(_curValue.ToString());
            });
        }
    }
}