using System.Collections.Generic;
using CardWar_v2.Entities;
using CardWar_v2.Enums;
using Unity.VisualScripting;
using UnityEngine;

namespace CardWar_v2.GameControl
{
    public class FightLogger
    {
        public struct DamageRecord
        {
            public float PhysicalDamage;
            public float MagicalDamage;
            public float PureDamage;
        }

        public class CharacterRecord
        {
            public CharacterCard Character;
            public DamageRecord DamageDealt;
            public DamageRecord DamageTaken;
            public float RecoveryAmount;
            public bool IsDead;

            public CharacterRecord(CharacterCard character)
            {
                Character = character;
                DamageDealt = new DamageRecord();
                DamageTaken = new DamageRecord();
                RecoveryAmount = 0f;
                IsDead = false;

                character.OnDeath.AddListener(() => IsDead = true);
                Debug.Log($"Logging character '{character.Name}' death.");
            }
        }

        public class ActionRecord
        {
            public int CurTurn;
            public List<(CharacterCard, SkillCard)> Actions = new();

            public ActionRecord(int curTurn)
            {
                CurTurn = curTurn;
            }

            public string GetActionSummary()
            {
                string summary = $"Turn {CurTurn}:\n";
                foreach (var action in Actions)
                {
                    summary += $"- {action.Item1.Name} used skill {action.Item2.Name}.\n";
                }
                return summary;
            }
        }

        private List<CharacterCard> _selfTeam = new();
        
        // General
        public Level CurLevel { get; private set; }
        public int TurnCount { get; private set;} = 0;
        public List<ActionRecord> ActionRecords { get; private set; } = new();

        // Ally
        public List<CharacterRecord> AllyRecords { get; private set; } = new();

        // Enemy
        public List<CharacterRecord> EnemyRecords { get; private set; } = new();

        public FightLogger()
        {
            _selfTeam = new();
            CurLevel = null;
        }

        public FightLogger(List<CharacterCard> selfTeam, Level level)
        {
            _selfTeam = new(selfTeam);
            CurLevel = level;
        }

        public void StartTracking()
        {
            GameplayManager.Instance.OnTurnChanged.AddListener(() => 
            {
                TurnCount++;
                ActionRecords.Add(new(TurnCount));
            });

            _selfTeam.ForEach(c =>
            {
                var record = new CharacterRecord(c);
                AllyRecords.Add(record);

                
                c.OnUseSkill.AddListener(s =>
                {
                    var actionRecord = ActionRecords[^1];
                    actionRecord.Actions.Add((c, s));
                });
            });

            CurLevel.Enemies.ForEach(c =>
            {
                var record = new CharacterRecord(c);
                EnemyRecords.Add(record);
            });
        }
    }
}