using System.Collections.Generic;
using CardWar.Entities;
using UnityEngine;

namespace CardWar.Views
{
    public abstract class RegionView : MonoBehaviour
    {
        public List<Card> CardsInRegion { get => GetCardsInRegion(); }

        // protected Board _board;

        protected abstract List<Card> GetCardsInRegion();
        // public abstract void AddCard(Card card);

        public abstract void RemoveCard(Card card);
    }
}

