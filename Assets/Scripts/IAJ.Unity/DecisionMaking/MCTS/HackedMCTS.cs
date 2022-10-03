using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;
using System;
using Action = Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.Action;
using System.Collections.Generic;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class HackedMCTS : MCTS
    {
        public HackedMCTS(CurrentStateWorldModel currentStateWorldModel) : base(currentStateWorldModel)
        {
           
        }

        protected override Reward Playout(WorldModel initialPlayoutState)
        {
            var currentState = initialPlayoutState;
            var maxPlayDepth = 15;
            var depth = 0;
            WorldModel newState = initialPlayoutState;
            //var seq = new List<Action>();
            while (!currentState.IsTerminal() && depth<maxPlayDepth) {
                var possibleActions = currentState.GetExecutableActions(); 
                var action = getBiasedAction(currentState, possibleActions);
                //seq.Add(action);
                //var action = possibleActions[RandomGenerator.Next(possibleActions.Length)];
                newState = currentState.GenerateChildWorldModel();
                action.ApplyActionEffects(newState);
                newState.CalculateNextPlayer();
                currentState = newState;
                depth++;
               
            } //TODO ver o aspecto da reward ser baseada no player

            return  new Reward(newState.GetScore()/* , currentState.GetNextPlayer() */);
        }

        protected Action getBiasedAction(WorldModel currentState, ForwardModel.Action[] possibleActions) {
            var bestH = float.MaxValue;
            Action bestA = null;
            foreach (var action in possibleActions) {
                if(action==null) break;
                var h = action.GetHValue(currentState);
                //var h = 0;
                if (h < bestH) {
                    bestA = action;
                    bestH = h;
                }
            }
            
            return bestA;
        }

    protected override MCTSNode BestChild(MCTSNode node)
        {
            if (node.ChildNodes.Count == 0) {
                return null;
            }
            MCTSNode bestChild = node.ChildNodes[0];
            var bestH = 100.0f;
            foreach (var child in node.ChildNodes) {
                var action = child.Action;
                if (action != null) {
                    var h = action.GetHValue(node.State);
                    if (h < bestH) {
                        bestH = h;
                        bestChild = child;
                    }
                }
            }
            //Debug.Log(bestRatio);
            return bestChild;
        }

    }
}