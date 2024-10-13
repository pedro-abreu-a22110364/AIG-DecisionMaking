using UnityEngine;
using Assets.Scripts.IAJ.Unity.Utils;
using Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel
{
    public class FixedArrayWorldModel : WorldModel
    {
        // Constants for array indices
        private enum PropertyIndex
        {
            MANA = 0, MAXMANA, XP, MAXHP, HP, SHIELDHP, MAXSHIELDHP, MONEY,
            TIME, LEVEL, POSITION_X, POSITION_Y, POSITION_Z, DURATION, PREVIOUSLEVEL, PREVIOUSMONEY
        }

        private enum GoalIndex
        {
            SURVIVE_GOAL = 0, GAIN_LEVEL_GOAL, BE_QUICK_GOAL, GET_RICH_GOAL
        }

        // Arrays to store world properties, consumables, and enemies
        private object[] properties;
        private bool[] consumables;
        private bool[] enemies;
        private float[] goalValues;

        protected FixedArrayWorldModel Parent { get; set; }

        // Constructor for the base world model
        public FixedArrayWorldModel(GameManager gameManager, AutonomousCharacter character, List<Action> actions, List<Goal> goals)
        {
            this.properties = new object[16]; // 16 properties to store world state
            this.consumables = new bool[10];
            this.enemies = new bool[5];
            this.goalValues = new float[4];

            var baseStats = gameManager.Character.baseStats;

            // Initialize properties from character stats
            properties[(int)PropertyIndex.MANA] = baseStats.Mana;
            properties[(int)PropertyIndex.MAXMANA] = baseStats.MaxMana;
            properties[(int)PropertyIndex.XP] = baseStats.XP;
            properties[(int)PropertyIndex.MAXHP] = baseStats.MaxHP;
            properties[(int)PropertyIndex.HP] = baseStats.HP;
            properties[(int)PropertyIndex.SHIELDHP] = baseStats.ShieldHP;
            properties[(int)PropertyIndex.MAXSHIELDHP] = baseStats.MaxShieldHp;
            properties[(int)PropertyIndex.MONEY] = baseStats.Money;
            properties[(int)PropertyIndex.TIME] = baseStats.Time;
            properties[(int)PropertyIndex.LEVEL] = baseStats.Level;
            Vector3 pos = character.gameObject.transform.position;
            properties[(int)PropertyIndex.POSITION_X] = pos.x;
            properties[(int)PropertyIndex.POSITION_Y] = pos.y;
            properties[(int)PropertyIndex.POSITION_Z] = pos.z;
            properties[(int)PropertyIndex.DURATION] = 0.0f;
            properties[(int)PropertyIndex.PREVIOUSLEVEL] = 1;
            properties[(int)PropertyIndex.PREVIOUSMONEY] = 0;

            // Initialize goals array from goals list
            foreach (Goal goal in goals)
            {
                if (goal.Name == "Survive")
                    this.goalValues[(int)GoalIndex.SURVIVE_GOAL] = goal.InsistenceValue;
                else if (goal.Name == "GainLevel")
                    this.goalValues[(int)GoalIndex.GAIN_LEVEL_GOAL] = goal.InsistenceValue;
                else if (goal.Name == "BeQuick")
                    this.goalValues[(int)GoalIndex.BE_QUICK_GOAL] = goal.InsistenceValue;
                else if (goal.Name == "GetRich")
                    this.goalValues[(int)GoalIndex.GET_RICH_GOAL] = goal.InsistenceValue;
            }

            this.GameManager = gameManager;
            this.Character = character;
            this.Actions = new List<Action>(actions);
            this.Actions.Shuffle();
            this.ActionEnumerator = this.Actions.GetEnumerator();
            this.NextPlayer = 0;
        }

        // Constructor for a child world model
        public FixedArrayWorldModel(FixedArrayWorldModel parent)
        {
            this.properties = new object[16];
            this.consumables = new bool[10];
            this.enemies = new bool[5];
            this.goalValues = new float[4];

            // Use Array.Copy for efficient copying
            System.Array.Copy(parent.properties, this.properties, 16);
            System.Array.Copy(parent.consumables, this.consumables, 10);
            System.Array.Copy(parent.enemies, this.enemies, 5);
            System.Array.Copy(parent.goalValues, this.goalValues, 4);

            this.GameManager = parent.GameManager;
            this.Character = parent.Character;
            this.Actions = new List<Action>(parent.Actions);
            this.Actions.Shuffle();
            this.ActionEnumerator = this.Actions.GetEnumerator();

            this.Parent = parent;
            this.NextPlayer = 0;
        }

        public override WorldModel GenerateChildWorldModel()
        {
            return new FixedArrayWorldModel(this);
        }

        // Refactored GetProperty with array access
        public override object GetProperty(string propertyName)
        {
            switch (propertyName)
            {
                case PropertiesName.MANA:
                    return properties[(int)PropertyIndex.MANA];
                case PropertiesName.MAXMANA:
                    return properties[(int)PropertyIndex.MAXMANA];
                case PropertiesName.XP:
                    return properties[(int)PropertyIndex.XP];
                case PropertiesName.MAXHP:
                    return properties[(int)PropertyIndex.MAXHP];
                case PropertiesName.HP:
                    return properties[(int)PropertyIndex.HP];
                case PropertiesName.ShieldHP:
                    return properties[(int)PropertyIndex.SHIELDHP];
                case PropertiesName.MaxShieldHP:
                    return properties[(int)PropertyIndex.MAXSHIELDHP];
                case PropertiesName.MONEY:
                    return properties[(int)PropertyIndex.MONEY];
                case PropertiesName.TIME:
                    return properties[(int)PropertyIndex.TIME];
                case PropertiesName.LEVEL:
                    return properties[(int)PropertyIndex.LEVEL];
                case PropertiesName.POSITION:
                    return new Vector3(
                        (float)properties[(int)PropertyIndex.POSITION_X],
                        (float)properties[(int)PropertyIndex.POSITION_Y],
                        (float)properties[(int)PropertyIndex.POSITION_Z]);
                case PropertiesName.DURATION:
                    return properties[(int)PropertyIndex.DURATION];
                case PropertiesName.PreviousLEVEL:
                    return properties[(int)PropertyIndex.PREVIOUSLEVEL];
                case PropertiesName.PreviousMONEY:
                    return properties[(int)PropertyIndex.PREVIOUSMONEY];
                default:
                    return this.GameManager.disposableObjects.ContainsKey(propertyName);
            }
        }

        // Refactored SetProperty with direct array access
        public override void SetProperty(string propertyName, object value)
        {
            switch (propertyName)
            {
                case PropertiesName.MANA:
                    properties[(int)PropertyIndex.MANA] = value;
                    break;
                case PropertiesName.MAXMANA:
                    properties[(int)PropertyIndex.MAXMANA] = value;
                    break;
                case PropertiesName.XP:
                    properties[(int)PropertyIndex.XP] = value;
                    break;
                case PropertiesName.MAXHP:
                    properties[(int)PropertyIndex.MAXHP] = value;
                    break;
                case PropertiesName.HP:
                    properties[(int)PropertyIndex.HP] = value;
                    break;
                case PropertiesName.ShieldHP:
                    properties[(int)PropertyIndex.SHIELDHP] = value;
                    break;
                case PropertiesName.MaxShieldHP:
                    properties[(int)PropertyIndex.MAXSHIELDHP] = value;
                    break;
                case PropertiesName.MONEY:
                    properties[(int)PropertyIndex.MONEY] = value;
                    break;
                case PropertiesName.TIME:
                    properties[(int)PropertyIndex.TIME] = value;
                    break;
                case PropertiesName.LEVEL:
                    properties[(int)PropertyIndex.LEVEL] = value;
                    break;
                case PropertiesName.POSITION:
                    Vector3 pos = (Vector3)value;
                    properties[(int)PropertyIndex.POSITION_X] = pos.x;
                    properties[(int)PropertyIndex.POSITION_Y] = pos.y;
                    properties[(int)PropertyIndex.POSITION_Z] = pos.z;
                    break;
                case PropertiesName.DURATION:
                    properties[(int)PropertyIndex.DURATION] = value;
                    break;
            }
        }

        public int GetGoalArrayIndex(string goalName)
        {
            if (goalName == "Survive")
                return (int)GoalIndex.SURVIVE_GOAL;
            else if (goalName == "GainLevel")
                return (int)GoalIndex.GAIN_LEVEL_GOAL;
            else if (goalName == "BeQuick")
                return (int)GoalIndex.BE_QUICK_GOAL;
            else if (goalName == "GetRich")
                return (int)GoalIndex.GET_RICH_GOAL;
            return -1;
        }

        public override float GetGoalValue(string goalName)
        {
            FixedArrayWorldModel current = this;
            int goalIndex = GetGoalArrayIndex(goalName);

            // Traverse up the parent hierarchy until we find the goal value or reach the root
            while (current != null)
            {
                if (goalIndex != -1)
                {
                    return current.goalValues[goalIndex];
                }
                current = current.Parent;
            }

            // If the goal is not found, return a default value (e.g., 0) or handle it appropriately
            return 0.0f; // Or throw an exception if a goal must always exist
        }

        public override void SetGoalValue(string goalName, float value)
        {
            // Ensure GoalValues is initialized before setting the value
            if (this.goalValues == null)
            {
                this.goalValues = new float[4];
            }

            int goalIndex = GetGoalArrayIndex(goalName);

            this.goalValues[goalIndex] = value;
        }

        // Optimized terminal check
        public override bool IsTerminal()
        {
            int hp = (int)properties[(int)PropertyIndex.HP];
            float time = (float)properties[(int)PropertyIndex.TIME];
            int money = (int)properties[(int)PropertyIndex.MONEY];

            return hp <= 0 || time >= GameManager.GameConstants.TIME_LIMIT || (NextPlayer == 0 && money == 25);
        }

        // Optimized GetScore method
        public override float GetScore()
        {
            int money = (int)properties[(int)PropertyIndex.MONEY];
            int hp = (int)properties[(int)PropertyIndex.HP];
            float time = (float)properties[(int)PropertyIndex.TIME];

            if (hp <= 0 || time >= GameManager.GameConstants.TIME_LIMIT) return 0.0f;
            if (NextPlayer == 0 && money == 25 && hp > 0) return 1.0f;

            return timeAndMoneyScore(time, money) * levelScore() * hpScore(hp) * timeScore(time);
        }

        // Helper methods (unchanged logic)
        private float timeAndMoneyScore(float time, int money) => (time - 6 * money) switch
        {
            > 30 => 0f,
            < 0 => 0.6f,
            _ => 0.3f,
        };

        private float timeScore(float time) => 1 - time / GameManager.GameConstants.TIME_LIMIT;

        private float levelScore()
        {
            int level = (int)this.GetProperty(PropertiesName.LEVEL);
            return level switch
            {
                2 => 1f,
                1 => 0.4f,
                _ => 0f,
            };
        }

        private float hpScore(int hp) => hp switch
        {
            > 18 => 1f,
            > 12 => 0.6f,
            > 6 => 0.1f,
            _ => 0.01f,
        };
    }
}
