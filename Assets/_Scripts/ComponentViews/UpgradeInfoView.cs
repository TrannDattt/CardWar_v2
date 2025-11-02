using CardWar_v2.Entities;
using CardWar_v2.GameControl;
using CardWar_v2.SceneViews;
using CardWar_v2.Session;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardWar_v2.ComponentViews
{
    public class UpgradeInfoView : InfoView
    {
        [SerializeField] private StatView _curStat;
        [SerializeField] private Image _arrow;
        [SerializeField] private StatView _nextStat;

        [SerializeField] private TextMeshProUGUI _goldCost;
        [SerializeField] private TextMeshProUGUI _gemCost;

        [SerializeField] private Button _levelUpBtn;

        private const int MAX_LEVEL = 10;

        private Player CurPLayer => PlayerSessionManager.Instance.CurPlayer;

        public override void UpdateInfoView(CharacterCard charCard)
        {
            var curLevel = charCard.Level;
            var curStat = charCard.GetStatAtLevel(curLevel);
            var nextStat = charCard.GetStatAtLevel(curLevel + 1);

            _curStat.UpdateView(curLevel, curStat, curLevel == MAX_LEVEL);

            if (curLevel == MAX_LEVEL)
            {
                _nextStat.gameObject.SetActive(false);
                _goldCost.gameObject.SetActive(false);
                _gemCost.gameObject.SetActive(false);
                _levelUpBtn.gameObject.SetActive(false);
                return;
            }

            _nextStat.gameObject.SetActive(true);
            _goldCost.gameObject.SetActive(true);
            _gemCost.gameObject.SetActive(true);
            _levelUpBtn.gameObject.SetActive(true);

            _nextStat.UpdateView(curLevel + 1, nextStat, (curLevel + 1) == MAX_LEVEL);

            var goldCost = GetGoldCost(curLevel + 1);
            var gemCost = GetGemCost(curLevel + 1);
            _goldCost.SetText(goldCost.ToString());
            if (gemCost == 0) _gemCost.gameObject.SetActive(false);
            else _gemCost.SetText(gemCost.ToString());

            _levelUpBtn.onClick.RemoveAllListeners();
            _levelUpBtn.onClick.AddListener(() => LevelUpCharacter(charCard, goldCost, gemCost));
        }

        private void LevelUpCharacter(CharacterCard charCard, int goldCost, int gemCost)
        {
            if (CurPLayer.Gold < goldCost || CurPLayer.Gem < gemCost)
            {
                // TODO: Pop up notice ??
                return;
            }

            charCard.Level++;
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
    }
}

