using CardWar_v2.GameControl;
using UnityEngine;
using UnityEngine.UI;

namespace CardWar_v2.ComponentViews
{
    public class GameButtonView : MonoBehaviour
    {
        private Button _button;

        void Start()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(() => GameAudioManager.Instance.PlaySFX(GameAudioManager.ESfx.ButtonClick, restart: true));
            }
        }

        void Awake()
        {
            _button = GetComponentInChildren<Button>();
        }
    }
}

