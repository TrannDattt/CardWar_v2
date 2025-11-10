using System.Collections.Generic;
using System.Linq;
using CardWar.Entities;
using CardWar_v2.Datas;
using CardWar_v2.Entities;
using CardWar_v2.Factories;
using CardWar_v2.ComponentViews;
using Unity.VisualScripting;
using UnityEngine;

namespace CardWar_v2.ComponentViews
{
    public class DeckView : MonoBehaviour
    {
        private Deck _deck;

        public void CreateNewDeck(List<CharacterCard> chars)
        {
            _deck = new(chars);
        }

        public void DrawCard(out SkillCardView drawnCard)
        {
            _deck.DrawCard(out drawnCard, transform);
        }

        public void RemoveCard(SkillCard card)
        {
            _deck.RemoveCard(card);
        }

        public void RemoveDeadCards(CharacterCard characterCard)
        {
            _deck.RemoveDeadCard(characterCard);
        }
    }
    
    public class Deck
    {
        public List<SkillCard> _cardsInDeck = new(); // Refer to the original deck
        public List<SkillCard> _cardsRemainInDeck = new();

        public Deck(List<CharacterCard> chars)
        {
            _cardsInDeck.Clear();
            if (chars == null || chars.Count == 0) return;

            foreach (var c in chars)
            {
                c.SkillCards.ForEach(s =>
                {
                    for (int i = 0; i < 3; i++)
                    {
                        _cardsInDeck.Add(s);
                    }
                });
            }

            _cardsRemainInDeck = new(_cardsInDeck);
            // Debug.Log($"Check: {_cardsInDeck[0].Owner == _cardsRemainInDeck[0].Owner}");
        }

        private void RefillDeck()
        {
            _cardsRemainInDeck = new(_cardsInDeck);
        }

        public void DrawCard(out SkillCardView drawnCard, Transform parent)
        {
            var skillCard = GetRandomCard();
            drawnCard = CardFactory.Instance.CreateCardView(skillCard, rotation: Quaternion.Euler(0, 180, 0), parent: parent.transform);
        }

        public SkillCard GetRandomCard()
        {
            if (_cardsInDeck.Count == 0) return null;

            if (_cardsRemainInDeck.Count == 0)
            {
                RefillDeck();
            }

            var skillCard = _cardsRemainInDeck[Random.Range(0, _cardsRemainInDeck.Count)];
            RemoveCard(skillCard);
            return skillCard;
        }

        public void RemoveCard(SkillCard card)
        {
            if (!_cardsRemainInDeck.Contains(card))
            {
                return;
            }

            _cardsRemainInDeck.Remove(card);
        }

        public void RemoveDeadCard(CharacterCard charCard)
        {
            var deadCards = _cardsInDeck.Where(sc => sc.Owner == charCard).ToList();
            deadCards.ForEach(sc => RemoveCard(sc));

            _cardsRemainInDeck.Clear();
            RefillDeck();
        }
    }
}

