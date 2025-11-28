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
        public struct EffectSetting
        {
            public ESkillEffect EffectType;
            public Sprite Icon;
            public ParticleSystem OnApplyFX;
            public ParticleSystem OnActiveFX;
        }

        [SerializeField] private List<EffectSetting> _iconMappings;
        [SerializeField] private EffectView _effectViewPrefab;

        public Dictionary<ESkillEffect, EffectSetting> EffectDict { get; private set; } = new();

        protected override void Awake()
        {
            base.Awake();

            _iconMappings.ForEach(mapping =>
            {
                EffectDict[mapping.EffectType] = mapping;
            });
        }

        public EffectView CreateEffectView(SkillEffect effect, RectTransform parent)
        {
            // if (_effectViewPool.Count == 0)
            // {
            //     newEffectView.gameObject.SetActive(false);
            //     _effectViewPool.Enqueue(newEffectView);
            // }

            var effectView = Instantiate(_effectViewPrefab, parent);
            // var effectView = _effectViewPool.Dequeue();
            effectView.GetComponent<RectTransform>().SetParent(parent);
            // effectView.gameObject.SetActive(true);
            effectView.SetEffect(effect, EffectDict[effect.EffectType]);

            return effectView;
        }

        public void ReturnEffectView(EffectView effectView)
        {
            // effectView.RecycleView();
            // effectView.GetComponent<RectTransform>().SetParent(transform);
            // effectView.gameObject.SetActive(false);
            // _effectViewPool.Enqueue(effectView);

            Destroy(effectView.gameObject);
        }
    }
}