using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardWar.Entities;
using CardWar.Enums;
using CardWar.Factories;
using CardWar.Untils;
using CardWar.Views;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.EventSystems.PointerEventData;

namespace CardWar.GameViews
{
    public class IngameSceneView : Singleton<IngameSceneView>
    {
        // This class can be used to manage the overall scene view for the game.
        // It can include references to various UI elements, game state, and other components.

        // Example fields could include:
        // - Reference to the CardSelectorView
        // - Reference to the CardDetailView
        // - Methods to update the scene based on game events
        [SerializeField] private Canvas _mainCanvas;
        [SerializeField] private PlayerHandView _playerHandView;
        [SerializeField] private PlayerDeckView _playerDeckView;
        [SerializeField] private BoardView _boardView;
        [SerializeField] private CardDetailView _cardDetailView;
        [SerializeField] private CardSelectorView _cardSelectorView;

        private UnityEvent DoCardAnimation = new();

        void Start()
        {
            _cardDetailView.HideCardDetail();
            _cardSelectorView.HideCardSelector();
            _boardView.Initialize();
        }

        #region Draw Card Logic
        public void DrawCard()
        {
            // TODO: Check in turn
            // TODO: Check deck empty
            _playerDeckView.DrawCard(out var drawnCard);
            _playerHandView.AddCardToHand(drawnCard, out var cardView);
            cardView?.OnCardClicked.AddListener((e) =>
            {
                if (e.button == InputButton.Right)
                {
                    _cardDetailView.ShowCardDetail(cardView.BaseCard);
                }
            });
        }
        #endregion

        #region Summon Condition Logic
        public async Task<List<Card>> CheckSummonConditions(MonsterCard mCard)
        {
            var requiredSacrificesCount = mCard.SummonCondiction.Sacrifces.Sum(s => s.Amount);

            var selectedCards = await _cardSelectorView.ShowCardToSelect(_boardView.CardsInRegion, requiredSacrificesCount);
            if (selectedCards == null)
            {
                //TODO: Show warning
                Debug.LogWarning("Cancel selection");
                return null;
            }

            return selectedCards;
        }
        #endregion

        #region Play Card Logic
        private async void PlayCard(Card card, RegionView fromRegion)
        {
            // TODO: Check in turn
            if (card.CardType == ECardType.Spell)
            {
                // _boardView.AddCardToSlot(card, availableSlots[0]);
                // fromRegion.RemoveCard(card);
                return;
            }

            // TODO: Check summon conditions
            if (card is MonsterCard mCard && mCard.SummonCondiction.Sacrifces.Length > 0)
            {
                var sacrificedCards = await CheckSummonConditions(mCard);
                if (sacrificedCards == null)
                {
                    Debug.Log("Summon failed or canceled");
                    return;
                }
            }

            List<SlotView> availableSlots = new();
            _boardView.ShowAvailableSlots(card.CardType, out availableSlots);

            IEnumerator onClick(SlotView slot)
            {
                // TODO: Consume sacrifice (if needed)

                fromRegion.RemoveCard(card);
                _boardView.AddCardToSlot(card, slot);
                _boardView.HideAllSlots();
                foreach (var s in availableSlots)
                {
                    s.OnSlotClicked.RemoveAllListeners();
                }

                //Do animation
                // DoCardAnimation?.Invoke();
                // DoCardAnimation.RemoveAllListeners();
                yield return StartCoroutine(PlayCardAnimation(card, fromRegion, slot));

                //Spawn model, may be optimize later
                yield return StartCoroutine(SpawnModelAnimation(card, slot));

                // Debug.Log($"Card {card.Name} placed in slot at position {slot.transform.position}");
            }
            ;

            availableSlots.ForEach(slot => slot.OnSlotClicked.AddListener(() => StartCoroutine(onClick(slot))));
        }

        public IEnumerator PlayCardAnimation(Card card, RegionView fromRegion, SlotView toSlot)
        {
            var animLength = 0.3f;

            var canvasTransform = _mainCanvas.gameObject.GetComponent<RectTransform>();
            var curPosition = fromRegion.gameObject.GetComponent<RectTransform>().position;
            var cardView = CardFactory.Instance.CreateCardView(card, curPosition, parent: canvasTransform);
            var cardRectTransform = cardView.GetComponent<RectTransform>();

            var slotWorldPosition = toSlot.transform.position;
            var slotScreenPosition = Camera.main.WorldToScreenPoint(slotWorldPosition);
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                canvasTransform,
                slotScreenPosition,
                null,
                out var slotCanvasPosition
            );

            var sequence = DOTween.Sequence();
            sequence.Append(cardRectTransform.DOMove(slotCanvasPosition, animLength).SetEase(Ease.InOutQuad));
            sequence.AppendInterval(.2f);
            sequence.OnComplete(() =>
            {
                CardFactory.Instance.RecycleCardView(cardView);
            });
            yield return sequence.WaitForCompletion();
        }

        public IEnumerator SpawnModelAnimation(Card card, SlotView slot)
        {
            var animLength = 0.5f;
            var sequence = DOTween.Sequence();

            var model = CardFactory.Instance.CreateCardModel(card, parent: slot.transform).Model;
            // var model = Instantiate(card.Model, slot.transform);

            if (card.CardType == ECardType.Monster)
            {
                sequence.Append(model.transform.DORotate(new Vector3(0, 360, 0), animLength, RotateMode.FastBeyond360).SetEase(Ease.InOutQuad));
                sequence.AppendInterval(.2f);
                sequence.OnComplete(() =>
                {
                    model.transform.rotation = Quaternion.Euler(0, 0, 0);
                });
            }

            yield return sequence.WaitForCompletion();
        }

        public void PlaySelectedCardInHand()
        {
            if (_playerHandView.SelectedCardView == null) return;
            PlayCard(_playerHandView.SelectedCardView.BaseCard, _playerHandView);
        }
        #endregion
    }
}