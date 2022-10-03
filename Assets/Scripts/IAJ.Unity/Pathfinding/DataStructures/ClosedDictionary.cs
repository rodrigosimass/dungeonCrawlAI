using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures
{
    public class ClosedDictionary : IClosedSet{

        private Dictionary<int, NodeRecord> NodeRecords { get; set; }

        public void Initialize(){
            this.NodeRecords = new Dictionary<int, NodeRecord>();
        }

        public void AddToClosed(NodeRecord nodeRecord){
            this.NodeRecords.Add(nodeRecord.GetHashCode(), nodeRecord);
        }

        public void RemoveFromClosed(NodeRecord nodeRecord){
            this.NodeRecords.Remove(nodeRecord.GetHashCode());
        }

        //should return null if the node is not found
        public NodeRecord SearchInClosed(NodeRecord nodeRecord){
            return this.NodeRecords.Values.FirstOrDefault(n => n.Equals(nodeRecord));
        }

        public ICollection<NodeRecord> All(){
            return this.NodeRecords.Values;
        }
    }
}