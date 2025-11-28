// using CardWar.Factories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardWar_v2.ComponentViews;
using CardWar_v2.Entities;
using CardWar_v2.Factories;
using CardWar_v2.GameControl;
using CardWar_v2.Untils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CardWar_v2.Factories.IconFactory;

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
        // private List<CharacterCard> _enemyTeam = new();
        private List<CharacterCard> _selfTeam = new();

        private IconFactory _iconFactory => IconFactory.Instance;

        private async Task ClearLevelDetail()
        {
            // _enemyTeam.Clear();
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

        public async void SetLevelDetail(Level level)
        {
            await ClearLevelDetail();

            if (level == null) return;

            // Level level = new(data);
            // var level = CurLevel;

            _name.SetText($"Level {level.Chapter} - {level.Room}");
            level.Enemies.ForEach(e =>
            {
                _iconFactory.CreateNewCharIcon(e, true, _enemyListRt);
                // _enemyTeam.Add(e);
                // Debug.Log($"Add enemy {e.Name} to list");
            });

            _iconFactory.CreateNewIcon(EIconType.Exp, level.Rewards.Exp, _rewardListRt);
            _iconFactory.CreateNewIcon(EIconType.Gold, level.Rewards.Gold, _rewardListRt);
            if (!level.ClearCheck) _iconFactory.CreateNewIcon(EIconType.Gem, level.Rewards.Gem, _rewardListRt);
            
            _charList.ShowCharacterIcons(true, true);
            _fightBtn.onClick.AddListener(() =>
            {
                GameplayManager.Instance.StartCampaignLevel(level, _selfTeam);
            });
        }

        void Start()
        {
            _backBtn.onClick.AddListener(async () => await SceneNavigator.Instance.ChangeScene(EScene.MainMenu, async () =>
            {
                var mainMenuScene = FindFirstObjectByType<MainMenuSceneView>();
                await mainMenuScene.ChangeTab(MainMenuSceneView.EMenuTab.Fight);
            }));

            _characters.ForEach(c =>
            {
                c.OnIconClicked.AddListener(() => 
                {
                    _charList.ShowList();
                });
            });

            _charList.SetSelectAmount(3);
            _charList.OnIconClicked.AddListener((i, isSelected) => 
            {
                // Debug.Log($"{(!isSelected ? "Uns" : "S")}electd card {i.BaseCard.Name}");
                var charIcon = _characters.FirstOrDefault(c => c.BaseCard.Data == (isSelected ? null : i.BaseCard.Data));
                charIcon.SetBaseCard(isSelected ? i.BaseCard : null, true, false);
                if (isSelected) _selfTeam.Add(i.BaseCard);
                else _selfTeam.Remove(i.BaseCard);
            });
            
            GameAudioManager.Instance.PlayBackgroundMusic(GameAudioManager.EBgm.Home);
        }

        void OnEnable()
        {
            SetLevelDetail(CurLevel);
        }
    }
}