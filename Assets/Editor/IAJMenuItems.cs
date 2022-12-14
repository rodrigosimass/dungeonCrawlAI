using UnityEngine;
using UnityEditor;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.HPStructures;
using Assets.Scripts.IAJ.Unity.Pathfinding.Path;
using Assets.Scripts.IAJ.Unity.Utils;
using Assets.Scripts;
using RAIN.Navigation.NavMesh;
using System.Collections.Generic;
using RAIN.Navigation.Graph;
using Assets.Scripts.IAJ.Unity.Pathfinding;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;

public class IAJMenuItems  {

    [MenuItem("IAJ/Create Cluster Graph")]
    private static void CreateClusterGraph()
    {
        Cluster cluster;
        Gateway gateway;

        //get cluster game objects
        var clusters = GameObject.FindGameObjectsWithTag("Cluster");
        //get gateway game objects
        var gateways = GameObject.FindGameObjectsWithTag("Gateway");
        //get the NavMeshGraph from the current scene
        NavMeshPathGraph navMesh = GameObject.Find("Navigation Mesh").GetComponent<NavMeshRig>().NavMesh.Graph;

        ClusterGraph clusterGraph = ScriptableObject.CreateInstance<ClusterGraph>();

        //create gateway instances for each gateway game object
        for(int i = 0; i < gateways.Length; i++)
        {
            var gatewayGO = gateways[i];
            gateway = ScriptableObject.CreateInstance<Gateway>();
            gateway.Initialize(i,gatewayGO);
            clusterGraph.gateways.Add(gateway);
        }

        foreach(var clusterGO in clusters)
        {

            cluster = ScriptableObject.CreateInstance<Cluster>();
            cluster.Initialize(clusterGO);
            clusterGraph.clusters.Add(cluster);

            //determine intersection between cluster and gateways and add connections when they intersect
            foreach(var gate in clusterGraph.gateways)
            {
                if (MathHelper.BoundingBoxIntersection(cluster.min, cluster.max, gate.min, gate.max))
                {
                    cluster.gateways.Add(gate);
                    gate.clusters.Add(cluster);
                }
            }
        }

        //clusterGraph.RegisterNodeLocalizations(GetNodesHack(navMesh));


        // Second stage of the algorithm, calculation of the Gateway table

        GlobalPath solution = null;
        float cost;
        Gateway startGate;
        Gateway endGate;

        //var pathfindingManager = new PathfindingManager();
        var pathfindingManager = new AutonomousCharacter();
        pathfindingManager.Initialize(navMesh, new NodeArrayAStarPathFinding(navMesh, new EuclideanDistanceHeuristic()));


        int numberOfGateways = clusterGraph.gateways.Count;
        int numberOfGatewayDistanceTableEntries = numberOfGateways * numberOfGateways;

        clusterGraph.gatewayDistanceTable = new GatewayDistanceTableRow[numberOfGateways];

        int counter = 0;

        for (int i = 0; i < numberOfGateways; i++)
        {
            var tableRow = ScriptableObject.CreateInstance<GatewayDistanceTableRow>();
            tableRow.entries = new GatewayDistanceTableEntry[numberOfGateways];
            clusterGraph.gatewayDistanceTable[i] = tableRow;

            for (int j = 0; j < numberOfGateways; j++)
            {
                startGate = clusterGraph.gateways[i];
                endGate = clusterGraph.gateways[j];

                if (i == j)
                {
                    cost = 0;
                }
                else
                {
                    Debug.Log("Calculating shortest path between gateway pair " + counter + "/" + "" + numberOfGatewayDistanceTableEntries);

                    pathfindingManager.AStarPathFinding.InitializePathfindingSearch(startGate.center, endGate.center);

                    while (pathfindingManager.AStarPathFinding.InProgress)
                    {
                        pathfindingManager.AStarPathFinding.Search(out solution, false);
                    }

                    if (solution != null)
                    {
                        cost = solution.Length;
                    }
                    else
                    {
                        cost = float.MaxValue;
                    }
                }

                var tableEntry = ScriptableObject.CreateInstance<GatewayDistanceTableEntry>();
                tableEntry.startGatewayPosition = startGate.center;
                tableEntry.endGatewayPosition = endGate.center;
                tableEntry.shortestDistance = cost;
                tableRow.entries[j] = tableEntry;
                counter++;
            }
        }

        //create a new asset that will contain the ClusterGraph and save it to disk
        clusterGraph.SaveToAssetDatabase();
    }


    private static List<NavigationGraphNode> GetNodesHack(NavMeshPathGraph graph)
    {
        //this hack is needed because in order to implement NodeArrayA* you need to have full acess to all the nodes in the navigation graph in the beginning of the search
        //unfortunately in RAINNavigationGraph class the field which contains the full List of Nodes is private
        //I cannot change the field to public, however there is a trick in C#. If you know the name of the field, you can access it using reflection (even if it is private)
        //using reflection is not very efficient, but it is ok because this is only called once in the creation of the class
        //by the way, NavMeshPathGraph is a derived class from RAINNavigationGraph class and the _pathNodes field is defined in the base class,
        //that's why we're using the type of the base class in the reflection call
        return (List<NavigationGraphNode>)Assets.Scripts.IAJ.Unity.Utils.Reflection.GetInstanceField(typeof(RAINNavigationGraph), graph, "_pathNodes");
    }
}
