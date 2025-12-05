// using CardWar.Factories;
using System.Collections.Generic;
using System.Linq;
using CardWar_v2.ComponentViews;
using CardWar_v2.Entities;
using CardWar_v2.GameControl;
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
        [SerializeField] private TMP_InputField _userNameEdit;
        [SerializeField] private Button _editBtn;
        [SerializeField] private Button _confirmBtn;

        [SerializeField] private TextMeshProUGUI _userLevel;
        [SerializeField] private FillBarView _userExpBar;

        private Player CurPLayer => PlayerSessionManager.Instance.CurPlayer;
        private List<CharacterCard> CharacterList => PlayerSessionManager.Instance.PlayerableCharacters;

        private CharacterCard _selectedChar;

        public void EditName()
        {
            _userNameEdit.SetTextWithoutNotify(CurPLayer.Name);

            _userName.enabled = false;
            _userNameEdit.gameObject.SetActive(true);
            _editBtn.gameObject.SetActive(false);
            _confirmBtn.gameObject.SetActive(true);
        }

        private void UpdateName()
        {
            _userName.SetText(CurPLayer.Name);

            _userName.enabled = true;
            _userNameEdit.gameObject.SetActive(false);
            _editBtn.gameObject.SetActive(true);
            _confirmBtn.gameObject.SetActive(false);
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
        
        public void Initialize()
        {
            _selectedChar = CharacterList.FirstOrDefault(c => c.IsUnlocked && c.Model != null);

            UpdateName();
            UpdateExpBar();

            _characterHallView.ChangeCharacter(
                _selectedChar
            // CharacterList.FirstOrDefault(c => c.IsUnlocked)
            );
        }

        void Start()
        {
            _editBtn.onClick.AddListener(EditName);
            _confirmBtn.onClick.AddListener(() =>
            {
                var newName = _userNameEdit.text;
                PlayerSessionManager.Instance.CurPlayer.UpdateName(newName);
                UpdateName();
            });
            CurPLayer.OnPlayerExpUpdated.AddListener((levelUpTime) => UpdateExpBar(levelUpTime));
        }
    }
}