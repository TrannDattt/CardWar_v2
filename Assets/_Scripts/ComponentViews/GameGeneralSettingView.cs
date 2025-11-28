using System;
using UnityEngine;
using UnityEngine.UI;

namespace CardWar_v2.ComponentViews
{
    public abstract class GameSettingMenuView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _raycastBlockBtn;
        [SerializeField] private Button _closeViewBtn;

        public virtual void OpenMenu()
        {
            _canvasGroup.alpha = 1;
            _canvasGroup.blocksRaycasts = true;
            _raycastBlockBtn.interactable = true;
        }

        public virtual void CloseMenu()
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.blocksRaycasts = false;
            _raycastBlockBtn.interactable = false;
        }

        protected virtual void Start()
        {
            _raycastBlockBtn.onClick.AddListener(CloseMenu);
            _closeViewBtn.onClick.AddListener(CloseMenu);
            CloseMenu();
        }
    }

    public class GameGeneralSettingView : GameSettingMenuView
    {
        [SerializeField] private Button _exitGameBtn;

        protected override void Start()
        {
            _exitGameBtn.onClick.AddListener(() => 
            {
                YesNoDialogView.Instance.OpenDialog("Are you sure you want to quit the game?\nYour data will be save automatically.",
                                                    "Confirm",
                                                    // () => Debug.Log("Quit Game!"),
                                                    () => Application.Quit(),
                                                    "Cancel",
                                                    null);
            });

            base.Start();
        }
    }
}

