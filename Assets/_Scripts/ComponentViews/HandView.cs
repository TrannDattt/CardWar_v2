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

        public void ArrangeHand(bool excludeLast = false)
        {
            // Debug.Log($"Arrange card");
            int cardCount = _cardInHand.Count;
            if (cardCount == 0) return;
            // var firstPos = transform.position + new Vector3((-cardCount) / 2 * _spaceX, 0, (-cardCount) / 2 * _spaceZ);
            var firstPos = transform.position + (-cardCount) / 2 * _space;

            _cardInHand[0].transform.DOMove(firstPos, .2f).SetEase(Ease.InOutQuad);
            for (int i = 1; i < cardCount; i++)
            {
                // if (i == cardCount - 1 && excludeLast) continue;
                var cardPos = firstPos + i * _space;
                _cardInHand[i].transform.DOMove(cardPos, .2f).SetEase(Ease.InOutQuad);
            }
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

            // cardView.OnCardDrop.AddListener(async () =>
            // {
            //     if (CheckCardInHandRange(cardView))
            //     {
            //         _selectedCard = null;
            //         if (_cardInHand.Contains(cardView)) return;
            //         Debug.Log($"Insert card {cardView.BaseCard} to index {_selectedCardIndex}");
            //         _cardInHand.Insert(_selectedCardIndex, cardView);
            //         ArrangeHand();
            //     }
            //     else
            //     {
            //         IngameSceneView.Instance.SelfPlayCard(cardView);
            //     }
            // });

            // cardView.OnCardGrab.AddListener(async () =>
            // {
            //     _selectedCard = cardView;
            //     _selectedCardIndex = _cardInHand.IndexOf(cardView);
            //     _cardInHand.Remove(cardView);
            //     ArrangeHand();
            // });

            ArrangeHand(true);
        }

        public void RemoveCard(SkillCardView cardView)
        {
            if (!_cardInHand.Contains(cardView) && _selectedCard != cardView) return;
            _cardInHand.Remove(cardView);

            ArrangeHand();
        }

        public async Task RemoveDeadCards(CharacterCard charCard)
        {
            var deadCards = _cardInHand.Where(sc => sc.BaseCard.Owner == charCard).ToList();
            foreach(var c in deadCards)
            {
                await c.DestroySkill(1);
                RemoveCard(c);
            }
        }
    }
}

