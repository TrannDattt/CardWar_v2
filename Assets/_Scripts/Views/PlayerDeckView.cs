using System.Collections.Generic;
using CardWar.Datas;
using CardWar.Entities;
using UnityEngine;

namespace CardWar.Views
{
    public class PlayerDeckView : RegionView
    {
        // TODO: TEST//////////////////
        [SerializeField] private CardData[] _testDatas;
        // [SerializeField] private RectTransform _testParent;
        ////////////////////////

        private List<Card> _cardsInDeck = new();

        public void CreateNewDeck(CardData[] cardDatas)
        {
            _cardsInDeck.Clear();
            if (cardDatas == null || cardDatas.Length == 0) return;

            foreach (var data in cardDatas)
            {
                switch (data)
                {
                    case MonsterCardData:
                        _cardsInDeck.Add(new MonsterCard(data));
                        break;
                    case SpellCardData:
                        _cardsInDeck.Add(new SpellCard(data));
                        break;
                    case ConstructCardData:
                        _cardsInDeck.Add(new ConstructCard(data));
                        break;
                }
            }
        }

        public void DrawCard(out Card drawnCard)
        {
            if (_cardsInDeck.Count == 0)
            {
                drawnCard = null;
                return;
            }

            drawnCard = _cardsInDeck[0];
            _cardsInDeck.RemoveAt(0);
        }

        public void ShuffleDeck()
        {
            int n = _cardsInDeck.Count;
            System.Random rng = new();
            while (n > 1)
            {
                int k = rng.Next(n--);
                (_cardsInDeck[k], _cardsInDeck[n]) = (_cardsInDeck[n], _cardsInDeck[k]);
            }
        }

        // TODO: TEST//////////////////
        private void Start()
        {
            CreateNewDeck(_testDatas);
        }
        ///////////////////////

        protected override List<Card> GetCardsInRegion()
        {
            return _cardsInDeck;
        }

        public override void RemoveCard(Card card)
        {
            // throw new System.NotImplementedException();
        }
    }
}

