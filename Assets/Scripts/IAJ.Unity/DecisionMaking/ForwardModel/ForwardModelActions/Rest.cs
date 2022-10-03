using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.GameManager;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class Rest : Action
    {
        public AutonomousCharacter Character { get; private set; }
        private int expectedHPChange;

        public Rest(AutonomousCharacter character) : base("Rest")
        {
            this.Character = character;
            this.expectedHPChange = 2;
        }

        public override bool CanExecute()
        {
            return this.Character.GameManager.characterData.HP < this.Character.GameManager.characterData.MaxHP / 3;
        }
        

        public override bool CanExecute(WorldModel worldModel)
        {
            int hp = (int)worldModel.GetProperty(Properties.HP);
            int maxHp = (int)worldModel.GetProperty(Properties.MAXHP);

            return hp < maxHp / 3;
        }

        public override void Execute()
        {
            this.Character.GameManager.Rest();
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            int hp = (int)worldModel.GetProperty(Properties.HP);
            int maxHp = (int)worldModel.GetProperty(Properties.MAXHP);

            int change = Mathf.Min(hp + this.expectedHPChange, maxHp);

            worldModel.SetProperty(Properties.HP, change);
        }

        public override float GetGoalChange(Goal goal)
        {
            float change = 0.0f;

            if(goal.Name == AutonomousCharacter.BE_QUICK_GOAL){
                change += goal.InsistenceValue;
            }

            return change;
        }

        public override float GetHValue(WorldModel worldModel)
        {
            return base.GetHValue(worldModel);
        }
    }
}