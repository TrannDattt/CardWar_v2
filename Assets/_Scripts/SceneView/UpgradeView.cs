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
        [SerializeField] private UpgradeButtonView _upgradeButton;

        [SerializeField] private CharacterListView _charList;

        // private CharacterCard _selectedChar;

        private Player CurPLayer => PlayerSessionManager.Instance.CurPlayer;
        private List<CharacterCard> AllChars => PlayerSessionManager.Instance.CharacterList;

        private const int MAX_LEVEL = 10;

        public void ChangeSelectedChar(CharacterCard newChar)
        {
            // _selectedChar = newChar;
            var curLevel = newChar.Level;
            var nextLevel = newChar.IsUnlocked ? curLevel + 1 : MAX_LEVEL;

            _characterHallView.ChangeCharacter(newChar);
            _upgradeInfo.UpdateInfoView(newChar, curLevel == MAX_LEVEL);
            _skillInfo.UpdateInfoView(newChar, curLevel == MAX_LEVEL);
            
            if (!newChar.IsUnlocked || curLevel >= MAX_LEVEL)
            {
                _upgradeButton.gameObject.SetActive(false);
                return;
            }
            _upgradeButton.gameObject.SetActive(true);

            var goldCost = GetGoldCost(curLevel + 1);
            var gemCost = GetGemCost(curLevel + 1);
            _upgradeButton.UpdateCostInfo(goldCost, gemCost);

            _upgradeButton.AddListener(() =>
            {
                LevelUpCharacter(newChar, goldCost, gemCost);
                ChangeSelectedChar(newChar);
            });

            Debug.Log($"Show character {newChar} info at level {newChar.Level}");
        }

        private void LevelUpCharacter(CharacterCard charCard, int goldCost, int gemCost)
        {
            if (CurPLayer.Gold < goldCost || CurPLayer.Gem < gemCost)
            {
                // TODO: Pop up notice ??
                Debug.Log("Not enough resources to level up");
                return;
            }

            charCard.LevelUp();
            Debug.Log($"Level up character {charCard.Name} to level {charCard.Level}");
            CurPLayer.UpdatePlayerCurrency(-goldCost, -gemCost);

            //TODO: Do some animation ??
        }

        private int GetGoldCost(int nextLevel)
        {
            return nextLevel switch
            {
                2 => 500,
                3 => 2000,
                4 => 5000,
                5 => 10000,
                6 => 15000,
                7 => 20000,
                8 => 50000,
                9 => 75000,
                10 => 100000,
                _ => 0
            };
        }

        private int GetGemCost(int nextLevel)
        {
            if (nextLevel == 5) return 5;
            if (nextLevel == MAX_LEVEL) return 10;
            return 0;
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
            // Debug.Log($"Total char amount: {AllChars.Count}");
            // ChangeSelectedChar(AllChars[0]);
            _charList.ShowCharacterIcons(false, (i) => ChangeSelectedChar(i.BaseCard));
            ShowUpgradeInfo();
        }
    }

    public abstract class InfoView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;

        public void ShowView()
        {
            _canvasGroup.alpha = 1;
            _canvasGroup.blocksRaycasts = true;
        }

        public void HideView()
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.blocksRaycasts = false;
        }

        public abstract void UpdateInfoView(CharacterCard charCard, bool isMaxLevel);
    }
}