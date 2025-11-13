using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardWar.Enums;
using CardWar.Untils;
using CardWar_v2.Datas;
using CardWar_v2.Entities;
using CardWar_v2.Session;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

namespace CardWar_v2.GameControl
{
    public class PlayerSessionManager : Singleton<PlayerSessionManager>
    {
        public Player CurPlayer { get; private set; } = new(new());
        public List<CharacterCard> CharacterList { get; private set; } = new();
        public List<ShopItem> ShopItemList { get; private set; } = new();
        public List<Level> CampaignLevels { get; private set; } = new();
        public Level CurLevel { get; private set; }

        // private Dictionary<ECharacter, CharacterDataJson> _characterDataDict = new();

        public UnityEvent OnFinishLoadingSession;

        // ------------------
        // TODO: TEST
        public void UpdateName(string newName)
        {
            CurPlayer.UpdateName(newName);
        }

        public void UpdatePlayerExp(int amount)
        {
            CurPlayer.UpdatePlayerExp(amount);
        }

        public void UpdateGold(int amount)
        {
            CurPlayer.UpdateGold(amount);
        }

        public void UpdateGem(int amount)
        {
            CurPlayer.UpdateGem(amount);
        }

        //-------------------
        // TODO: Save load more properly
        #region Save Data
        public async void SaveSessionData()
        {
            var charJSONs = new CharacterListJson();
            CharacterList.ForEach(c => charJSONs.Characters.Add(new(c)));
            SessionSaveLoad.SaveToFile(charJSONs, "characters");

            var itemJSONs = new ShopDataJson();
            ShopItemList.ForEach(i => itemJSONs.Items.Add(new(i)));
            SessionSaveLoad.SaveToFile(itemJSONs, "shop-items");

            SessionSaveLoad.SaveToFile(CurPlayer.Data, "player");
            SessionSaveLoad.SaveToFile(new CampaignProgressJson(CurLevel), "campaign-progress");

            //TODO: Save player ranking
        }
        #endregion

        #region Load Data
        public async Task FetchAllCharacters()
        {
            CharacterList.Clear();

            var handle = Addressables.LoadAssetsAsync<CharacterCardData>("Characters");
            await handle.Task;
            var charDatas = handle.Result;

            var charJSONs = SessionSaveLoad.LoadFromFile<CharacterListJson>("characters").Characters;

            // Debug.Log($"Char amount loaded: {charJSONs.Count}");
            for (int i = 0; i < charDatas.Count; i++)
            {
                CharacterCard newChar;
                if (i >= charJSONs.Count) newChar = new(charDatas[i]);
                else newChar = new(charDatas[i], charJSONs[i].Level, charJSONs[i].IsUnlocked);

                CharacterList.Add(newChar);

                newChar.OnCardLevelUp.AddListener(SaveSessionData);
                newChar.OnCardUnlock.AddListener(SaveSessionData);
                Debug.Log($"Create character {newChar.Name} with id: {newChar.Data.Id}");
            }
        }

        public async Task FetchAllShopItems()
        {
            ShopItemList.Clear();

            var handle = Addressables.LoadAssetsAsync<ShopItemData>("ShopItems");
            await handle.Task;
            var itemDatas = handle.Result;

            var itemJSONs = SessionSaveLoad.LoadFromFile<ShopDataJson>("shop-items").Items;

            for (int i = 0; i < itemDatas.Count; i++)
            {
                ShopItem newItem;
                if (i >= itemJSONs.Count) newItem = new(itemDatas[i]);
                else newItem = new(itemDatas[i], itemJSONs[i]);

                ShopItemList.Add(newItem);

                newItem.OnItemBought.AddListener(SaveSessionData);
            }
            Debug.Log($"Createed {ShopItemList.Count} shop items");
        }

        public CharacterCard GetCharById(string id)
        {
            return CharacterList.FirstOrDefault(c => c.Data.Id == id);
        }

        public async Task FetchAllCampaignLevels()
        {
            CampaignLevels.Clear();
            
            var handle = Addressables.LoadAssetsAsync<LevelData>("Levels");
            await handle.Task;
            foreach(LevelData d in handle.Result)
            {
                Level newLevel = new(d);
                newLevel.OnLevelClear.AddListener(SaveSessionData);
                CampaignLevels.Add(newLevel);
            }
        }
        
        public async Task LoadCurrentLevelData()
        {
            if (CampaignLevels.Count == 0) await FetchAllCampaignLevels();
            var campaignProgressJSON = SessionSaveLoad.LoadFromFile<CampaignProgressJson>("campaign-progress");
            CurLevel = campaignProgressJSON != null ? CampaignLevels[0] 
                                                    : CampaignLevels.FirstOrDefault(l => l.Data.Id == campaignProgressJSON.LevelId);

            if(CurLevel == null)
            {
                Debug.LogError("Cant find current level");
            }
        }

        protected override async void Awake()
        {
            var playerData = SessionSaveLoad.LoadFromFile<PlayerDataJson>("player");
            if (playerData != null)
            {
                CurPlayer = new(playerData);

                CurPlayer.OnPlayerNameUpdated.AddListener(SaveSessionData);
                CurPlayer.OnPlayerAvatarUpdated.AddListener(SaveSessionData);
                CurPlayer.OnPlayerExpUpdated.AddListener((_) => SaveSessionData());
                CurPlayer.OnGemUpdated.AddListener(SaveSessionData);
                CurPlayer.OnGoldUpdated.AddListener(SaveSessionData);
            }

            await FetchAllCharacters();
            await FetchAllShopItems();
            await LoadCurrentLevelData();

            OnFinishLoadingSession?.Invoke();
        }
        #endregion
    }
}