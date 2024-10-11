using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions
{
    public class Teleport : Action
    {

        public Teleport(AutonomousCharacter character) : base("Teleport", character) { }

        public override bool CanExecute()
        {
            // Check if Sir Uthgard has enough Mana to teleport
            return Character.baseStats.Mana >= 5;
        }

        public override void Execute()
        {
            // Execute the teleport by spending Mana and moving to the initial position
            Character.baseStats.Mana -= 5;

            // Teleport to the initial position
            GameManager.Instance.Teleport();
            Debug.Log("Teleport executed: Sir Uthgard teleported to the initial position.");
        }

        public override float GetDuration()
        {
            return 1.0f;
        }
    }
}
