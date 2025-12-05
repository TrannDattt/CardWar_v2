using System.Linq;
using System.Threading.Tasks;
using CardWar_v2.GameControl;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CardWar_v2.ComponentViews
{
    public class BattleLogView : GameSettingMenuView
    {
        [SerializeField] private RectTransform _container;
        [SerializeField] private RectTransform _logRt;
        [SerializeField] private LogTextView _logTextPrefab;

        private float _preferredHeight;

        public async Task OpenMenu(FightLogger logger)
        {
            base.OpenMenu();

            await DOTween.To(() => 0f,
                            y => _container.sizeDelta = new(_container.sizeDelta.x, y),
                            _preferredHeight,
                            .5f).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();

            UpdateLog(logger);
        }

        public async Task CloseMenu(bool doAnim)
        {
            if (doAnim) await DOTween.To(() => _container.sizeDelta.y,
                            y => _container.sizeDelta = new(_container.sizeDelta.x, y),
                            0f,
                            .5f).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();

            base.CloseMenu();
        }

        public void UpdateLog(FightLogger logger)
        {
            int lastRecordTurn = 0;

            if (_logRt.childCount > 0)
            {
                _logRt.GetChild(_logRt.childCount - 1).TryGetComponent<LogTextView>(out var lastRecord);
                lastRecordTurn = lastRecord.RecordTurn;
                lastRecord.SetLog(logger.ActionRecords.FirstOrDefault(r => r.Turn == lastRecordTurn));
            }

            foreach (var r in logger.ActionRecords)
            {
                if (r.Turn <= lastRecordTurn) continue;
                var text = Instantiate(_logTextPrefab, _logRt);
                text.SetLog(r);
            }
        }

        protected override async void Start()
        {
            _preferredHeight = _container.rect.height;
            _raycastBlockBtn.onClick.AddListener(async () => await CloseMenu(true));
            _closeViewBtn.onClick.AddListener(async () => await CloseMenu(true));
            await CloseMenu(false);
        }
    }
}

