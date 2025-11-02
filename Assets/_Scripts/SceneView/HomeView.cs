// using CardWar.Factories;
using System.Collections.Generic;
using CardWar_v2.ComponentViews;
using CardWar_v2.Entities;
using CardWar_v2.GameControl;
using CardWar_v2.Session;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// using CardWar.Views;

namespace CardWar_v2.SceneViews
{
    public class HomeView : MonoBehaviour
    {
        // Char Hall
        [SerializeField] private CharacterHallView _characterHallView;

        // User Info
        [SerializeField] private Image _userAvatar;
        [SerializeField] private TextMeshProUGUI _userName;
        [SerializeField] private TextMeshProUGUI _userLevel;
        [SerializeField] private FillBarView _userExpBar;

        private Player CurPLayer => PlayerSessionManager.Instance.CurPlayer;
        private List<CharacterCard> CharacterList => PlayerSessionManager.Instance.CharacterList;

        private CharacterCard _selectedChar;

        private void UpdatePlayerName()
        {
            _userName.SetText(CurPLayer.Name);
        }

        private async void UpdateExpBar(int levelUpTime = 0)
        {
            for(int i = 0; i < levelUpTime; i++)
            {
                await _userExpBar.UpdateBarByFillAmount(1);
                _userLevel.SetText($"Lv. {CurPLayer.Level - levelUpTime + i}");
                _userExpBar.SetFillValue(0);
            }

            int expToNextLevel = CurPLayer.GetExpToNextLevel(CurPLayer.Level);
            await _userExpBar.UpdateBarByValue(CurPLayer.Exp, expToNextLevel);
            _userLevel.SetText($"Lv. {CurPLayer.Level}");
        }

        // private void UpdateUserInfo()
        // {
        //     // _userAvatar.sprite = PlayerData.Avatar;
        //     UpdatePlayerName();
        //     UpdatePlayerExp();


        //     _characterHallView.ChangeCharacter(
        //         _selectedChar
        //     // CharacterList.FirstOrDefault(c => c.IsUnlocked)
        //     );
        // }

        public void Initialize()
        {
            _selectedChar = CharacterList[0];

            UpdatePlayerName();
            UpdateExpBar();

            _characterHallView.ChangeCharacter(
                _selectedChar
            // CharacterList.FirstOrDefault(c => c.IsUnlocked)
            );
        }

        void Start()
        {
            CurPLayer.OnPlayerNameUpdated.AddListener(() => UpdatePlayerName());
            CurPLayer.OnPlayerExpUpdated.AddListener((levelUpTime) => UpdateExpBar(levelUpTime));
        }
    }
}