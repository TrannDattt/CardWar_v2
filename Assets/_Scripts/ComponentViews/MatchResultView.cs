using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CardWar_v2.Entities;
using CardWar_v2.Factories;
using CardWar_v2.GameControl;
using CardWar_v2.SceneViews;
using CardWar_v2.Untils;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CardWar_v2.GameControl.GameAudioManager;

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

        public IEnumerator ShowResult(Level level, bool isWin, FightLogger logger)
        {
            // Debug.Log($"Conclude match: {isWin}");
            GameplayManager.Instance.PauseGame();
            GameAudioManager.Instance.PlaySFX(isWin ? ESfx.WinMatch : ESfx.LoseMatch);
            
            ShowUI();

            _resultText.SetText(isWin ? "Victory!" : "Defeat");
            bool turnConditionCheck = logger.TurnCount <= level.Data.TurnConditionCheck;
            bool allAliveCheck = logger.AllyRecords.TrueForAll(r => !r.IsDead);

            yield return _stars[0].ShowStarResult(isWin, true);
            yield return _stars[1].ShowStarResult(isWin && turnConditionCheck, true);
            yield return _stars[2].ShowStarResult(isWin && allAliveCheck, true);

            yield return _clearCondition.CheckCondiction("Defeat all enemies", isWin, true);
            yield return _timeCondition.CheckCondiction($"Clear within {level.Data.TurnConditionCheck} turns", isWin && turnConditionCheck, true);
            yield return _surviveCondition.CheckCondiction("All allies are alive", allAliveCheck, true);

            if (isWin) 
            {
                bool isClearBefore = level.ClearCheck;
                level.ClearLevel(turnConditionCheck, allAliveCheck);

                var expIcon = IconFactory.Instance.CreateNewIcon(IconFactory.EIconType.Exp, level.Rewards.Exp, _rewardContainer);
                yield return expIcon.SetIconAlpha(0f);
                yield return expIcon.SetIconAlpha(1f, 0.3f);
                // Debug.Log($"Give player {level.Rewards.Exp} EXP");

                var goldIcon = IconFactory.Instance.CreateNewIcon(IconFactory.EIconType.Gold, level.Rewards.Gold, _rewardContainer);
                yield return goldIcon.SetIconAlpha(0f);
                yield return goldIcon.SetIconAlpha(1f, 0.3f);
                // Debug.Log($"Give player {level.Rewards.Gold} Gold");

                if (!isClearBefore)
                {
                    var gemIcon = IconFactory.Instance.CreateNewIcon(IconFactory.EIconType.Gem, level.Rewards.Gem, _rewardContainer);
                    yield return gemIcon.SetIconAlpha(0f);
                    yield return gemIcon.SetIconAlpha(1f, 0.3f);
                    // Debug.Log($"Give player {level.Rewards.Gem} Gem");
                }
            }

            _nextButton.gameObject.SetActive(isWin);
            _homeButton.gameObject.SetActive(true);
            _retryButton.gameObject.SetActive(true);
        }

        public void ShowUI()
        {
            _rt.gameObject.SetActive(true);
        }

        public void HideUI()
        {
            _rt.gameObject.SetActive(false);
            _nextButton.gameObject.SetActive(false);
            _retryButton.gameObject.SetActive(false);
            _homeButton.gameObject.SetActive(false);
        }

        void Start()
        {
            _homeButton.onClick.AddListener(async () => await SceneNavigator.Instance.ChangeScene(EScene.MainMenu));
            _retryButton.onClick.AddListener(() => 
            {
                GameplayManager.Instance.StartNewFight();
                HideUI();
            });
            _nextButton.onClick.AddListener(async () => await SceneNavigator.Instance.ChangeScene(EScene.Campaign, () =>
            {
                var campaignView = FindFirstObjectByType<CampaignView>();
                campaignView.SetLevelDetail();
            }));
        }
    }
}

