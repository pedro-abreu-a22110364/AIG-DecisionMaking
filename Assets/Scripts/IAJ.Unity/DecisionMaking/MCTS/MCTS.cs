using Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using System;
using System.Collections.Generic;
using UnityEngine;
using Action = Assets.Scripts.IAJ.Unity.DecisionMaking.HeroActions.Action;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class MCTS
    {
        public const float C = 1.4f; // Exploration constant
        public bool InProgress { get; private set; }
        protected int MaxIterations { get; set; }
        protected int MaxIterationsPerFrame { get; set; }
        protected int NumberPlayouts { get; set; }
        protected int PlayoutDepthLimit { get; set; }
        public MCTSNode BestFirstChild { get; set; }

        public List<MCTSNode> BestSequence { get; set; }
        public WorldModel BestActionSequenceEndState { get; set; }
        public int CurrentIterations { get; protected set; }
        protected int FrameCurrentIterations { get; set; }
        protected WorldModel InitialState { get; set; }
        protected MCTSNode InitialNode { get; set; }
        protected System.Random RandomGenerator { get; set; }

        // Information and Debug Properties
        public int MaxPlayoutDepthReached { get; set; }
        public int MaxSelectionDepthReached { get; set; }
        public float TotalProcessingTime { get; set; }

        public MCTS(WorldModel currentStateWorldModel, int maxIter, int maxIterFrame, int playouts, int playoutDepthLimit)
        {
            this.InitialState = currentStateWorldModel;
            this.MaxIterations = maxIter;
            this.MaxIterationsPerFrame = maxIterFrame;
            this.NumberPlayouts = playouts;
            this.PlayoutDepthLimit = playoutDepthLimit;
            this.InProgress = false;
            this.RandomGenerator = new System.Random();
        }

        public void InitializeMCTSearch()
        {
            this.InitialState.Initialize();
            this.MaxPlayoutDepthReached = 0;
            this.MaxSelectionDepthReached = 0;
            this.CurrentIterations = 0;
            this.FrameCurrentIterations = 0;
            this.TotalProcessingTime = 0.0f;

            // Create root node n0 for state s0
            this.InitialNode = new MCTSNode(this.InitialState)
            {
                Action = null,
                Parent = null,
                PlayerID = 0
            };
            this.InProgress = true;
            this.BestFirstChild = null;
        }

        public Action ChooseAction()
        {
            MCTSNode selectedNode;
            float reward;

            var startTime = Time.realtimeSinceStartup;

            // Main MCTS Loop
            while (this.CurrentIterations < this.MaxIterations)
            {
                selectedNode = this.Selection(this.InitialNode); // Selection + Expansion

                if (GameManager.Instance.StochasticWorld)
                {
                    reward = this.PlayoutStochastic(selectedNode.State);    // Playout
                } 
                else
                {
                    reward = this.Playout(selectedNode.State);       // Playout
                }

                this.Backpropagate(selectedNode, reward);        // Backpropagation
                this.CurrentIterations++;
            }

            this.TotalProcessingTime += Time.realtimeSinceStartup - startTime;

            this.InProgress = false ;
            return this.BestAction(this.InitialNode);            // Return the best action
        }

        // Selection and Expansion
        protected MCTSNode Selection(MCTSNode initialNode)
        {
            MCTSNode currentNode = initialNode;

            while (!currentNode.State.IsTerminal())
            {
                if (currentNode.ChildNodes.Count < currentNode.State.GetExecutableActions().Length)
                {
                    return this.Expand(currentNode);
                }
                else
                {
                    currentNode = this.BestUCTChild(currentNode); // UCT-based child selection
                }
            }
            return currentNode;
        }

        protected MCTSNode Expand(MCTSNode parent)
        {
            var executableActions = parent.State.GetExecutableActions();
            var untriedActions = new List<Action>();

            // Find untried actions
            foreach (var action in executableActions)
            {
                bool alreadyTried = false;
                foreach (var child in parent.ChildNodes)
                {
                    if (child.Action == action)
                    {
                        alreadyTried = true;
                        break;
                    }
                }
                if (!alreadyTried)
                {
                    untriedActions.Add(action);
                }
            }

            if (untriedActions.Count > 0)
            {
                var randomAction = untriedActions[this.RandomGenerator.Next(untriedActions.Count)];
                WorldModel newState = parent.State.GenerateChildWorldModel();
                randomAction.ApplyActionEffects(newState);
                newState.CalculateNextPlayer();

                MCTSNode newNode = new MCTSNode(newState)
                {
                    Parent = parent,
                    Action = randomAction,
                    PlayerID = 1 - parent.PlayerID // Alternate between players
                };

                parent.ChildNodes.Add(newNode);
                return newNode;
            }

            return null;
        }

        //Normal MCTS playout
        protected virtual float Playout(WorldModel initialStateForPlayout)
        {
            int depth = 0;
            var currentState = initialStateForPlayout.GenerateChildWorldModel();

            while (!currentState.IsTerminal() && depth < this.PlayoutDepthLimit)
            {
                var executableActions = currentState.GetExecutableActions();
                if (executableActions.Length == 0) break;

                var randomAction = executableActions[this.RandomGenerator.Next(executableActions.Length)];
                randomAction.ApplyActionEffects(currentState);
                depth++;
            }

            return currentState.GetScore();
        } 

        //Secret Level 2 - Limited Playout MCTS
        /*protected virtual float Playout(WorldModel initialStateForPlayout)
        {
            int depth = 0;
            var currentState = initialStateForPlayout.GenerateChildWorldModel();

            while (!currentState.IsTerminal() && depth < 50)
            {
                var executableActions = currentState.GetExecutableActions();
                if (executableActions.Length == 0) break;

                var randomAction = executableActions[this.RandomGenerator.Next(executableActions.Length)];
                randomAction.ApplyActionEffects(currentState);

                depth++;
            }

            if (!currentState.IsTerminal())
            {
                return this.HeuristicEvaluation(currentState);
            }

            return currentState.GetScore();
        }

        protected virtual float HeuristicEvaluation(WorldModel state)
        {
            float playerHealth = (int)state.GetProperty(PropertiesName.HP); 
            float playerLevel = (int)state.GetProperty(PropertiesName.LEVEL);

            float heuristicScore = playerHealth * 0.5f
                                  + playerLevel * 1.0f;

            // Return the heuristic score
            return heuristicScore;
        } */



        protected virtual float PlayoutStochastic(WorldModel initialStateForPlayout)
        {
            float bestReward = float.MinValue;

            for (int i = 0; i < this.NumberPlayouts; i++)
            {
                WorldModel currentState = initialStateForPlayout.GenerateChildWorldModel(); // Clone the state for each playout
                int depth = 0;

                // Perform a single playout simulation until terminal state or depth limit
                while (!currentState.IsTerminal() && depth < this.PlayoutDepthLimit)
                {
                    var executableActions = currentState.GetExecutableActions();
                    if (executableActions.Length == 0) break;

                    var randomAction = executableActions[this.RandomGenerator.Next(executableActions.Length)];
                    randomAction.ApplyActionEffects(currentState);

                    currentState.CalculateNextPlayer(); // Determine the next player after action

                    depth++;
                }

                // Update the best reward if this playout's score is higher
                float playoutScore = currentState.GetScore();
                if (playoutScore > bestReward)
                {
                    bestReward = playoutScore;
                }
            }

            // Return the best reward among all playouts
            return bestReward;
        }

        protected virtual void Backpropagate(MCTSNode node, float reward)
        {
            while (node != null)
            {
                node.N++;

                //if (node.Parent != null && node.Parent.PlayerID == 1)
                if (node.PlayerID == 1)
                {
                    node.Q -= reward; // Opponent aims to minimize the root player's reward
                }
                else
                {
                    node.Q += reward; // Root player aims to maximize the reward
                }

                node = node.Parent;
            }
        }

        protected virtual MCTSNode BestUCTChild(MCTSNode node)
        {
            MCTSNode bestChild = null;
            float bestValue = float.MinValue;

            foreach (var child in node.ChildNodes)
            {
                // Use UCT formula
                float uctValue = (child.Q / child.N) + C * Mathf.Sqrt(Mathf.Log(node.N) / child.N);

                if (uctValue > bestValue)
                {
                    bestValue = uctValue;
                    bestChild = child;
                }
            }

            return bestChild;
        }

        // Final action selection strategy: Max Child (highest average reward)
        protected MCTSNode BestChild(MCTSNode node)
        {
            MCTSNode bestChild = null;
            float bestValue = float.MinValue;

            foreach (var child in node.ChildNodes)
            {
                float childValue = child.Q / child.N; // Exploit based on average reward
                if (childValue > bestValue)
                {
                    bestValue = childValue;
                    bestChild = child;
                }
            }

            return bestChild;
        }

        // Returns the best action based on the final chosen strategy
        protected Action BestAction(MCTSNode node)
        {
            var bestChild = this.BestChild(node);
            if (bestChild == null) return null;

            this.BestFirstChild = bestChild;

            // For debugging purposes
            this.BestSequence = new List<MCTSNode> { bestChild };
            node = bestChild;
            this.BestActionSequenceEndState = node.State;

            while (!node.State.IsTerminal())
            {
                bestChild = this.BestChild(node);
                if (bestChild == null) break;

                this.BestSequence.Add(bestChild);
                node = bestChild;
                this.BestActionSequenceEndState = node.State;
            }

            return this.BestFirstChild.Action;
        }
    }
}