// using CardWar.Factories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardWar_v2.ComponentViews;
using CardWar_v2.GameControl;
using CardWar_v2.Untils;
using DG.Tweening;
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

        public async Task ChangeChapterView(int chapter)
        {
            if (chapter < 1 || chapter > PlayerSessionManager.Instance.CampaignLevels[^1].Chapter) return;

            var viewSize = _chapterViewParent.rect.width;
            await DOTween.To(() => _chapterViewParent.sizeDelta.x,
                       x => _chapterViewParent.sizeDelta = new(x, _chapterViewParent.sizeDelta.y),
                       0f,
                       .5f).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();

            if (_curChapterView) Destroy(_curChapterView.gameObject);
            // Debug.Log($"Open chapter {chapter}");
            _curChapterView = Instantiate(_chapterViews.FirstOrDefault(c => c.ChapterNumber == chapter), _chapterViewParent);
            _curChapterView.transform.SetSiblingIndex(0);

            _curChapterView.gameObject.SetActive(true);
            _nextBtn = _curChapterView.NextBtn;
            _preBtn = _curChapterView.PreBtn;

            await DOTween.To(() => 0f,
                       x => _chapterViewParent.sizeDelta = new(x, _chapterViewParent.sizeDelta.y),
                       viewSize,
                       .5f).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();

            _nextBtn.onClick.RemoveAllListeners();
            _nextBtn.onClick.AddListener(async () => await ChangeChapterView(chapter + 1));
            _preBtn.onClick.RemoveAllListeners();
            _preBtn.onClick.AddListener(async () => await ChangeChapterView(chapter - 1));
        }

        public async void Initialize()
        {
            if (PlayerSessionManager.Instance.CampaignLevels.Count > 0) await ChangeChapterView(1);
        }

        async void OnEnable()
        {
            if (PlayerSessionManager.Instance.CampaignLevels.Count > 0) await ChangeChapterView(1);
        }
    }
}