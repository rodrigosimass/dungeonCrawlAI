using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using Assets.Scripts.IAJ.Unity.Pathfinding.Path;
using RAIN.Navigation.Graph;
using RAIN.Navigation.NavMesh;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding
{
    public class NodeArrayAStarPathFinding : AStarPathfinding
    {
        protected NodeRecordArray NodeRecordArray { get; set; }
        public NodeArrayAStarPathFinding(NavMeshPathGraph graph, IHeuristic heuristic) : base(graph,null,null,heuristic)
        {
            //do not change this
            var nodes = this.GetNodesHack(graph);
            this.NodeRecordArray = new NodeRecordArray(nodes);
            this.Open = this.NodeRecordArray;
            this.Closed = this.NodeRecordArray;
            
        }

        protected override void ProcessChildNode(NodeRecord bestNode, NavigationGraphEdge connectionEdge, int edgeIndex)
        {
            float f;
            float g;
            float h;

            var childNode = connectionEdge.ToNode;
            var childNodeRecord = this.NodeRecordArray.GetNodeRecord(childNode);

            if (childNodeRecord == null)
            {
                //this piece of code is used just because of the special start nodes and goal nodes added to the RAIN Navigation graph when a new search is performed.
                //Since these special goals were not in the original navigation graph, they will not be stored in the NodeRecordArray and we will have to add them
                //to a special structure
                //it's ok if you don't understand this, this is a hack and not part of the NodeArrayA* algorithm, just do NOT CHANGE THIS, or your algorithm will not work
                childNodeRecord = new NodeRecord
                {
                    node = childNode,
                    parent = bestNode,
                    status = NodeStatus.Unvisited
                };
                this.NodeRecordArray.AddSpecialCaseNode(childNodeRecord);
            } 

            // precompute values, but only change them if necessary (see AlterNode)
            g = bestNode.gValue + (childNodeRecord.node.LocalPosition-bestNode.node.LocalPosition).magnitude;
            h = this.Heuristic.H(childNodeRecord.node, this.GoalNode) * this.weight;
            f = F(g, h);

            if(childNodeRecord.status == NodeStatus.Unvisited){
                this.AlterNode(childNodeRecord, bestNode, g, f);
                this.Open.AddToOpen(childNodeRecord);
            } 
            else if (childNodeRecord.status == NodeStatus.Open && childNodeRecord.fValue > f){
                NodeRecord oldNode = this.Open.SearchInOpen(childNodeRecord);
                this.AlterNode(childNodeRecord, bestNode, g, f);
                this.Open.Replace(oldNode, childNodeRecord);
            }
            else if (childNodeRecord.status == NodeStatus.Closed && childNodeRecord.fValue > f){
                this.AlterNode(childNodeRecord, bestNode, g, f);
                this.Open.AddToOpen(childNodeRecord);
            }

        }

        private void AlterNode(NodeRecord node, NodeRecord parent, float g, float f){
            node.parent = parent;
            node.gValue = g;
            node.fValue = f;
        }
            
        private List<NavigationGraphNode> GetNodesHack(NavMeshPathGraph graph)
        {
            //this hack is needed because in order to implement NodeArrayA* you need to have full acess to all the nodes in the navigation graph in the beginning of the search
            //unfortunately in RAINNavigationGraph class the field which contains the full List of Nodes is private
            //I cannot change the field to public, however there is a trick in C#. If you know the name of the field, you can access it using reflection (even if it is private)
            //using reflection is not very efficient, but it is ok because this is only called once in the creation of the class
            //by the way, NavMeshPathGraph is a derived class from RAINNavigationGraph class and the _pathNodes field is defined in the base class,
            //that's why we're using the type of the base class in the reflection call
            return (List<NavigationGraphNode>) Utils.Reflection.GetInstanceField(typeof(RAINNavigationGraph), graph, "_pathNodes");
        }
    }
}
