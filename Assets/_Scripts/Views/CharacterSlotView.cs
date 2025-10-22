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

        public void PlaceCard(CharacterModelView model)
        {
            if (model != null && !IsEmpty) return;
            CharInSlot = model;
        }

        public void RemoveCard(bool doRecycle)
        {
            if (CharInSlot == null) return;

            if (doRecycle) CardFactory.Instance.RecycleCardModel(CharInSlot);
            CharInSlot = null;
        }
    }
}

