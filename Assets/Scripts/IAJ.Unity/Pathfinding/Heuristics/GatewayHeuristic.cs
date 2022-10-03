using RAIN.Navigation.Graph;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.HPStructures;
using UnityEngine;
using UnityEditor;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics
{
    public class GatewayHeuristic : IHeuristic
    {
        public float H(NavigationGraphNode node, NavigationGraphNode goalNode)
        {
            var clusterGraph = Resources.Load<ClusterGraph>("ClusterGraph");

            Cluster startCluster = clusterGraph.Quantize(node);
            Cluster endCluster = clusterGraph.Quantize(goalNode);


            EuclideanDistanceHeuristic euclidean = new EuclideanDistanceHeuristic();

            if(startCluster is null){
                return euclidean.H(node, goalNode);
            }

            if(startCluster.Localize() == endCluster.Localize()){
                return euclidean.H(node, goalNode);
            }

            float h = float.PositiveInfinity;
            int size = clusterGraph.gateways.Count;

            var startGWs = startCluster.gateways;
            var endGWs = endCluster.gateways; 

            for(int i = 0; i < size; i++){

                Gateway startGateway = clusterGraph.gateways[i];

                if (!startGWs.Contains(startGateway)) continue;

                for(int j = 0; j < size; j++){
                    
                    Gateway endGateway = clusterGraph.gateways[j];
                    if (!endGWs.Contains(endGateway)) continue;

                    var sum = euclidean.H(node.Position, startGateway.Localize()) + 3 * clusterGraph.gatewayDistanceTable[i].entries[j].shortestDistance + euclidean.H(endGateway.Localize(), goalNode.Position);

                    h = Mathf.Min(h, sum);
                }
            }

            return h;
        }
    }
}
