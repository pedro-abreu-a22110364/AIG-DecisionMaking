using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class MCTSBiasedPlayout : MCTS
    {
        public MCTSBiasedPlayout(WorldModel currentStateWorldModel, int maxIter, int maxIterFrame, int playouts, int playoutDepthLimit)
            : base(currentStateWorldModel, maxIter, maxIterFrame, playouts, playoutDepthLimit)
        {
        }

        // Override the playout function with a biased playout
        protected override float Playout(WorldModel initialStateForPlayout)
        {
            int depth = 0;
            var currentState = initialStateForPlayout.GenerateChildWorldModel();

            while (!currentState.IsTerminal() && depth < this.PlayoutDepthLimit)
            {
                var executableActions = currentState.GetExecutableActions();
                if (executableActions.Length == 0) break;

                // Select the action based on a heuristic-biased method instead of uniform random
                var selectedAction = this.BiasedActionSelection(executableActions, currentState);

                selectedAction.ApplyActionEffects(currentState);
                depth++;
            }

            if (!currentState.IsTerminal())
            {
                return this.HeuristicEvaluation(currentState);
            }

            return currentState.GetScore(); // Return the final reward after the playout
        }

        // Biased action selection method
        protected HeroActions.Action BiasedActionSelection(HeroActions.Action[] executableActions, WorldModel currentState)
        {
            // Define a heuristic for each action (example: some actions are preferred over others)
            Dictionary<HeroActions.Action, float> actionScores = new Dictionary<HeroActions.Action, float>();

            foreach (var action in executableActions)
            {
                // Calculate the heuristic score for each action (e.g., based on action type, utility, etc.)
                actionScores[action] = this.EvaluateActionHeuristic(action, currentState);
            }

            // Normalize scores to convert them into probabilities
            float totalScore = actionScores.Values.Sum();
            float randomValue = (float)this.RandomGenerator.NextDouble() * totalScore;

            // Select action based on weighted random selection
            float cumulativeScore = 0.0f;
            foreach (var action in actionScores)
            {
                cumulativeScore += action.Value;
                if (randomValue <= cumulativeScore)
                {
                    return action.Key;
                }
            }

            return executableActions[0]; // Fallback in case something goes wrong (shouldn't happen)
        }

        // Example heuristic evaluation function for actions
        protected float EvaluateActionHeuristic(HeroActions.Action action, WorldModel currentState)
        {
            return action.GetHValue(currentState);
        }
        protected virtual float HeuristicEvaluation(WorldModel state)
        {
            float playerMoney = (int)state.GetProperty(PropertiesName.MONEY);
            float playerLevel = (int)state.GetProperty(PropertiesName.LEVEL);

            float heuristicScore = playerMoney * 0.5f
                                  + playerLevel * 1.0f;

            // Return the heuristic score
            return heuristicScore;
        }
    }
}
