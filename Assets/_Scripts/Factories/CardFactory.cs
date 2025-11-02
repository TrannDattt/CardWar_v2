namespace CardWar_v2.Factories
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using CardWar.Entities;
    using CardWar.Untils;
    using CardWar_v2.Entities;
    using CardWar_v2.ComponentViews;
    using UnityEngine;

    public class CardFactory : Singleton<CardFactory>
    {
        [SerializeField] private SkillCardView _cardViewPrefab;
        [SerializeField] private CharacterModelView _charModelViewPrefab;

        private Queue<SkillCardView> _cardViewPool = new();
        private Queue<CharacterModelView> _cardModelPool = new();

        public void Initialize()
        {
            _cardModelPool.Clear();
            _cardViewPool.Clear();
        }

        #region Spawn Card View
        public SkillCardView CreateCardView(SkillCard card, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
        {
            if (card == null)
            {
                Debug.LogWarning("Cannot create CardView: card is null");
                return null;
            }

            if (_cardViewPool.Count == 0)
            {
                var cardView = Instantiate(_cardViewPrefab, parent, true);
                _cardViewPool.Enqueue(cardView);
            }

            var pooledCardView = _cardViewPool.Dequeue();
            pooledCardView.SetBaseCard(card);
            // Debug.Log($"Check: {card.Owner == pooledCardView.BaseCard.Owner}");

            var cardTransform = pooledCardView.transform;
            cardTransform.SetParent(parent);
            cardTransform.position = parent == null ? Vector3.zero : (position == default ? parent.position : position);
            cardTransform.rotation = parent == null ? Quaternion.identity : (rotation == default ? parent.rotation : rotation);
            cardTransform.localScale = Vector3.one;

            pooledCardView.gameObject.SetActive(true);
            return pooledCardView;
        }

        public void RecycleCardView(SkillCardView cardView)
        {
            if (cardView == null) return;

            // cardView.SetBaseCard(null);
            // Debug.Log($"Recycling card {cardView.BaseCard?.Name}");
            cardView.RecycleCard();
            cardView.transform.SetParent(transform);

            _cardViewPool.Enqueue(cardView);
        }
        #endregion

        #region Spawn Card Model
        public CharacterModelView CreateCharModel(CharacterCard card, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
        {
            if (card == null)
            {
                Debug.LogWarning("Cannot create CharModel: card is null");
                return null;
            }

            if (_cardModelPool.Count == 0)
            {
                var cardModel = Instantiate(_charModelViewPrefab, parent, true);
                _cardModelPool.Enqueue(cardModel);
            }

            var pooledCardModel = _cardModelPool.Dequeue();
            pooledCardModel.SetBaseCard(card);
            // Debug.Log($"Check: {card == pooledCardModel.BaseCard}");
            // Debug.Log($"Check: {card.SkillCards[0].Owner == pooledCardModel.BaseCard}");

            var modelTransform = pooledCardModel.transform;
            modelTransform.SetParent(parent);
            modelTransform.position = parent == null ? Vector3.zero : (position == default ? parent.position : position);
            modelTransform.rotation = parent == null ? Quaternion.identity : (rotation == default ? parent.rotation : rotation);
            modelTransform.localScale = Vector3.one;

            pooledCardModel.gameObject.SetActive(true);
            return pooledCardModel;
        }

        public void RecycleCardModel(CharacterModelView cardModel)
        {
            if (cardModel == null) return;

            // cardView.SetBaseCard(null);
            // Debug.Log($"Recycling card {cardView.BaseCard?.Name}");
            cardModel.RecycleModel();
            cardModel.transform.SetParent(transform);

            _cardModelPool.Enqueue(cardModel);
        }
        #endregion
    }
}