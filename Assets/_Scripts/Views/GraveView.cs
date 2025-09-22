using System.Collections.Generic;
using CardWar.Entities;

namespace CardWar.Views
{
    public class GraveView : RegionView
    {
        // public override void AddCard(Card card)
        // {
        //     throw new System.NotImplementedException();
        // }

        public override void RemoveCard(Card card)
        {
            throw new System.NotImplementedException();
        }

        protected override List<Card> GetCardsInRegion()
        {
            return new List<Card>();
        }
    }
}

