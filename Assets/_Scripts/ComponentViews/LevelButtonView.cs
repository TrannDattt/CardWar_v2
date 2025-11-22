using System.Collections.Generic;
using System.Threading.Tasks;
using CardWar_v2.Datas;
using CardWar_v2.GameControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardWar_v2.ComponentViews
{
    public class LevelButtonView : MonoBehaviour
    {
        [SerializeField] private LevelData _levelData;
        [SerializeField] private List<MatchResultStarView> _starImages;
        [SerializeField] private TextMeshProUGUI _buttonText;
        [SerializeField] private Button _button;
        [SerializeField] private Image _lockImage;

        public int RoomNumber => _levelData.Room;
        public int ChapterNumber => _levelData.Chapter;

        public async void Initialize()
        {
            var level = PlayerSessionManager.Instance.CampaignLevels.Find(l => l.Chapter == ChapterNumber && l.Room == RoomNumber);
            var levelIndex = PlayerSessionManager.Instance.CampaignLevels.IndexOf(level);

            _button.interactable = levelIndex == 0 || PlayerSessionManager.Instance.CampaignLevels[levelIndex - 1].ClearCheck;
            _lockImage.gameObject.SetActive(!_button.interactable);

            await _starImages[0].ShowStarResult(level.ClearCheck, false);
            await _starImages[1].ShowStarResult(level.ClearCheck && level.TurnConditionCheck, false);
            await _starImages[2].ShowStarResult(level.ClearCheck && level.AllAliveCheck, false);

            _buttonText.SetText($"{ChapterNumber}-{RoomNumber}");
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() =>
            {
                GameplayManager.Instance.SetLevelDetail(level);
            });
        }
    }
}

