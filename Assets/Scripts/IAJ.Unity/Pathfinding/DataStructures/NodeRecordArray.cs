using System;
using UnityEngine;
using System.Collections.Generic;
using RAIN.Navigation.Graph;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures
{
    public class NodeRecordArray : IOpenSet, IClosedSet
    {
        private NodeRecord[] NodeRecords { get; set; }
        private List<NodeRecord> SpecialCaseNodes { get; set; } 
        private NodePriorityHeap Open { get; set; }

        public NodeRecordArray(List<NavigationGraphNode> nodes)
        {
            //this method creates and initializes the NodeRecordArray for all nodes in the Navigation Graph
            this.NodeRecords = new NodeRecord[nodes.Count];
            
            for(int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                node.NodeIndex = i; //we're setting the node Index because RAIN does not do this automatically
                this.NodeRecords[i] = new NodeRecord {node = node, status = NodeStatus.Unvisited};
            }

            this.SpecialCaseNodes = new List<NodeRecord>();

            this.Open = new NodePriorityHeap();
        }

        public NodeRecord GetNodeRecord(NavigationGraphNode node)
        {
            //do not change this method
            //here we have the "special case" node handling
            if (node.NodeIndex == -1)
            {
                for (int i = 0; i < this.SpecialCaseNodes.Count; i++)
                {
                    if (node == this.SpecialCaseNodes[i].node)
                    {
                        return this.SpecialCaseNodes[i];
                    }
                }
                return null;
            }
            else
            {
                return  this.NodeRecords[node.NodeIndex];
            }
        }

        public void AddSpecialCaseNode(NodeRecord node)
        {
            this.SpecialCaseNodes.Add(node);
        }

        void IOpenSet.Initialize()
        {
            this.Open.Initialize();
            //we want this to be very efficient (that's why we use for)
            for (int i = 0; i < this.NodeRecords.Length; i++)
            {
                this.NodeRecords[i].status = NodeStatus.Unvisited;
            }

            this.SpecialCaseNodes.Clear();
        }

        void IClosedSet.Initialize()
        {
            //TODO implement
            //throw new NotImplementedException();
        }

        public void AddToOpen(NodeRecord nodeRecord)
        {
            var node = this.GetNodeRecord(nodeRecord.node);
            node.status = NodeStatus.Open;
            this.Open.AddToOpen(node);
        }

        public void AddToClosed(NodeRecord nodeRecord)
        {
            var node = this.GetNodeRecord(nodeRecord.node);
            node.status = NodeStatus.Closed;
        }

        public NodeRecord SearchInOpen(NodeRecord nodeRecord)
        {
            var node = this.GetNodeRecord(nodeRecord.node);
            return node.status == NodeStatus.Open ? node : null;
        }

        public NodeRecord SearchInClosed(NodeRecord nodeRecord)
        {
            var node = this.GetNodeRecord(nodeRecord.node);
            return node.status == NodeStatus.Closed ? node : null;
        }

        public NodeRecord GetBestAndRemove()
        {
            return this.Open.GetBestAndRemove();
        }

        public NodeRecord PeekBest()
        {
            return this.Open.PeekBest();
        }

        public void Replace(NodeRecord nodeToBeReplaced, NodeRecord nodeToReplace)
        {
            this.Open.Replace(nodeToBeReplaced, nodeToReplace);
        }

        public void RemoveFromOpen(NodeRecord nodeRecord)
        {
            var node = this.GetNodeRecord(nodeRecord.node);
            node.status = NodeStatus.Unvisited;
            this.Open.RemoveFromOpen(node);
        }

        public void RemoveFromClosed(NodeRecord nodeRecord)
        {
            var node = this.GetNodeRecord(nodeRecord.node);
            node.status = NodeStatus.Unvisited;
        }

        ICollection<NodeRecord> IOpenSet.All()
        {
            return this.Open.All();
        }

        ICollection<NodeRecord> IClosedSet.All()
        {
            var result = new List<NodeRecord>();
            for (int i = 0; i < this.NodeRecords.Length; i++)
            {
                var node = this.NodeRecords[i];
                if(node.status == NodeStatus.Closed){
                    result.Add(node);
                }
            }
            return result;
        }

        public int CountOpen()
        {
            return this.Open.CountOpen();
        }
    }
}
