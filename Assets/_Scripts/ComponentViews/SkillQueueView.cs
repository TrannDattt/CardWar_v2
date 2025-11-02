using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardWar_v2.Entities;
using DG.Tweening;
using UnityEngine;

namespace CardWar_v2.ComponentViews
{
    public class SkillQueueView : MonoBehaviour
    {
        public List<SkillCardView> CardQueue { get; private set; } = new();

        [SerializeField] private List<SkillSlotView> _queueSlots;

        public void Initialize()
        {
            CardQueue.Clear();

            _queueSlots.ForEach(s => s.PrepareSlot());
        }

        public void AddCard(SkillCardView cardView)
        {
            CardQueue.Add(cardView);

            GetNextEmptySlot().PlaceCard(cardView);

            // Debug.Log($"Skill queue: {CardQueue.Count}/3 skills");
        }

        public void RemoveCard(SkillCardView cardView, bool doRecycle)
        {
            CardQueue.Remove(cardView);
            GetSlotByCard(cardView).RemoveCard(doRecycle);

            ArrangeQueue();
        }

        private void ArrangeQueue()
        {
            for (int i = 0; i < _queueSlots.Count - 1; i++)
            {
                if (!_queueSlots[i].IsEmpty || _queueSlots[i + 1].IsEmpty)
                {
                    continue;
                }

                var cardView = _queueSlots[i + 1].CardInSlot;
                _queueSlots[i + 1].RemoveCard(false);
                _queueSlots[i].PlaceCard(cardView);

                cardView.transform.DOMove(_queueSlots[i].transform.position, .2f).SetEase(Ease.InOutQuad);
            }
        }

        public SkillSlotView GetNextEmptySlot()
        {
            return _queueSlots.FirstOrDefault(s => s.IsEmpty);
        }

        public SkillSlotView GetSlotByCard(SkillCardView cardView)
        {
            return _queueSlots.FirstOrDefault(s => s.CardInSlot == cardView);
        }
    }
}

