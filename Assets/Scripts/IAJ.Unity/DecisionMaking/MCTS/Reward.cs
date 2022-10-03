namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class Reward
    {
        public float Value { get; set; }
        public int PlayerID { get; set; }

        public Reward(float v) {
            Value = v;
            PlayerID = 0;
        }

        public Reward(float v, int p) {
            Value = v;
            PlayerID = p;
        }

    }
}
