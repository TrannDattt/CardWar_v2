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

        // TODO: Should make curve for animation

        [SerializeField] private Canvas _mainCanvas;
        [SerializeField] private PlayerHandView _playerHandView;
        [SerializeField] private PlayerDeckView _playerDeckView;
        [SerializeField] private BoardView _boardView;
        [SerializeField] private GraveView _graveView;
        [SerializeField] private CardDetailView _cardDetailView;
        [SerializeField] private CardSelectorView _cardSelectorView;

        void Start()
        {
            _cardDetailView.HideCardDetail();
            _cardSelectorView.HideCardSelector();
            _boardView.Initialize();
        }

        #region Draw Card Logic
        public static Vector2 ConvertAnchoredPosition(RectTransform from, RectTransform to, Vector2 anchoredPos)
        {
            Vector3 worldPos = from.TransformPoint(anchoredPos);
            Vector3 localPos = to.InverseTransformPoint(worldPos);
            return localPos;
        }

        private IEnumerator DrawCardAnimation(CardView cardView, RegionView fromRegion, Action callback = null)
        {
            // var animLength = 0.5f;
            var handOffset = new Vector3(0, 20);
            var canvasTransform = _mainCanvas.GetComponent<RectTransform>();
            Vector2 startPos;
            var mainCam = Camera.main;
            if (fromRegion is BoardView)
            {
                var slotWorlPos = _boardView.GetSlotByCard(cardView.BaseCard).transform.position;
                var slotScreenPos = Camera.main.WorldToScreenPoint(slotWorlPos);
                RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    canvasTransform,
                    slotScreenPos,
                    null,
                    out var canvasPos
                );
                startPos = canvasPos;
            }
            else
            {
                var fromRect = fromRegion.GetComponent<RectTransform>();
                startPos = fromRect.TransformPoint(fromRect.rect.center);
            }

            var toRect = _playerHandView.GetComponent<RectTransform>();
            var endPos = toRect.TransformPoint(toRect.rect.center);
            var cardRectTransform = cardView.GetComponent<RectTransform>();
            cardRectTransform.position = startPos;

            var sequence = DOTween.Sequence();
            sequence.Append(cardRectTransform.DOMove(endPos, 0.3f).SetEase(Ease.InOutQuad));
            sequence.Append(cardRectTransform.DORotateQuaternion(Quaternion.identity, .2f).SetEase(Ease.InOutQuad));
            sequence.AppendInterval(.2f);
            sequence.OnComplete(() =>
            {
                cardView.transform.rotation = Quaternion.identity;
                callback?.Invoke();
            });

            yield return sequence.WaitForCompletion();
        }

        public void DrawCard()
        {
            // TODO: Check in turn and auto draw
            _playerDeckView.DrawCard(out var drawnCard);
            if (drawnCard == null) return;

            StartCoroutine(DrawCardAnimation(drawnCard, _playerDeckView, () =>
            {
                _playerHandView.AddCardToHand(drawnCard);
            }));

            drawnCard.OnCardClicked.AddListener((e) =>
            {
                if (e.button == InputButton.Right)
                {
                    _cardDetailView.ShowCardDetail(drawnCard.BaseCard);
                }
            });
        }
        #endregion

        #region Do Sacrifice Logic
        public async Task<List<Card>> CheckSummonConditions(MonsterCard mCard)
        {
            var requiredSacrificesCount = mCard.SummonCondiction.Sacrifces.Sum(s => s.Amount);

            var selectedCards = await _cardSelectorView.ShowCardToSelect(_boardView.CardsInRegion,
                                                                        cards => CheckSacrificeMetRequired(mCard, cards));
            if (selectedCards == null)
            {
                return null;
            }

            return selectedCards;
        }

        private bool CheckSacrificeMetRequired(MonsterCard mCard, List<Card> selectedCards) {
            if (!selectedCards.All(c => c.GetType() == typeof(MonsterCard))) return false;

            var requiredSac = mCard.SummonCondiction.Sacrifces;
            var selectedMCards = selectedCards.OfType<MonsterCard>().ToList();
            foreach (var sac in requiredSac)
            {
                var selectedSac = selectedMCards.Where(s => s.Tier == sac.Tier);
                if (selectedSac.Count() != sac.Amount) return false;
            }

            return true;
        }

        private void DoSacrifice(List<Card> sacrifices)
        {
            sacrifices.ForEach(c =>
            {
                _boardView.RemoveCard(c);
                _graveView.AddCardToGrave(c);
            });
        }
        
        private IEnumerator PlaySacrificeAnimation(List<Card> sacrifices, SlotView selectedSlot, Action callback = null)
        {
            // var animLength = 0.5f;
            var canvasTransform = _mainCanvas.GetComponent<RectTransform>();
            var toSlotWorldPosition = selectedSlot.transform.position;
            var toSlotScreenPosition = Camera.main.WorldToScreenPoint(toSlotWorldPosition);
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                canvasTransform,
                toSlotScreenPosition,
                null,
                out var toSlotCanvasPosition
            );

            var sequence = DOTween.Sequence();
            sacrifices.ForEach(s =>
            {
                var fromSlot = _boardView.GetSlotByCard(s);
                var fromSlotWorldPosition = fromSlot.transform.position;
                var fromSlotScreenPosition = Camera.main.WorldToScreenPoint(fromSlotWorldPosition);
                RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    canvasTransform,
                    fromSlotScreenPosition,
                    null,
                    out var fromSlotCanvasPosition
                );
                var cardView = CardFactory.Instance.CreateCardView(s, fromSlotCanvasPosition);
                var cardRectTransform = cardView.GetComponent<RectTransform>();
                sequence.Append(cardRectTransform.DOMove(toSlotScreenPosition, .3f).SetEase(Ease.InOutQuad));
                sequence.Join(cardRectTransform.DOScale(.1f * Vector3.one, .15f).SetEase(Ease.InOutQuad).SetDelay(.15f));
                sequence.AppendInterval(.1f);
                sequence.AppendCallback(() =>
                {
                    CardFactory.Instance.RecycleCardView(cardView);
                });
            });

            sequence.AppendCallback(() =>
            {
                callback?.Invoke();
            });

            yield return sequence.WaitForCompletion();
        }
        #endregion

        #region Play Card Logic
        private async void PlayCard(Card card, RegionView fromRegion)
        {
            // TODO: Check in turn
            if (card.CardType == ECardType.Spell)
            {
                var spellSlot = _boardView.GetSlotsByCardType(ECardType.Spell)[0];

                IEnumerator PlaySpellCard()
                {
                    fromRegion.RemoveCard(card);
                    _boardView.AddCardToSlot(card, spellSlot);
                    _boardView.HideAllSlots();

                    yield return StartCoroutine(PlayCardAnimation(card, fromRegion, spellSlot));
                    
                    // TODO: Add spell to spell queue and remove card from slot. 
                    // Wait until queue is resolved, play summon model anim, apply effects and then move to grave
                }

                StartCoroutine(PlaySpellCard());

                return;
            }

            List<Card> sacrificedCards = new();
            if (card is MonsterCard mCard && mCard.SummonCondiction.Sacrifces.Length > 0)
            {
                sacrificedCards = await CheckSummonConditions(mCard);
                if (sacrificedCards == null)
                {
                    Debug.Log("Summon failed or canceled");
                    return;
                }
            }
            List<SlotView> availableSlots = new();
            _boardView.ShowAvailableSlots(card.CardType, out availableSlots);

            var sacrificeCopy = new List<Card>(sacrificedCards);
            IEnumerator PlayCardToSlot(SlotView slot)
            {
                fromRegion.RemoveCard(card);
                _boardView.AddCardToSlot(card, slot);
                _boardView.HideAllSlots();
                foreach (var s in availableSlots)
                {
                    s.OnSlotClicked.RemoveAllListeners();
                }

                //Do animation
                yield return StartCoroutine(PlayCardAnimation(card, fromRegion, slot, () =>
                {
                    if (card is MonsterCard mCard && mCard.SummonCondiction.Sacrifces.Length > 0)
                    {
                        StartCoroutine(PlaySacrificeAnimation(sacrificeCopy, slot, () =>
                        {
                            DoSacrifice(sacrificeCopy);
                        }));
                    }
                }));


                yield return StartCoroutine(SpawnModelAnimation(card, slot));
            }
            ;

            availableSlots.ForEach(slot => slot.OnSlotClicked.AddListener(() => StartCoroutine(PlayCardToSlot(slot))));
        }

        private IEnumerator PlayCardAnimation(Card card, RegionView fromRegion, SlotView toSlot, Action callback = null)
        {
            var animLength = 0.3f;

            var canvasTransform = _mainCanvas.GetComponent<RectTransform>();
            var curPosition = fromRegion.GetComponent<RectTransform>().position;
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
            sequence.Append(cardRectTransform.DOMove(slotScreenPosition, animLength).SetEase(Ease.InOutQuad));
            sequence.AppendInterval(.2f);
            sequence.OnComplete(() =>
            {
                callback?.Invoke();
                CardFactory.Instance.RecycleCardView(cardView);
            });
            yield return sequence.WaitForCompletion();
        }

        private IEnumerator SpawnModelAnimation(Card card, SlotView slot, Action callback = null)
        {
            // var animLength = 0.5f;
            var sequence = DOTween.Sequence();

            var cardModel = CardFactory.Instance.CreateCardModel(card, parent: slot.transform);
            var model = cardModel.Model;
            var baseScale = model.transform.localScale;
            model.transform.localScale = .1f * Vector3.one;
            // var model = Instantiate(card.Model, slot.transform);

            if (card.CardType == ECardType.Monster)
            {
                sequence.Append(model.transform.DORotate(new Vector3(0, 360, 0), .5f, RotateMode.FastBeyond360).SetEase(Ease.InOutQuad));
                sequence.Join(model.transform.DOScale(baseScale, .5f).SetEase(Ease.OutBack, overshoot: 2f));
                sequence.AppendInterval(.2f);
                sequence.OnComplete(() =>
                {
                    model.transform.rotation = Quaternion.Euler(0, 0, 0);
                    cardModel.OnModelClicked.AddListener((e) =>
                    {
                        if (e.button == InputButton.Right)
                        {
                            _cardDetailView.ShowCardDetail(cardModel.BaseCard);
                        }
                        else if (e.button == InputButton.Left)
                        {
                            //TODO: Toggle model selection for next action
                        }
                    });
                    callback?.Invoke();
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