using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;
using System.Linq;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.GOB
{
    public class GOBDecisionMaking
    {
        public bool InProgress { get; set; }
        public float TotalProcessingTime { get; set; }
        public int TotalActionCombinationsProcessed { get; set; }
        private List<Goal> goals { get; set; }
        private List<HeroActions.Action> actions { get; set; }

        public Dictionary<HeroActions.Action, float> ActionDiscontentment { get; set; }

        public HeroActions.Action secondBestAction { get; private set; }
        public HeroActions.Action thirdBestAction { get; private set; }

        public GOBDecisionMaking(List<HeroActions.Action> _actions, List<Goal> goals)
        {
            this.actions = _actions;
            this.goals = goals;
            this.ActionDiscontentment = new Dictionary<HeroActions.Action, float>();
        }

        public static float CalculateDiscontentment(HeroActions.Action action, List<Goal> goals, AutonomousCharacter character)
        {
            var discontentment = 0.0f;
            var duration = action.GetDuration();

            foreach (var goal in goals)
            {
                // Calculate the new value after the action
                float changeValue = action.GetGoalChange(goal) + duration * goal.ChangeRate;

                // Normalize the goal value after the change
                var newValue = goal.NormalizeGoalValue(goal.InsistenceValue + changeValue, goal.Min, goal.Max);

                // Accumulate the discontentment from this goal
                discontentment += goal.GetDiscontentment(newValue);
            }

            return discontentment;
        }

        public HeroActions.Action ChooseAction(AutonomousCharacter character)
        {
            var startTime = Time.realtimeSinceStartup;

            InProgress = true;
            HeroActions.Action bestAction = null;
            float bestValue = float.PositiveInfinity;
            secondBestAction = null;
            thirdBestAction = null;
            ActionDiscontentment.Clear();
            float value;

            foreach (var action in actions)
            {
                if (action.CanExecute())
                {
                    // Calculate discontentment for the current action
                    value = CalculateDiscontentment(action, goals, character);
                    ActionDiscontentment[action] = value;

                    // Find the best, second-best, and third-best actions
                    if (value < bestValue)
                    {
                        thirdBestAction = secondBestAction;
                        secondBestAction = bestAction;
                        bestAction = action;
                        bestValue = value;
                    }
                    else if (secondBestAction == null || value < ActionDiscontentment[secondBestAction])
                    {
                        thirdBestAction = secondBestAction;
                        secondBestAction = action;
                    }
                    else if (thirdBestAction == null || value < ActionDiscontentment[thirdBestAction])
                    {
                        thirdBestAction = action;
                    }
                }
                this.TotalActionCombinationsProcessed++;
            }

            this.TotalProcessingTime += Time.realtimeSinceStartup - startTime;
            InProgress = false;
            return bestAction;
        }
    }
}
