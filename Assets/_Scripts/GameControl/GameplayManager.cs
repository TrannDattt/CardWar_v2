using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CardWar.Enums;
using CardWar.GameViews;
using CardWar.Untils;
using JetBrains.Annotations;
using UnityEngine;

namespace CardWar.GameControl
{
    public class GameplayManager : Singleton<GameplayManager>
    {
        private IngameSceneView _ingameScene;

        #region Turn Logic
        public EPlayerTarget CurTurn { get; private set; } = EPlayerTarget.Self;
        public EPlayerTarget PlayerMiniTurn { get; private set; } // Use for summoning cards and using skill in order

        private void ChangeTurn(EPlayerTarget nextTurn) {
            CurTurn = nextTurn;
            PlayerMiniTurn = CurTurn;
            Debug.Log($"It's {CurTurn}'s turn now.");

            ChangePhase(EPhase.Opening);
        }

        public void ChangeToNextTurn()
        {
            var nextTurn = CurTurn == EPlayerTarget.Self ? EPlayerTarget.Enemy : EPlayerTarget.Self;
            ChangeTurn(nextTurn);
        }

        private void ChangeMiniTurn(EPlayerTarget nextTurn) {
            PlayerMiniTurn = nextTurn;
        }

        public void ChangeToNextMiniTurn()
        {
            PlayerMiniTurn = PlayerMiniTurn == EPlayerTarget.Self ? EPlayerTarget.Enemy : EPlayerTarget.Self;
        }
        #endregion

        #region Turn Logic
        //TODO: Phase logic and change phase
        public APhase CurPhase { get; private set; }
        // private Dictionary<EPhase, APhase> _phaseDict = new()
        // {
        //     {EPhase.Opening, new OpeningPhase(EPhase.Opening)},
        //     {EPhase.PreSetUp, new PreSetUpPhase(EPhase.PreSetUp)},
        //     {EPhase.Attack, new AttackPhase(EPhase.Attack)},
        //     {EPhase.PostSetUp, new PostSetUpPhase(EPhase.PostSetUp)},
        //     {EPhase.Conclude, new ConcludePhase(EPhase.Conclude)},
        // };

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
            public abstract void Enter();
            public abstract void Do();
            public abstract void Exit();
        }

        private class OpeningPhase : APhase
        {
            public OpeningPhase(EPhase phaseType) : base(phaseType)
            {
            }

            public override void Do()
            {
                if (_isFinished) Instance.ChangeToNextPhase();
            }

            public override async void Enter()
            {
                _isFinished = false;
                await IngameScene.DrawCard(CurTurn);
                _isFinished = true;

                //TODO: Use skill that active at opening phase
            }

            public override void Exit()
            {
            }
        }

        private class PreSetUpPhase : APhase
        {
            public PreSetUpPhase(EPhase phaseType) : base(phaseType)
            {
            }

            public override void Do()
            {
                if(Instance.PlayerMiniTurn == EPlayerTarget.Enemy)
                {
                    Instance.ChangeToNextMiniTurn();
                    Debug.Log($"Changed to: {Instance.PlayerMiniTurn}");
                    IngameScene.EnemyAutoPlayCard();
                }
            }

            public override void Enter()
            {
                Instance.ChangeMiniTurn(CurTurn);
            }

            public override void Exit()
            {
                Instance.ChangeMiniTurn(CurTurn);
            }
        }

        private class AttackPhase : APhase
        {
            public AttackPhase(EPhase phaseType) : base(phaseType)
            {
            }

            public override void Do()
            {
                if(CurTurn == EPlayerTarget.Enemy)
                {
                    IngameScene.AutoDoCardAttack();
                }
            }

            public override void Enter()
            {
                if(CurTurn == EPlayerTarget.Self)
                {
                    IngameScene.DoCardsAttack();
                }
            }

            public override void Exit()
            {
            }
        }

        private class PostSetUpPhase : APhase
        {
            public PostSetUpPhase(EPhase phaseType) : base(phaseType)
            {
            }

            public override void Do()
            {
                if(Instance.PlayerMiniTurn == EPlayerTarget.Enemy)
                {
                    Instance.ChangeToNextMiniTurn();
                    IngameScene.EnemyAutoPlayCard();
                }
            }

            public override void Enter()
            {
                Instance.ChangeMiniTurn(CurTurn);
            }

            public override void Exit()
            {
                Instance.ChangeMiniTurn(CurTurn);
            }
        }

        private class ConcludePhase : APhase
        {
            public ConcludePhase(EPhase phaseType) : base(phaseType)
            {
            }

            public override void Do()
            {
            }

            public override void Enter()
            {
                //TODO: Use skill that active at conclude phase
            }

            public override void Exit()
            {
                // Instance.ChangeToNextTurn();
            }
        }

        private APhase GetPhase(EPhase phaseType)
        {
            return phaseType switch
            {
                EPhase.Opening => new OpeningPhase(EPhase.Opening),
                EPhase.PreSetUp => new PreSetUpPhase(EPhase.PreSetUp),
                EPhase.Attack => new AttackPhase(EPhase.Attack),
                EPhase.PostSetUp => new PostSetUpPhase(EPhase.PostSetUp),
                EPhase.Conclude => new ConcludePhase(EPhase.Conclude),
                _ => null
            };
        }

        private void ChangePhase(EPhase nextPhase)
        {
            CurPhase?.Exit();
            // CurPhase = _phaseDict[nextPhase];
            CurPhase = GetPhase(nextPhase);
            CurPhase.Enter();

            Debug.Log($"{CurPhase.Type} started.");
        }

        public void ChangeToNextPhase()
        {
            if(PlayerMiniTurn != CurTurn)
            {
                Debug.LogWarning("Cant change phase now.");
                return;
            }

            if (CurPhase.Type == EPhase.Conclude) 
            {
                ChangeToNextTurn();
                return;
            }

            var nextPhase = CurPhase.Type switch
            {
                EPhase.Opening => EPhase.PreSetUp,
                EPhase.PreSetUp => EPhase.Attack,
                EPhase.Attack => EPhase.PostSetUp,
                EPhase.PostSetUp => EPhase.Conclude,
                // EPhase.Conclude => EPhase.Opening,
                _ => EPhase.None
            };

            // var nextPhase = _curPhase++;
            // if (nextPhase == EPhase.None) nextPhase++;

            ChangePhase(nextPhase);
        }
        #endregion

        #region Gameplay Logic
        //TODO: Manage the game flow, things happen when game start, conclude match, ...
        // Make a state machine ??
        public async void StartGame()
        {
            _ingameScene.InitScene();

            var selfDrawTask = _ingameScene.DrawCard(EPlayerTarget.Self, 3);
            var enemyDrawTask = _ingameScene.DrawCard(EPlayerTarget.Enemy, 3);

            await Task.WhenAll(selfDrawTask, enemyDrawTask);

            ChangeTurn(EPlayerTarget.Self);
        }
        #endregion

        #region Init For Testing
        void Start()
        {
            _ingameScene = IngameSceneView.Instance;

            StartGame();
        }

        void Update()
        {
            if(CurPhase != null)
            {
                CurPhase.Do();
            }
        }
        #endregion
    }
}