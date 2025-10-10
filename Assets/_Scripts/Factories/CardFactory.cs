namespace CardWar.Factories
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using CardWar.Datas;
    using CardWar.Entities;
    using CardWar.Untils;
    using CardWar.Views;
    using UnityEngine;

    public class CardFactory : Singleton<CardFactory>
    {
        [SerializeField] private Canvas _mainCanvas;

        [SerializeField] private CardView _cardViewPrefab;
        [SerializeField] private CardModelView _cardModelViewPrefab;

        private Queue<CardView> _cardViewPool = new();
        private Queue<CardModelView> _cardModelPool = new();

        #region Spawn Card View
        public CardView CreateCardView(Card card, Vector3 position = default, Quaternion rotation = default, RectTransform parent = default)
        {
            if (card == null)
            {
                Debug.LogWarning("Cannot create CardView: card is null");
                return null;
            }

            var realParent = parent == default ? _mainCanvas.GetComponent<RectTransform>() : parent;
            if (_cardViewPool.Count == 0)
            {
                var cardView = Instantiate(_cardViewPrefab, realParent, true);
                _cardViewPool.Enqueue(cardView);
            }

            var pooledCardView = _cardViewPool.Dequeue();
            pooledCardView.SetBaseCard(card);

            var rectTransform = pooledCardView.GetComponent<RectTransform>();
            rectTransform.SetParent(realParent, true);
            rectTransform.position = position == default ? realParent.position : position;
            rectTransform.rotation = rotation == default ? realParent.rotation : rotation;
            rectTransform.localScale = Vector3.one;

            pooledCardView.gameObject.SetActive(true);
            return pooledCardView;
        }

        public void RecycleCardView(CardView cardView)
        {
            if (cardView == null) return;

            // cardView.SetBaseCard(null);
            // Debug.Log($"Recycling card {cardView.BaseCard?.Name}");
            cardView.GetComponent<RectTransform>().SetParent(transform, true);
            cardView.OnCardClicked.RemoveAllListeners();
            _cardViewPool.Enqueue(cardView);
            cardView.gameObject.SetActive(false);
        }
        #endregion

        #region Spawn Card Model
        public CardModelView CreateCardModel(Card card, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
        {
            if (card == null)
            {
                Debug.LogWarning("Cannot create CardModel: card is null");
                return null;
            }

            if (_cardModelPool.Count == 0)
            {
                var cardModel = Instantiate(_cardModelViewPrefab, parent, true);
                _cardModelPool.Enqueue(cardModel);
            }

            var pooledCardModel = _cardModelPool.Dequeue();
            pooledCardModel.SetBaseCard(card);

            pooledCardModel.transform.SetParent(parent, true);
            if (parent)
            {
                pooledCardModel.transform.SetPositionAndRotation(position == default ? parent.position : position,
                                                                rotation == default ? parent.rotation : rotation);
            }
            else
            {
                pooledCardModel.transform.SetPositionAndRotation(position, rotation == default ? Quaternion.identity : rotation);
            }
            pooledCardModel.transform.localScale = Vector3.one;

            pooledCardModel.gameObject.SetActive(true);
            return pooledCardModel;
        }

        //TODO: Recycle working fine when attack but not when sacrifice ???
        public void RecycleCardModel(CardModelView cardModel)
        {
            if (cardModel == null) return;

            // cardView.SetBaseCard(null);
            // Debug.Log($"Recycling card {cardView.BaseCard?.Name}");
            cardModel.transform.SetParent(transform, true);
            cardModel.OnModelClicked.RemoveAllListeners();
            _cardModelPool.Enqueue(cardModel);
            cardModel.gameObject.SetActive(false);
        }
        #endregion
    }
}