using System.Collections.Generic;
using CardWar_v2.Datas;
using CardWar_v2.Entities;
using CardWar_v2.Enums;
using CardWar_v2.Factories;
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

        // Use for after match record
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
            }
        }

        // Use for ingame record
        public class ActionRecord
        {
            public int Turn;
            public List<(CharacterCard, string)> Actions = new();

            public ActionRecord(int curTurn)
            {
                Turn = curTurn;
            }

            public string GetActionSummary()
            {
                string summary = $"<color=#000000>Turn {Turn}:</color>\n";
                foreach (var action in Actions)
                {
                    summary += $"- {action.Item1.Name}: {action.Item2}.\n";
                }
                return summary;
            }
        }

        private List<CharacterCard> _selfTeam = new();
        
        // General
        public Level CurLevel { get; private set; }
        public int TurnCount { get; private set;} = 0;
        public List<ActionRecord> ActionRecords { get; private set; }

        // Ally
        public List<CharacterRecord> AllyRecords { get; private set; }

        // Enemy
        public List<CharacterRecord> EnemyRecords { get; private set; }

        public FightLogger()
        {
            _selfTeam = new();
            CurLevel = null;

            ActionRecords = new();
            AllyRecords = new();
            EnemyRecords = new();
        }

        public FightLogger(List<CharacterCard> selfTeam, Level level)
        {
            _selfTeam = new(selfTeam);
            CurLevel = level;

            ActionRecords = new();
            AllyRecords = new();
            EnemyRecords = new();
        }

        public void StartTracking()
        {
            GameplayManager.Instance.OnTurnChanged.AddListener(() => 
            {
                TurnCount++;
                ActionRecords.Add(new(TurnCount));
            });

#region Ally Record
            _selfTeam.ForEach(c =>
            {
                //TODO: Add listener for after match record
                var record = new CharacterRecord(c);
                AllyRecords.Add(record);
                
                c.OnUseSkill.AddListener(s =>
                {
                    var actionRecord = ActionRecords.Count > 1 ? ActionRecords[^1] : ActionRecords[0];
                    actionRecord.Actions.Add((c, $"Used skill {s.Name}"));
                });

                c.OnApplyEffect.AddListener(e =>
                {
                    var actionRecord = ActionRecords.Count > 1 ? ActionRecords[^1] : ActionRecords[0];
                    var textColor = EffectViewFactory.Instance.EffectDict[e.EffectType].TextColor;
                    var colorHex = ColorUtility.ToHtmlStringRGB(textColor);
                    actionRecord.Actions.Add((c, $"Applied effect <color=#{colorHex}>{e.EffectType}</color>"));
                });

                c.OnTakingDamage.AddListener((source, amount) =>
                {
                    var actionRecord = ActionRecords.Count > 1 ? ActionRecords[^1] : ActionRecords[0];
                    var effectDict = EffectViewFactory.Instance.EffectDict;
                    bool isSkillEffect = source is SkillEffect skillEffect;

                    Color dmgColor = Color.white;
                    Color sourceColor = Color.white;
                    string sourceText = "";

                    switch (source)
                    {
                        case SkillEffect effect:
                            var color = effectDict[effect.EffectType].TextColor;
                            dmgColor = color;
                            sourceColor = color;
                            sourceText = effect.EffectType.ToString();
                            break;

                        case CharacterCard character:
                            dmgColor = effectDict[ESkillEffect.None].TextColor;
                            sourceText = (source as CharacterCard)?.Name ?? "Unknown";
                            break;
                    }

                    string dmgHex = ColorUtility.ToHtmlStringRGB(dmgColor);
                    string sourceHex = ColorUtility.ToHtmlStringRGB(sourceColor);
                    string actionText =
                        $"{(amount >= 0 ? "Took" : "Gain")} " +
                        $"<color=#{dmgHex}>{amount}</color> damage " +
                        $"from <color=#{sourceHex}>{sourceText}</color>";

                    actionRecord.Actions.Add((c, actionText));
                });

                c.OnDeath.AddListener(() =>
                {
                    var actionRecord = ActionRecords.Count > 1 ? ActionRecords[^1] : ActionRecords[0];
                    actionRecord.Actions.Add((c, "Died"));
                });
            });
#endregion

#region Enemy Record
            CurLevel.Enemies.ForEach(c =>
            {
                var record = new CharacterRecord(c);
                EnemyRecords.Add(record);

                c.OnUseSkill.AddListener(s =>
                {
                    var actionRecord = ActionRecords.Count > 1 ? ActionRecords[^1] : ActionRecords[0];
                    actionRecord.Actions.Add((c, $"Used skill {s.Name}"));
                });

                c.OnApplyEffect.AddListener(e =>
                {
                    var actionRecord = ActionRecords.Count > 1 ? ActionRecords[^1] : ActionRecords[0];
                    var textColor = EffectViewFactory.Instance.EffectDict[e.EffectType].TextColor;
                    var colorHex = ColorUtility.ToHtmlStringRGB(textColor);
                    actionRecord.Actions.Add((c, $"Applied effect <color=#{colorHex}>{e.EffectType}</color>"));
                });

                c.OnTakingDamage.AddListener((source, amount) =>
                {
                    var actionRecord = ActionRecords.Count > 1 ? ActionRecords[^1] : ActionRecords[0];
                    var effectDict = EffectViewFactory.Instance.EffectDict;
                    bool isSkillEffect = source is SkillEffect skillEffect;

                    Color dmgColor = Color.white;
                    Color sourceColor = Color.white;
                    string sourceText = "";

                    switch (source)
                    {
                        case SkillEffect effect:
                            var color = effectDict[effect.EffectType].TextColor;
                            dmgColor = color;
                            sourceColor = color;
                            sourceText = effect.EffectType.ToString();
                            break;

                        case CharacterCard character:
                            dmgColor = effectDict[ESkillEffect.None].TextColor;
                            sourceText = (source as CharacterCard)?.Name ?? "Unknown";
                            break;
                    }

                    string dmgHex = ColorUtility.ToHtmlStringRGB(dmgColor);
                    string sourceHex = ColorUtility.ToHtmlStringRGB(sourceColor);
                    string actionText =
                        $"{(amount >= 0 ? "Took" : "Gain")} " +
                        $"<color=#{dmgHex}>{amount}</color> damage " +
                        $"from <color=#{sourceHex}>{sourceText}</color>";

                    actionRecord.Actions.Add((c, actionText));
                });

                c.OnDeath.AddListener(() =>
                {
                    var actionRecord = ActionRecords.Count > 1 ? ActionRecords[^1] : ActionRecords[0];
                    actionRecord.Actions.Add((c, "Died"));
                });
            });
#endregion
        }
    }
}