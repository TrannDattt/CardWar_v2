// using CardWar.Factories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CardWar_v2.ComponentViews;
using CardWar_v2.Entities;
using CardWar_v2.GameControl;
using DG.Tweening;
using UnityEngine;

// using CardWar.Views;

namespace CardWar_v2.SceneViews
{
    public class MainMenuSceneView : MonoBehaviour
    {
        public enum EMenuTab
        {
            Home,
            Info,
            Fight,
            Upgrade,
            Shop,
        }

        [SerializeField] private List<MenuTab> _menuTabs;
        [SerializeField] private NavBarView _navBar;

        [SerializeField] private CounterView _goldCounter;
        [SerializeField] private CounterView _gemCounter;

        [SerializeField] private float _rectOffsetX = 1960f;
        [SerializeField] private float _offsetX;

        private EMenuTab _activeTab;
        private Player CurPlayer => PlayerSessionManager.Instance.CurPlayer;

        private async Task ChangeTab(EMenuTab tab)
        {
            // Debug.Log($"Changing to tab: {tab}");
            if (_activeTab == tab) return;

            var activeTab = _menuTabs.Find(t => t.Tab == _activeTab);
            var newTab = _menuTabs.Find(t => t.Tab == tab);
            if (activeTab == null || newTab == null) return;

            var (rectOffsetX, offsetX) = _menuTabs.IndexOf(newTab) - _menuTabs.IndexOf(activeTab) > 0
                                        ? (-_rectOffsetX, -_offsetX)
                                        : (_rectOffsetX, _offsetX);

            var sequence = DOTween.Sequence();
            foreach (var c in activeTab.Containers)
            {
                if (c.TryGetComponent<RectTransform>(out var rt))
                {
                    rt.anchoredPosition = Vector2.zero;
                    sequence.Join(rt.DOAnchorPosX(rectOffsetX, 0.2f));
                }
                else
                {
                    sequence.Join(c.transform.DOMoveX(offsetX, 0.2f));
                }
            }

            foreach (var c in newTab.Containers)
            {
                if (c.TryGetComponent<RectTransform>(out var rt))
                {
                    sequence.Join(rt.DOAnchorPosX(0f, 0.2f));
                }
                else
                {
                    sequence.Join(c.transform.DOMoveX(0f, 0.2f));
                }
            }

            sequence.OnComplete(() =>
            {
                _activeTab = tab;
            });

            await sequence.AsyncWaitForCompletion();
        }
        
        public void UpdateCurrencyCounter()
        {
            Debug.Log($"Update currencies => Gold: {CurPlayer.Gold} - Gem: {CurPlayer.Gem}");
            _goldCounter.SetCount(CurPlayer.Gold);
            _gemCounter.SetCount(CurPlayer.Gem);
        }

        void Start()
        {
            _activeTab = EMenuTab.Home;
            _navBar.NavButtons.ForEach(nb =>
            {
                nb.Button.onClick.AddListener(async () => await ChangeTab(nb.Tab));
            });

            CurPlayer.OnGoldUpdated.AddListener(() => _goldCounter.SetCount(CurPlayer.Gold));
            CurPlayer.OnGemUpdated.AddListener(() => _gemCounter.SetCount(CurPlayer.Gem));
        }
    }

    [Serializable]
    public class MenuTab
    {
        public MainMenuSceneView.EMenuTab Tab;
        public List<GameObject> Containers;
    }
}