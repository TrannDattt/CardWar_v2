using System.Collections.Generic;
using CardWar.Datas;
using CardWar.Entities;
using CardWar.Factories;
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
        private List<Card> _cardsInDeckArchived; // Refer to the original deck

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

            _cardsInDeckArchived = new(_cardsInDeck);
        }

        public void DrawCard(out CardView drawnCard, bool drawOnTop = true)
        {
            if (_cardsInDeck.Count == 0)
            {
                drawnCard = null;
                Debug.Log($"No card in {name}");
                return;
            }

            var cardIndex = drawOnTop ? 0 : Random.Range(0, _cardsInDeck.Count);
            drawnCard = CardFactory.Instance.CreateCardView(_cardsInDeck[cardIndex], rotation: Quaternion.Euler(0, 180, 0), parent: GetComponent<RectTransform>());
            RemoveCard(_cardsInDeck[cardIndex]);
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
        private void Awake()
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
            if (!_cardsInDeck.Contains(card))
            {
                return;
            }

            _cardsInDeck.Remove(card);
        }
    }
}

