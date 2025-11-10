using System;
using CardWar_v2.Datas;
using CardWar_v2.Enums;
using CardWar_v2.GameControl;
using CardWar_v2.Session;
using UnityEngine;
using UnityEngine.Events;

namespace CardWar_v2.Entities
{
    public class ShopItem
    {
        public string Id;
        public string ItemId;
        public Sprite Icon;
        public EShopItemType Type;
        public int StockAmount;
        public bool UnlimitedAmount;
        public int GoldCost;
        public int GemCost;

        public UnityEvent OnItemBought { get; set; } = new();

        public ShopItem(ShopItemData data)
        {
            Id = data.Id;
            Type = data.Type;
            (ItemId, Icon) = Type switch
            {
                EShopItemType.Character => ((data.Item as CharacterCardData).Id, (data.Item as CharacterCardData).Image),
                _ => throw new Exception("Item not found!")
            };
            StockAmount = data.StockAmount;
            UnlimitedAmount = data.UnlimitedAmount;
            GoldCost = data.GoldCost;
            GemCost = data.GemCost;
        }

        public ShopItem(ShopItemData data, ShopItemDataJson json)
        {
            Id = json.Id;
            Type = data.Type;
            (ItemId, Icon) = Type switch
            {
                EShopItemType.Character => ((data.Item as CharacterCardData).Id, (data.Item as CharacterCardData).Image),
                _ => throw new Exception("Item not found!")
            };
            StockAmount = json.StockAmount;
            GoldCost = data.GoldCost;
            GemCost = data.GemCost;
        }

        public void BuyItem(int amount)
        {
            if (UnlimitedAmount) amount = 0;
            StockAmount -= amount;

            switch (Type)
            {
                case EShopItemType.Character:
                    var charCard = PlayerSessionManager.Instance.GetCharById(ItemId);
                    if (charCard == null)
                    {
                        Debug.LogError($"Cant find item with id: {ItemId}");
                        return;
                    }
                    charCard.UnlockCard();
                    break;

                default:
                    break;
            }

            OnItemBought?.Invoke();
        }
    }
}

