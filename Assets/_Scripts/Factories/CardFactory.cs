namespace CardWar.Factories
{
    using System.Collections;
    using System.Collections.Generic;
    using CardWar.Datas;
    using CardWar.Entities;
    using CardWar.Untils;
    using CardWar.Views;
    using UnityEngine;

    public class CardFactory : Singleton<CardFactory>
    {
        [SerializeField] private CardView _cardViewPrefab;

        private Queue<CardView> _cardViewPool = new();

        public CardView CreateCardView(Card card, Vector3 position = default, Quaternion rotation = default, Vector3 scale = default, RectTransform parent = null)
        {
            if (card == null)
            {
                Debug.LogWarning("Cannot create CardView: card is null");
                return null;
            }

            if (_cardViewPool.Count == 0)
            {
                var cardView = Instantiate(_cardViewPrefab);
                _cardViewPool.Enqueue(cardView);
            }

            var pooledCardView = _cardViewPool.Dequeue();
            pooledCardView.SetBaseCard(card);

            var rectTransform = pooledCardView.GetComponent<RectTransform>();
            rectTransform.SetParent(parent);
            rectTransform.position = position;
            rectTransform.rotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z);
            rectTransform.localScale = scale == default ? Vector3.one : scale;

            pooledCardView.gameObject.SetActive(true);
            return pooledCardView;
        }

        public void RecycleCardView(CardView cardView)
        {
            if (cardView == null) return;

            // cardView.SetBaseCard(null);
            cardView.GetComponent<RectTransform>().SetParent(transform);
            _cardViewPool.Enqueue(cardView);
            cardView.gameObject.SetActive(false);
        }
    }
}