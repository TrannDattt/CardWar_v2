using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public IEnumerator SelectButton(EMenuTab key)
        {
            var button = NavButtons.FirstOrDefault(b => b.Tab == key).Button;
            if(button == null) yield break;
            yield return _buttonSelectOverlay.MoveView(button.GetComponent<RectTransform>());
        }

        void Awake()
        {
            NavButtons.ForEach(b =>
            {
                if (b.DoAddListener)
                {
                    b.Button.onClick.AddListener(() => {
                        StartCoroutine(SelectButton(b.Tab));
                    });
                }
            });
        }
    }

    [Serializable]
    public class NavButton
    {
        public EMenuTab Tab;
        public Button Button;
        public bool DoAddListener;
    }
}

