using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CardWar.Enums;
using CardWar.Untils;
using CardWar_v2.Enums;
using CardWar_v2.GameViews;
using JetBrains.Annotations;
using UnityEngine;

namespace CardWar_v2.GameControl
{
    public class GameplayManager : Singleton<GameplayManager>
    {
        private IngameSceneView _ingameScene;

        #region Turn Logic
        public EPlayerTarget CurTurn { get; private set; } = EPlayerTarget.Self;

        private void ChangeTurn(EPlayerTarget nextTurn) {
            CurTurn = nextTurn;
            Debug.Log($"It's {CurTurn}'s turn now.");

            ChangePhase(EPhase.Opening);
        }

        public void ChangeToNextTurn()
        {
            var nextTurn = CurTurn == EPlayerTarget.Self ? EPlayerTarget.Enemy : EPlayerTarget.Self;
            ChangeTurn(nextTurn);
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
                if (IngameScene.CheckQueueFull()) Instance.ChangeToNextPhase();
            }

            public override async void Enter()
            {
                // _isFinished = false;
                //TODO: Use skill that active at opening phase

                if(CurTurn == EPlayerTarget.Enemy)
                {
                    await IngameScene.AutoSelectCard(3);
                    return;
                }

                await IngameScene.DrawCard(3);
            }

            public override void Exit()
            {
            }
        }

        private class AttackPhase : APhase
        {
            public AttackPhase(EPhase phaseType) : base(phaseType)
            {
            }

            public override void Do()
            {
            }

            public override async void Enter()
            {
                IngameScene.ExercuteSkillQueue();
            }

            public override void Exit()
            {
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
                Instance.ChangeToNextPhase();
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
                EPhase.Attack => new AttackPhase(EPhase.Attack),
                EPhase.Exercute => new ConcludePhase(EPhase.Exercute),
                _ => null
            };
        }

        private void ChangePhase(EPhase nextPhase)
        {
            CurPhase?.Exit();
            // CurPhase = _phaseDict[nextPhase];
            CurPhase = GetPhase(nextPhase);
            Debug.Log($"{CurPhase.Type} started.");
            CurPhase.Enter();
        }

        public void ChangeToNextPhase()
        {
            if (CurPhase.Type == EPhase.Exercute) 
            {
                ChangeToNextTurn();
                return;
            }

            var nextPhase = CurPhase.Type switch
            {
                EPhase.Opening => EPhase.Attack,
                EPhase.Attack => EPhase.Exercute,
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

            var selfDrawTask = _ingameScene.DrawCard(3);

            await Task.WhenAll(selfDrawTask);

            ChangeTurn(EPlayerTarget.Enemy);
            // ChangeTurn(EPlayerTarget.Self);
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