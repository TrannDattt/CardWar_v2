using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardWar.Enums;
using CardWar_v2.Datas;
using CardWar_v2.Entities;
using CardWar_v2.Session;
using CardWar_v2.Untils;
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
        public Level CurLevel => CampaignLevels.FirstOrDefault(l => !l.ClearCheck) ?? CampaignLevels.Last();

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
        public void SavePlayerData()
        {
            SessionSaveLoad.SaveToFile(CurPlayer.Data, "player");
        }

        public void SaveCharacterData()
        {
            var charJSONs = new CharacterListJson();
            CharacterList.ForEach(c => charJSONs.Characters.Add(new(c)));
            SessionSaveLoad.SaveToFile(charJSONs, "characters");
        }

        public void SaveShopItemData()
        {
            var itemJSONs = new ShopDataJson();
            ShopItemList.ForEach(i => itemJSONs.Items.Add(new(i)));
            SessionSaveLoad.SaveToFile(itemJSONs, "shop-items");
        }

        public void SaveCampaignProgress()
        {
            var levelJSONs = new List<LevelDataJSON>();
            CampaignLevels.ForEach(l => levelJSONs.Add(new LevelDataJSON(l)));
            SessionSaveLoad.SaveToFile(levelJSONs, "campaign-progress");
        }

        public void SaveSessionData()
        {
            SaveCharacterData();
            SaveShopItemData();
            SavePlayerData();
            SaveCampaignProgress();
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

                newChar.OnCardLevelUp.AddListener(SaveCharacterData);
                newChar.OnCardUnlock.AddListener(SaveCharacterData);
                // Debug.Log($"Create character {newChar.Name} with id: {newChar.Data.Id}");
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

                newItem.OnItemBought.AddListener(SaveShopItemData);
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
            
            var levelsJSON = SessionSaveLoad.LoadFromFile<List<LevelDataJSON>>("campaign-progress");
            var handle = Addressables.LoadAssetsAsync<LevelData>("Levels");
            await handle.Task;
            var levelDatas = handle.Result;

            for (int i = 0; i < levelDatas.Count; i++)
            {
                var (clearCheck, turnConditionCheck, allAliveCheck) = i >= levelsJSON.Count ? (false, false, false) 
                                                                    : (levelsJSON[i].ClearCheck, levelsJSON[i].TurnConditionCheck, levelsJSON[i].AllAliveCheck);
                Level newLevel = new(levelDatas[i], clearCheck, turnConditionCheck, allAliveCheck);
                newLevel.OnLevelClear.AddListener(SaveCampaignProgress);
                CampaignLevels.Add(newLevel);
            }
            CampaignLevels = CampaignLevels.OrderBy(l => l.Chapter).ThenBy(l => l.Room).ToList();

            Debug.Log($"Loaded {CampaignLevels.Count} campaign levels");
            Debug.Log("Current Level JSON: " + (CurLevel == null ? "null" : $"Chapter {CurLevel.Chapter} - Room {CurLevel.Room}"));
            Debug.Log($"Final level: Chapter {CampaignLevels[^1].Chapter} - Room {CampaignLevels[^1].Room}");
        }

        protected override async void Awake()
        {
            var playerData = SessionSaveLoad.LoadFromFile<PlayerDataJson>("player");
            if (playerData != null)
            {
                CurPlayer = new(playerData);

                CurPlayer.OnPlayerNameUpdated.AddListener(SavePlayerData);
                CurPlayer.OnPlayerAvatarUpdated.AddListener(SavePlayerData);
                CurPlayer.OnPlayerExpUpdated.AddListener((_) => SavePlayerData());
                CurPlayer.OnGemUpdated.AddListener(SavePlayerData);
                CurPlayer.OnGoldUpdated.AddListener(SavePlayerData);
            }

            await FetchAllCharacters();
            await FetchAllShopItems();
            await FetchAllCampaignLevels();

            OnFinishLoadingSession?.Invoke();
        }
        #endregion
    }
}