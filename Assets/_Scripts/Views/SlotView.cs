using CardWar.Entities;
using CardWar.Enums;
using CardWar.Factories;
using UnityEngine;
using UnityEngine.Events;

namespace CardWar.Views
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SlotView : MonoBehaviour
    {
        // Property for slot type ????
        private SpriteRenderer _spriteRenderer;
        private Color _defaultColor = new(1, 1, 1, 1);
        private Color _transparentColor = new(1, 1, 1, 0);
        public bool IsShown => _spriteRenderer.color.a > 0;

        [field: SerializeField] public ECardType SlotType { get; private set; }
        public UnityEvent OnSlotClicked;

        public Card CardInSlot { get; private set; }
        public bool IsEmpty => CardInSlot == null;

        public void PrepareSlot()
        {
            OnSlotClicked.RemoveAllListeners();
            PlaceCard(null);
            HideSlot();
        }

        public void ShowSlot()
        {
            _spriteRenderer.color = _defaultColor;
        }

        public void HideSlot()
        {
            _spriteRenderer.color = _transparentColor;
        }

        // TODO: TEST
        public void PlaceCard(Card card)
        {
            if (card != null && !IsEmpty) return;
            CardInSlot = card;
            // HideSlot();
        }

        void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void OnMouseDown()
        {
            if (IsShown)
            {
                OnSlotClicked?.Invoke();

                Debug.Log($"Slot at position {transform.position} clicked.");
                // HideSlot();
            }
        }
    }
}

