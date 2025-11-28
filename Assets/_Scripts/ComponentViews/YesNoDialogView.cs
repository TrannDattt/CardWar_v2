using CardWar_v2.Untils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CardWar_v2.ComponentViews
{
    public class YesNoDialogView : Singleton<YesNoDialogView>
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _message;
        [SerializeField] private Button _yesOptionBtn;
        [SerializeField] private Button _noOptionBtn;
        [SerializeField] private Button _closeBtn;

        public void OpenDialog(string message,
                               string yesText = "Yes",
                               UnityAction yesCallback = null,
                               string noText = "No",
                               UnityAction noCallback = null)
        {
            _message.SetText(message);

            _yesOptionBtn.GetComponentInChildren<TextMeshProUGUI>().SetText(yesText);
            _yesOptionBtn.onClick.AddListener(() =>
            {
                CloseDialog();
                yesCallback?.Invoke();
            });

            _noOptionBtn.GetComponentInChildren<TextMeshProUGUI>().SetText(noText);
            _noOptionBtn.onClick.AddListener(() =>
            {
                CloseDialog();
                noCallback?.Invoke();
            });

            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        public void CloseDialog()
        {
            _yesOptionBtn.onClick.RemoveAllListeners();
            _noOptionBtn.onClick.RemoveAllListeners();

            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        void Start()
        {
            _closeBtn.onClick.AddListener(CloseDialog);
            CloseDialog();
        }
    }
}

