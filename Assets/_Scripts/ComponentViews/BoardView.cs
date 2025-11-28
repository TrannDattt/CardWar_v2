using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardWar_v2.Entities;
using CardWar_v2.Enums;
using UnityEngine;

namespace CardWar_v2.ComponentViews
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
            var region = playerTarget == EPlayerTarget.Ally ? _selfRegion : _enemyRegion;
            var pos = posTarget == EPositionTarget.Random ? region.GetRandomPos() : posTarget;
            return region.GetSlotByPosition(pos);
        }

        public void AddCharToSlot(CharacterModelView model, CharacterSlotView slot)
        {
            if (model == null) return;

            slot.PlaceCard(model);
        }

        public void RemoveCard(CharacterCard card, EPlayerTarget target)
        {
            if (card == null)
            {
                Debug.LogWarning("Card not found");
                return;
            }
            var region = target == EPlayerTarget.Ally ? _selfRegion : _enemyRegion;
            var slot = region.GetSlotByCard(card);
            slot.RemoveCard(true);
        }

        public CharacterModelView GetCharacterByCard(EPlayerTarget region, CharacterCard cardBase)
        {
            var pr = region == EPlayerTarget.Ally ? _selfRegion : _enemyRegion;
            return pr.GetCharByCard(cardBase);
        }

        public CharacterModelView GetCharacterByPos(EPlayerTarget region, EPositionTarget pos, bool flexPos)
        {
            var pr = region == EPlayerTarget.Ally ? _selfRegion : _enemyRegion;
            var posTarget = pos == EPositionTarget.Random ? pr.GetRandomPos() : pos;
            var character = pr.GetCharByPos(posTarget);

            if (character == null && flexPos) return GetAdditionTarget(region, posTarget);
            return character;
        }

        private CharacterModelView GetAdditionTarget(EPlayerTarget region, EPositionTarget curPos)
        {
            var pr = region == EPlayerTarget.Ally ? _selfRegion : _enemyRegion;
            if (curPos == EPositionTarget.Front)
            {
                if (pr.GetCharByPos(EPositionTarget.Mid) == null) return pr.GetCharByPos(EPositionTarget.Back);
                return pr.GetCharByPos(EPositionTarget.Mid);
            }

            if (curPos == EPositionTarget.Mid)
            {
                if (pr.GetCharByPos(EPositionTarget.Back) == null) return pr.GetCharByPos(EPositionTarget.Front);
                return pr.GetCharByPos(EPositionTarget.Back);
            }
            
            if (pr.GetCharByPos(EPositionTarget.Mid) == null) return pr.GetCharByPos(EPositionTarget.Front);
            return pr.GetCharByPos(EPositionTarget.Mid);
        }

        public List<CharacterModelView> GetCharactersInRegion(EPlayerTarget region)
        {
            var pr = region == EPlayerTarget.Ally ? _selfRegion : _enemyRegion;
            return pr.Slots.Select(s => s.CharInSlot).Where(c => c != null).ToList();
        }

        public async Task DestroyDeadChar(CharacterCard charCard, EPlayerTarget targetSide)
        {
            if (charCard == null) return;
            var region = targetSide == EPlayerTarget.Ally ? _selfRegion : _enemyRegion;
            // Debug.Log($"Destroy dead character {charCard.Name} at region {region}");
            var slot = region.GetSlotByCard(charCard);
            if (slot == null) return;
            // Debug.Log($"and at position {slot}");
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

        public EPositionTarget GetRandomPos()
        {
            var pos = Slots.Where(s => !s.IsEmpty).Select(s => s.SlotPos).ToList();
            var randomIndex = UnityEngine.Random.Range(0, pos.Count);
            return pos[randomIndex];
        }

        public CharacterSlotView GetSlotByCard(CharacterCard card)
        {
            if (card == null) return null;
            return Slots.FirstOrDefault(s => !s.IsEmpty && s.CharInSlot.BaseCard == card);
        }

        public CharacterSlotView GetSlotByPosition(EPositionTarget pos)
        {
            var posTarget = pos == EPositionTarget.Random ? GetRandomPos() : pos;
            return Slots.FirstOrDefault(s => s.SlotPos == pos);
        }

        public CharacterModelView GetCharByCard(CharacterCard card)
        {
            return Slots.Select(s => s.CharInSlot).FirstOrDefault(cm => cm != null && cm.BaseCard == card);
        }

        public CharacterModelView GetCharByPos(EPositionTarget pos)
        {
            var posTarget = pos == EPositionTarget.Random ? GetRandomPos() : pos;
            return Slots.FirstOrDefault(s => s.SlotPos == pos).CharInSlot;
        }
    }
}

