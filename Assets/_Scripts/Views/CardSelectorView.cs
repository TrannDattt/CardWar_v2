using System.Collections.Generic;
using CardWar.Entities;
using UnityEngine;

namespace CardWar.Views
{
    public class CardSelectorView : MonoBehaviour
    {
        [SerializeField] protected CanvasGroup _canvasGroup;
        [SerializeField] protected RectTransform _selectorContent;

        protected Card[] _cardsToShow;
        protected int _selectableAmount = 1;
        protected List<Card> _selectedCards = new();

        public void ShowCardSelector(Card[] cards)
        {
            _cardsToShow = cards;

            // TODO: Show all card from _cardsToShow

            _canvasGroup.alpha = 1;
        }

        public void HideCardSelector()
        {
            _canvasGroup.alpha = 0;
        }
    }
}

