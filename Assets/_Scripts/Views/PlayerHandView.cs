using System.Collections.Generic;
using System.Linq;
using CardWar.Datas;
using CardWar.Entities;
using CardWar.Factories;
using UnityEngine;
using UnityEngine.UI;

namespace CardWar.Views
{
    public class PlayerHandView : RegionView
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _handContent;
        [SerializeField] private HorizontalLayoutGroup _layoutGroup;
        [SerializeField] private RectTransform _selectFrame;

        public CardView SelectedCardView { get; private set; }

        public void AddCardToHand(Card card, out CardView cardView)
        {
            if (card == null)
            {
                cardView = null;
                return;
            }

            cardView = CardFactory.Instance.CreateCardView(card, parent: _handContent);
            var localCardView = cardView;
            localCardView.OnCardClicked.AddListener((_) => TrackSelectedCard(localCardView));

            // Adjust layout if needed
            // LayoutRebuilder.ForceRebuildLayoutImmediate(_handContent);
        }

        public override void RemoveCard(Card card)
        {
            if (card == null) return;
            // Debug.Log($"Removing card {card.Name} from hand");

            var cardView = _handContent.GetComponentsInChildren<CardView>().ToList()
                .FirstOrDefault(cv => cv.BaseCard == card);
            if (cardView != null)
            {
                cardView.OnCardClicked.RemoveListener((_) => TrackSelectedCard(cardView));
                CardFactory.Instance.RecycleCardView(cardView);
                // Adjust layout if needed
                // LayoutRebuilder.ForceRebuildLayoutImmediate(_handContent);
            }
            
            if (SelectedCardView == cardView)
            {
                _selectFrame.gameObject.SetActive(false);
                SelectedCardView = null;
            }
        }

        private void TrackSelectedCard(CardView cardView)
        {
            _selectFrame.gameObject.SetActive(true);
            _selectFrame.position = cardView.GetComponent<RectTransform>().position;
            SelectedCardView = cardView;
        }

        protected override List<Card> GetCardsInRegion()
        {
            var cards = new List<Card>();
            cards.AddRange(_handContent.GetComponentsInChildren<CardView>().ToList()
                .Select(cv => cv.BaseCard));
            return cards;
        }

        void Start()
        {
            _selectFrame.gameObject.SetActive(false);
        }
    }
}

