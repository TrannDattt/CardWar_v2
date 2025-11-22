using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CardWar_v2.Entities;
using CardWar_v2.Factories;
using CardWar_v2.GameControl;
using CardWar_v2.Untils;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardWar_v2.ComponentViews
{
    public class MatchResultView : MonoBehaviour
    {
        [SerializeField] private RectTransform _rt;
        [SerializeField] private TextMeshProUGUI _resultText;

        [Header("Stars")]
        [SerializeField] private List<MatchResultStarView> _stars;

        [Header("Conditions")]
        [SerializeField] private MatchResultConditionView _clearCondition;
        [SerializeField] private MatchResultConditionView _timeCondition;
        [SerializeField] private MatchResultConditionView _surviveCondition;

        [Header("Rewards")]
        [SerializeField] private RectTransform _rewardContainer;

        [Header("Buttons")]
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _homeButton;

        public async Task ShowResult(Level level, bool isWin, FightLogger logger)
        {
            GameplayManager.Instance.PauseGame();
            foreach(Transform child in _rewardContainer)
            {
                IconFactory.Instance.RecycleIconView(child.GetComponent<IconView>());
            }

            _nextButton.gameObject.SetActive(isWin);
            
            ShowUI();

            _resultText.SetText(isWin ? "Victory!" : "Defeat");
            bool turnConditionCheck = logger.TurnCount <= level.Data.TurnConditionCheck;
            bool allAliveCheck = logger.AllyRecords.TrueForAll(r => !r.IsDead);

            await _stars[0].ShowStarResult(isWin, true);
            await _stars[1].ShowStarResult(isWin && turnConditionCheck, true);
            await _stars[2].ShowStarResult(isWin && allAliveCheck, true);

            await _clearCondition.CheckCondiction("Defeat all enemies", isWin, true);
            await _timeCondition.CheckCondiction($"Clear within {level.Data.TurnConditionCheck} turns", isWin && turnConditionCheck, true);
            await _surviveCondition.CheckCondiction("All allies are alive", allAliveCheck, true);

            if (isWin) 
            {
                level.ClearLevel(turnConditionCheck, allAliveCheck);
                var expIcon = IconFactory.Instance.CreateNewIcon(IconFactory.EIconType.Exp, level.Rewards.Exp, _rewardContainer);
                await expIcon.SetIconAlpha(0f);
                await expIcon.SetIconAlpha(1f, 0.3f);
                // Debug.Log($"Give player {level.Rewards.Exp} EXP");

                var goldIcon = IconFactory.Instance.CreateNewIcon(IconFactory.EIconType.Gold, level.Rewards.Gold, _rewardContainer);
                await goldIcon.SetIconAlpha(0f);
                await goldIcon.SetIconAlpha(1f, 0.3f);
                // Debug.Log($"Give player {level.Rewards.Gold} Gold");

                var gemIcon = IconFactory.Instance.CreateNewIcon(IconFactory.EIconType.Gem, level.Rewards.Gem, _rewardContainer);
                await gemIcon.SetIconAlpha(0f);
                await gemIcon.SetIconAlpha(1f, 0.3f);
                // Debug.Log($"Give player {level.Rewards.Gem} Gem");
            }
        }

        public void ShowUI()
        {
            _rt.gameObject.SetActive(true);
        }

        public void HideUI()
        {
            _rt.gameObject.SetActive(false);
        }

        void Start()
        {
            _homeButton.onClick.AddListener(async () => await SceneNavigator.Instance.ChangeScene(EScene.MainMenu));
            _retryButton.onClick.AddListener(() => 
            {
                GameplayManager.Instance.StartNewFight();
                HideUI();
            });
            _nextButton.onClick.AddListener(async () => await SceneNavigator.Instance.ChangeScene(EScene.Campaign));
        }
    }
}

