using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.Game;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions
{
    public class GetManaPotion : WalkToTargetAndExecuteAction
    {
        private const int MANA_RESTORE_AMOUNT = 10; // Amount of mana restored by the potion

        public GetManaPotion(AutonomousCharacter character, GameObject target)
            : base("GetManaPotion", character, target)
        {
        }

        // Check if the action can be executed (e.g., the character needs mana)
        public override bool CanExecute()
        {
            if (!base.CanExecute()) return false;
            return Character.baseStats.Mana < Character.baseStats.MaxMana; // Only execute if mana is not full
        }

        // Check execution conditions in the world model (simulation)
        public override bool CanExecute(WorldModel worldModel)
        {
            if (!base.CanExecute(worldModel)) return false;

            var currentMana = (int)worldModel.GetProperty(PropertiesName.MANA);
            var maxMana = (int)worldModel.GetProperty(PropertiesName.MAXMANA);
            return currentMana < maxMana; // Only execute if mana is less than max
        }

        // Execute the action in the actual game world
        public override void Execute()
        {
            base.Execute();
            GameManager.Instance.GetManaPotion(this.Target); // Call the game manager to get the mana potion
        }

        // Apply the effects of the action in the simulated world (i.e., update the world model)
        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

            // Restore mana to the full amount
            worldModel.SetProperty(PropertiesName.MANA, MANA_RESTORE_AMOUNT); // Set mana to the restored amount

            // Disable the target object in the simulation so it can't be reused
            worldModel.SetProperty(this.Target.name, false);
        }

        // Predict how much this action will change the character's goals
        public override float GetGoalChange(Goal goal)
        {
            var change = base.GetGoalChange(goal);

            if (goal.Name == AutonomousCharacter.SURVIVE_GOAL)
            {
                // If mana is needed for survival-related actions, reduce the insistence of the survival goal
                change -= goal.InsistenceValue * 0.2f; // Adjust value based on mana importance
            }
            else if (goal.Name == AutonomousCharacter.BE_QUICK_GOAL)
            {
                // Mana might help with being quick, adjust accordingly
                change += this.Duration;
            }

            return change;
        }

        // Calculate the heuristic value of this action, depending on the character's current mana level
        public override float GetHValue(WorldModel worldModel)
        {
            var currentMana = (int)worldModel.GetProperty(PropertiesName.MANA);
            var maxMana = (int)worldModel.GetProperty(PropertiesName.MAXMANA);

            // If mana is low, this action should be prioritized. Otherwise, deprioritize it
            return currentMana / (float)maxMana * 0.5f + base.GetHValue(worldModel) * 0.5f;
        }
    }
}

