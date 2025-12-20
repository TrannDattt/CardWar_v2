using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardWar_v2.Entities;
using CardWar_v2.Factories;
using CardWar_v2.SceneViews;
using DG.Tweening;
using UnityEngine;

namespace CardWar_v2.ComponentViews
{
    public class HandView : MonoBehaviour
    {
        [SerializeField] private Transform _content;

        [SerializeField] private float _handRange;
        // [SerializeField] private BoxCollider _handRange;
        [SerializeField] Vector3 _space;

        private SkillCardView _selectedCard;
        private int _selectedCardIndex;

        private List<SkillCardView> _cardInHand = new();

        public void Initialize()
        {
            _cardInHand.ForEach(c => CardFactory.Instance.RecycleCardView(c));
            _cardInHand.Clear();
        }

        public void ArrangeHand()
        {
            // Debug.Log($"Start arrange card");
            int cardCount = _cardInHand.Count;
            if (cardCount == 0) return;
            // if (cardCount == 0) yield break;
            var firstPos = transform.position + (-cardCount) / 2 * _space;

            var sequence = DOTween.Sequence();
            for (int i = 0; i < cardCount; i++)
            {
                var cardPos = firstPos + i * _space;
                // _cardInHand[i].transform.DOKill();
                // _cardInHand[i].transform.DOMove(cardPos, .2f).SetEase(Ease.InOutQuad);
                // sequence.Join(_cardInHand[i].transform.DOMove(cardPos, .2f).SetEase(Ease.InOutQuad));
                _cardInHand[i].transform.position = cardPos;
            }
            // Debug.Log($"Finish arrange card");

            // yield return null;
            // yield return sequence.WaitForCompletion();
        }

        public Vector3 GetCardPos(int index = default)
        {
            if (_cardInHand.Count == 0) return transform.position;

            if (index == default) index = _cardInHand.Count - 1;

            return _cardInHand[index].transform.position + _space;
        }

        public void AddCardToHand(SkillCardView cardView)
        {
            _cardInHand.Add(cardView);
            cardView.transform.SetParent(_content);
            ArrangeHand();
        }

        public void RemoveCard(SkillCardView cardView)
        {
            if (!_cardInHand.Contains(cardView) && _selectedCard != cardView) return;
            _cardInHand.Remove(cardView);
            ArrangeHand();
        }

        public IEnumerator RemoveDeadCards(CharacterCard charCard)
        {
            var deadCards = _cardInHand.Where(sc => sc.BaseCard.Owner == charCard).ToList();
            foreach(var c in deadCards)
            {
                yield return c.DestroySkill(1);
                RemoveCard(c);
            }
        }
    }
}

