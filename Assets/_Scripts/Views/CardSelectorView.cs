using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CardWar.Entities;
using CardWar.Factories;
using UnityEngine;

namespace CardWar.Views
{
    public class CardSelectorView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _selectorContent;

        private int _selectableAmount = 1;
        private List<Card> _selectedCards = new();
        private TaskCompletionSource<List<Card>> _tcs;

        public Task<List<Card>> ShowCardToSelect(List<Card> cards, int requiredAmount)
        {
            _selectableAmount = requiredAmount;
            _selectedCards.Clear();
            _tcs = new();

            foreach (var c in cards)
            {
                var cardView = CardFactory.Instance.CreateCardView(c, parent: _selectorContent);
                cardView.OnCardClicked.AddListener((_) => HandleCardViewSelected(cardView));
            }

            _canvasGroup.alpha = 1;

            return _tcs.Task;
        }

        public void OnConfirmSelection()
        {
            //TODO: Check tier and amount
            if (_selectedCards.Count != _selectableAmount)
            {
                Debug.LogWarning("Selection not met the requirement");
                return;
            }

            _tcs?.TrySetResult(_selectedCards);
            HideCardSelector();
        }

        public void OnCancelSelection()
        {
            _tcs?.TrySetResult(null);
            HideCardSelector();
        }

        public void HideCardSelector()
        {
            _selectedCards.Clear();
            _canvasGroup.alpha = 0;

            foreach (Transform child in _selectorContent)
            {
                if (child.TryGetComponent<CardView>(out var cardView))
                {
                    cardView.OnCardClicked.RemoveListener((_) => HandleCardViewSelected(cardView));
                    CardFactory.Instance.RecycleCardView(cardView);
                }
            }
        }

        private void HandleCardViewSelected(CardView cardView)
        {
            if (_selectedCards.Contains(cardView.BaseCard))
            {
                _selectedCards.Remove(cardView.BaseCard);
            }
            else
            {
                _selectedCards.Add(cardView.BaseCard);
            }
        }
    }
}

