using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.Game;
using Unity.Collections;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.GOB
{
    public class DepthLimitedGOAPDecisionMaking
    {
        public const int MAX_DEPTH = 2;
        public int ActionCombinationsProcessedPerFrame { get; set; }
        public float TotalProcessingTime { get; set; }
        public int TotalActionCombinationsProcessed { get; set; }
        public bool InProgress { get; set; }

        public WorldModel InitialWorldModel { get; set; }
        private List<Goal> Goals { get; set; }
        private WorldModel[] Models { get; set; }
        private Action[] LevelAction { get; set; }
        public Action[] BestActionSequence { get; private set; }
        public Action BestAction { get; private set; }
        public float BestDiscontentmentValue { get; private set; }
        private int CurrentDepth {  get; set; }

        public DepthLimitedGOAPDecisionMaking(WorldModel currentStateWorldModel, AutonomousCharacter character)
        {
            this.ActionCombinationsProcessedPerFrame = 2000;
            this.Goals = character.Goals;
            this.InitialWorldModel = currentStateWorldModel;
        }

        public void InitializeDecisionMakingProcess()
        {
            this.InProgress = true;
            this.TotalProcessingTime = 0.0f;
            this.TotalActionCombinationsProcessed = 0;
            this.CurrentDepth = 0;
            this.Models = new DictionaryWorldModel[MAX_DEPTH + 1];
            //this.Models = new FixedArrayWorldModel[MAX_DEPTH + 1];
            this.Models[0] = this.InitialWorldModel;
            this.LevelAction = new Action[MAX_DEPTH];
            this.BestActionSequence = new Action[MAX_DEPTH];
            this.BestAction = null;
            this.BestDiscontentmentValue = float.MaxValue;
            this.InitialWorldModel.Initialize();
        }

        public Action ChooseAction()
        {
            var processedActions = 0;
            var startTime = Time.realtimeSinceStartup;

            float currentValue;
            Action nextAction;

            while (this.CurrentDepth >= 0 && processedActions < this.ActionCombinationsProcessedPerFrame)
            {
                if (this.CurrentDepth >= MAX_DEPTH)
                {
                    currentValue = this.Models[this.CurrentDepth].Character.CalculateDiscontentment(this.Models[this.CurrentDepth]);

                    if (currentValue < this.BestDiscontentmentValue)
                    {
                        this.BestDiscontentmentValue = currentValue;
                        this.BestAction = this.LevelAction[0];

                        for (int i = 0; i < this.CurrentDepth; i++)
                        {
                            this.BestActionSequence[i] = this.LevelAction[i];
                        }
                    }

                    this.CurrentDepth--;
                    continue;
                }

                nextAction = this.Models[this.CurrentDepth].GetNextAction();

                if (nextAction != null)
                {
                    this.Models[this.CurrentDepth + 1] = this.Models[this.CurrentDepth].GenerateChildWorldModel();
                    nextAction.ApplyActionEffects(this.Models[this.CurrentDepth + 1]);

                    if (this.Models[this.CurrentDepth + 1].IsTerminal())
                    {
                        continue;
                    }

                    this.Models[this.CurrentDepth + 1].Character.UpdateGoalsInsistence(this.Models[this.CurrentDepth + 1]);
                    this.LevelAction[this.CurrentDepth] = nextAction;
                    this.CurrentDepth++;
                    processedActions++;
                    this.TotalActionCombinationsProcessed++;
                } 
                else
                {
                    this.CurrentDepth--;
                }
            }

            this.TotalProcessingTime += Time.realtimeSinceStartup - startTime;
            this.InProgress = false;
            return this.BestAction;
        }
    }
}
