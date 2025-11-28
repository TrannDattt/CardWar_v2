using System.Threading.Tasks;
using CardWar_v2.Entities;
using CardWar_v2.Factories;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CardWar_v2.Factories.EffectViewFactory;

namespace CardWar_v2.ComponentViews
{
    public class EffectView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _durCount;
        
        // private ParticleSystem _fx;
        private SkillEffect _skillEffect;

        public void SetEffect(SkillEffect effect, EffectSetting setting)
        {
            _skillEffect = effect;

            _durCount.SetText(effect.Duration.ToString());
            _icon.sprite = setting.Icon;
            _skillEffect.OnEffectUpdated.AddListener(UpdateDuration);

            // if (setting.FX == null) return;
            // _fx = Instantiate(setting.FX, GetComponentInParent<Canvas>().gameObject.GetComponent<RectTransform>());
            // _fx.gameObject.SetActive(false);
            // _skillEffect.OnDoEffect.AddListener(async () =>
            // {
            //     _fx.gameObject.SetActive(true);
            //     _fx.Play();
            //     while (_fx.isPlaying)
            //     {
            //         await Task.Yield();
            //     }
            //     _fx.gameObject.SetActive(false);
            // });
        }

        private void UpdateDuration()
        {
            var newDur = _skillEffect.Duration;
            if(newDur <= 0)
            {
                // if (_fx != null) Destroy(_fx.gameObject);
                EffectViewFactory.Instance.ReturnEffectView(this);
            }

            _durCount.SetText(newDur.ToString());
        }

        public void RecycleView()
        {
        }
    }
}

