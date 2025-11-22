// using CardWar.Factories;
using System.Collections.Generic;
using System.Linq;
using CardWar_v2.ComponentViews;
using CardWar_v2.GameControl;
using CardWar_v2.Untils;
using UnityEngine;
using UnityEngine.UI;

// using CardWar.Views;

namespace CardWar_v2.SceneViews
{
    public class FightView : MonoBehaviour
    {
        [SerializeField] private RectTransform _chapterViewParent;
        [SerializeField] private List<LevelSelectView> _chapterViews;

        private LevelSelectView _curChapterView;
        private Button _nextBtn;
        private Button _preBtn;

        public void ChangeChapterView(int chapter)
        {
            if (chapter < 1 || chapter > PlayerSessionManager.Instance.CampaignLevels[^1].Chapter) return;

            if (_curChapterView) Destroy(_curChapterView.gameObject);
            _curChapterView = Instantiate(_chapterViews.FirstOrDefault(c => c.ChapterNumber == chapter), _chapterViewParent);

            _curChapterView.gameObject.SetActive(true);
            _nextBtn = _curChapterView.NextBtn;
            _preBtn = _curChapterView.PreBtn;

            _nextBtn.onClick.RemoveAllListeners();
            _nextBtn.onClick.AddListener(() => ChangeChapterView(chapter + 1));
            _preBtn.onClick.RemoveAllListeners();
            _preBtn.onClick.AddListener(() => ChangeChapterView(chapter - 1));
        }

        void OnEnable()
        {
            if (PlayerSessionManager.Instance.CampaignLevels.Count > 0) ChangeChapterView(1);   
        }
    }
}