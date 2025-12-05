using System.Collections.Generic;
using CardWar_v2.GameControl;
using UnityEngine;
using UnityEngine.UI;

namespace CardWar_v2.ComponentViews
{
    public class LevelSelectView : MonoBehaviour
    {
        [field: SerializeField] public int ChapterNumber { get; private set; }
        [field: SerializeField] public Button NextBtn  { get; private set; }
        [field: SerializeField] public Button PreBtn  { get; private set; }
        [SerializeField] private RectTransform _levelButtonParent;

        private List<LevelButtonView> _levelBtns = new();

        void OnEnable()
        {
            _levelBtns.ForEach(b =>
            {
                b.Initialize(ChapterNumber);
            });

            NextBtn.gameObject.SetActive(ChapterNumber < PlayerSessionManager.Instance.CampaignLevels[^1].Chapter);
            PreBtn.gameObject.SetActive(ChapterNumber > 1);
        }

        void Awake()
        {
            _levelBtns = new List<LevelButtonView>(_levelButtonParent.GetComponentsInChildren<LevelButtonView>(true));
        }
    }
}

