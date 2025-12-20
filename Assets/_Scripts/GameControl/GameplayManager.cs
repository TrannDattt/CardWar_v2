using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CardWar_v2.Entities;
using CardWar_v2.Enums;
using CardWar_v2.SceneViews;
using CardWar_v2.Untils;
using UnityEngine;
using UnityEngine.Events;

namespace CardWar_v2.GameControl
{
    public class GameplayManager : Singleton<GameplayManager>
    {
        private IngameSceneView _ingameScene;

        #region Turn Logic
        public EPlayerTarget CurTurn { get; private set; } = EPlayerTarget.Ally;
        private int _turnChangeTime;
        public UnityEvent OnTurnChanged { get; private set; } = new();

        private IEnumerator ChangeTurn(EPlayerTarget nextTurn) {
            CurTurn = nextTurn;
            _turnChangeTime++;
            // Debug.Log($"It's {CurTurn}'s turn now.");

            yield return ChangePhase(EPhase.Opening);
        }

        public IEnumerator ChangeToNextTurn()
        {
            var nextTurn = CurTurn == EPlayerTarget.Ally ? EPlayerTarget.Enemy : EPlayerTarget.Ally;
            yield return ChangeTurn(nextTurn);
        }
        #endregion

        #region Phase Logic
        private APhase _curPhase;
        public EPhase CurPhase => _curPhase != null ? _curPhase.Type : EPhase.None;

        public abstract class APhase
        {
            public EPhase Type { get; private set; }
            protected IngameSceneView IngameScene => Instance._ingameScene;
            protected EPlayerTarget CurTurn => Instance.CurTurn;
            protected bool _isFinished;

            public APhase(EPhase phaseType) 
            {
                Type = phaseType;
                _isFinished = false;
            }
            public abstract IEnumerator Enter();

            public virtual void Do() 
            {
                if (_isFinished) Exit();
            }

            public virtual IEnumerator Exit()
            {
                yield return Instance.ChangeToNextPhase();
            }
        }

        private class OpeningPhase : APhase
        {
            public OpeningPhase(EPhase phaseType) : base(phaseType)
            {
            }

            public override void Do()
            {
                if (!_isFinished && IngameScene.CheckQueueFull()) _isFinished = true;

                base.Do();
            }

            public override IEnumerator Enter()
            {
                // _isFinished = false;
                if(CurTurn == EPlayerTarget.Enemy)
                {
                    yield return IngameScene.AutoSelectCard(3);
                    yield break;
                }

                yield return IngameScene.DrawCard(3);
            }

            public override IEnumerator Exit()
            {
                yield return base.Exit();
            }
        }

        private class AttackPhase : APhase
        {
            public AttackPhase(EPhase phaseType) : base(phaseType)
            {
            }

            public override void Do()
            {
                base.Do();
            }

            public override IEnumerator Enter()
            {
                yield return IngameScene.ExercuteSkillQueue();

                _isFinished = true;
            }

            public override IEnumerator Exit()
            {
                yield return base.Exit();
            }
        }

        private class ConcludePhase : APhase
        {
            public ConcludePhase(EPhase phaseType) : base(phaseType)
            {
            }

            public override void Do()
            {
                base.Do();
            }

            public override IEnumerator Enter()
            {
                if (Instance._turnChangeTime % 2 == 0)
                {
                    yield return IngameScene.DoEffectsOnChars(CurTurn);

                    Instance.OnTurnChanged?.Invoke();
                }
                    
                _isFinished = true;
            }

            public override IEnumerator Exit()
            {
                yield return base.Exit();
            }
        }

        public class NonePhase : APhase
        {
            public NonePhase(EPhase phaseType) : base(phaseType)
            {
            }

            public override void Do()
            {
                base.Do();
            }

            public override IEnumerator Enter()
            {
                yield return null;
            }

            public override IEnumerator Exit()
            {
                yield return base.Exit();
            }
        }

        private APhase GetPhase(EPhase phaseType)
        {
            if (_ingameScene == null || _ingameScene.IsEnd)
                return new NonePhase(EPhase.None);

            return phaseType switch
            {
                EPhase.Opening => new OpeningPhase(EPhase.Opening),
                EPhase.Attack => new AttackPhase(EPhase.Attack),
                EPhase.Conclude => new ConcludePhase(EPhase.Conclude),
                _ => new NonePhase(EPhase.None)
            };
        }

        private IEnumerator ChangePhase(EPhase nextPhase)
        {
            yield return _curPhase?.Exit();
            // CurPhase = _phaseDict[nextPhase];
            _curPhase = GetPhase(nextPhase);
            // Debug.Log($"{_curPhase.Type} started.");
            yield return _curPhase.Enter();
        }

        public IEnumerator ChangeToNextPhase()
        {
            if (_curPhase.Type == EPhase.Conclude) 
            {
                yield return ChangeToNextTurn();
                yield break;
            }

            var nextPhase = _curPhase.Type switch
            {
                EPhase.Opening => EPhase.Attack,
                EPhase.Attack => EPhase.Conclude,
                // EPhase.Conclude => EPhase.Opening,
                _ => EPhase.None
            };
            yield return ChangePhase(nextPhase);
        }
        #endregion

        //TODO: Move to GameManager
        #region Gameplay Logic
        private Level _curLevel;
        private List<CharacterCard> _selfTeam = new();

        public async void StartNewFight()
        {
            ResumeGame();

            _selfTeam.ForEach(c => c.ResetCharStat());
            _curLevel.Enemies.ForEach(c => c.ResetCharStat());

            IEnumerator SetupMatch()
            {
                _ingameScene = FindFirstObjectByType<IngameSceneView>();
                // Debug.Log("Set up match");
                yield return _ingameScene.SetupMatch(_selfTeam, _curLevel);
                _turnChangeTime = 0;

                // Debug.Log("Draw cards");
                yield return _ingameScene.DrawCard(3);
                Debug.Log("Change turn");
                yield return ChangeTurn(EPlayerTarget.Ally);
                OnTurnChanged?.Invoke();
            }

            await SceneNavigator.Instance.ChangeScene(EScene.Ingame, () => StartCoroutine(SetupMatch()));
        }

        public void StartCampaignLevel(Level level, List<CharacterCard> selfTeam)
        {
            _selfTeam = selfTeam;
            _curLevel = level;
            StartNewFight();
        }

        public void PauseGame()
        {
            Time.timeScale = 0;
        }

        public void ResumeGame()
        {
            Time.timeScale = 1;
        }
        #endregion

        #region Init For Testing
        void Start()
        {
            SceneNavigator.Instance.OnSceneLoaded.AddListener(scene =>
            {
                if (scene != EScene.Ingame)
                    StartCoroutine(ChangePhase(EPhase.None));
            });
        }

        void Update()
        {
            _curPhase?.Do();
        }
        #endregion
    }
}