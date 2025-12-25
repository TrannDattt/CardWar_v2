using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CardWar_v2.ComponentViews
{
    public class ShopShelfView : MonoBehaviour
    {
        [field: SerializeField] public RectTransform ItemContainer {get; private set;}
        [SerializeField] private int _shelfSize = 5;

        public bool IsFull => GetAllItems().Count >= _shelfSize;

        public List<ShopItemView> GetAllItems()
        {
            return ItemContainer.GetComponentsInChildren<ShopItemView>().ToList();
        }
    }
}

