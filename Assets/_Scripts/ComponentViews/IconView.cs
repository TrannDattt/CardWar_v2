using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CardWar_v2.ComponentViews
{
    public class IconView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] protected Image _icon;
        [SerializeField] protected TextMeshProUGUI _amount;

        public UnityEvent OnIconClicked = new();
        public UnityEvent OnPointerEnterIcon = new();
        public UnityEvent OnPointerExitIcon = new();

        public void SetIcon(Sprite icon, int amount = 1)
        {
            _icon.sprite = icon;
            _amount.gameObject.SetActive(amount > 1);
            _amount.SetText($"x {amount}");
            
            OnIconClicked.RemoveAllListeners();
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

