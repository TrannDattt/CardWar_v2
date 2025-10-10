using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardWar.Entities;
using CardWar.Enums;
using CardWar.Factories;
using CardWar.GameControl;
using CardWar.Interfaces;
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
        [SerializeField] private PlayerHandView _selfHandView;
        [SerializeField] private PlayerDeckView _selfDeckView;
        [SerializeField] private BoardView _boardView;
        [SerializeField] private GraveView _graveView;
        [SerializeField] private CardDetailView _cardDetailView;
        [SerializeField] private CardSelectorView _cardSelectorView;

        [SerializeField] private PlayerDeckView _enemyDeckView;
        [SerializeField] private PlayerHandView _enemyHandView;

        private EPlayerTarget _curTurn => GameplayManager.Instance.CurTurn;
        private EPlayerTarget _playerMiniTurn => GameplayManager.Instance.PlayerMiniTurn;
        private EPhase _curPhase => GameplayManager.Instance.CurPhase.Type;

        #region Draw Card Logic
        private async Task DrawCardAnimation(CardView cardView, RegionView fromRegion, PlayerHandView toHand, Action callback = null)
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

            var toRect = toHand.GetComponent<RectTransform>();
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

            await sequence.AsyncWaitForCompletion();
        }

        public async Task DrawCard(EPlayerTarget targetPlayer, int amount = 1)
        {
            // Debug.Log($"Drawn {amount} cards from {targetPlayer}'s deck to {targetPlayer}'s hand");
            var fromDeck = targetPlayer == EPlayerTarget.Self ? _selfDeckView : _enemyDeckView;
            var toHand = targetPlayer == EPlayerTarget.Self ? _selfHandView : _enemyHandView;

            for (int a = 0; a < amount; a++)
            {
                fromDeck.DrawCard(out var drawnCard);
                if (drawnCard == null)
                {
                    Debug.Log($"Failed to draw from {targetPlayer}'s deck");
                    break;
                }

                await DrawCardAnimation(drawnCard, fromDeck, toHand, () =>
                {
                    toHand.AddCardToHand(drawnCard);
                });

                drawnCard.OnCardClicked.AddListener((e) =>
                {
                    if (e.button == InputButton.Right)
                    {
                        _cardDetailView.ShowCardDetail(drawnCard.BaseCard);
                    }
                });
            }
        }
        #endregion

        #region Do Sacrifice Logic
        public async Task<List<Card>> CheckSummonConditions(MonsterCard mCard)
        {
            var requiredSacrificesCount = mCard.SummonCondiction.Sacrifces.Sum(s => s.Amount);

            var selectedCards = await _cardSelectorView.ShowCardToSelect(_boardView.GetCardsInPlayerRegion(EPlayerTarget.Self),
                                                                        checkFunc: cards => CheckSacrificeMetRequired(mCard, cards));
            if (selectedCards == null)
            {
                return null;
            }

            return selectedCards;
        }

        private bool CheckSacrificeMetRequired(MonsterCard mCard, List<Card> selectedCards)
        {
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
        #endregion

        #region Destroy Card Logic
        private void DestroyCard(Card card, EPlayerTarget owner)
        {
            _boardView.RemoveCard(card, owner);
            _graveView.AddCardToGrave(card, owner);
        }

        private async Task DestroyCardAnimation(Card card, EPlayerTarget owner, Action callback = null)
        {
            // var animLength = 0.5f;
            var canvasTransform = _mainCanvas.GetComponent<RectTransform>();
            var toPos = _graveView.GetComponent<RectTransform>().position;

            var fromSlot = _boardView.GetSlotByCard(card, owner);
            var fromSlotWorldPosition = fromSlot.transform.position;
            var fromSlotScreenPosition = Camera.main.WorldToScreenPoint(fromSlotWorldPosition);
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                canvasTransform,
                fromSlotScreenPosition,
                null,
                out var fromSlotCanvasPosition
            );

            var cardModel = fromSlot.GetComponentInChildren<CardModelView>();
            cardModel.gameObject.SetActive(false);

            var cardView = CardFactory.Instance.CreateCardView(card, fromSlotCanvasPosition);
            var cardRectTransform = cardView.GetComponent<RectTransform>();

            var sequence = DOTween.Sequence();
            sequence.Append(cardRectTransform.DOMove(toPos, .5f).SetEase(Ease.InOutQuad));
            sequence.Join(cardRectTransform.DOScale(.5f * Vector3.one, .5f).SetEase(Ease.InOutQuad));
            sequence.AppendInterval(.2f);
            //TODO: Add anim for grave effect
            sequence.OnComplete(() =>
            {
                CardFactory.Instance.RecycleCardView(cardView);
                DOTween.Kill(cardModel.transform);
                callback?.Invoke();
            });

            await sequence.AsyncWaitForCompletion();
        }
        #endregion

        #region Play Card Logic
        private async void PlayCard(Card card, EPlayerTarget owner, RegionView fromRegion)
        {
            var fromHand = owner == EPlayerTarget.Self ? _selfHandView : _enemyHandView;

            if (card.CardType == ECardType.Spell)
            {
                var spellSlot = _boardView.GetPlayerSlots(ECardType.Spell, EPlayerTarget.Self)[0];

                async Task PlaySpellCard()
                {
                    fromRegion.RemoveCard(card);
                    _boardView.AddCardToSlot(card, spellSlot);
                    _boardView.HideAllSlots();

                    await PlayCardAnimation(card, fromRegion, spellSlot);

                    // TODO: Add spell to spell queue and remove card from slot. 
                    // Wait until queue is resolved, play summon model anim, apply effects and then move to grave
                }

                await PlaySpellCard();

                return;
            }

            List<Card> sacrificedCards = new();
            List<SlotView> availableSlots = new();
            List<Card> sacrificeCopy = new();

            // --------TEST--------
            //TODO: Auto select target if in enemy's turn
            if (owner == EPlayerTarget.Enemy)
            {
                if (card is MonsterCard mCard && mCard.SummonCondiction.Sacrifces.Length > 0)
                {
                    var availableSacrifices = _boardView.GetCardsInPlayerRegion(EPlayerTarget.Enemy).OfType<MonsterCard>().ToList();
                    foreach (var sac in mCard.SummonCondiction.Sacrifces)
                    {
                        var selectedSac = availableSacrifices.Where(s => s.Tier == sac.Tier).Take(sac.Amount);
                        // if (selectedSac.Count() != sac.Amount)
                        // {
                        //     Debug.Log("Not enough sacrifices for enemy to summon");
                        //     return;
                        // }
                        sacrificedCards.AddRange(selectedSac);
                        availableSacrifices = availableSacrifices.Except(selectedSac).ToList();
                    }
                    sacrificeCopy = new List<Card>(sacrificedCards);
                }

                var toSlot = _boardView.GetPlayerSlots(ECardType.Monster, EPlayerTarget.Enemy, false).FirstOrDefault();
                if (toSlot == null)
                {
                    Debug.Log("No available slot for enemy to summon");
                    return;
                }
                await PlayCardToSlot(toSlot, EPlayerTarget.Enemy);
            }
            else
            {

                // --------------------
                if (card is MonsterCard mCard && mCard.SummonCondiction.Sacrifces.Length > 0)
                {
                    sacrificedCards = await CheckSummonConditions(mCard);
                    if (sacrificedCards == null)
                    {
                        Debug.Log("Summon failed or canceled");
                        return;
                    }
                }

                _boardView.ShowAvailableSlots(card.CardType, out availableSlots);

                sacrificeCopy = new List<Card>(sacrificedCards);
                availableSlots.ForEach(slot => slot.OnSlotClicked.AddListener(async () => await PlayCardToSlot(slot, EPlayerTarget.Self)));
            }

            async Task PlayCardToSlot(SlotView slot, EPlayerTarget owner)
            {
                fromRegion.RemoveCard(card);
                _boardView.AddCardToSlot(card, slot);
                _boardView.HideAllSlots();
                foreach (var s in availableSlots)
                {
                    s.OnSlotClicked.RemoveAllListeners();
                }

                //Do animation
                if (card is MonsterCard mCard && mCard.SummonCondiction.Sacrifces.Length > 0)
                {
                    foreach (var s in sacrificeCopy)
                    {
                        await DestroyCardAnimation(s, owner, () =>
                        {
                            DestroyCard(s, owner);
                        });
                    }
                }

                await PlayCardAnimation(card, fromRegion, slot);
                await SpawnModelAnimation(card, slot);

                //TODO: Make an Unity Event ???
                // ChangeMiniTurn();
                if(owner == EPlayerTarget.Self)
                {
                    GameplayManager.Instance.ChangeToNextMiniTurn();
                }
            }
            ;
        }

        private async Task PlayCardAnimation(Card card, RegionView fromRegion, SlotView toSlot, Action callback = null)
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
            await sequence.AsyncWaitForCompletion();
        }

        private async Task SpawnModelAnimation(Card card, SlotView slot, Action callback = null)
        {
            // var animLength = 0.5f;
            var sequence = DOTween.Sequence();

            var cardModel = CardFactory.Instance.CreateCardModel(card, slot.transform.position, Quaternion.identity, slot.transform);
            var model = cardModel.Model;
            model.transform.rotation = slot.transform.rotation;

            var baseScale = model.transform.localScale;
            var baseRotation = model.transform.rotation;

            if (card.CardType == ECardType.Monster)
            {
                model.transform.localScale = .1f * Vector3.one;
                sequence.Append(model.transform.DORotate(new Vector3(0, 360, 0), .5f, RotateMode.FastBeyond360).SetEase(Ease.InOutQuad));
                sequence.Join(model.transform.DOScale(baseScale, .5f).SetEase(Ease.OutBack, overshoot: 2f));
                sequence.AppendInterval(.2f);
                sequence.OnComplete(() =>
                {
                    model.transform.rotation = baseRotation;
                    cardModel.OnModelClicked.AddListener((e) =>
                    {
                        if (e.button == InputButton.Right)
                        {
                            _cardDetailView.ShowCardDetail(cardModel.BaseCard);
                        }
                        else if (e.button == InputButton.Left)
                        {
                            //TODO: Toggle model selection for next action ????
                        }
                    });
                    callback?.Invoke();
                });
            }

            await sequence.AsyncWaitForCompletion();
        }

        public void PlaySelectedCardInHand()
        {
            if (_playerMiniTurn == EPlayerTarget.Enemy)
            {
                Debug.LogWarning("It's not your turn!");
                return;
            }

            if (_selfHandView.SelectedCardView == null) return;
            PlayCard(_selfHandView.SelectedCardView.BaseCard, EPlayerTarget.Self, _selfHandView);
        }

        // ---------TEST--------
        // TODO: Enemy auto play a card that can be summoned
        private bool CheckCanSummonCard(Card card)
        {
            if (_boardView.GetPlayerSlots(card.CardType, EPlayerTarget.Enemy, false).Count == 0)
            {
                return false;
            }

            if (card is MonsterCard mCard && mCard.SummonCondiction.Sacrifces.Length > 0)
            {
                var availableSacrifices = _boardView.GetCardsInPlayerRegion(EPlayerTarget.Enemy).OfType<MonsterCard>().ToList();
                foreach (var sac in mCard.SummonCondiction.Sacrifces)
                {
                    var selectedSac = availableSacrifices.Where(s => s.Tier == sac.Tier).Take(sac.Amount);
                    if (selectedSac.Count() != sac.Amount)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void EnemyAutoPlayCard()
        {
            // if (_playerMiniTurn == EPlayerTarget.Self)
            // {
            //     Debug.LogWarning("It's not your turn!");
            //     return;
            // }

            var cardToSummon = _enemyHandView.CardsInRegion.FirstOrDefault(c => CheckCanSummonCard(c));
            if (cardToSummon == null)
            {
                Debug.Log("Enemy has no valid card to play");
                return;
            }
            PlayCard(cardToSummon, EPlayerTarget.Enemy, _enemyHandView);
        }
        // --------------------
        #endregion

        #region Monster Skill Logic
        //TODO: Monster use skill and add to skill queue
        #endregion

        #region Card Attack Logic
        public async void DoCardsAttack()
        {
            var attackerPlayer = _curTurn;
            var targetPlayer = attackerPlayer == EPlayerTarget.Self ? EPlayerTarget.Enemy : EPlayerTarget.Self;
            var attackerCards = _boardView.GetCardsInPlayerRegion(attackerPlayer).OfType<MonsterCard>().ToList();

            foreach (var attacker in attackerCards)
            {
                //TODO: MC will attack player if no target on board
                var targets = _boardView.GetCardsInPlayerRegion(targetPlayer).ToList();
                Card target = null;

                // // --------TEST--------
                // //TODO: Auto select target if in enemy's turn
                if (attackerPlayer == EPlayerTarget.Enemy)
                {
                    //     target = targets.OrderBy(t => (t as IDamagable).Hp).FirstOrDefault();
                    //     await PlayAttackAniamtion(attackerPlayer, targetPlayer, attacker, target, () =>
                    //     {
                    //         //TODO: Do damage to player if attacker's ATK > target's HP
                    //         if ((target as IDamagable).Hp <= 0)
                    //         {
                    //             DestroyCard(target, targetPlayer);
                    //         }
                    //         else if(target is MonsterCard targetMonster)
                    //         {
                    //             attacker.TakeDamage(targetMonster.Atk);
                    //             // Debug.Log($"1_Attacker {attacker}'s HP after attack: {attacker.Hp}");
                    //         }
                    //     });
                    continue;
                }
                // --------------------

                Debug.Log($"[Attack Phase] {attacker.Name} is going to attack...");

                var selectedTarget = await _cardSelectorView.ShowCardToSelect(targets);

                if (selectedTarget == null || selectedTarget.Count == 0)
                {
                    Debug.LogWarning($"Bỏ qua lượt tấn công của {attacker.Name} (không có mục tiêu).");
                    continue;
                }

                target = selectedTarget.First();
                await PlayAttackAniamtion(attackerPlayer, targetPlayer, attacker, target, () =>
                {
                    //TODO: Do damage to player if attacker's ATK > target's HP
                    if ((target as IDamagable).Hp <= 0)
                    {
                        DestroyCard(target, targetPlayer);
                    }
                    else if (target is MonsterCard targetMonster)
                    {
                        attacker.TakeDamage(targetMonster.Atk);
                        // Debug.Log($"1_Attacker {attacker}'s HP after attack: {attacker.Hp}");
                    }
                });
            }

            // ChangeToNextTurn();
        }

        private async Task PlayAttackAniamtion(EPlayerTarget attacker, EPlayerTarget target, MonsterCard attackerCard, Card targetCard, Action callback = null)
        {
            var attackerModel = _boardView.GetSlotByCard(attackerCard, attacker).GetComponentInChildren<CardModelView>();
            var targetModel = _boardView.GetSlotByCard(targetCard, target).GetComponentInChildren<CardModelView>();

            Vector3 offset = new(0, 0, 2);
            var startPos = attackerModel.transform.position;
            var moveDir = targetModel.transform.position - startPos;
            var finalPos = startPos + (moveDir - Mathf.Sign(moveDir.z) * offset);
            var rotateAngle = Quaternion.LookRotation(moveDir);

            var sequence = DOTween.Sequence();
            sequence.Append(attackerModel.Model.transform.DORotateQuaternion(rotateAngle, .1f).SetEase(Ease.InOutQuad));
            sequence.Append(attackerModel.transform.DOMove(finalPos, .5f).SetEase(Ease.InOutQuad));
            sequence.AppendCallback(() =>
            {
                (attackerModel.BaseCard as MonsterCard).DoAttack(targetCard as IDamagable);
            });
            // sequence.AppendInterval(2f);
            sequence.Append(attackerModel.Model.transform.DORotateQuaternion(Quaternion.Euler(0, 180, 0), .1f).SetEase(Ease.InOutQuad));
            sequence.Append(attackerModel.transform.DOMove(startPos, .5f).SetEase(Ease.InOutQuad));
            sequence.Append(attackerModel.Model.transform.DORotateQuaternion(Quaternion.Euler(0, 180, 0) * Quaternion.Inverse(rotateAngle), .1f).SetEase(Ease.InOutQuad));
            sequence.OnComplete(() =>
            {
                attackerModel.Model.transform.rotation = Quaternion.Euler(0, 0, 0);
                // Debug.Log($"3_Attacker {attackerCard}'s HP after attack: {attackerCard.Hp}");
                callback?.Invoke();
            });

            await sequence.AsyncWaitForCompletion();
        }

        // --------TEST-------
        //TODO: Enemy auto choose target to attack
        public async void AutoDoCardAttack()
        {
            if (_curTurn != EPlayerTarget.Enemy) return;

            var attackerPlayer = _curTurn;
            var targetPlayer = attackerPlayer == EPlayerTarget.Self ? EPlayerTarget.Enemy : EPlayerTarget.Self;
            var attackerCards = _boardView.GetCardsInPlayerRegion(attackerPlayer).OfType<MonsterCard>().ToList();

            foreach (var attacker in attackerCards)
            {
                //TODO: MC will attack player if no target on board
                var targets = _boardView.GetCardsInPlayerRegion(targetPlayer).ToList();
                Card target = null;

                target = targets.OrderBy(t => (t as IDamagable).Hp).FirstOrDefault();
                await PlayAttackAniamtion(attackerPlayer, targetPlayer, attacker, target, () =>
                {
                    //TODO: Do damage to player if attacker's ATK > target's HP
                    if ((target as IDamagable).Hp <= 0)
                    {
                        DestroyCard(target, targetPlayer);
                    }
                    else if (target is MonsterCard targetMonster)
                    {
                        attacker.TakeDamage(targetMonster.Atk);
                        // Debug.Log($"1_Attacker {attacker}'s HP after attack: {attacker.Hp}");
                    }
                });
            }

            // ChangeToNextTurn();
        }
        // -------------------
        #endregion

        #region Init For Testing
        public void InitScene()
        {
            _cardDetailView.HideCardDetail();
            _cardSelectorView.HideCardSelector();
            _boardView.Initialize();
        }
        #endregion
    }
}