namespace CardWar_v2.Factories
{
    using System.Collections.Generic;
    using CardWar.Untils;
    using CardWar_v2.ComponentViews;
    using CardWar_v2.Entities;
    using UnityEngine;

    public class ShopItemFactory : Singleton<ShopItemFactory>
    {
        [SerializeField] private ShopItemView _itemViewPrefab;

        private Queue<ShopItemView> _itemPool = new();

        public ShopItemView CreateNewIcon(ShopItem item, RectTransform parent)
        {
            if (_itemPool.Count == 0)
            {
                var itemView = Instantiate(_itemViewPrefab);
                itemView.gameObject.SetActive(false);
                _itemPool.Enqueue(itemView);
            }

            var pooledItemView = _itemPool.Dequeue();
            pooledItemView.SetItem(item);
            var rt = pooledItemView.GetComponent<RectTransform>();
            rt.SetParent(parent);
            rt.localScale = Vector3.one;

            item.OnItemBought.AddListener(() =>
            {
                if (item.StockAmount <= 0) pooledItemView.gameObject.SetActive(false);
            });

            pooledItemView.gameObject.SetActive(true);
            return pooledItemView;
        }

        public void RecycleItemView(ShopItemView itemView)
        {
            if (itemView == null) return;
            itemView.transform.SetParent(transform);

            _itemPool.Enqueue(itemView);
        }
    }
}