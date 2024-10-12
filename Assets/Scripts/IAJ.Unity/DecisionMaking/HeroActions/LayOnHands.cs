using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions
{
    public class LayOnHands : Action
    {
        private const int MANA_COST = 7; // Mana cost for lay on hands
        private const int LEVEL = 2; //Level needed to lay on hands

        public LayOnHands(AutonomousCharacter character) : base("LayOnHands", character) { }

        public override bool CanExecute()
        {
            var currentLevel = Character.baseStats.Level;

            var currentHP = Character.baseStats.HP;

            var maxHP = Character.baseStats.MaxHP;

            var currentMana = Character.baseStats.Mana;

            return currentMana >= MANA_COST && currentLevel >= LEVEL && currentHP < maxHP;
        }

        public override bool CanExecute(WorldModel worldModel)
        {
            var currentLevel = (int)worldModel.GetProperty(PropertiesName.LEVEL);

            var currentHP = (int)worldModel.GetProperty(PropertiesName.HP);

            var maxHP = (int)worldModel.GetProperty(PropertiesName.MAXHP);

            var currentMana = (int)worldModel.GetProperty(PropertiesName.MANA);

            return currentMana >= MANA_COST && currentLevel >= LEVEL && currentHP < maxHP;
        }

        public override void Execute()
        {
            // Execute Lay On Hands by spending Mana and restoring HP
            GameManager.Instance.LayOnHands();
            Debug.Log("Lay On Hands executed: Full HP restored.");
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

            worldModel.SetProperty(PropertiesName.HP, Character.baseStats.MaxHP); // Restores HP

            // Deduct mana cost from the character
            var currentMana = (int)worldModel.GetProperty(PropertiesName.MANA);
            worldModel.SetProperty(PropertiesName.MANA, currentMana - MANA_COST); // Deduct 5 mana
        }

        public override float GetGoalChange(Goal goal)
        {
            var change = base.GetGoalChange(goal);
            // You can add logic here if you want to adjust goal changes based on shield effects

            return change;
        }

        public override float GetDuration()
        {
            // Assuming this action takes a specific amount of time in-game (e.g., 2 seconds)
            return 2.0f;
        }
        public override float GetHValue(WorldModel worldModel)
        {
            var currentHP = (int)worldModel.GetProperty(PropertiesName.HP);
            var maxHP = (int)worldModel.GetProperty(PropertiesName.MAXHP);

            return currentHP / maxHP * 0.5f;
        }
    }
}
