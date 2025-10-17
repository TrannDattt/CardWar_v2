using CardWar_v2.Entities;
using CardWar_v2.Factories;
using UnityEngine;

namespace CardWar_v2.Views
{
    public class SkillSlotView : MonoBehaviour
    {
        public SkillCardView CardInSlot { get; private set; }
        public bool IsEmpty => CardInSlot == null;

        public void PrepareSlot()
        {
            // OnSlotClicked.RemoveAllListeners();
            RemoveCard(true);
        }

        public void PlaceCard(SkillCardView cardView)
        {
            if (cardView != null && !IsEmpty) return;
            CardInSlot = cardView;
            cardView.transform.SetParent(transform);
        }

        public void RemoveCard(bool doRecycle)
        {
            if (CardInSlot == null) return;

            if (doRecycle)
            {
                CardFactory.Instance.RecycleCardView(CardInSlot);
            }
            else
            {
                CardInSlot.transform.SetParent(null);
            }
            CardInSlot = null;
        }
    }
}

