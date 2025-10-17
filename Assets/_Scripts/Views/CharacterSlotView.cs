using CardWar.Entities;
using CardWar.Enums;
using CardWar_v2.Entities;
using CardWar_v2.Enums;
using CardWar_v2.Factories;
using CardWar_v2.Views;
using UnityEngine;
using UnityEngine.Events;

namespace CardWar_v2.Views
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class CharacterSlotView : MonoBehaviour
    {
        [field: SerializeField] public EPositionTarget SlotPos { get; private set; }

        // public UnityEvent OnSlotClicked = new();

        public CharacterModelView CharInSlot { get; private set; }
        public bool IsEmpty => CharInSlot == null;

        public void PrepareSlot()
        {
            // OnSlotClicked.RemoveAllListeners();
            RemoveCard(true);
        }

        public void PlaceCard(CharacterCard card)
        {
            if (card != null && !IsEmpty) return;
            var model = CardFactory.Instance.CreateCharModel(card, parent: transform);
            CharInSlot = model;
            // Debug.Log($"Check: {card == model.BaseCard}");
            // Debug.Log($"Check: {card.SkillCards[0].Owner == model.BaseCard}");
        }

        public void RemoveCard(bool doRecycle)
        {
            if (CharInSlot == null) return;

            if (doRecycle) CardFactory.Instance.RecycleCardModel(CharInSlot);
            CharInSlot = null;
        }
    }
}

