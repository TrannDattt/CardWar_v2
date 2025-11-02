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
        public async Task FetchAllCharacters()
        {
            CharacterList.Clear();

            var handle = Addressables.LoadAssetsAsync<CharacterCardData>("Characters", null);
            await handle.Task;

            var charDatas = handle.Result;
            var charJSONs = SessionSaveLoad.LoadFromFile<CharacterListJson>("characters").Characters;
            // if(charDatas.Count != charJSONs.Count)
            // {
            //     Debug.LogError("Character amount is not match");
            //     return;
            // }

            for (int i = 0; i < charDatas.Count; i++)
            {
                if (i >= charJSONs.Count)
                {
                    CharacterList.Add(new(charDatas[i]));
                    continue;
                }

                CharacterList.Add(new(charDatas[i], charJSONs[i].Level, charJSONs[i].IsUnlocked));
            }
        }

        protected override async void Awake()
        {
            // var playerData = SessionSaveLoad.LoadFromFile<PlayerDataJson>("player");
            var playerData = SessionSaveLoad.LoadFromFile<PlayerDataJson>("player");
            if(playerData != null)
            {
                CurPlayer = new(playerData);
            }
            await FetchAllCharacters();
            // UpdatePlayerData(playerData);
            OnFinishLoadingSession?.Invoke();
        }

        private void OnApplicationQuit()
        {
            var charJSONs = new CharacterListJson();
            CharacterList.ForEach(c => charJSONs.Characters.Add(new(c)));

            SessionSaveLoad.SaveToFile(CurPlayer.Data, "player");
            SessionSaveLoad.SaveToFile(charJSONs, "characters");
        }
    }
}