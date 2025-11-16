using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardWar.Enums;
using CardWar.Untils;
using CardWar_v2.Entities;
using CardWar_v2.Enums;
using CardWar_v2.SceneViews;
using JetBrains.Annotations;
using UnityEngine;

namespace CardWar_v2.GameControl
{
    public class GameplayManager : Singleton<GameplayManager>
    {
        private IngameSceneView _ingameScene;

        #region Turn Logic
        public EPlayerTarget CurTurn { get; private set; } = EPlayerTarget.Ally;
        private int _turnChangeTime;

        private void ChangeTurn(EPlayerTarget nextTurn) {
            CurTurn = nextTurn;
            _turnChangeTime++;
            Debug.Log($"It's {CurTurn}'s turn now.");

            ChangePhase(EPhase.Opening);
        }

        public void ChangeToNextTurn()
        {
            var nextTurn = CurTurn == EPlayerTarget.Ally ? EPlayerTarget.Enemy : EPlayerTarget.Ally;
            ChangeTurn(nextTurn);
        }
        #endregion

        #region Turn Logic
        //TODO: Phase logic and change phase
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
                await IngameScene.ExercuteSkillQueue();

                Instance.ChangeToNextPhase();
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

            public override async void Enter()
            {
                if (Instance._turnChangeTime % 2 == 0) await IngameScene.DoEffectsOnChars(CurTurn);
                    
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
                EPhase.Conclude => new ConcludePhase(EPhase.Conclude),
                _ => null
            };
        }

        private void ChangePhase(EPhase nextPhase)
        {
            _curPhase?.Exit();
            // CurPhase = _phaseDict[nextPhase];
            _curPhase = GetPhase(nextPhase);
            Debug.Log($"{_curPhase.Type} started.");
            _curPhase.Enter();
        }

        public void ChangeToNextPhase()
        {
            if (_curPhase.Type == EPhase.Conclude) 
            {
                ChangeToNextTurn();
                return;
            }

            var nextPhase = _curPhase.Type switch
            {
                EPhase.Opening => EPhase.Attack,
                EPhase.Attack => EPhase.Conclude,
                // EPhase.Conclude => EPhase.Opening,
                _ => EPhase.None
            };

            // var nextPhase = _curPhase++;
            // if (nextPhase == EPhase.None) nextPhase++;

            ChangePhase(nextPhase);
        }
        #endregion

        #region Gameplay Logic
        // public async void StartGame()
        public async void StartGame(List<CharacterCard> selfTeam, List<CharacterCard> enemyTeam)
        {
            // Debug.Log($"1.Start game with self team: {selfTeam[0].Name}, {selfTeam[1].Name}, {selfTeam[2].Name}");
            // Debug.Log($" and enemy team: {enemyTeam[0].Name}");

            SceneNavigator.Instance.OnSceneLoaded.AddListener(async (sk) =>
            {
                if(sk != EScene.Ingame) return;

                _ingameScene = FindFirstObjectByType<IngameSceneView>();
                await _ingameScene.InitScene(selfTeam, enemyTeam);
                _turnChangeTime = 0;

                await _ingameScene.DrawCard(3);
                // var selfDrawTask = _ingameScene.DrawCard(3);
                // await Task.WhenAll(selfDrawTask);
                ChangeTurn(EPlayerTarget.Ally); 
            });

            await SceneNavigator.Instance.ChangeScene(EScene.Ingame);
        }
        #endregion

        #region Init For Testing
        // void Start()
        // {
        //     StartGame();
        // }

        void Update()
        {
            _curPhase?.Do();
        }
        #endregion
    }
}