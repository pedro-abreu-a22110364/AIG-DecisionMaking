using UnityEngine;
using Assets.Scripts.IAJ.Unity.Utils;
using Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions;
using System.Collections.Generic;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel
{
    // Implementation of a WorldModel using fixed-size arrays (no dictionaries)
    public class FixedArrayWorldModel : WorldModel
    {
        // Constants for array indices corresponding to specific properties
        private const int MANA_INDEX = 0;
        private const int MAXMANA_INDEX = 1;
        private const int XP_INDEX = 2;
        private const int MAXHP_INDEX = 3;
        private const int HP_INDEX = 4;
        private const int SHIELDHP_INDEX = 5;
        private const int MAXSHIELDHP_INDEX = 6;
        private const int MONEY_INDEX = 7;
        private const int TIME_INDEX = 8;
        private const int LEVEL_INDEX = 9;
        private const int POSITION_X_INDEX = 10;
        private const int POSITION_Y_INDEX = 11;
        private const int POSITION_Z_INDEX = 12;
        private const int DURATION_INDEX = 13;
        private const int PREVIOUSLEVEL_INDEX = 14;
        private const int PREVIOUSMONEY_INDEX = 15;

        // Array to store all the world properties
        private object[] properties;
        private bool[] consumables;
        private bool[] enemies;

        protected FixedArrayWorldModel Parent { get; set; }

        // Constructor to initialize the World Model
        public FixedArrayWorldModel(GameManager gameManager, AutonomousCharacter character, List<Action> actions, List<Goal> goals)
        {
            // Initialize the arrays with a fixed size
            this.properties = new object[16]; // 16 properties to store world state
            this.consumables = new bool[10]; // Example size for consumables (adjust as needed)
            this.enemies = new bool[5]; // Example size for enemies (adjust as needed)

            // Set initial properties based on the character's base stats
            this.properties[MANA_INDEX] = gameManager.Character.baseStats.Mana;
            this.properties[MAXMANA_INDEX] = gameManager.Character.baseStats.MaxMana;
            this.properties[XP_INDEX] = gameManager.Character.baseStats.XP;
            this.properties[MAXHP_INDEX] = gameManager.Character.baseStats.MaxHP;
            this.properties[HP_INDEX] = gameManager.Character.baseStats.HP;
            this.properties[SHIELDHP_INDEX] = gameManager.Character.baseStats.ShieldHP;
            this.properties[MAXSHIELDHP_INDEX] = gameManager.Character.baseStats.MaxShieldHp;
            this.properties[MONEY_INDEX] = gameManager.Character.baseStats.Money;
            this.properties[TIME_INDEX] = gameManager.Character.baseStats.Time;
            this.properties[LEVEL_INDEX] = gameManager.Character.baseStats.Level;
            this.properties[POSITION_X_INDEX] = gameManager.Character.gameObject.transform.position.x;
            this.properties[POSITION_Y_INDEX] = gameManager.Character.gameObject.transform.position.y;
            this.properties[POSITION_Z_INDEX] = gameManager.Character.gameObject.transform.position.z;
            this.properties[DURATION_INDEX] = 0.0f;
            this.properties[PREVIOUSLEVEL_INDEX] = 1;
            this.properties[PREVIOUSMONEY_INDEX] = 0;

            this.GameManager = gameManager;
            this.Character = character;
            this.Actions = new List<Action>(actions);
            this.Actions.Shuffle();
            this.ActionEnumerator = this.Actions.GetEnumerator();
            this.NextPlayer = 0;
        }

        // Constructor to create a child world model based on the parent world model
        public FixedArrayWorldModel(FixedArrayWorldModel parent)
        {
            // Initialize the arrays
            this.properties = new object[16]; // 16 properties for the world state
            this.consumables = new bool[10]; // Example size for consumables
            this.enemies = new bool[5]; // Example size for enemies

            // Copy over the properties from the parent
            for (int i = 0; i < this.properties.Length; i++)
            {
                this.properties[i] = parent.properties[i]; // Copy property values
            }

            // Copy over the consumables state from the parent
            for (int i = 0; i < this.consumables.Length; i++)
            {
                this.consumables[i] = parent.consumables[i]; // Copy consumable state
            }

            // Copy over the enemies state from the parent
            for (int i = 0; i < this.enemies.Length; i++)
            {
                this.enemies[i] = parent.enemies[i]; // Copy enemies state
            }

            // Set the character, actions, and game manager to be the same as the parent's
            this.GameManager = parent.GameManager;
            this.Character = parent.Character;

            // Copy the actions list and shuffle it
            this.Actions = new List<Action>(parent.Actions); // Copy actions from the parent
            this.Actions.Shuffle(); // Shuffle actions (to ensure randomness)
            this.ActionEnumerator = this.Actions.GetEnumerator();

            this.Parent = parent;

            // Default NextPlayer setup
            this.NextPlayer = 0;
        }


        // Generate a child world model based on the current world state
        public override WorldModel GenerateChildWorldModel()
        {
            return new FixedArrayWorldModel(this.GameManager, this.Character, this.Actions, new List<Goal>());
        }

        // Get a property from the array based on the property name
        public override object GetProperty(string propertyName)
        {
            switch (propertyName)
            {
                case PropertiesName.MANA:
                    return this.GameManager.Character.baseStats.Mana;
                case PropertiesName.MAXMANA:
                    return this.GameManager.Character.baseStats.MaxMana;
                case PropertiesName.XP:
                    return this.GameManager.Character.baseStats.XP;
                case PropertiesName.MAXHP:
                    return this.GameManager.Character.baseStats.MaxHP;
                case PropertiesName.HP:
                    return this.GameManager.Character.baseStats.HP;
                case PropertiesName.ShieldHP:
                    return this.GameManager.Character.baseStats.ShieldHP;
                case PropertiesName.MaxShieldHP:
                    return this.GameManager.Character.baseStats.MaxShieldHp;
                case PropertiesName.MONEY:
                    return this.GameManager.Character.baseStats.Money;
                case PropertiesName.TIME:
                    return this.GameManager.Character.baseStats.Time;
                case PropertiesName.LEVEL:
                    return this.GameManager.Character.baseStats.Level;
                case PropertiesName.POSITION:
                    return this.GameManager.Character.gameObject.transform.position;
                case PropertiesName.DURATION:
                    return 0.0f;
                case PropertiesName.PreviousLEVEL:
                    return 1;
                case PropertiesName.PreviousMONEY:
                    return 0;
                default:
                    return this.GameManager.disposableObjects.ContainsKey(propertyName); // Could also handle consumables and enemies if needed
            }
        }

        // Set a property in the array
        public override void SetProperty(string propertyName, object value)
        {
            switch (propertyName)
            {
                case PropertiesName.MANA:
                    properties[MANA_INDEX] = value;
                    break;
                case PropertiesName.MAXMANA:
                    properties[MAXMANA_INDEX] = value;
                    break;
                case PropertiesName.XP:
                    properties[XP_INDEX] = value;
                    break;
                case PropertiesName.MAXHP:
                    properties[MAXHP_INDEX] = value;
                    break;
                case PropertiesName.HP:
                    properties[HP_INDEX] = value;
                    break;
                case PropertiesName.ShieldHP:
                    properties[SHIELDHP_INDEX] = value;
                    break;
                case PropertiesName.MaxShieldHP:
                    properties[MAXSHIELDHP_INDEX] = value;
                    break;
                case PropertiesName.MONEY:
                    properties[MONEY_INDEX] = value;
                    break;
                case PropertiesName.TIME:
                    properties[TIME_INDEX] = value;
                    break;
                case PropertiesName.LEVEL:
                    properties[LEVEL_INDEX] = value;
                    break;
                case PropertiesName.POSITION:
                    Vector3 position = (Vector3)value;
                    properties[POSITION_X_INDEX] = position.x;
                    properties[POSITION_Y_INDEX] = position.y;
                    properties[POSITION_Z_INDEX] = position.z;
                    break;
                case PropertiesName.DURATION:
                    properties[DURATION_INDEX] = value;
                    break;
                case PropertiesName.PreviousLEVEL:
                    properties[PREVIOUSLEVEL_INDEX] = value;
                    break;
                case PropertiesName.PreviousMONEY:
                    properties[PREVIOUSMONEY_INDEX] = value;
                    break;
            }
        }

        public override float GetGoalValue(string goalName)
        {
            return 0;
        }

        public override void SetGoalValue(string goalName, float value)
        {
        }

        // Check if the world state is terminal
        public override bool IsTerminal()
        {
            return (int)properties[HP_INDEX] <= 0 || (float)properties[TIME_INDEX] >= GameManager.GameConstants.TIME_LIMIT || (NextPlayer == 0 && (int)properties[MONEY_INDEX] == 25);
        }

        // Calculate the score based on the current world state
        public override float GetScore()
        {
            int money = (int)properties[MONEY_INDEX];
            int HP = (int)properties[HP_INDEX];
            float time = (float)properties[TIME_INDEX];

            if (HP <= 0 || time >= GameManager.GameConstants.TIME_LIMIT) // Lose
                return 0.0f;
            else if (NextPlayer == 0 && money == 25 && HP > 0) // Win
                return 1.0f;
            else // Non-terminal state
            {
                return timeAndMoneyScore(time, money) * levelScore() * hpScore(HP) * timeScore(time);
            }
        }

        // Helper method to calculate time and money score
        private float timeAndMoneyScore(float time, int money)
        {
            float relationTimeMoney = time - 6 * money;

            if (relationTimeMoney > 30)
                return 0;
            else if (relationTimeMoney < 0)
                return 0.6f;
            else
                return 0.3f;
        }

        // Helper method for time score
        private float timeScore(float time)
        {
            return (1 - time / GameManager.GameConstants.TIME_LIMIT);
        }

        // Helper method for level score
        private float levelScore()
        {
            int level = (int)this.GetProperty(PropertiesName.LEVEL);
            if (level == 2)
                return 1f;
            else if (level == 1)
                return 0.4f;
            else
                return 0;
        }

        private float hpScore(int hp)
        {
            if (hp > 18) //survives orc and dragon
                return 1f;
            if (hp > 12) //survives dragon or two orcs
                return 0.6f;
            else if (hp > 6) //survives orc
                return 0.1f;
            else
                return 0.01f;

        }
    }
}