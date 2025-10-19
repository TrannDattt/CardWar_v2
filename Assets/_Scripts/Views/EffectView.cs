using System.Threading.Tasks;
using CardWar_v2.Entities;
using CardWar_v2.Factories;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardWar_v2.Views
{
    public class EffectView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _durCount;

        private SkillEffect _skillEffect;

        public void SetEffect(SkillEffect effect, Sprite icon = null)
        {
            _skillEffect = effect;

            _icon.sprite = icon;
            _durCount.SetText(effect.Duration.ToString());
            _skillEffect.OnEffectUpdated.AddListener(UpdateDuration);
        }

        private void UpdateDuration()
        {
            var newDur = _skillEffect.Duration;
            if(newDur <= 0)
            {
                EffectViewFactory.Instance.ReturnEffectView(this);
            }

            _durCount.SetText(newDur.ToString());
        }

        public void RecycleView()
        {
        }
    }
}

