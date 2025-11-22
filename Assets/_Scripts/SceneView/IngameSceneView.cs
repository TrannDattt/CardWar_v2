using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// using CardWar.Factories;
using CardWar_v2.Entities;
using CardWar_v2.Enums;
using CardWar_v2.Factories;
using CardWar_v2.GameControl;
using CardWar_v2.ComponentViews;

// using CardWar.Views;
using DG.Tweening;
using UnityEngine;
using static UnityEngine.EventSystems.PointerEventData;
using UnityEngine.UI;

namespace CardWar_v2.SceneViews
{
    public class IngameSceneView : MonoBehaviour
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
        [SerializeField] private MatchResultView _concludeMatchView;
        
        [Header("Buttons")]
        [SerializeField] private Button _restartBtn;
        
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
            sequence.Join(cardView.transform.DOScale(2.4f * Vector3.one, animLength).SetEase(Ease.InOutQuad));
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
                if (IsEnd) return;
                var skillCard = _skillQueueView.CardQueue[0];
                var skill = skillCard.BaseCard;
                var caster = _boardView.GetCharacterByCard(casterSide, skill.Owner);
                
                if (caster.BaseCard.ActiveEffects.ContainsKey(ESkillEffect.Silence))
                {
                    Debug.Log($"Caster '{caster}' is silenced and cannot use skill {skill.Name}");
                    _skillQueueView.RemoveCard(skillCard, true);
                    continue;
                }

                caster.BaseCard.OnUseSkill?.Invoke(skill);
                // Debug.Log($"Caster '{caster}' using skill {skill.Name}");
                foreach (var ss in skill.SubSkills)
                {
                    var targetSide = ss.TargetSide == casterSide ? EPlayerTarget.Ally : EPlayerTarget.Enemy;
                    var targets = new List<CharacterModelView>();
                    foreach(var pt in ss.PositionTargets)
                    {
                        // Debug.Log($"Get target {pt} at position {targetSide}");
                        bool isFlexTarget = _boardView.GetCharactersInRegion(targetSide).Count >= ss.PositionTargets.Count;
                        if (pt == EPositionTarget.Self) targets.Add(caster);
                        else targets.Add(_boardView.GetCharacterByPos(targetSide, pt, isFlexTarget));
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
            var charModel = CardFactory.Instance.CreateCharModel(card, playerSide, parent: slot.transform);

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
            if (charModel == null) return;
            await _boardView.DestroyDeadChar(charModel.BaseCard, targetSide);
            // Debug.Log($"Remove card of character {charModel.BaseCard.Name}");
            
            if (targetSide == EPlayerTarget.Enemy)
            {
                _enemyDeck.RemoveDeadCard(charModel.BaseCard);
            }
            else if (targetSide == EPlayerTarget.Ally)
            {
                _deckView.RemoveDeadCards(charModel.BaseCard);
                await _handView.RemoveDeadCards(charModel.BaseCard);
            }

            CheckFinishGame();
        }
        // #endregion

        // #region Conclude Match
        private async void CheckFinishGame()
        {
            var enemyCount = _boardView.GetCharactersInRegion(EPlayerTarget.Enemy).Count;
            if(enemyCount == 0)
            {
                IsEnd = true;
                await _concludeMatchView.ShowResult(_curLevel, true, _fightLogger);
                return;
            }

            var allyCount = _boardView.GetCharactersInRegion(EPlayerTarget.Ally).Count;
            if(allyCount == 0)
            {
                IsEnd = true;
                await _concludeMatchView.ShowResult(_curLevel, false, _fightLogger);
            }
        }
        #endregion

        #region Init For Testing
        // [SerializeField] private List<CharacterCardData> _selfCharDatas;
        // [SerializeField] private List<CharacterCardData> _enemyCharDatas;

        private Level _curLevel;
        private Deck _enemyDeck = new(null);
        private FightLogger _fightLogger;

        public bool IsEnd {get; private set;}

        public async Task SetupMatch(List<CharacterCard> selfTeam, Level level)
        {
            IsEnd = false;
            _curLevel = level;
            var enemyTeam = level.Enemies;

            _deckView.CreateNewDeck(selfTeam);
            _enemyDeck = new(enemyTeam);
            _handView.Initialize();
            _skillQueueView.Initialize();
            _boardView.Initialize();

            _fightLogger = new(selfTeam, level);
            _concludeMatchView.HideUI();

            var maxIndex = Mathf.Max(selfTeam.Count, enemyTeam.Count) - 1;
            for (int i = 0; i <= maxIndex; i++)
            {
                if (i < selfTeam.Count) await PlayCharCard(selfTeam[i], EPlayerTarget.Ally, (EPositionTarget)i);
                if (i < enemyTeam.Count) await PlayCharCard(enemyTeam[i], EPlayerTarget.Enemy, (EPositionTarget)i);
            }
        }

        void Start()
        {
            _restartBtn.onClick.AddListener(() =>
            {
                GameplayManager.Instance.StartNewFight();
            });
        }
        #endregion
    }
}