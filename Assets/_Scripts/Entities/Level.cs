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

        public UnityEvent OnLevelClear { get; set; } = new();

        public Level(LevelData data)
        {
            Data = data;

            Enemies.Clear();
            Data.Enemies.ForEach(e =>
            {
                var enemy = PlayerSessionManager.Instance.GetCharById(e.Id);
                Debug.Log($"Create enemy {enemy.Name} from data {e.name}");
                Enemies.Add(enemy);
            });
        }
    }
}

