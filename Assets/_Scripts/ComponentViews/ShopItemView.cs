using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CardWar_v2.ComponentViews
{
    public class ShopItemView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _goldCost;
        [SerializeField] private TextMeshProUGUI _gemCost;

        public UnityEvent OnItemClicked = new();

        public void OnPointerClick(PointerEventData eventData)
        {
            OnItemClicked?.Invoke();
        }
    }
}

