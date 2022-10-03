using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using System;
using System.Collections.Generic;
using UnityEngine;
using Action = Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.Action;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class MCTS
    {
        public const float C = 1.4f;
        public bool InProgress { get; private set; }
        public int MaxIterationsProcessedPerFrame { get; set; }
        public int playoutDepth { get; set; }
        public int MaxSelectionDepthReached { get; set; }
        public float TotalProcessingTime { get; private set; }
        public MCTSNode BestFirstChild { get; set; }
        public List<Action> BestActionSequence { get; private set; }
        public int playOutsPerSelection { get; set; }
        protected int CurrentIterationsInFrame { get; set; }
        protected int CurrentDepth { get; set; }

        protected CurrentStateWorldModel CurrentStateWorldModel { get; set; }
        public MCTSNode InitialNode { get; set; }
        protected System.Random RandomGenerator { get; set; }
        public Action BestAction { get; private set; }


        
        public MCTS(CurrentStateWorldModel currentStateWorldModel)
        {
            this.playoutDepth = Int32.MaxValue;
            this.InProgress = false;
            this.CurrentStateWorldModel = currentStateWorldModel;
            this.RandomGenerator = new System.Random();
        }


        public void InitializeMCTSearch()
        {
            this.MaxSelectionDepthReached = 0;
            this.CurrentIterationsInFrame = 0;
            this.TotalProcessingTime = 0.0f;
            this.CurrentStateWorldModel.Initialize();
            this.InitialNode = new MCTSNode(this.CurrentStateWorldModel)
            {
                Action = null,
                Parent = null,
                PlayerID = 0
            };
            this.InProgress = true;
            this.BestFirstChild = null;
            this.BestActionSequence = new List<Action>();
        }

        public Action Run()
        {
            var startTime = Time.realtimeSinceStartup;
            MCTSNode selectedNode;
            Reward reward;

            this.CurrentIterationsInFrame = 0;
            
            while (CurrentIterationsInFrame < MaxIterationsProcessedPerFrame) {
                selectedNode = Selection(InitialNode);
                
                for (int i = 0; i<playOutsPerSelection;i++) {
                    reward = Playout(selectedNode.State);
                    Backpropagate(selectedNode,reward);
                } 
                CurrentIterationsInFrame++;

            }

            
            this.BestAction = BestChild(InitialNode).Action;
            var elapsedTime = Time.realtimeSinceStartup -startTime;
            this.TotalProcessingTime += elapsedTime;
            InProgress = false;
            var bestAction = BestFinalAction(InitialNode);
            Debug.Log(BestChild(InitialNode).Q);
            //determinBestActionSeq();
            return bestAction;

        }

        protected void determinBestActionSeq() {
            var currentNode = InitialNode;
            this.BestActionSequence = new List<Action>();
            if (InitialNode.ChildNodes.Count==0)
                return;
            currentNode = BestChild(InitialNode);
            while (!currentNode.isTerminal()) {
                this.BestActionSequence.Add(currentNode.Action);
                currentNode = BestChild(currentNode);
                if (currentNode==null)
                    break;
            }
        }

        protected MCTSNode Selection(MCTSNode initialNode)
        {
            /* Action nextAction;
            MCTSNode currentNode = initialNode;
            //MCTSNode bestChild;

            while (currentNode.ChildNodes.Count!=0) {

                nextAction = currentNode.State.GetNextAction();

                if (nextAction != null) // not fully expanded
                    return Expand(currentNode, nextAction);
                else  // fully expanded
                    currentNode = BestChild(currentNode);
            }
            if (currentNode==InitialNode) {
                nextAction = currentNode.State.GetNextAction();
                return Expand(currentNode, nextAction);
            }
            return currentNode; */
            Action nextAction;
            MCTSNode currentNode = initialNode;
            //MCTSNode bestChild;

            do {

                nextAction = currentNode.State.GetNextAction();

                if (nextAction != null) // not fully expanded
                    return Expand(currentNode, nextAction);
                else  // fully expanded
                    currentNode = BestChild(currentNode);
            }while (currentNode.ChildNodes.Count!=0);
            return currentNode;
        }

        protected virtual Reward Playout(WorldModel initialPlayoutState)
        {
            var currentState = initialPlayoutState;
            while (!currentState.IsTerminal()) {
                var possibleActions = currentState.GetExecutableActions(); 
                var action = possibleActions[RandomGenerator.Next(possibleActions.Length)];
                var newState = currentState.GenerateChildWorldModel();
                action.ApplyActionEffects(newState);
                newState.CalculateNextPlayer();
                currentState = newState;
               
            } //TODO ver o aspecto da reward ser baseada no player
            //Debug.Log("score="+currentState.GetScore());
            return  new Reward(currentState.GetScore()/* , currentState.GetNextPlayer() */);
        }

        protected virtual void Backpropagate(MCTSNode node, Reward reward)
        {
            var currentNode = node;

            while (currentNode!=null) {
                currentNode.N++;
                //TODO o player influencia a reward? acho que nao e preciso fazer nada
                currentNode.Q += reward.Value;
                currentNode = currentNode.Parent;
            }
        }

        protected MCTSNode Expand(MCTSNode parent, Action action)
        {
            var newState = parent.State.GenerateChildWorldModel();
            action.ApplyActionEffects(newState);
            var newChild = new MCTSNode(newState);
            newChild.Parent = parent;
            newChild.Action = action;
            newChild.N = 0;
            newChild.Q = 0.0f;
            parent.ChildNodes.Add(newChild);
            return newChild;
        }

        //gets the best child of a node, using the UCT formula
        protected virtual MCTSNode BestUCTChild(MCTSNode node)
        {
            if (node.ChildNodes.Count == 0) {
                return null;
            }

            float bestRatio = (node.ChildNodes[0].Q / (float)node.ChildNodes[0].N) + Mathf.Sqrt(2 * (Mathf.Log(node.N) / (float)node.ChildNodes[0].N));
            MCTSNode bestChild = node.ChildNodes[0];
            var bestN = node.ChildNodes[0].N;

            foreach (var child in node.ChildNodes) {
                var ratio = (child.Q / (float)child.N) + Mathf.Sqrt(2 * (Mathf.Log(node.N) / (float)child.N));
                if (ratio > bestRatio) {
                    bestRatio = ratio;
                    bestChild = child;
                }
                if(ratio == bestRatio && child.N>bestN) {
                    bestRatio = ratio;
                    bestChild = child;
                    bestN = child.N;
                }
            }
            return bestChild;
        }

        //this method is very similar to the bestUCTChild, but it is used to return the final action of the MCTS search, and so we do not care about
        //the exploration factor
        protected virtual MCTSNode BestChild(MCTSNode node)
        {
            if (node.ChildNodes.Count == 0) {
                return null;
            }
            float bestRatio = node.ChildNodes[0].Q / (float)node.ChildNodes[0].N;
            MCTSNode bestChild = node.ChildNodes[0];
            var bestN = node.ChildNodes[0].N;

            foreach (var child in node.ChildNodes) {
                var ratio = child.Q / (float)child.N;
                if (ratio > bestRatio) {
                    bestRatio = ratio;
                    bestChild = child;
                    bestN = child.N;
                }
                if(ratio == bestRatio && child.N>bestN) {
                    bestRatio = ratio;
                    bestChild = child;
                    bestN = child.N;
                }
            }
            //Debug.Log(bestRatio);
            return bestChild;
        }


        protected Action BestFinalAction(MCTSNode node)
        {
            var bestChild = this.BestChild(node);
            if (bestChild == null) return null;

            this.BestFirstChild = bestChild;

            //this is done for debugging proposes only
            this.BestActionSequence = new List<Action>();
            this.BestActionSequence.Add(bestChild.Action);
            node = bestChild;

            while(node.ChildNodes.Count>0)
            {
                bestChild = this.BestChild(node);
                if (bestChild == null) break;
                this.BestActionSequence.Add(bestChild.Action);
                node = bestChild;    
            }

            return this.BestFirstChild.Action;
        }

    }
}
