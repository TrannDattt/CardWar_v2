using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardWar_v2.Datas;
using CardWar_v2.GameControl;
using CardWar_v2.SceneViews;
using CardWar_v2.Untils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardWar_v2.ComponentViews
{
    public class LevelButtonView : MonoBehaviour
    {
        [SerializeField] private int _roomNumber;
        [SerializeField] private List<MatchResultStarView> _starImages;
        [SerializeField] private TextMeshProUGUI _buttonText;
        [SerializeField] private Button _button;
        [SerializeField] private Image _lockImage;

        public async void Initialize(int chapter)
        {
            var level = PlayerSessionManager.Instance.CampaignLevels.FirstOrDefault(l => l.Chapter == chapter && l.Room == _roomNumber);
            if (level == null) return;
            // if (level == null) Debug.Log($"Level {chapter}-{_roomNumber} not found");
            var levelIndex = PlayerSessionManager.Instance.CampaignLevels.IndexOf(level);

            var enableState = levelIndex == 0 || PlayerSessionManager.Instance.CampaignLevels[levelIndex - 1].ClearCheck;
            _button.interactable = enableState;
            _lockImage.gameObject.SetActive(!enableState);
            if (TryGetComponent<GameButtonView>(out var buttonView))
                buttonView.enabled = enableState;

            await _starImages[0].ShowStarResult(level.ClearCheck, false);
            await _starImages[1].ShowStarResult(level.ClearCheck && level.TurnConditionCheck, false);
            await _starImages[2].ShowStarResult(level.ClearCheck && level.AllAliveCheck, false);

            _buttonText.SetText($"{chapter}-{_roomNumber}");
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(async () =>
            {
                await SceneNavigator.Instance.ChangeScene(EScene.Campaign, () =>
                {
                    var campaignView = FindFirstObjectByType<CampaignView>();
                    campaignView.SetLevelDetail(level);
                });
            });
        }
    }
}

