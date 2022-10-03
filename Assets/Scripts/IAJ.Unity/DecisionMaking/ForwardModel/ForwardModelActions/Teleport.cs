using Assets.Scripts.IAJ.Unity.DecisionMaking.GOB;
using Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel;
using Assets.Scripts.GameManager;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.ForwardModel.ForwardModelActions
{
    public class Teleport : Action
    {
        public AutonomousCharacter Character { get; private set; }

        private float expectedMANAChange;

        public Teleport(AutonomousCharacter character) : base("Teleport")
        {
            this.Character = character;
            this.expectedMANAChange = 5;
        }

        public override bool CanExecute()
        {
            var level = this.Character.GameManager.characterData.Level;
            var mana = this.Character.GameManager.characterData.Mana;

            return level >= 2 && mana >= 5;
        }
        

        public override bool CanExecute(WorldModel worldModel)
        {
            dynamic mana;
            var obj = worldModel.GetProperty(Properties.MANA);
            try { mana = (int)obj; } catch { mana = (float)obj; }

            int level = (int)worldModel.GetProperty(Properties.LEVEL);
            
            return level >= 2 && mana >= 5;
        }

        public override void Execute()
        {
            this.Character.GameManager.Teleport();
        }

        public override void ApplyActionEffects(WorldModel worldModel)
        {
            dynamic mana;
            var obj = worldModel.GetProperty(Properties.MANA);
            try { mana = (int)obj; } catch { mana = (float)obj; }
            worldModel.SetProperty(Properties.MANA, mana - this.expectedMANAChange);
        }

        public override float GetGoalChange(Goal goal)
        {
            float change = 0.0f;

            if (goal.Name == AutonomousCharacter.SURVIVE_GOAL)
            {
                change += -this.expectedMANAChange;
            }

            return change;
        }

        public override float GetHValue(WorldModel worldModel)
        {
            return base.GetHValue(worldModel);
        }
    }
}
