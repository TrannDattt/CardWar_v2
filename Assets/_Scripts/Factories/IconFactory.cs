namespace CardWar_v2.Factories
{
    using System.Collections.Generic;
    using CardWar.Untils;
    using CardWar_v2.ComponentViews;
    using CardWar_v2.Entities;
    using UnityEngine;

    public class IconFactory : Singleton<IconFactory>
    {
        [SerializeField] private IconView _iconPrefab;
        [SerializeField] private CharacterIconView _charIconPrefab;

        private Queue<IconView> _iconPool = new();
        private Queue<CharacterIconView> _charIconPool = new();

        // public IconView CreateNewIcon(EIconType type, int amount, RectTransform parent)
        public IconView CreateNewIcon(int amount, RectTransform parent)
        {
            if (_iconPool.Count == 0)
            {
                var iconView = Instantiate(_iconPrefab, parent);
                iconView.gameObject.SetActive(false);
                _iconPool.Enqueue(iconView);
            }

            var pooledIconView = _iconPool.Dequeue();
            pooledIconView.SetIcon(null, amount);
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
            iconView.gameObject.SetActive(false);

            if (iconView is CharacterIconView charIcon)
            {
                _charIconPool.Enqueue(charIcon);
                return;
            }

            _iconPool.Enqueue(iconView);
        }
    }
}