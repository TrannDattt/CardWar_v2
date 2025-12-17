// using CardWar.Factories;
using System.Collections;
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
        [SerializeField] private int _mapWidth;

        private LevelSelectView _curChapterView;
        private Button _nextBtn;
        private Button _preBtn;

        public IEnumerator ChangeChapterView(int chapter)
        {
            if (chapter < 1 || chapter > PlayerSessionManager.Instance.CampaignLevels[^1].Chapter) yield break;

            GameAudioManager.Instance.PlaySFX(GameAudioManager.ESfx.ClosePaper);
            yield return DOTween.To(() => _chapterViewParent.sizeDelta.x,
                                    x => _chapterViewParent.sizeDelta = new(x, _chapterViewParent.sizeDelta.y),
                                    0f,
                                    .5f).SetEase(Ease.InOutQuad).WaitForCompletion();
            // await DOTween.To(() => _chapterViewParent.sizeDelta.x,
            //            x => _chapterViewParent.sizeDelta = new(x, _chapterViewParent.sizeDelta.y),
            //            0f,
            //            .5f).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();

            if (_curChapterView) Destroy(_curChapterView.gameObject);
            // Debug.Log($"Open chapter {chapter}");
            _curChapterView = Instantiate(_chapterViews.FirstOrDefault(c => c.ChapterNumber == chapter), _chapterViewParent);
            _curChapterView.transform.SetSiblingIndex(0);

            _curChapterView.gameObject.SetActive(true);
            _nextBtn = _curChapterView.NextBtn;
            _preBtn = _curChapterView.PreBtn;

            GameAudioManager.Instance.PlaySFX(GameAudioManager.ESfx.OpenPaper);
            yield return DOTween.To(() => 0f,
                                    x => _chapterViewParent.sizeDelta = new(x, _chapterViewParent.sizeDelta.y),
                                    _mapWidth,
                                    .5f).SetEase(Ease.InOutQuad).WaitForCompletion();
            // await DOTween.To(() => 0f,
            //            x => _chapterViewParent.sizeDelta = new(x, _chapterViewParent.sizeDelta.y),
            //            viewSize,
            //            .5f).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();

            _nextBtn.onClick.RemoveAllListeners();
            _nextBtn.onClick.AddListener(() => StartCoroutine(ChangeChapterView(chapter + 1)));
            // _nextBtn.onClick.AddListener(async () => await ChangeChapterView(chapter + 1));
            _preBtn.onClick.RemoveAllListeners();
            _preBtn.onClick.AddListener(() => StartCoroutine(ChangeChapterView(chapter - 1)));
            // _preBtn.onClick.AddListener(async () => await ChangeChapterView(chapter - 1));
        }

        public void Initialize()
        {
            if (PlayerSessionManager.Instance.CampaignLevels.Count > 0) StartCoroutine(ChangeChapterView(1));
        }

        void OnEnable()
        {
            if (PlayerSessionManager.Instance.CampaignLevels.Count > 0) StartCoroutine(ChangeChapterView(1));
        }
    }
}