using RAIN.Navigation.Graph;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics
{
    public class EuclideanDistanceHeuristic : IHeuristic
    {
        public float H(NavigationGraphNode node, NavigationGraphNode goalNode)
        {
            /*var sum = (node.LocalPosition.x - goalNode.LocalPosition.x) * (node.LocalPosition.x - goalNode.LocalPosition.x) + (node.LocalPosition.z - goalNode.LocalPosition.z) * (node.LocalPosition.z - goalNode.LocalPosition.z);
            //var sum = (goalNode.LocalPosition.x - node.LocalPosition.x) * (goalNode.LocalPosition.x - node.LocalPosition.x) + (goalNode.LocalPosition.z - node.LocalPosition.z) * (goalNode.LocalPosition.z - node.LocalPosition.z);

            return Mathf.Sqrt(sum);*/
            //return H(node.Position, goalNode.Position);
            //return Vector3.Distance(goalNode.Position, node.Position);
            var dx = goalNode.LocalPosition.x-node.Position.x;
            var dZ = goalNode.LocalPosition.z-node.Position.z;
            return  Mathf.Sqrt(dx*dx+dZ*dZ);

        }

        public float H(Vector3 node, Vector3 goalNode){
            //var sum = (node.x - goalNode.x) * (node.x - goalNode.x) + (node.z - goalNode.z) * (node.z - goalNode.z);
            //return Mathf.Sqrt(sum);
            var dx = goalNode.x-node.x;
            var dZ = goalNode.z-node.z;
            return  Mathf.Sqrt(dx*dx+dZ*dZ);
            //return Vector3.Distance(node, goalNode);
        }
    }
}