// using CardWar.Factories;
using System.Collections.Generic;
using CardWar_v2.ComponentViews;
using CardWar_v2.Entities;
using CardWar_v2.GameControl;
using UnityEngine;

// using CardWar.Views;

namespace CardWar_v2.SceneViews
{
    public class UpgradeView : MonoBehaviour
    {
        [SerializeField] private CharacterHallView _characterHallView;

        [SerializeField] private UpgradeInfoView _upgradeInfo;
        [SerializeField] private SkillInfoView _skillInfo;

        [SerializeField] private GameObject _charList;

        private CharacterCard _selectedChar;
        private List<CharacterCard> AllChars => PlayerSessionManager.Instance.CharacterList;

        public void ChangeSelectedChar(CharacterCard newChar)
        {
            _selectedChar = newChar;

            _characterHallView.ChangeCharacter(newChar);
            _upgradeInfo.UpdateInfoView(newChar);
        }

        public void ShowUpgradeInfo()
        {
            _skillInfo.HideView();
            _upgradeInfo.ShowView();
        }

        public void ShowSkillInfo()
        {
            _upgradeInfo.HideView();
            _skillInfo.ShowView();
        }

        public void Initialize()
        {
            Debug.Log($"Total char amount: {AllChars.Count}");
            ChangeSelectedChar(AllChars[0]);
            ShowUpgradeInfo();
        }
    }

    public abstract class InfoView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;

        public void ShowView()
        {
            _canvasGroup.alpha = 1;
        }

        public void HideView()
        {
            _canvasGroup.alpha = 0;
        }

        public abstract void UpdateInfoView(CharacterCard charCard);
    }
}