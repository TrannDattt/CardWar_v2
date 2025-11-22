using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CardWar_v2.ComponentViews
{
    public class IconView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] protected CanvasGroup _canvasGroup;
        [SerializeField] protected Image _icon;
        [SerializeField] protected Sprite _defaultIcon;
        [SerializeField] protected TextMeshProUGUI _amount;

        public UnityEvent OnIconClicked = new();
        public UnityEvent OnPointerEnterIcon = new();
        public UnityEvent OnPointerExitIcon = new();

        public void SetIcon(Sprite icon, int amount = 1)
        {
            icon = icon == null ? _defaultIcon : icon;
            _icon.sprite = icon;
            _amount.gameObject.SetActive(amount > 1);
            _amount.SetText($"x {amount}");   
        }

        public void RecycleIcon()
        {
            OnIconClicked.RemoveAllListeners();
        }

        public async Task SetIconAlpha(float alpha, float transitionTime = 0f)
        {
            await _canvasGroup.DOFade(alpha, transitionTime).SetEase(Ease.InOutQuad).SetUpdate(true).AsyncWaitForCompletion();
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            OnIconClicked?.Invoke();
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            OnPointerEnterIcon?.Invoke();
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            OnPointerExitIcon?.Invoke();
        }
    }
}

