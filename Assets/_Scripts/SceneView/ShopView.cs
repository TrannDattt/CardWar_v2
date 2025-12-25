// using CardWar.Factories;
using System.Collections.Generic;
using System.Linq;
using CardWar_v2.ComponentViews;
using CardWar_v2.Entities;
using CardWar_v2.Factories;
using CardWar_v2.GameControl;
using UnityEngine;

// using CardWar.Views;

namespace CardWar_v2.SceneViews
{
    public class ShopView : MonoBehaviour
    {
        [SerializeField] private RectTransform _container;
        [SerializeField] private ShopShelfView _shelfViewPrefab;
        [SerializeField] private ShopPurchaseConfirmationView _itemPurchaseConfirmation;

        private List<ShopItemView> _itemViews = new();

        private List<ShopItem> ItemList => PlayerSessionManager.Instance.ShopItemList;

        public void Initialize()
        {
            ShopShelfView curShelf = null;

            foreach (var i in ItemList)
            {
                if (i.StockAmount == 0) continue;
                if (curShelf == null || curShelf.IsFull) 
                    curShelf = Instantiate(_shelfViewPrefab, _container);

                var itemView = ShopItemFactory.Instance.CreateNewIcon(i, curShelf.ItemContainer);
                i.OnItemBought.AddListener(() =>
                {
                    if (i.StockAmount <= 0) ShopItemFactory.Instance.RecycleItemView(itemView);
                    if (curShelf != null && curShelf.GetAllItems().Count == 0) 
                        Destroy(curShelf.gameObject);
                });

                itemView.OnItemClicked.AddListener(() => _itemPurchaseConfirmation.OpenPopup(itemView.CurItem));
            }

            _itemPurchaseConfirmation.ClosePopup();
        }
    }
}