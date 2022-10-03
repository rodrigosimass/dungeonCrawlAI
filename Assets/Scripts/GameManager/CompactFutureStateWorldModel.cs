using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.GameManager
{
    public class CompactFutureStateWorldModel : CompactWorldModel
    {
        protected GameManager GameManager { get; set; }
        protected int NextPlayer { get; set; }
        protected Action NextEnemyAction { get; set; }
        protected Action[] NextEnemyActions { get; set; }

        public CompactFutureStateWorldModel(GameManager gameManager, List<Action> actions) : base(actions)
        {
            this.GameManager = gameManager;
            this.NextPlayer = 0;
        }

        public CompactFutureStateWorldModel(CompactFutureStateWorldModel parent) : base(parent)
        {
            this.GameManager = parent.GameManager;
        }

        public override CompactWorldModel GenerateChildWorldModel()
        {
            return new CompactFutureStateWorldModel(this);
        }

        public override bool IsTerminal()
        {
            int HP = (int)this.GetProperty(Properties.HP);
            float time = (float)this.GetProperty(Properties.TIME);
            int money = (int)this.GetProperty(Properties.MONEY);

            return HP <= 0 ||  time >= GameManager.TIME_LIMIT || (this.NextPlayer == 0 && money == 25);
        }

        public override float GetScore()
        {
            int money = (int)this.GetProperty(Properties.MONEY);
            int HP = (int)this.GetProperty(Properties.HP);
            float time = (float)this.GetProperty(Properties.TIME);

            if (HP <= 0) return 0.0f;
            else if (money == 25)
            {
                return (1.0f - time*0.005f);
            }
            else return 0.0f;
        }

        public override int GetNextPlayer()
        {
            return this.NextPlayer;
        }

        public override void CalculateNextPlayer()
        {
            //if the previous player was the enemy, we want the hero to have the opportunity to make a decision
            if(this.Parent.GetNextPlayer() == 1)
            {
                
                this.NextPlayer = 0;
                return;
            }

            Vector3 position = (Vector3)this.GetProperty(Properties.POSITION);
            bool enemyEnabled;

            //basically if the hero is close enough to an enemy, the next player will be the enemy.
            foreach (var enemy in this.GameManager.enemies)
            {
                enemyEnabled = (bool) this.GetProperty(enemy.name);
                if (enemyEnabled && (enemy.transform.position - position).sqrMagnitude <= 1000)
                {
                    this.NextPlayer = 1;
                    this.NextEnemyAction = new EnemyAttack(this.GameManager.autonomousCharacter, enemy);
                    this.NextEnemyActions = new Action[] { this.NextEnemyAction };
                    return; 
                }
            }
            this.NextPlayer = 0;
            //if not, then the next player will be player 0 (the Hero)
        }

        public override Action GetNextAction()
        {
            Action action;
            if (this.NextPlayer == 1)
            {
                action = this.NextEnemyAction;
                this.NextEnemyAction = null;
                return action;
            }
            else return base.GetNextAction();
        }

        public override Action[] GetExecutableActions()
        {
            if (this.NextPlayer == 1)
            {
                return this.NextEnemyActions;
            }
            else return base.GetExecutableActions();
        }

    }
}
