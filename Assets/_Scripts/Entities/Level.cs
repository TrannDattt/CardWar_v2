using System.Collections.Generic;
using System.Linq;
using CardWar.Enums;
using CardWar_v2.Datas;
using CardWar_v2.GameControl;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace CardWar_v2.Entities
{
    public class Level
    {
        public LevelData Data { get; private set; }

        public int Chapter => Data.Chapter;
        public int Room => Data.Room;

        public List<CharacterCard> Enemies = new();
        public Reward Rewards => Data.Rewards;

        // Clear State
        public bool ClearCheck { get; private set; }
        public bool TurnConditionCheck { get; private set; }
        public bool AllAliveCheck { get; private set; }

        public UnityEvent OnLevelClear { get; set; } = new();

        public Level(LevelData data, bool clearCkeck, bool turnConditionCheck, bool allAliveCheck)
        {
            Data = data;
            ClearCheck = clearCkeck;
            TurnConditionCheck = turnConditionCheck;
            AllAliveCheck = allAliveCheck;

            Data.Enemies.ForEach(e =>
            {
                Enemies.Add(new(e.Data, e.Level));
            });
        }

        public void ClearLevel(bool turnConditionCheck, bool allAliveCheck)
        {
            if(!ClearCheck)
            {
                PlayerSessionManager.Instance.CurPlayer.UpdatePlayerExp(Rewards.Exp);
                PlayerSessionManager.Instance.CurPlayer.UpdatePlayerCurrency(Rewards.Gold, Rewards.Gem);
            }

            ClearCheck = true;
            TurnConditionCheck = turnConditionCheck;
            AllAliveCheck = allAliveCheck;

            OnLevelClear?.Invoke();
        }
    }
}

