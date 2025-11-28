using CardWar_v2.Entities;
using CardWar_v2.Factories;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardWar_v2.ComponentViews
{
    public class EffectDetailView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _detail;

        public void ShowDetail(SkillEffect effect)
        {
            _icon.sprite = EffectViewFactory.Instance.EffectDict[effect.EffectType].Icon;
            _detail.SetText(effect.GetDescription());

            gameObject.SetActive(true);
        }

        public void HideDetail()
        {
            gameObject.SetActive(false);
        }

        // void OnDisable()
        // {
        //     Destroy(gameObject);
        // }
    }
}

