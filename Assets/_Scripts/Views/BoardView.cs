using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardWar.Entities;
using CardWar.Enums;
using CardWar_v2.Entities;
using CardWar_v2.Enums;
using CardWar_v2.Views;
using UnityEngine;

namespace CardWar_v2.Views
{
    public class BoardView : MonoBehaviour
    {
        [SerializeField] private PlayerRegion _selfRegion;
        [SerializeField] private PlayerRegion _enemyRegion;

        public void Initialize()
        {
            _selfRegion.Initialize();
            _enemyRegion.Initialize();
        }

        public CharacterSlotView GetPlayerSlots(EPlayerTarget playerTarget, EPositionTarget posTarget)
        {
            var region = playerTarget == EPlayerTarget.Self ? _selfRegion : _enemyRegion;
            return region.GetSlotByPosition(posTarget);
        }

        public void AddCardToSlot(CharacterCard card, EPlayerTarget playerTarget, EPositionTarget posTarget)
        {
            if (card == null) return;

            var region = playerTarget == EPlayerTarget.Self ? _selfRegion : _enemyRegion;
            var slot = region.GetSlotByPosition(posTarget);
            slot.PlaceCard(card);
        }

        public void RemoveCard(CharacterCard card, EPlayerTarget target)
        {
            if (card == null)
            {
                Debug.LogWarning("Card not found");
                return;
            }
            var region = target == EPlayerTarget.Self ? _selfRegion : _enemyRegion;
            var slot = region.GetSlotByCard(card);
            slot.RemoveCard(true);
        }

        public CharacterModelView GetCharacterByCard(EPlayerTarget region, CharacterCard cardBase)
        {
            var pr = region == EPlayerTarget.Self ? _selfRegion : _enemyRegion;
            return pr.GetCharByCard(cardBase);
        }

        public CharacterModelView GetCharacterByPos(EPlayerTarget region, EPositionTarget pos)
        {
            var pr = region == EPlayerTarget.Self ? _selfRegion : _enemyRegion;
            return pr.GetCharByPos(pos);
        }

        public async Task DestroyDeadChar(CharacterCard charCard, EPlayerTarget targetSide)
        {
            var region = targetSide == EPlayerTarget.Self ? _selfRegion : _enemyRegion;
            var slot = region.GetSlotByCard(charCard);
            var cardView = slot.CharInSlot;

            slot.RemoveCard(false);
            await cardView.DestroyChar(1);
        }
    }

    [Serializable]
    public class PlayerRegion
    {
        public List<CharacterSlotView> Slots;

        public void Initialize()
        {
            Slots.ForEach(slot => slot.PrepareSlot());
        }

        public CharacterSlotView GetSlotByCard(CharacterCard card)
        {
            return Slots.FirstOrDefault(s => s.CharInSlot != null && s.CharInSlot.BaseCard == card);
        }

        public CharacterSlotView GetSlotByPosition(EPositionTarget pos)
        {
            return Slots.FirstOrDefault(s => s.SlotPos == pos);
        }

        public CharacterModelView GetCharByCard(CharacterCard card)
        {
            return Slots.Select(s => s.CharInSlot).FirstOrDefault(cm => cm != null && cm.BaseCard == card);
        }

        public CharacterModelView GetCharByPos(EPositionTarget pos)
        {
            return Slots.FirstOrDefault(s => s.SlotPos == pos).CharInSlot;
        }
    }
}

