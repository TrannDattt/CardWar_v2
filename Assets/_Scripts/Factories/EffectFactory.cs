namespace CardWar_v2.Factories
{
    using System.Collections.Generic;
    using CardWar_v2.Entities;
    using CardWar_v2.Enums;
    using CardWar_v2.ComponentViews;
    using UnityEngine;
    using System;
    using CardWar_v2.Untils;

    public class EffectViewFactory : Singleton<EffectViewFactory>
    {
        [Serializable]
        struct IconMapping
        {
            public ESkillEffect EffectType;
            public Sprite Icon;
        }

        [SerializeField] private List<IconMapping> _iconMappings;
        [SerializeField] private EffectView _effectViewPrefab;

        private Dictionary<ESkillEffect, Sprite> _iconDict = new();
        private Queue<EffectView> _effectViewPool = new();

        protected override void Awake()
        {
            base.Awake();

            _iconMappings.ForEach(mapping =>
            {
                _iconDict[mapping.EffectType] = mapping.Icon;
            });
        }

        public EffectView CreateEffectView(SkillEffect effect, RectTransform parent)
        {
            if (_effectViewPool.Count == 0)
            {
                var newEffectView = Instantiate(_effectViewPrefab, parent);
                newEffectView.gameObject.SetActive(false);
                _effectViewPool.Enqueue(newEffectView);
            }

            var effectView = _effectViewPool.Dequeue();
            effectView.GetComponent<RectTransform>().SetParent(parent);
            effectView.gameObject.SetActive(true);
            effectView.SetEffect(effect, _iconDict[effect.EffectType]);

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