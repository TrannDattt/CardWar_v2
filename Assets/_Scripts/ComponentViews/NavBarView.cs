using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static CardWar_v2.SceneViews.MainMenuSceneView;

namespace CardWar_v2.ComponentViews
{
    public class NavBarView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private ButtonSelectOverlay _buttonSelectOverlay;
        public List<NavButton> NavButtons;

        void Awake()
        {
            var buttons = NavButtons.Select(nb => nb.Button).ToList();
            buttons.ForEach(b =>
            {
                b.onClick.AddListener(() => {
                    _buttonSelectOverlay.MoveView(b.GetComponent<RectTransform>());
                });
            });
        }
    }

    [Serializable]
    public class NavButton
    {
        public EMenuTab Tab;
        public Button Button;
    }
}

