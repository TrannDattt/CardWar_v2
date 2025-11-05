using System.Runtime.CompilerServices;
using CardWar_v2.Entities;
using CardWar_v2.GameControl;
using CardWar_v2.SceneViews;
using CardWar_v2.Session;
using UnityEngine;
using UnityEngine.UI;

namespace CardWar_v2.ComponentViews
{
    public class UpgradeInfoView : InfoView
    {
        [SerializeField] private StatView _curStat;
        [SerializeField] private Image _arrow;
        [SerializeField] private StatView _nextStat;

        private const int MAX_LEVEL = 10;

        public override void UpdateInfoView(CharacterCard charCard, bool isMaxLevel)
        {
            var curLevel = charCard.Level;
            var nextLevel = charCard.IsUnlocked ? curLevel + 1 : MAX_LEVEL;
            var curStat = charCard.GetStatAtLevel(curLevel);
            var nextStat = charCard.GetStatAtLevel(nextLevel);

            _curStat.UpdateView(curLevel, curStat, isMaxLevel);

            if (isMaxLevel || !charCard.IsUnlocked)
            {
                _arrow.gameObject.SetActive(false);
                _nextStat.gameObject.SetActive(false);
                return;
            }

            _arrow.gameObject.SetActive(true);
            _nextStat.gameObject.SetActive(true);
            _nextStat.UpdateView(nextLevel, nextStat, nextLevel == MAX_LEVEL);
        }
    }
}

