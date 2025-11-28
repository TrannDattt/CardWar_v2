namespace CardWar_v2.Factories
{
    using System.Collections.Generic;
    using CardWar_v2.ComponentViews;
    using CardWar_v2.Entities;
    using CardWar_v2.Untils;
    using UnityEngine;

    public class ShopItemFactory : Singleton<ShopItemFactory>
    {
        [SerializeField] private ShopItemView _itemViewPrefab;

        public ShopItemView CreateNewIcon(ShopItem item, RectTransform parent)
        {
            var pooledItemView = Instantiate(_itemViewPrefab, parent);
            pooledItemView.SetItem(item);
            var rt = pooledItemView.GetComponent<RectTransform>();
            rt.SetParent(parent);
            rt.localScale = Vector3.one;

            pooledItemView.gameObject.SetActive(true);
            return pooledItemView;
        }

        public void RecycleItemView(ShopItemView itemView)
        {
            if (itemView == null) return;
            // itemView.transform.SetParent(transform);

            // _itemPool.Enqueue(itemView);
            Destroy(itemView.gameObject);
        }
    }
}