using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;
using System;
using Action = Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.Action;
using System.Collections.Generic;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class LimitedBiasedPlayoutMCTS : MCTS
    {
        public LimitedBiasedPlayoutMCTS(CurrentStateWorldModel currentStateWorldModel) : base(currentStateWorldModel){    
        }

        protected override Reward Playout(WorldModel initialPlayoutState)
        {
            var currentState = initialPlayoutState;
            var maxPlayDepth = this.playoutDepth;
            var depth = 0;
            WorldModel newState = null;
            Action action;
            var seq =  new List<Action>();
            Dictionary<string, object> props = null;
            while (!currentState.IsTerminal() && depth<maxPlayDepth) {
                var possibleActions = currentState.GetExecutableActions(); 
                action = getBiasedAction(currentState, possibleActions);
                seq.Add(action);
                //var action = possibleActions[RandomGenerator.Next(possibleActions.Length)];
                newState = currentState.GenerateChildWorldModel();
                action.ApplyActionEffects(newState);
                newState.CalculateNextPlayer();
                props = newState.getProperties();
                currentState = newState;
                depth++;
               
            } //TODO ver o aspecto da reward ser baseada no playerA

           return   calculateStateScore(currentState);
        }

        protected Reward calculateStateScore(WorldModel state) {
            float value = 0.0f;
            var HP = (int)state.GetProperty(Properties.HP);
            var TIME = (float)state.GetProperty(Properties.TIME);
            var MANA = (int)state.GetProperty(Properties.MANA);
            var ShieldHP = (int)state.GetProperty(Properties.ShieldHP);
            var MONEY = (int)state.GetProperty(Properties.MONEY);
            var LEVEL = (int)state.GetProperty(Properties.LEVEL);
            
            if(HP<0||TIME>150.0) return new Reward(value);

            value += HP * 10;
            value += ( 150 - TIME ) * 10;
            value += MANA * 10;
            value += ShieldHP * 10;
            value += MONEY * 2;
            value += LEVEL * 100;
            //Debug.Log("MONEY="+(int)state.GetProperty(Properties.MONEY));
            //Debug.Log("value of state:"+value);
            return new Reward(value);
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

        protected override void Backpropagate(MCTSNode node, Reward reward)
        {
            var currentNode = node;

            while (currentNode!=null) {
                //TODO o player influencia a reward? acho que nao e preciso fazer nada
                currentNode.N++;
                if(reward.Value>currentNode.Q)
                    currentNode.Q = reward.Value;

                currentNode = currentNode.Parent;
            }
        }
        protected override MCTSNode BestChild(MCTSNode node)
        {
            if (node.ChildNodes.Count == 0) {
                return null;
            }
            float bestValue = node.ChildNodes[0].Q;
            MCTSNode bestChild = node.ChildNodes[0];

            foreach (var child in node.ChildNodes) {
                var value = child.Q;
                if (value > bestValue) {
                    bestValue = value;
                    bestChild = child;
                }
            }
            return bestChild;
        }

    }
}