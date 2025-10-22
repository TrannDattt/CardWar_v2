using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardWar.Interfaces;

// using CardWar.Factories;
using CardWar.Untils;
using CardWar_v2.Datas;
using CardWar_v2.Entities;
using CardWar_v2.Enums;
using CardWar_v2.Factories;
using CardWar_v2.GameControl;
using CardWar_v2.Views;

// using CardWar.Views;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.EventSystems.PointerEventData;

namespace CardWar_v2.GameViews
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
        [SerializeField] private HandView _handView;
        [SerializeField] private DeckView _deckView;
        [SerializeField] private BoardView _boardView;
        [SerializeField] private SkillQueueView _skillQueueView;

        [SerializeField] private CardDetailView _cardDetailView;
        //         [SerializeField] private CardSelectorView _cardSelectorView;

        //         [SerializeField] private PlayerDeckView _enemyDeckView;
        //         [SerializeField] private PlayerHandView _enemyHandView;

        //         private EPlayerTarget _curTurn => GameplayManager.Instance.CurTurn;
        //         private EPlayerTarget _playerMiniTurn => GameplayManager.Instance.PlayerMiniTurn;
        //         private EPhase _curPhase => GameplayManager.Instance.CurPhase.Type;

        #region Draw Card
        private async Task DrawCardAnimation(SkillCardView cardView, Action callback = null)
        {
            // var animLength = 0.5f;
            var endPos = _handView.GetCardPos(default);
            // Debug.Log($"Next pos: {endPos}");
            var cardTransform = cardView.transform;

            var sequence = DOTween.Sequence();
            sequence.Append(cardTransform.DOMove(endPos, 0.3f).SetEase(Ease.InOutQuad));
            sequence.Append(cardTransform.DORotateQuaternion(Quaternion.identity, .2f).SetEase(Ease.InOutQuad));
            sequence.AppendInterval(.2f);
            sequence.OnComplete(() =>
            {
                cardView.transform.rotation = Quaternion.identity;
                callback?.Invoke();
            });

            await sequence.AsyncWaitForCompletion();
        }

        public async Task DrawCard(int amount = 1)
        {
            // Debug.Log($"Drawn {amount} cards from {targetPlayer}'s deck to {targetPlayer}'s hand");

            for (int a = 0; a < amount; a++)
            {
                _deckView.DrawCard(out var drawnCard);
                if (drawnCard == null)
                {
                    Debug.Log($"Failed to draw from Self's deck");
                    break;
                }

                await DrawCardAnimation(drawnCard, () =>
                {
                    _handView.AddCardToHand(drawnCard);
                });

                drawnCard.OnCardClick.AddListener(async (e) =>
                {
                    if (GameplayManager.Instance.CurPhase == EPhase.Opening)
                    {
                        if (e.button == InputButton.Right)
                        {
                            Debug.Log($"1.Show detail of card {drawnCard.BaseCard}");
                            await _cardDetailView.ShowSkillDetail(drawnCard.BaseCard);
                        }
                        else if (e.button == InputButton.Left)
                        {
                            if (drawnCard.GetComponentInParent<HandView>()) SelfPlayCard(drawnCard);
                            else if (drawnCard.GetComponentInParent<SkillQueueView>()) WithdrawCard(drawnCard);
                        }
                    }
                });
            }
        }
        #endregion

        #region Play Skill Card
        public async void SelfPlayCard(SkillCardView cardView)
        {
            var slot = _skillQueueView.GetNextEmptySlot();
            if (slot == null)
            {
                Debug.LogWarning("No more space in queue");
                return;
            }

            _handView.RemoveCard(cardView);
            _skillQueueView.AddCard(cardView);

            await PlayCardAnimation(cardView, slot);
        }

        public async Task AutoSelectCard(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                var skillCard = _enemyDeck.GetRandomCard();
                var slot = _skillQueueView.GetNextEmptySlot();
                var cardView = CardFactory.Instance.CreateCardView(skillCard, parent: slot.transform);
                await PlayCardAnimation(cardView, slot, () =>
                {
                    _skillQueueView.AddCard(cardView);
                });
            }
        }

        private async Task PlayCardAnimation(SkillCardView cardView, SkillSlotView toSlot, Action callback = null)
        {
            var animLength = 0.3f;

            var sequence = DOTween.Sequence();
            sequence.Append(cardView.transform.DOMove(toSlot.transform.position, animLength).SetEase(Ease.InOutQuad));
            sequence.Join(cardView.transform.DOScale(3 * Vector3.one, animLength).SetEase(Ease.InOutQuad));
            // sequence.AppendInterval(.2f);
            sequence.OnComplete(() =>
            {
                callback?.Invoke();
            });

            await sequence.AsyncWaitForCompletion();
        }

        public async void WithdrawCard(SkillCardView cardView)
        {
            _skillQueueView.RemoveCard(cardView, false);
            await WithdrawCardAnimation(cardView, () =>
            {
                _handView.AddCardToHand(cardView);
            });
        }

        private async Task WithdrawCardAnimation(SkillCardView cardView, Action callback = null)
        {
            var animLength = 0.3f;
            var endPos = _handView.GetCardPos(default);
            var cardTransform = cardView.transform;

            var sequence = DOTween.Sequence();
            sequence.Append(cardView.transform.DOMove(endPos, animLength).SetEase(Ease.InOutQuad));
            sequence.Join(cardView.transform.DOScale(Vector3.one, animLength).SetEase(Ease.InOutQuad));
            // sequence.AppendInterval(.2f);
            sequence.OnComplete(() =>
            {
                callback?.Invoke();
            });

            await sequence.AsyncWaitForCompletion();
        }

        public bool CheckQueueFull() => _skillQueueView.GetNextEmptySlot() == null;
        #endregion

        //          #region Play Character Card
        //         private async Task SpawnModelAnimation(Card card, SlotView slot, Action callback = null)
        //         {
        //             // var animLength = 0.5f;
        //             var sequence = DOTween.Sequence();

        //             var cardModel = CardFactory.Instance.CreateCardModel(card, slot.transform.position, Quaternion.identity, slot.transform);
        //             var model = cardModel.Model;
        //             model.transform.rotation = slot.transform.rotation;

        //             var baseScale = model.transform.localScale;
        //             var baseRotation = model.transform.rotation;

        //             if (card.CardType == ECardType.Monster)
        //             {
        //                 model.transform.localScale = .1f * Vector3.one;
        //                 sequence.Append(model.transform.DORotate(new Vector3(0, 360, 0), .5f, RotateMode.FastBeyond360).SetEase(Ease.InOutQuad));
        //                 sequence.Join(model.transform.DOScale(baseScale, .5f).SetEase(Ease.OutBack, overshoot: 2f));
        //                 sequence.AppendInterval(.2f);
        //                 sequence.OnComplete(() =>
        //                 {
        //                     model.transform.rotation = baseRotation;
        //                     cardModel.OnModelClicked.AddListener((e) =>
        //                     {
        //                         if (e.button == InputButton.Right)
        //                         {
        //                             _cardDetailView.ShowCardDetail(cardModel.BaseCard);
        //                         }
        //                         else if (e.button == InputButton.Left)
        //                         {
        //                             //TODO: Toggle model selection for next action ????
        //                         }
        //                     });
        //                     callback?.Invoke();
        //                 });
        //             }

        //             await sequence.AsyncWaitForCompletion();
        //         }

        //         public void PlaySelectedCardInHand()
        //         {
        //             if (_playerMiniTurn == EPlayerTarget.Enemy)
        //             {
        //                 Debug.LogWarning("It's not your turn!");
        //                 return;
        //             }

        //             if (_selfHandView.SelectedCardView == null) return;
        //             PlayCard(_selfHandView.SelectedCardView.BaseCard, EPlayerTarget.Self, _selfHandView);
        //         }

        //         // ---------TEST--------
        //         // TODO: Enemy auto play a card that can be summoned
        //         private bool CheckCanSummonCard(Card card)
        //         {
        //             if (_boardView.GetPlayerSlots(card.CardType, EPlayerTarget.Enemy, false).Count == 0)
        //             {
        //                 return false;
        //             }

        //             if (card is MonsterCard mCard && mCard.SummonCondiction.Sacrifces.Length > 0)
        //             {
        //                 var availableSacrifices = _boardView.GetCardsInPlayerRegion(EPlayerTarget.Enemy).OfType<MonsterCard>().ToList();
        //                 foreach (var sac in mCard.SummonCondiction.Sacrifces)
        //                 {
        //                     var selectedSac = availableSacrifices.Where(s => s.Tier == sac.Tier).Take(sac.Amount);
        //                     if (selectedSac.Count() != sac.Amount)
        //                     {
        //                         return false;
        //                     }
        //                 }
        //             }

        //             return true;
        //         }

        //         public void EnemyAutoPlayCard()
        //         {
        //             // if (_playerMiniTurn == EPlayerTarget.Self)
        //             // {
        //             //     Debug.LogWarning("It's not your turn!");
        //             //     return;
        //             // }

        //             var cardToSummon = _enemyHandView.CardsInRegion.FirstOrDefault(c => CheckCanSummonCard(c));
        //             if (cardToSummon == null)
        //             {
        //                 Debug.Log("Enemy has no valid card to play");
        //                 return;
        //             }
        //             PlayCard(cardToSummon, EPlayerTarget.Enemy, _enemyHandView);
        //         }
        //         // --------------------
        // #endregion

        #region Do Skill
        public async void ExercuteSkillQueue()
        {
            // Debug.Log($"Exercuting queue with {_skillQueueView.CardQueue.Count} skills");
            var casterSide = GameplayManager.Instance.CurTurn;
            var targetSide = casterSide == EPlayerTarget.Self ? EPlayerTarget.Enemy : EPlayerTarget.Self;

            while (_skillQueueView.CardQueue.Count > 0)
            {
                var skillCard = _skillQueueView.CardQueue[0];
                var skill = skillCard.BaseCard;
                var caster = _boardView.GetCharacterByCard(casterSide, skill.Owner);
                // Debug.Log($"Caster '{caster}' using skill");
                foreach (var ss in skill.SubSkills)
                {
                    var targets = ss.PosTargets.Select(pt => _boardView.GetCharacterByPos(targetSide, pt)).ToList();
                    if (targets.Count == 0)
                    {
                        //TODO: Animation for card if no targets
                        continue;
                    }

                    await caster.UseSkill(ss, targets);

                    var destroyTask = new List<Task>();
                    foreach (var t in targets)
                    {
                        if (t == null) continue;
                        if (t.Hp > 0) continue;
                        destroyTask.Add(DestroyChar(t, targetSide));

                        if (targetSide == EPlayerTarget.Enemy)
                        {
                            _enemyDeck.RemoveDeadCard(t.BaseCard);
                        }
                        else if (targetSide == EPlayerTarget.Self)
                        {
                            _deckView.RemoveDeadCards(t.BaseCard);
                            destroyTask.Add(_handView.RemoveDeadCards(t.BaseCard));
                        }
                    }
                    await Task.WhenAll(destroyTask);
                }
                _skillQueueView.RemoveCard(skillCard, true);
            }

            GameplayManager.Instance.ChangeToNextPhase();
        }

        public async Task DoEffectsOnChars(EPlayerTarget casterSide)
        {
            var targetSide = casterSide == EPlayerTarget.Self ? EPlayerTarget.Enemy : EPlayerTarget.Self;
            var targetChars = _boardView.GetCharactersInRegion(targetSide);
            var targetTasks = targetChars.Select(c => c.BaseCard.DoEffects());
            await Task.WhenAll(targetTasks);

            var casterChars = _boardView.GetCharactersInRegion(casterSide);
            var casterTasks = casterChars.Select(c => c.BaseCard.DoEffects());
            await Task.WhenAll(casterTasks);
        }
        #endregion

        #region Play Char Card
        private async Task PlayCharCard(CharacterCard card, EPlayerTarget playerSide, EPositionTarget position, Action callback = null)
        {
            var slot = _boardView.GetPlayerSlots(playerSide, position);
            var charModel = CardFactory.Instance.CreateCharModel(card, parent: slot.transform);

            _boardView.AddCharToSlot(charModel, slot);

            charModel.OnModelClicked.AddListener(async (_) =>
            {
                await _cardDetailView.ShowCharDetail(charModel.BaseCard);
            });
        }
        #endregion

        #region Char Die
        public async Task DestroyChar(CharacterModelView charModel, EPlayerTarget targetSide)
        {
            // Debug.Log($"Character {charModel.BaseCard.Name} died");
            await _boardView.DestroyDeadChar(charModel.BaseCard, targetSide);
        }
        //         public async void DoCardsAttack()
        //         {
        //             var attackerPlayer = _curTurn;
        //             var targetPlayer = attackerPlayer == EPlayerTarget.Self ? EPlayerTarget.Enemy : EPlayerTarget.Self;
        //             var attackerCards = _boardView.GetCardsInPlayerRegion(attackerPlayer).OfType<MonsterCard>().ToList();

        //             foreach (var attacker in attackerCards)
        //             {
        //                 //TODO: MC will attack player if no target on board
        //                 var targets = _boardView.GetCardsInPlayerRegion(targetPlayer).ToList();
        //                 Card target = null;

        //                 // // --------TEST--------
        //                 // //TODO: Auto select target if in enemy's turn
        //                 if (attackerPlayer == EPlayerTarget.Enemy)
        //                 {
        //                     //     target = targets.OrderBy(t => (t as IDamagable).Hp).FirstOrDefault();
        //                     //     await PlayAttackAniamtion(attackerPlayer, targetPlayer, attacker, target, () =>
        //                     //     {
        //                     //         //TODO: Do damage to player if attacker's ATK > target's HP
        //                     //         if ((target as IDamagable).Hp <= 0)
        //                     //         {
        //                     //             DestroyCard(target, targetPlayer);
        //                     //         }
        //                     //         else if(target is MonsterCard targetMonster)
        //                     //         {
        //                     //             attacker.TakeDamage(targetMonster.Atk);
        //                     //             // Debug.Log($"1_Attacker {attacker}'s HP after attack: {attacker.Hp}");
        //                     //         }
        //                     //     });
        //                     continue;
        //                 }
        //                 // --------------------

        //                 Debug.Log($"[Attack Phase] {attacker.Name} is going to attack...");

        //                 var selectedTarget = await _cardSelectorView.ShowCardToSelect(targets);

        //                 if (selectedTarget == null || selectedTarget.Count == 0)
        //                 {
        //                     Debug.LogWarning($"Bỏ qua lượt tấn công của {attacker.Name} (không có mục tiêu).");
        //                     continue;
        //                 }

        //                 target = selectedTarget.First();
        //                 await PlayAttackAniamtion(attackerPlayer, targetPlayer, attacker, target, () =>
        //                 {
        //                     //TODO: Do damage to player if attacker's ATK > target's HP
        //                     if ((target as IDamagable).Hp <= 0)
        //                     {
        //                         DestroyCard(target, targetPlayer);
        //                     }
        //                     else if (target is MonsterCard targetMonster)
        //                     {
        //                         attacker.TakeDamage(targetMonster.Atk);
        //                         // Debug.Log($"1_Attacker {attacker}'s HP after attack: {attacker.Hp}");
        //                     }
        //                 });
        //             }

        //             // ChangeToNextTurn();
        //         }

        //         private async Task PlayAttackAniamtion(EPlayerTarget attacker, EPlayerTarget target, MonsterCard attackerCard, Card targetCard, Action callback = null)
        //         {
        //             var attackerModel = _boardView.GetSlotByCard(attackerCard, attacker).GetComponentInChildren<CardModelView>();
        //             var targetModel = _boardView.GetSlotByCard(targetCard, target).GetComponentInChildren<CardModelView>();

        //             Vector3 offset = new(0, 0, 2);
        //             var startPos = attackerModel.transform.position;
        //             var moveDir = targetModel.transform.position - startPos;
        //             var finalPos = startPos + (moveDir - Mathf.Sign(moveDir.z) * offset);
        //             var rotateAngle = Quaternion.LookRotation(moveDir);

        //             var sequence = DOTween.Sequence();
        //             sequence.Append(attackerModel.Model.transform.DORotateQuaternion(rotateAngle, .1f).SetEase(Ease.InOutQuad));
        //             sequence.Append(attackerModel.transform.DOMove(finalPos, .5f).SetEase(Ease.InOutQuad));
        //             sequence.AppendCallback(() =>
        //             {
        //                 (attackerModel.BaseCard as MonsterCard).DoAttack(targetCard as IDamagable);
        //             });
        //             // sequence.AppendInterval(2f);
        //             sequence.Append(attackerModel.Model.transform.DORotateQuaternion(Quaternion.Euler(0, 180, 0), .1f).SetEase(Ease.InOutQuad));
        //             sequence.Append(attackerModel.transform.DOMove(startPos, .5f).SetEase(Ease.InOutQuad));
        //             sequence.Append(attackerModel.Model.transform.DORotateQuaternion(Quaternion.Euler(0, 180, 0) * Quaternion.Inverse(rotateAngle), .1f).SetEase(Ease.InOutQuad));
        //             sequence.OnComplete(() =>
        //             {
        //                 attackerModel.Model.transform.rotation = Quaternion.Euler(0, 0, 0);
        //                 // Debug.Log($"3_Attacker {attackerCard}'s HP after attack: {attackerCard.Hp}");
        //                 callback?.Invoke();
        //             });

        //             await sequence.AsyncWaitForCompletion();
        //         }

        //         // --------TEST-------
        //         //TODO: Enemy auto choose target to attack
        //         public async void AutoDoCardAttack()
        //         {
        //             if (_curTurn != EPlayerTarget.Enemy) return;

        //             var attackerPlayer = _curTurn;
        //             var targetPlayer = attackerPlayer == EPlayerTarget.Self ? EPlayerTarget.Enemy : EPlayerTarget.Self;
        //             var attackerCards = _boardView.GetCardsInPlayerRegion(attackerPlayer).OfType<MonsterCard>().ToList();

        //             foreach (var attacker in attackerCards)
        //             {
        //                 //TODO: MC will attack player if no target on board
        //                 var targets = _boardView.GetCardsInPlayerRegion(targetPlayer).ToList();
        //                 Card target = null;

        //                 target = targets.OrderBy(t => (t as IDamagable).Hp).FirstOrDefault();
        //                 await PlayAttackAniamtion(attackerPlayer, targetPlayer, attacker, target, () =>
        //                 {
        //                     //TODO: Do damage to player if attacker's ATK > target's HP
        //                     if ((target as IDamagable).Hp <= 0)
        //                     {
        //                         DestroyCard(target, targetPlayer);
        //                     }
        //                     else if (target is MonsterCard targetMonster)
        //                     {
        //                         attacker.TakeDamage(targetMonster.Atk);
        //                         // Debug.Log($"1_Attacker {attacker}'s HP after attack: {attacker.Hp}");
        //                     }
        //                 });
        //             }

        //             // ChangeToNextTurn();
        //         }
        //         // -------------------
        #endregion

        #region Init For Testing
        [SerializeField] private List<CharacterCardData> _selfCharDatas;
        [SerializeField] private List<CharacterCardData> _enemyCharDatas;

        private Deck _enemyDeck = new(null);

        public async Task InitScene()
        {
            if (_selfCharDatas.Count > 3 || _enemyCharDatas.Count > 3)
            {
                Debug.LogError("Character datas exceed limitation");
                return;
            }
            // _cardDetailView.HideCardDetail();
            // _cardSelectorView.HideCardSelector();
            var selfChars = _selfCharDatas.Select(c => new CharacterCard(c)).ToList();
            var enemyChars = _enemyCharDatas.Select(c => new CharacterCard(c)).ToList();

            _deckView.CreateNewDeck(selfChars);
            _enemyDeck = new(enemyChars);

            _skillQueueView.Initialize();
            _boardView.Initialize();
            for (int i = 0; i < 3; i++)
            {
                // TODO: Add event on click show detail for char
                await PlayCharCard(selfChars[i], EPlayerTarget.Self, (EPositionTarget)i);
                await PlayCharCard(enemyChars[i], EPlayerTarget.Enemy, (EPositionTarget)i);
            }
        }
        #endregion
    }
}