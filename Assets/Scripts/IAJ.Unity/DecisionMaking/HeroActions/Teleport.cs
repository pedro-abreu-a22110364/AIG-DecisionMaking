using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions
{
    public class Teleport : Action
    {
        private const int MANA_COST = 5; // Mana cost for teleport
        private const int LEVEL = 2; //Level needed to teleport

        public Teleport(AutonomousCharacter character) : base("Teleport", character) { }

        public override bool CanExecute()
        {
            // Check if Sir Uthgard has enough Mana to teleport
            return Character.baseStats.Mana >= MANA_COST && Character.baseStats.Level >= LEVEL;
        }

        public override bool CanExecute(WorldModel worldModel)
        {
            var currentLevel = (int)worldModel.GetProperty(PropertiesName.LEVEL);

            var currentMana = (int)worldModel.GetProperty(PropertiesName.MANA);

            return currentMana >= MANA_COST && currentLevel >= LEVEL; // Execute only if enough mana is available
        }

        public override void Execute()
        {
            // Teleport to the initial position
            GameManager.Instance.Teleport();
            Debug.Log("Teleport executed: Sir Uthgard teleported to the initial position.");
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

            // Deduct mana cost from the character
            var currentMana = (int)worldModel.GetProperty(PropertiesName.MANA);
            worldModel.SetProperty(PropertiesName.MANA, currentMana - MANA_COST); // Deduct 5 mana

            var current = (Vector3)worldModel.GetProperty(PropertiesName.POSITION);
            worldModel.SetProperty(PropertiesName.POSITION, GameManager.Instance.initialPosition);
        }

        public override float GetGoalChange(Goal goal)
        {
            var change = base.GetGoalChange(goal);
            // You can add logic here if you want to adjust goal changes based on shield effects

            return change;
        }

        public override float GetDuration()
        {
            return 1.0f;
        }

        public override float GetHValue(WorldModel worldModel)
        {
            return -10; //ToDo if the character is far from the objetive this should be prioritize
        }
    }
}
