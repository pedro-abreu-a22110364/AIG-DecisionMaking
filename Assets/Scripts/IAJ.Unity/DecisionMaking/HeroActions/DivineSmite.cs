using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.IAJ.Unity.Utils;
using System;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions
{
    public class DivineSmite : WalkToTargetAndExecuteAction
    {
        private const int MANA_COST = 2; // Cost of using Divine Smite
        private int xpChange;             // XP gained from the attack

        public DivineSmite(AutonomousCharacter character, GameObject target) : base("DivineSmite", character, target)
        {
            // Set XP change based on the type of enemy defeated
            this.xpChange = target.tag.Equals("Skeleton") ? 3 : 0; // Set appropriate XP gain for defeating skeletons
        }

        public override float GetGoalChange(Goal goal)
        {
            var change = base.GetGoalChange(goal);

            if (goal.Name == AutonomousCharacter.GAIN_LEVEL_GOAL)
            {
                change += -xpChange; // Deduct XP for using a smite
            }

            return change;
        }

        public override void Execute()
        {
            base.Execute();
            if (CanExecute()) // Check if action can be executed
            {
                GameManager.Instance.DivineSmite(this.Target);
            }
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            // Call base method to apply any effects defined in WalkToTargetAndExecuteAction
            base.ApplyActionEffects(worldModel);

            // Immediately defeat the target and gain XP if it's an undead
            if (this.Target.tag.Equals("Skeleton"))
            {
                // Disable the target object to indicate it's defeated
                worldModel.SetProperty(this.Target.name, false);

                // Gain XP for defeating the skeleton
                int currentXP = (int)worldModel.GetProperty(PropertiesName.XP);
                worldModel.SetProperty(PropertiesName.XP, currentXP + this.xpChange);

                // Deduct mana cost for using Divine Smite
                int currentMana = (int)worldModel.GetProperty(PropertiesName.MANA);
                worldModel.SetProperty(PropertiesName.MANA, currentMana - MANA_COST);
            }
        }

        public override bool CanExecute()
        {
            // Check if the character has enough mana to use Divine Smite
            return base.CanExecute() && Character.baseStats.Mana >= MANA_COST && this.Target.tag.Equals("Skeleton");
        }

        public override bool CanExecute(WorldModel worldModel)
        {
            // Check if the character has enough mana to use Divine Smite
            return base.CanExecute(worldModel) && (int)worldModel.GetProperty(PropertiesName.MANA) >= MANA_COST && this.Target.tag.Equals("Skeleton");
        }

        public override float GetHValue(WorldModel worldModel)
        {
            // Heuristic value might remain similar as it was before
            int xp = (int)worldModel.GetProperty(PropertiesName.XP);
            int level = (int)worldModel.GetProperty(PropertiesName.LEVEL);

            return base.GetHValue(worldModel) * 0.5f + ((float)Math.Min(xpChange, 1)) * 0.5f; // Adjust heuristic based on XP change
        }
    }
}
