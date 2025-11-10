using System.Collections.Generic;
using UnityEngine;

namespace CardWar_v2.ComponentViews
{
    public class ShopShelfView : MonoBehaviour
    {
        [SerializeField] private RectTransform _container;
        [SerializeField] private int _size;

        public bool IsShelfFull => Items.Count == _size;

        public List<ShopItemView> Items { get; private set; } = new();

        public void AddItem(ShopItemView item)
        {
            var rt = item.GetComponent<RectTransform>();
            rt.SetParent(_container);
            rt.localScale = Vector3.one;

            Items.Add(item);
        }
    }
}

