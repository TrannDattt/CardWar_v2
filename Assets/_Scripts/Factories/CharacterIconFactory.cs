namespace CardWar_v2.Factories
{
    using System.Collections.Generic;
    using CardWar.Untils;
    using CardWar_v2.ComponentViews;
    using CardWar_v2.Entities;
    using UnityEngine;

    public class CharacterIconFactory : Singleton<CharacterIconFactory>
    {
        [SerializeField] private CharacterIconView _charIconPrefab;

        private Queue<CharacterIconView> _iconPool = new();

        public CharacterIconView CreateNewIcon(CharacterCard card, RectTransform parent)
        {
            if (card == null)
            {
                Debug.LogWarning("Cannot create CardView: card is null");
                return null;
            }

            if (_iconPool.Count == 0)
            {
                var iconView = Instantiate(_charIconPrefab, parent, true);
                iconView.gameObject.SetActive(false);
                _iconPool.Enqueue(iconView);
            }

            var pooledIconView = _iconPool.Dequeue();
            pooledIconView.SetBaseCard(card);
            // Debug.Log($"Check: {card.Owner == pooledCardView.BaseCard.Owner}");

            var iconTransform = pooledIconView.GetComponent<RectTransform>();
            iconTransform.SetParent(parent);
            // cardTransform.position = parent == null ? Vector3.zero : (position == default ? parent.position : position);
            // cardTransform.rotation = parent == null ? Quaternion.identity : (rotation == default ? parent.rotation : rotation);
            iconTransform.localScale = Vector3.one;

            pooledIconView.gameObject.SetActive(true);
            return pooledIconView;
        }

        public void RecycleIconView(CharacterIconView iconView)
        {
            if (iconView == null) return;

            // cardView.SetBaseCard(null);
            // Debug.Log($"Recycling card {cardView.BaseCard?.Name}");
            // cardView.RecycleCard();
            iconView.transform.SetParent(transform);

            _iconPool.Enqueue(iconView);
        }
    }
}