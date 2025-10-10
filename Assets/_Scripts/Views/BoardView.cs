using System;
using System.Collections.Generic;
using System.Linq;
using CardWar.Entities;
using CardWar.Enums;
using UnityEngine;

namespace CardWar.Views
{
    public class BoardView : RegionView
    {
        [SerializeField] private PlayerRegion _selfRegion;
        [SerializeField] private PlayerRegion _enemyRegion;
        [SerializeField] private SlotView _spellSlot;
        private List<SlotView> MonsterSlots => GetPlayerSlots(ECardType.Monster, EPlayerTarget.Both);
        private List<SlotView> ConstructSlots => GetPlayerSlots(ECardType.Construct, EPlayerTarget.Both);

        public void Initialize()
        {
            _selfRegion.Initialize();
            _enemyRegion.Initialize();
            _spellSlot.PrepareSlot();
        }

        public List<SlotView> GetPlayerSlots(ECardType cardType, EPlayerTarget regionTarget, bool includeOccupied = true)
        {
            var (selfSlots, enemySlots) = cardType switch
            {
                ECardType.Monster => (_selfRegion.MonsterSlots, _enemyRegion.MonsterSlots),
                ECardType.Construct => (_selfRegion.ConstructSlots, _enemyRegion.ConstructSlots),
                _ => (new List<SlotView> { _spellSlot }, new List<SlotView> { _spellSlot })
            };

            return regionTarget switch
            {
                EPlayerTarget.Self => selfSlots.Where(slot => includeOccupied || slot.IsEmpty).ToList(),
                EPlayerTarget.Enemy => enemySlots.Where(slot => includeOccupied || slot.IsEmpty).ToList(),
                EPlayerTarget.Both => selfSlots.Concat(enemySlots).Where(slot => includeOccupied || slot.IsEmpty).ToList(),
                _ => new List<SlotView>()
            };
        }

        public void ShowAvailableSlots(ECardType cardType, out List<SlotView> availableSlots, EPlayerTarget target = EPlayerTarget.Self)
        {
            availableSlots = GetPlayerSlots(cardType, target, false);
            availableSlots.ForEach(slot => slot.ShowSlot());
        }

        public void HideAllSlots()
        {
            MonsterSlots.ForEach(slot => slot.HideSlot());
            ConstructSlots.ForEach(slot => slot.HideSlot());
            _spellSlot.HideSlot();
        }

        public void AddCardToSlot(Card card, SlotView slot)
        {
            if (card == null || slot == null) return;
            slot.PlaceCard(card);
            slot.HideSlot();
        }

        public SlotView GetSlotByCard(Card card, EPlayerTarget target = EPlayerTarget.Self)
        {
            if (card == null) return null;
            return GetPlayerSlots(ECardType.Monster, target)
                .Concat(GetPlayerSlots(ECardType.Construct, target))
                .Concat(new[] { _spellSlot })
                .FirstOrDefault(slot => !slot.IsEmpty && slot.CardInSlot == card);
        }

        public List<Card> GetCardsInPlayerRegion(EPlayerTarget target)
        {
            return target switch
            {
                EPlayerTarget.Self => _selfRegion.CardsInRegion,
                EPlayerTarget.Enemy => _enemyRegion.CardsInRegion,
                EPlayerTarget.Both => _selfRegion.CardsInRegion.Concat(_enemyRegion.CardsInRegion).ToList(),
                _ => new List<Card>()
            };
        }

        protected override List<Card> GetCardsInRegion()
        {
            var cards = new List<Card>();
            cards.AddRange(MonsterSlots.Where(slot => !slot.IsEmpty).Select(slot => slot.CardInSlot));
            cards.AddRange(ConstructSlots.Where(slot => !slot.IsEmpty).Select(slot => slot.CardInSlot));
            if (!_spellSlot.IsEmpty) cards.Add(_spellSlot.CardInSlot);
            return cards;
        }

        public void RemoveCard(Card card, EPlayerTarget target = EPlayerTarget.Self)
        {
            if (card == null)
            {
                Debug.LogWarning("Card not found");
                return;
            }
            var slot = GetSlotByCard(card, target);
            slot.RemoveCard();
        }

        public override void RemoveCard(Card card)
        {
            // throw new NotImplementedException();
        }
    }

    [Serializable]
    public class PlayerRegion
    {
        public List<SlotView> MonsterSlots;
        public List<SlotView> ConstructSlots;

        public List<Card> CardsInRegion => MonsterSlots.Where(s => !s.IsEmpty).Select(s => s.CardInSlot).ToList()
            .Concat(ConstructSlots.Where(s => !s.IsEmpty).Select(s => s.CardInSlot).ToList()).ToList();

        public void Initialize()
        {
            MonsterSlots.ForEach(slot => slot.PrepareSlot());
            ConstructSlots.ForEach(slot => slot.PrepareSlot());
        }
    }
}

