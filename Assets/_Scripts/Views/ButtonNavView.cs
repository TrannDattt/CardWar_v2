using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CardWar_v2.Views
{
    public class ButtonNavView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private List<Button> _navButtons;
        [SerializeField] private ButtonSelectOverlay _buttonSelectOverlay;

        void Start()
        {
            _navButtons.ForEach(b =>
            {
                b.onClick.AddListener(() => _buttonSelectOverlay.MoveView(b.transform.position));
            });
        }
    }
}

