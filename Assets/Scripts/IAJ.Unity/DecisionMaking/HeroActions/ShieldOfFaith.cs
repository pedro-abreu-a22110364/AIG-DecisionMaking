using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.IAJ.Unity.Utils;
using System;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions
{
    public class ShieldOfFaith : Action
    {
        private const int SHIELD_AMOUNT = 5; // Amount of shield provided
        private const int MANA_COST = 5; // Mana cost for casting Shield of Faith

        public ShieldOfFaith(AutonomousCharacter character) : base("GetManaPotion", character)
        {
        }

        // Check if the action can be executed (e.g., check if enough mana)
        public override bool CanExecute()
        {
            return Character.baseStats.Mana >= MANA_COST; // Execute only if enough mana is available
        }

        // Check execution conditions in the world model (simulation)
        public override bool CanExecute(WorldModel worldModel)
        {
            var currentMana = (int)worldModel.GetProperty(PropertiesName.MANA);
            return currentMana >= MANA_COST; // Execute only if enough mana is available
        }

        // Execute the action in the actual game world
        public override void Execute()
        {
            base.Execute();
            GameManager.Instance.ShieldOfFaith(); // Call the game manager to cast the shield
        }

        // Apply the effects of the action in the simulated world (i.e., update the world model)
        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

            // Set the shield value in the world model
            worldModel.SetProperty(PropertiesName.ShieldHP, SHIELD_AMOUNT);

            // Deduct mana cost from the character
            var currentMana = (int)worldModel.GetProperty(PropertiesName.MANA);
            worldModel.SetProperty(PropertiesName.MANA, currentMana - MANA_COST); // Deduct 5 mana
        }

        // Predict how much this action will change the character's goals
        public override float GetGoalChange(Goal goal)
        {
            var change = base.GetGoalChange(goal);
            // You can add logic here if you want to adjust goal changes based on shield effects

            return change;
        }

        // Calculate the heuristic value of this action
        public override float GetHValue(WorldModel worldModel)
        {
            // This action should be prioritized if the character has low HP
            var currentHP = (int)worldModel.GetProperty(PropertiesName.HP);
            var shieldHP = (int)worldModel.GetProperty(PropertiesName.ShieldHP);

            // If HP is low, this action should have higher priority
            return (currentHP < 5) ? 10.0f : 0.0f; // Prioritize if current HP is low
        }
    }
}
