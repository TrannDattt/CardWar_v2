using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CardWar_v2.GameControl.FightLogger;

namespace CardWar_v2.ComponentViews
{
    public class LogTextView : MonoBehaviour
    {
        [SerializeField] private RectTransform _rt;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Image _background;

        public int RecordTurn { get; private set; }

        // TODO: Change background base on turn
        public void SetLog(ActionRecord actionRecord)
        {
            RecordTurn = actionRecord.Turn;

            var bgColor = _background.color;
            _background.color = new(bgColor.r, bgColor.g, bgColor.b, RecordTurn % 2 == 0 ? .5f : .3f);

            _text.SetText(actionRecord.GetActionSummary());
            _text.ForceMeshUpdate();

            var preferredHeight = _text.preferredHeight;
            _rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight);
            // _rt.sizeDelta = new(_rt.sizeDelta.x, preferredHeight);
        }
    }
}

