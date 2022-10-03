using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class ShieldOfFaith : Action
    {
        protected AutonomousCharacter Character { get; set; }

        public ShieldOfFaith(AutonomousCharacter character) : base("ShieldOfFaith")
        {
            this.Character = character;
        }

        public override float GetGoalChange(Goal goal)
        {
            var change = base.GetGoalChange(goal);
            //TODO: a utility change tem que ser 5- shield actual senao ele vai querer fazer cast ao shield mesmo quando ja tem 5
            if (goal.Name == AutonomousCharacter.SURVIVE_GOAL) change -= 5.0f - this.Character.GameManager.characterData.ShieldHP;
            return change;
        }

        public override bool CanExecute()
        {
            return this.Character.GameManager.characterData.Mana >= 5;
        }

        public override bool CanExecute(WorldModel worldModel)
        {
            dynamic mana;
            var obj = worldModel.GetProperty(Properties.MANA);
            try { mana = (int)obj; } catch { mana = (float)obj; }
            return mana >= 5;
        }

        public override void Execute()
        {
            base.Execute();
            this.Character.GameManager.ShieldOfFaith();
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

            var goalValue = worldModel.GetGoalValue(AutonomousCharacter.SURVIVE_GOAL);
            worldModel.SetGoalValue(AutonomousCharacter.SURVIVE_GOAL, goalValue - 5.0f);

            dynamic mana;
            var obj = worldModel.GetProperty(Properties.MANA);
            try { mana = (int)obj; } catch { mana = (float)obj; }
            
            worldModel.SetProperty(Properties.MANA, mana - 5);

            worldModel.SetProperty(Properties.ShieldHP, 5);
        }

        public override float GetHValue(WorldModel worldModel)
        {
            var shield = (int)worldModel.GetProperty(Properties.ShieldHP);
            var spellValue = (5-shield) * 10;
            return base.GetHValue(worldModel) - spellValue;
        }
    }
}
