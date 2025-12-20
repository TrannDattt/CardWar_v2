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
using CardWar_v2.Datas;
using System.Collections;

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
        [SerializeField] private BattleLogView _battleLogView;

        [SerializeField] private Button _logBtn;
        
        #region Draw Card
        private IEnumerator DrawCardAnimation(SkillCardView cardView, Action callback = null)
        {
            // var animLength = 0.5f;
            // bool isFinished = false;
            var endPos = _handView.GetCardPos(default);
            // Debug.Log($"Next pos: {endPos}");
            var cardTransform = cardView.transform;

            var sequence = DOTween.Sequence();
            sequence.Append(cardTransform.DOMove(endPos, 0.3f).SetEase(Ease.InOutQuad));
            sequence.Append(cardTransform.DORotateQuaternion(Quaternion.identity, .2f).SetEase(Ease.InOutQuad));
            sequence.AppendInterval(.2f);
            sequence.OnComplete(() =>
            {
                // isFinished = true;
                cardView.transform.rotation = Quaternion.identity;
                callback?.Invoke();
            });
            // sequence.OnKill(() =>
            // {
            //     isFinished = true;
            //     cardView.transform.rotation = Quaternion.identity;
            //     callback?.Invoke();
            // });

            yield return sequence.WaitForCompletion();
            // yield return new WaitUntil(() => isFinished);
        }

        public IEnumerator DrawCard(int amount = 1)
        {
            Debug.Log($"Drawn {amount} cards");

            for (int a = 0; a < amount; a++)
            {
                _deckView.DrawCard(out var drawnCard);
                if (drawnCard == null)
                {
                    Debug.Log($"Failed to draw from Self's deck");
                    break;
                }

                Debug.Log($"Drawn card {a} anim");
                yield return DrawCardAnimation(drawnCard);
                _handView.AddCardToHand(drawnCard);

                Debug.Log($"Drawn card {a} callback");
                drawnCard.OnCardClick.AddListener((e) =>
                {
                    if (e.button == InputButton.Right)
                    {
                        // Debug.Log($"1.Show detail of card {drawnCard.BaseCard}");
                        StartCoroutine(_cardDetailView.ShowSkillDetail(drawnCard.BaseCard));
                    }
                    
                    if (GameplayManager.Instance.CurPhase == EPhase.Opening && e.button == InputButton.Left)
                    {
                        if (drawnCard.GetComponentInParent<HandView>()) SelfPlayCard(drawnCard);
                        else if (drawnCard.GetComponentInParent<SkillQueueView>()) WithdrawCard(drawnCard);
                    }
                });

                // yield return _handView.ArrangeHand();
                Debug.Log($"Finish draw card {a}");
            }
            Debug.Log($"Finish draw {amount} cards");
        }
        #endregion

        #region Play Skill Card
        public void SelfPlayCard(SkillCardView cardView)
        {
            var slot = _skillQueueView.GetNextEmptySlot();
            if (slot == null)
            {
                Debug.LogWarning("No more space in queue");
                return;
            }

            _handView.RemoveCard(cardView);
            _skillQueueView.AddCard(cardView);
            StartCoroutine(PlayCardAnimation(cardView, slot));
            // StartCoroutine(_handView.ArrangeHand());
        }

        public IEnumerator AutoSelectCard(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                var skillCard = _enemyDeck.GetRandomCard();
                var slot = _skillQueueView.GetNextEmptySlot();
                var cardView = CardFactory.Instance.CreateCardView(skillCard, parent: slot.transform);
                yield return PlayCardAnimation(cardView, slot, () =>
                {
                    _skillQueueView.AddCard(cardView);
                });
            }
        }

        private IEnumerator PlayCardAnimation(SkillCardView cardView, SkillSlotView toSlot, Action callback = null)
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

            yield return sequence.WaitForCompletion();
        }

        public void WithdrawCard(SkillCardView cardView)
        {
            _skillQueueView.RemoveCard(cardView, false);
            StartCoroutine(WithdrawCardAnimation(cardView, () =>
            {
                _handView.AddCardToHand(cardView);
            }));
        }

        private IEnumerator WithdrawCardAnimation(SkillCardView cardView, Action callback = null)
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

            yield return sequence.WaitForCompletion();
        }

        public bool CheckQueueFull() => _skillQueueView.GetNextEmptySlot() == null;
        #endregion

        #region Do Skill
        public IEnumerator ExercuteSkillQueue()
        {
            // Debug.Log($"Exercuting queue with {_skillQueueView.CardQueue.Count} skills");
            var casterSide = GameplayManager.Instance.CurTurn;
            List<(CharacterModelView, EventTrackingSkill)> trackingSkills = new();

            while (_skillQueueView.CardQueue.Count > 0)
            {
                if (IsEnd) yield break;
                var skillCard = _skillQueueView.CardQueue[0];
                var skill = skillCard.BaseCard;
                var caster = _boardView.GetCharacterByCard(casterSide, skill.Owner);

                caster.BaseCard.OnUseSkill?.Invoke(skill);
                
                if (caster.BaseCard.ActiveEffects.ContainsKey(ESkillEffect.Silence))
                {
                    // Debug.Log($"Caster '{caster}' is silenced and cannot use skill {skill.Name}");
                    var offsetAngleZ = 5f;
                    // Debug.Log("Shake");
                    Sequence sequence = DOTween.Sequence();

                    sequence.Append(skillCard.transform.DOLocalRotate(new Vector3(0, 0, offsetAngleZ), .05f, RotateMode.Fast));
                    sequence.Append(skillCard.transform.DOLocalRotate(new Vector3(0, 0, -offsetAngleZ), .1f, RotateMode.Fast));
                    sequence.Append(skillCard.transform.DOLocalRotate(new Vector3(0, 0, offsetAngleZ), .1f, RotateMode.Fast));
                    sequence.Append(skillCard.transform.DOLocalRotate(new Vector3(0, 0, -offsetAngleZ), .1f, RotateMode.Fast));
                    sequence.Append(skillCard.transform.DOLocalRotate(new Vector3(0, 0, 0f), .05f, RotateMode.Fast));
                    sequence.AppendInterval(.2f);
                    sequence.OnComplete(() =>
                    {
                        _skillQueueView.RemoveCard(skillCard, true);
                    });

                    yield return sequence.WaitForCompletion();
                    continue;
                }
                // Debug.Log($"Caster '{caster}' using skill {skill.Name}");
                List<CharacterModelView> targets = new();

                IEnumerator UseSkill(CharacterModelView caster, SubSkill ss)
                {
                    var targetSide = ss.TargetSide == casterSide ? EPlayerTarget.Ally : EPlayerTarget.Enemy;
                    if (!ss.PositionTargets.Contains(EPositionTarget.LastTarget)) targets.Clear();
                    
                    foreach(var pt in ss.PositionTargets)
                    {
                        if (targets.Count == _boardView.GetCharactersInRegion(targetSide).Count) break;
                        
                        if (pt == EPositionTarget.LastTarget) continue;
                        bool isFlexTarget = targets.Count <= ss.PositionTargets.Count;
                        // Debug.Log($"Get target {pt} at position {targetSide}");
                        //TODO: If pt = Front, Mid => it may select Mid, Mid because of no Back
                        if (pt == EPositionTarget.Self) targets.Add(caster);
                        else targets.Add(_boardView.GetUntargetedCharByPos(targets, targetSide, pt, isFlexTarget));
                    }
                    // var targets = ss.Targets.Select(pt => _boardView.GetCharacterByPos(targetSide, pt)).ToList();
                    if (targets.Count > 0)
                    {
                        yield return caster.UseSkill(ss, targets);
                    }
                }

                foreach (var ss in skill.SubSkills)
                {
                    yield return UseSkill(caster, ss);

                    if (ss.GetType() == typeof(ConditionalSkill) && (ss as ConditionalSkill).Checked) 
                    {
                        foreach (var ts in (ss as ConditionalSkill).TrueSkills)
                        {
                            yield return UseSkill(caster, ts);
                        }
                    }      

                    if (ss.GetType() == typeof(EventTrackingSkill))
                    {
                        trackingSkills.Add((caster, ss as EventTrackingSkill));
                    }              
                    
                    // var targetSide = ss.TargetSide == casterSide ? EPlayerTarget.Ally : EPlayerTarget.Enemy;
                    // if (!ss.PositionTargets.Contains(EPositionTarget.LastTarget)) targets.Clear();

                    // foreach(var pt in ss.PositionTargets)
                    // {
                    //     if (targets.Count == _boardView.GetCharactersInRegion(targetSide).Count) break;

                    //     if (pt == EPositionTarget.LastTarget) continue;
                    //     bool isFlexTarget = targets.Count <= ss.PositionTargets.Count;
                    //     // Debug.Log($"Get target {pt} at position {targetSide}");
                    //     //TODO: If pt = Front, Mid => it may select Mid, Mid because of no Back
                    //     if (pt == EPositionTarget.Self) targets.Add(caster);
                    //     else targets.Add(_boardView.GetUntargetedCharByPos(targets, targetSide, pt, isFlexTarget));
                    // }
                    // // var targets = ss.Targets.Select(pt => _boardView.GetCharacterByPos(targetSide, pt)).ToList();
                    // if (targets.Count == 0)
                    // {
                    //     continue;
                    // }

                    // await caster.UseSkill(ss, targets);
                }
                _skillQueueView.RemoveCard(skillCard, true);

                for (int i = trackingSkills.Count - 1; i >= 0; i--)
                {
                    var ets = trackingSkills[i];

                    if (!ets.Item2.Checked)
                        continue;

                    foreach (var ts in ets.Item2.TrueSkills)
                    {
                        yield return UseSkill(ets.Item1, ts);
                    }

                    trackingSkills.RemoveAt(i);
                }
            }
        }

        public IEnumerator DoEffectsOnChars(EPlayerTarget casterSide)
        {
            IEnumerator DoEffects(List<CharacterModelView> characters)
            {
                characters.ForEach(c => StartCoroutine(c.BaseCard.DoEffects()));
                yield return null;
            }

            var targetSide = casterSide == EPlayerTarget.Ally ? EPlayerTarget.Enemy : EPlayerTarget.Ally;
            var targetChars = _boardView.GetCharactersInRegion(targetSide);
            yield return DoEffects(targetChars);

            var casterChars = _boardView.GetCharactersInRegion(casterSide);
            yield return DoEffects(casterChars);
        }
        #endregion

        #region Play Char Card
        private IEnumerator PlayCharCard(CharacterCard card, EPlayerTarget playerSide, EPositionTarget position, Action callback = null)
        {
            var slot = _boardView.GetPlayerSlots(playerSide, position);
            var charModel = CardFactory.Instance.CreateCharModel(card, playerSide, parent: slot.transform);

            _boardView.AddCharToSlot(charModel, slot);

            charModel.OnModelClicked.AddListener((_) =>
            {
                StartCoroutine(_cardDetailView.ShowCharDetail(charModel.BaseCard));
            });

            charModel.BaseCard.OnDeath.AddListener(() =>
            {
                StartCoroutine(DestroyChar(charModel, playerSide));
            });

            yield return null;
        }
        #endregion

        #region Char Die
        public IEnumerator DestroyChar(CharacterModelView charModel, EPlayerTarget targetSide)
        {
            if (charModel == null) yield break;
            yield return _boardView.DestroyDeadChar(charModel.BaseCard, targetSide);
            // Debug.Log($"Remove card of character {charModel.BaseCard.Name}");
            
            if (targetSide == EPlayerTarget.Enemy)
            {
                _enemyDeck.RemoveDeadCard(charModel.BaseCard);
            }
            else if (targetSide == EPlayerTarget.Ally)
            {
                _deckView.RemoveDeadCards(charModel.BaseCard);
                yield return _handView.RemoveDeadCards(charModel.BaseCard);
            }

            CheckFinishGame();
        }
        // #endregion

        // #region Conclude Match
        private void CheckFinishGame()
        {
            var enemyCount = _boardView.GetCharactersInRegion(EPlayerTarget.Enemy).Count;
            if(enemyCount == 0)
            {
                IsEnd = true;
                StartCoroutine(_concludeMatchView.ShowResult(_curLevel, true, _fightLogger));
                return;
            }

            var allyCount = _boardView.GetCharactersInRegion(EPlayerTarget.Ally).Count;
            if(allyCount == 0)
            {
                IsEnd = true;
                StartCoroutine(_concludeMatchView.ShowResult(_curLevel, false, _fightLogger));
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

        public IEnumerator SetupMatch(List<CharacterCard> selfTeam, Level level)
        {
            IsEnd = false;
            _curLevel = level;
            var enemyTeam = level.Enemies;

            _deckView.CreateNewDeck(selfTeam);
            _enemyDeck = new(enemyTeam);
            _handView.Initialize();
            _skillQueueView.Initialize();
            _boardView.Initialize();
            _concludeMatchView.HideUI();

            var maxIndex = Mathf.Max(selfTeam.Count, enemyTeam.Count) - 1;
            for (int i = 0; i <= maxIndex; i++)
            {
                if (i < selfTeam.Count) yield return PlayCharCard(selfTeam[i], EPlayerTarget.Ally, (EPositionTarget)i);
                if (i < enemyTeam.Count) yield return PlayCharCard(enemyTeam[i], EPlayerTarget.Enemy, (EPositionTarget)i);
            }

            _fightLogger = new(selfTeam, level);
            _fightLogger.StartTracking();
        }

        void Start()
        {
            GameAudioManager.Instance.PlayBackgroundMusic(GameAudioManager.EBgm.Ingame);

            _logBtn.onClick.AddListener(async () => await _battleLogView.OpenMenu(_fightLogger));
        }
        #endregion
    }
}