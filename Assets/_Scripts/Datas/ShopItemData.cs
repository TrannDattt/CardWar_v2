using System;
using CardWar_v2.Enums;
using UnityEngine;

namespace CardWar_v2.Datas
{
    [CreateAssetMenu(menuName = "SO/Shop Item")]
    public class ShopItemData : ScriptableObject
    {
        public string Id => GetInstanceID().ToString();
        public CharacterCardData Item;
        public EShopItemType Type;
        public int StockAmount;
        public bool UnlimitedAmount;
        public int GoldCost;
        public int GemCost;
    }
}