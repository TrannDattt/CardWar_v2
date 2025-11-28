using CardWar_v2.GameControl;
using UnityEngine;
using UnityEngine.UI;

namespace CardWar_v2.ComponentViews
{
    public class GameAudioSettingView : GameSettingMenuView
    {
        [SerializeField] private Slider _bgmController;
        [SerializeField] private Slider _sfxController;
        [SerializeField] private Slider _voiceController;

        private GameAudioManager _audioManager => GameAudioManager.Instance;

        public override void OpenMenu()
        {
            _bgmController.value = _audioManager.BgmVolumn;
            _sfxController.value = _audioManager.SfxVolumn;
            _voiceController.value = _audioManager.VoiceVolumn;

            base.OpenMenu();
        }

        protected override void Start()
        {
            _bgmController.onValueChanged.AddListener((v) => _audioManager.ChangeBSMVolumn(v));
            _sfxController.onValueChanged.AddListener((v) => _audioManager.ChangeSFXVolumn(v));
            _voiceController.onValueChanged.AddListener((v) => _audioManager.ChangeVoiceVolumn(v));

            base.Start();
        }
    }
}

