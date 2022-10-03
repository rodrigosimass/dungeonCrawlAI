using Assets.Scripts.GameManager;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.IAJ.Unity.Utils;
using System;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class DivineSmite : WalkToTargetAndExecuteAction
    {
        private float expectedHPChange;
        private float expectedXPChange;
        private int xpChange;

        public DivineSmite(AutonomousCharacter character, GameObject target) : base("DivineSmite",character,target)
        {
            if (target.tag.Equals("Skeleton"))
            {
                this.expectedHPChange = 0;
                this.xpChange = 3;
                this.expectedXPChange = 2.7f;
            }
        }

        public override float GetGoalChange(Goal goal)
        {
            var change = base.GetGoalChange(goal);

            if (goal.Name == AutonomousCharacter.GAIN_LEVEL_GOAL)
            {
                change += -this.expectedXPChange;
            }
            
            return change;
        }

        public override bool CanExecute()
        {
            if (!base.CanExecute()) return false;
            return this.Character.GameManager.characterData.Mana >= 2;
        }

        public override bool CanExecute(WorldModel worldModel)
        {
            if (!base.CanExecute(worldModel)) return false;
            dynamic mana;
            var obj = worldModel.GetProperty(Properties.MANA);
            try { mana = (int)obj; } catch { mana = (float)obj; }
            return mana >= 2;
        }

        public override void Execute()
        {
            base.Execute();
            this.Character.GameManager.DivineSmite(this.Target);
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            base.ApplyActionEffects(worldModel);

            dynamic mana;
            var obj = worldModel.GetProperty(Properties.MANA);
            try { mana = (int)obj; } catch { mana = (float)obj; }
            
            int xp = (int)worldModel.GetProperty(Properties.XP);
   
            //disables the target object so that it can't be reused again
            worldModel.SetProperty(this.Target.name, false);

            // gain xp
            worldModel.SetProperty(Properties.XP, xp + this.xpChange);

            // modify level goal
            //var xpValue = worldModel.GetGoalValue(AutonomousCharacter.GAIN_LEVEL_GOAL);
            //worldModel.SetGoalValue(AutonomousCharacter.GAIN_LEVEL_GOAL, xpValue + this.xpChange);

            worldModel.SetProperty(Properties.MANA, mana - 2);
        }

        public override float GetHValue(WorldModel worldModel)
        {
            return base.GetHValue(worldModel) -100.0f;
        }
    }
}