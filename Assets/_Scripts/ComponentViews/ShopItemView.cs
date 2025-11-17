using CardWar_v2.Entities;
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
        [SerializeField] private Image _goldCost;
        [SerializeField] private Image _gemCost;
        [SerializeField] private TextMeshProUGUI _textGoldCost;
        [SerializeField] private TextMeshProUGUI _textGemCost;

        public UnityEvent OnItemClicked = new();

        public ShopItem CurItem { get; private set; } 

        public void SetItem(ShopItem item)
        {
            CurItem = item;

            _icon.sprite = item.Icon;

            _goldCost.gameObject.SetActive(item.GoldCost != 0);
            _textGoldCost.SetText(item.GoldCost.ToString());

            _gemCost.gameObject.SetActive(item.GemCost != 0);
            _textGemCost.SetText(item.GemCost.ToString());
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"Item clicked");
            OnItemClicked?.Invoke();
        }
    }
}

