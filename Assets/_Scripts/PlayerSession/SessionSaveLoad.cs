using System;
using System.Collections.Generic;
using System.IO;
using CardWar.Enums;
using CardWar_v2.Entities;
using Newtonsoft.Json;
using UnityEngine;

namespace CardWar_v2.Session
{
    public class SessionSaveLoad
    {
        private static string FolderPath => Application.persistentDataPath + "/userdata/";

        public static void SaveToFile<T>(T data, string fileName)
        {
            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);

            string path = Path.Combine(FolderPath, fileName + ".json");
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(path, json);
            Debug.Log($"✅ Saved {fileName}.json to {path}");
        }

        public static T LoadFromFile<T>(string fileName) where T : new()
        {
            string path = Path.Combine(FolderPath, fileName + ".json");

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                Debug.Log(json);
                return JsonConvert.DeserializeObject<T>(json);
            }

            Debug.LogWarning($"⚠ {fileName}.json not found, creating new one.");
            return new T();
        }
    }

    [Serializable]
    public class PlayerDataJson
    {
        public string Uid;
        // public Sprite Avatar;
        //TODO: Use id to serialize sprite
        public string Name = "DefaultPlayer";
        public int Level = 1;
        public int Exp;
        public int Gold;
        public int Gem;
    }

    [Serializable]
    public class PlayerRankingJson
    {
        public string Uid;
        public int Points;
    }

    [Serializable]
    public class CharacterDataJson
    {
        public string Id;
        public ECharacter Character;
        public bool IsUnlocked;
        public int Level;

        public CharacterDataJson() {}

        public CharacterDataJson(CharacterCard charCard)
        {
            Id = charCard.Data.Id;
            Character = charCard.Character;
            IsUnlocked = charCard.IsUnlocked;
            Level = charCard.Level;
        }
    }

    [Serializable]
    public class CharacterListJson
    {
        public List<CharacterDataJson> Characters = new();
    }

    [Serializable]
    public class ShopItemDataJson
    {
        public string Id;
        //TODO: More item details
        // public EItemType Type;
        public int GoldCost;
        public int GemCost;
        public int StockAmount;
    }

    [Serializable]
    public class ShopDataJson
    {
        public List<ShopItemDataJson> Items = new();
    }
}