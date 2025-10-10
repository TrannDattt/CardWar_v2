using System.Collections.Generic;
using CardWar.Entities;
using CardWar.Enums;

namespace CardWar.Views
{
    public class GraveView : RegionView
    {
        private List<Card> _cardsInGrave = new();
        private List<Card> _banishedCards = new();

        public void AddCardToGrave(Card card, EPlayerTarget owner)
        {
            if (card == null) return;
            _cardsInGrave.Add(card);
        }

        public void BanishCard(Card card)
        {
            if (card == null) return;
            _banishedCards.Add(card);
        }

        public override void RemoveCard(Card card)
        {
            if (card == null || !_cardsInGrave.Contains(card)) return;
            _cardsInGrave.Remove(card);
        }

        protected override List<Card> GetCardsInRegion()
        {
            return _cardsInGrave;
        }
    }
}

