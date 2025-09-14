using System;
using CardWar.Untils;

namespace CardWar.GameControl
{
    public class GameManager : Singleton<GameManager>
    {
        public enum EGameMenu
        {
            MainMenu,
            Ingame,
            PauseMenu,
            Settings,
            Inventory,
        }

        ////////////////
        public EGameMenu CurrentMenu { get; private set; } = EGameMenu.Ingame;
        ////////////////
    }
}