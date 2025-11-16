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
using CardWar_v2.ComponentViews;

// using CardWar.Views;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.EventSystems.PointerEventData;

namespace CardWar_v2.SceneViews
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

        #region Do Skill
        public async Task ExercuteSkillQueue()
        {
            // Debug.Log($"Exercuting queue with {_skillQueueView.CardQueue.Count} skills");
            var casterSide = GameplayManager.Instance.CurTurn;

            while (_skillQueueView.CardQueue.Count > 0)
            {
                var skillCard = _skillQueueView.CardQueue[0];
                var skill = skillCard.BaseCard;
                var caster = _boardView.GetCharacterByCard(casterSide, skill.Owner);
                Debug.Log($"Caster '{caster}' using skill {skill.Name}");
                foreach (var ss in skill.SubSkills)
                {
                    var targetSide = ss.TargetSide;
                    var targets = new List<CharacterModelView>();
                    foreach(var pt in ss.PositionTargets)
                    {
                        Debug.Log($"Get target {pt} at position {targetSide}");
                        if (pt == EPositionTarget.Self) targets.Add(caster);
                        else targets.Add(_boardView.GetCharacterByPos(targetSide, pt, false));
                    }
                    // var targets = ss.Targets.Select(pt => _boardView.GetCharacterByPos(targetSide, pt)).ToList();
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
                        else if (targetSide == EPlayerTarget.Ally)
                        {
                            _deckView.RemoveDeadCards(t.BaseCard);
                            destroyTask.Add(_handView.RemoveDeadCards(t.BaseCard));
                        }
                    }
                    await Task.WhenAll(destroyTask);
                }
                _skillQueueView.RemoveCard(skillCard, true);
            }
        }

        public async Task DoEffectsOnChars(EPlayerTarget casterSide)
        {
            var targetSide = casterSide == EPlayerTarget.Ally ? EPlayerTarget.Enemy : EPlayerTarget.Ally;
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
        #endregion

        #region Init For Testing
        // [SerializeField] private List<CharacterCardData> _selfCharDatas;
        // [SerializeField] private List<CharacterCardData> _enemyCharDatas;

        private Deck _enemyDeck = new(null);

        public async Task InitScene(List<CharacterCard> selfTeam, List<CharacterCard> enemyTeam)
        {
            // if (selfTeam.Count > 3 || e.Count > 3)
            // {
            //     Debug.LogError("Character datas exceed limitation");
            //     return;
            // }
            // _cardDetailView.HideCardDetail();
            // _cardSelectorView.HideCardSelector();
            // var selfChars = _selfCharDatas.Select(c => new CharacterCard(c)).ToList();
            // var enemyChars = _enemyCharDatas.Select(c => new CharacterCard(c)).ToList();
            // Debug.Log($"2.Start game with self team: {selfTeam[0].Name}, {selfTeam[1].Name}, {selfTeam[2].Name}");
            // Debug.Log($" and enemy team: {enemyTeam[0].Name}");

            Debug.Log(_deckView);
            _deckView.CreateNewDeck(selfTeam);
            _enemyDeck = new(enemyTeam);

            _skillQueueView.Initialize();
            _boardView.Initialize();

            var maxIndex = Mathf.Max(selfTeam.Count, enemyTeam.Count) - 1;
            for (int i = 0; i <= maxIndex; i++)
            {
                if (i < selfTeam.Count) await PlayCharCard(selfTeam[i], EPlayerTarget.Ally, (EPositionTarget)i);
                if (i < enemyTeam.Count) await PlayCharCard(enemyTeam[i], EPlayerTarget.Enemy, (EPositionTarget)i);
            }
        }
        #endregion
    }
}