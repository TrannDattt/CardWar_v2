namespace CardWar_v2.Factories
{
    using System.Collections.Generic;
    using CardWar.Untils;
    using CardWar_v2.Entities;
    using CardWar_v2.Enums;
    using CardWar_v2.Views;
    using UnityEngine;

    public class EffectViewFactory : Singleton<EffectViewFactory>
    {
        [SerializeField] private Sprite _regenIcon;
        [SerializeField] private Sprite _poisonIcon;

        [SerializeField] private EffectView _effectViewPrefab;

        private Dictionary<ESkillEffect, Sprite> _iconDict;
        private Queue<EffectView> _effectViewPool = new();

        protected override void Awake()
        {
            base.Awake();

            _iconDict = new Dictionary<ESkillEffect, Sprite>
            {
                { ESkillEffect.Regen, _regenIcon },
                { ESkillEffect.Poison, _poisonIcon },
            };
        }

        public EffectView CreateEffectView(SkillEffect effect, RectTransform parent)
        {
            if (_effectViewPool.Count == 0)
            {
                var newEffectView = Instantiate(_effectViewPrefab, parent);
                _effectViewPool.Enqueue(newEffectView);
            }

            var effectView = _effectViewPool.Dequeue();
            effectView.GetComponent<RectTransform>().SetParent(parent);
            effectView.gameObject.SetActive(true);
            effectView.SetEffect(effect);

            return effectView;
        }

        public void ReturnEffectView(EffectView effectView)
        {
            effectView.RecycleView();
            effectView.GetComponent<RectTransform>().SetParent(transform);
            effectView.gameObject.SetActive(false);
            _effectViewPool.Enqueue(effectView);
        }
    }
}