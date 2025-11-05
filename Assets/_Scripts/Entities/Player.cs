using CardWar_v2.GameControl;
using CardWar_v2.Session;
using UnityEngine;
using UnityEngine.Events;

namespace CardWar_v2.Entities
{
    public class Player
    {
        public PlayerDataJson Data { get; private set; }

        public string Name => Data.Name;
        // public Sprite Avatar => _data.Avartar;
        public int Level => Data.Level;
        public int Exp => Data.Exp;

        public int Gold => Data.Gold;
        public int Gem => Data.Gem;

        public UnityEvent OnPlayerNameUpdated = new();
        public UnityEvent OnPlayerAvatarUpdated = new();
        public UnityEvent<int> OnPlayerExpUpdated = new();
        public UnityEvent OnGemUpdated = new();
        public UnityEvent OnGoldUpdated = new();

        public Player(PlayerDataJson data) 
        {
            Data = data;
        }

        public void UpdateName(string newName)
        {
            Data.Name = newName;
            OnPlayerNameUpdated?.Invoke();
        }

        public void UpdatePlayerExp(int amount)
        {
            var newLevel = Level;
            while (Data.Exp + amount >= GetExpToNextLevel(newLevel))
            {
                amount = Data.Exp + amount - GetExpToNextLevel(newLevel);
                newLevel++;
                Data.Exp = 0;
            }

            Data.Exp += amount;
            OnPlayerExpUpdated?.Invoke(newLevel - Level);
            Data.Level = newLevel;
        }

        public int GetExpToNextLevel(int curLevel) => (int)(100 * Mathf.Pow(1.2f, curLevel - 1));

        public void UpdateGold(int amount)
        {
            Data.Gold += amount;
            OnGoldUpdated?.Invoke();
        }

        public void UpdateGem(int amount)
        {
            Data.Gem += amount;
            OnGemUpdated?.Invoke();
        }

        public void UpdatePlayerCurrency(int goldDelta, int gemDelta)
        {
            UpdateGold(goldDelta);
            UpdateGem(gemDelta);
        }
    }
}

