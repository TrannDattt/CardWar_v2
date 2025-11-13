// using CardWar.Factories;
using System.Collections.Generic;
using System.Threading.Tasks;
using CardWar.Untils;
using CardWar_v2.ComponentViews;
using CardWar_v2.Datas;
using CardWar_v2.Entities;
using CardWar_v2.Factories;
using CardWar_v2.GameControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// using CardWar.Views;

namespace CardWar_v2.SceneViews
{
    public class CampaignView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;

        [SerializeField] private Button _backBtn;
        [SerializeField] private Button _fightBtn;

        [Header("Level Detail")]
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private RectTransform _enemyListRt;
        [SerializeField] private RectTransform _rewardListRt;

        [Header("Lineup")]
        [SerializeField] private RectTransform _lineUpRt;
        [SerializeField] private List<CharacterIconView> _characters;
        [SerializeField] private CharacterListView _charList;

        private Level CurLevel => PlayerSessionManager.Instance.CurLevel;
        private List<CharacterCard> _enemyTeam = new();
        private List<CharacterCard> _selfTeam = new();

        private IconFactory _iconFactory => IconFactory.Instance;

        private async Task ClearLevelDetail()
        {
            _enemyTeam.Clear();
            _selfTeam.Clear();

            static async Task ClearIcons(RectTransform list)
            {
                foreach (RectTransform rt in list)
                {
                    if (rt.gameObject.TryGetComponent<IconView>(out var icon))
                        IconFactory.Instance.RecycleIconView(icon);
                }
            }

            await ClearIcons(_enemyListRt);
            await ClearIcons(_rewardListRt);

            _characters.ForEach(c => c.SetBaseCard(null, true));
        }

        // public void SetLevelDetail()
        public async void SetLevelDetail(LevelData data)
        // public void SetLevelDetail(Level level)
        {
            await ClearLevelDetail();

            Level level = new(data);
            // var level = CurLevel;

            _name.SetText($"Level {level.Chapter} - {level.Room}");
            level.Enemies.ForEach(e =>
            {
                _iconFactory.CreateNewCharIcon(e, true, _enemyListRt);
                _enemyTeam.Add(e);
                Debug.Log($"Add enemy {e.Name} to list");
            });

            _iconFactory.CreateNewIcon(level.Rewards.Exp, _rewardListRt);
            _iconFactory.CreateNewIcon(level.Rewards.Gold, _rewardListRt);
            _iconFactory.CreateNewIcon(level.Rewards.Gem, _rewardListRt);
        }

        void Start()
        {
            _backBtn.onClick.AddListener(() => SceneNavigator.Instance.BackToPreviousScene());
            _fightBtn.onClick.AddListener(() =>
            {

            });

            _characters.ForEach(c =>
            {
                c.OnIconClicked.AddListener(() => _charList.ShowCharacterIcons(true, true, (icon) =>
                {
                    c.SetBaseCard(icon.BaseCard, true);
                }));
            });
        }

        void OnEnable()
        {
            // SetLevelDetail(CurLevel);
        }
    }
}