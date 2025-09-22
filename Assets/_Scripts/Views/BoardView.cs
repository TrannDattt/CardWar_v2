using System.Collections.Generic;
using System.Linq;
using CardWar.Entities;
using CardWar.Enums;
using UnityEngine;

namespace CardWar.Views
{
    public class BoardView : RegionView
    {
        [SerializeField] private List<SlotView> _monsterSlots;
        [SerializeField] private List<SlotView> _constructSlots;
        [SerializeField] private SlotView _spellSlot;

        public void Initialize()
        {
            _monsterSlots.ForEach(slot => slot.PrepareSlot());
            _constructSlots.ForEach(slot => slot.PrepareSlot());
            _spellSlot.PrepareSlot();
        }

        public void ShowAvailableSlots(ECardType cardType, out List<SlotView> availableSlots)
        {
            availableSlots = new List<SlotView>();
            switch (cardType)
            {
                case ECardType.Monster:
                    availableSlots = _monsterSlots.Where(slot => slot.IsEmpty).ToList();
                    availableSlots.ForEach(slot => slot.ShowSlot());
                    break;
                case ECardType.Construct:
                    availableSlots = _constructSlots.Where(slot => slot.IsEmpty).ToList();
                    availableSlots.ForEach(slot => slot.ShowSlot());
                    break;
                case ECardType.Spell:
                    // if (_spellSlot.IsEmpty)
                    // {
                    //     availableSlots.Add(_spellSlot);
                    //     _spellSlot.ShowSlot();
                    // }
                    availableSlots.Add(_spellSlot);
                    break;
                default:
                    Debug.LogWarning("Unknown card type");
                    break;
            }
        }

        public void HideAllSlots()
        {
            _monsterSlots.ForEach(slot => slot.HideSlot());
            _constructSlots.ForEach(slot => slot.HideSlot());
            _spellSlot.HideSlot();
        }

        public void AddCardToSlot(Card card, SlotView slot)
        {
            if (card == null || slot == null) return;
            slot.PlaceCard(card);
            slot.HideSlot();
        }

        public SlotView GetSlotByCard(Card card)
        {
            if (card == null) return null;
            return _monsterSlots.Concat(_constructSlots).Concat(new[] { _spellSlot })
                .FirstOrDefault(slot => !slot.IsEmpty && slot.CardInSlot == card);
        }

        public List<SlotView> GetSlotsByCardType(ECardType cardType)
        {
            return cardType switch
            {
                ECardType.Monster => _monsterSlots,
                ECardType.Construct => _constructSlots,
                ECardType.Spell => new List<SlotView> { _spellSlot },
                _ => new List<SlotView>()
            };
        }

        protected override List<Card> GetCardsInRegion()
        {
            var cards = new List<Card>();
            cards.AddRange(_monsterSlots.Where(slot => !slot.IsEmpty).Select(slot => slot.CardInSlot));
            cards.AddRange(_constructSlots.Where(slot => !slot.IsEmpty).Select(slot => slot.CardInSlot));
            if (!_spellSlot.IsEmpty) cards.Add(_spellSlot.CardInSlot);
            return cards;
        }

        public override void RemoveCard(Card card)
        {
            // throw new System.NotImplementedException();
        }
    }
}

