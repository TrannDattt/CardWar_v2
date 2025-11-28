using CardWar_v2.GameControl;
using CardWar_v2.Untils;
using UnityEngine;
using UnityEngine.UI;

namespace CardWar_v2.ComponentViews
{
    public class GameBattleSettingView : GameSettingMenuView
    {
        [SerializeField] private Button _homeButton;
        [SerializeField] private Button _retryButton;

        protected override void Start()
        {
            _retryButton.onClick.AddListener(() =>
            {
                YesNoDialogView.Instance.OpenDialog("Are you sure to retry this battle?",
                                                    "Confirm",
                                                    () => GameplayManager.Instance.StartNewFight(),
                                                    "Cancel",
                                                    null);
            });

            _homeButton.onClick.AddListener(() =>
            {
                YesNoDialogView.Instance.OpenDialog("Return to main menu?",
                                                    "Confirm",
                                                    async () => await SceneNavigator.Instance.ChangeScene(EScene.MainMenu),
                                                    "Cancel",
                                                    null);
            });

            base.Start();
        }
    }
}

