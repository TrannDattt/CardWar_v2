using CardWar.Interfaces;
using CardWar_v2.Entities;
using CardWar_v2.Enums;
using CardWar_v2.Factories;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardWar_v2.ComponentViews
{
    public class DamageTextPopup : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private RectTransform _iconsRt;
        [SerializeField] private IconView _iconPrefab;
        [SerializeField] private float _popStrength;
        [SerializeField] private float _duration;

        public void Initialize(int amount, params ESkillEffect[] attributeEffects)
        {
            _text.SetText($"{(amount < 0 ? "+" : "-")} {Mathf.Abs(amount)}");
            _text.color = EffectViewFactory.Instance.EffectDict[attributeEffects[0]].TextColor;

            foreach(var ae in attributeEffects)
            {
                var effectIcon = Instantiate(_iconPrefab, _iconsRt);
                effectIcon.SetIcon(EffectViewFactory.Instance.EffectDict[ae].Icon);
            }
        }

        public async void Pop()
        {
            // gameObject.SetActive(true);
            await transform.DOMove(transform.position + _popStrength * Vector3.up, _duration)
                .SetEase(Ease.OutBack, overshoot: 1f)
                .AsyncWaitForCompletion();
            Destroy(gameObject);
        }
    }
}

