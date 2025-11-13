using System;
using CardWar_v2.Entities;
using CardWar_v2.GameControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardWar_v2.ComponentViews
{
    public class ShopPurchaseConfirmationView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _itemIcon;

        [SerializeField] private Button _increaseBtn;
        [SerializeField] private Button _decreaseBtn;
        [SerializeField] private TMP_InputField _quantity;
        private int Quantity => int.Parse(_quantity.text);

        [SerializeField] private TextMeshProUGUI _goldCost;
        [SerializeField] private TextMeshProUGUI _gemCost;

        private ShopItem _curItem;
        private Player CurPlayer => PlayerSessionManager.Instance.CurPlayer;

        public void OpenPopup(ShopItem item)
        {
            if (item == null) return;
            _canvasGroup.alpha = 1;
            _canvasGroup.blocksRaycasts = true;

            _curItem = item;
            _itemIcon.sprite = item.Icon;
            ChangeQuantity(1);
        }

        public void ClosePopup()
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.blocksRaycasts = false;
        }

        public void BuyItem()
        {
            if (_curItem == null) return;
            var goldCost = _curItem.GoldCost * Quantity;
            var gemCost = _curItem.GemCost * Quantity;
            if (CurPlayer.Gold < goldCost || CurPlayer.Gem < gemCost)
            {
                Debug.Log("Not enough resources to buy item");
                return;
            }
            _curItem.BuyItem(Quantity);
            CurPlayer.UpdatePlayerCurrency(-goldCost, -gemCost);
            if (_curItem.StockAmount == 0) ClosePopup();
        }

        private void ChangeQuantity(int newQuantity)
        {
            newQuantity = Mathf.Clamp(newQuantity, 0, _curItem.StockAmount);
            _quantity.SetTextWithoutNotify(newQuantity.ToString());

            _increaseBtn.interactable = newQuantity == _curItem.StockAmount;
            _decreaseBtn.interactable = newQuantity == 0;

            _goldCost.SetText((_curItem.GoldCost * Quantity).ToString());
            _gemCost.SetText((_curItem.GemCost * Quantity).ToString());
        }

        void Start()
        {
            _increaseBtn.onClick.AddListener(() =>
            {
                if (_curItem == null) return;
                ChangeQuantity(Quantity + 1);
            });

            _decreaseBtn.onClick.AddListener(() =>
            {
                if (_curItem == null) return;
                ChangeQuantity(Quantity - 1);
            });

            _quantity.onValueChanged.AddListener((v) =>
            {
                if (_curItem == null) return;
                var newQuantity = int.Parse(v);
                ChangeQuantity(newQuantity);
            });
        }
    }
}

