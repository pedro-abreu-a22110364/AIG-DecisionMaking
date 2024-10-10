using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions
{
    public class LayOnHands : Action
    {
        public LayOnHands(AutonomousCharacter character) : base("LayOnHands", character)
        {
        }

        public override bool CanExecute()
        {
            return Character.baseStats.Mana >= 7 && Character.baseStats.HP < Character.baseStats.MaxHP;
        }

        public override void Execute()
        {
            // Execute Lay On Hands by spending Mana and restoring HP
            Character.baseStats.Mana -= 7;
            Character.baseStats.HP = Character.baseStats.MaxHP; // Restores all HP
            Debug.Log("Lay On Hands executed: Full HP restored.");
        }

        public override float GetDuration()
        {
            // Assuming this action takes a specific amount of time in-game (e.g., 2 seconds)
            return 2.0f;
        }
    }
}
