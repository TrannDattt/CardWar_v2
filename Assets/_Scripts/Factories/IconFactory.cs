namespace CardWar_v2.Factories
{
    using System;
    using System.Collections.Generic;
    using CardWar_v2.ComponentViews;
    using CardWar_v2.Entities;
    using CardWar_v2.Untils;
    using UnityEngine;

    public class IconFactory : Singleton<IconFactory>
    {
        public enum EIconType
        {
            Exp,
            Gold,
            Gem,
            Item,
            Character,
        }

        [Serializable]
        public class Icon
        {
            public EIconType Type;
            public Sprite Image;
        }

        [SerializeField] private IconView _iconPrefab;
        [SerializeField] private CharacterIconView _charIconPrefab;
        [SerializeField] private List<Icon> _iconList; 

        private Queue<IconView> _iconPool = new();
        private Queue<CharacterIconView> _charIconPool = new();
        private Dictionary<EIconType, Sprite> _iconDict = new();

        // public IconView CreateNewIcon(EIconType type, int amount, RectTransform parent)
        public IconView CreateNewIcon(EIconType type, int amount, RectTransform parent)
        {
            if (_iconPool.Count == 0)
            {
                var iconView = Instantiate(_iconPrefab, parent);
                iconView.gameObject.SetActive(false);
                _iconPool.Enqueue(iconView);
            }

            var image = _iconDict[type];
            var pooledIconView = _iconPool.Dequeue();
            pooledIconView.SetIcon(image, amount);
            // Debug.Log($"Check: {card.Owner == pooledCardView.BaseCard.Owner}");

            var iconTransform = pooledIconView.GetComponent<RectTransform>();
            iconTransform.SetParent(parent);
            iconTransform.localScale = Vector3.one;

            pooledIconView.gameObject.SetActive(amount > 0);
            return pooledIconView;
        }

        public CharacterIconView CreateNewCharIcon(CharacterCard card, bool ignoreLock, RectTransform parent)
        {
            if (card == null)
            {
                Debug.LogWarning("Cannot create CardView: card is null");
                return null;
            }

            if (_charIconPool.Count == 0)
            {
                var iconView = Instantiate(_charIconPrefab, parent, true);
                iconView.gameObject.SetActive(false);
                _charIconPool.Enqueue(iconView);
            }

            var pooledIconView = _charIconPool.Dequeue();
            pooledIconView.SetBaseCard(card, ignoreLock);
            // Debug.Log($"Check: {card.Owner == pooledCardView.BaseCard.Owner}");

            var iconTransform = pooledIconView.GetComponent<RectTransform>();
            iconTransform.SetParent(parent);
            iconTransform.localScale = Vector3.one;

            pooledIconView.gameObject.SetActive(true);
            return pooledIconView;
        }

        public void RecycleIconView(IconView iconView)
        {
            if (iconView == null) return;

            // cardView.SetBaseCard(null);
            // Debug.Log($"Recycling card {cardView.BaseCard?.Name}");
            // cardView.RecycleCard();
            // iconView.transform.SetParent(transform);
            iconView.RecycleIcon();
            iconView.gameObject.SetActive(false);

            if (iconView is CharacterIconView charIcon)
            {
                _charIconPool.Enqueue(charIcon);
                return;
            }

            _iconPool.Enqueue(iconView);
        }

        protected override void Awake()
        {
            base.Awake();

            _iconList.ForEach(i => _iconDict[i.Type] = i.Image);
        }
    }
}