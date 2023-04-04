using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Priority_Queue;
using Tiles;
using UnityEngine;
using UnityEngine.Events;


    
// Under construction
// Uses Spawner and ManhattanDistance instead


public class AStar
{
    public UnityEvent<bool> ProcessingFinished;
    public Queue<Node> Open = new Queue<Node>();
    public List<int> OpenLocations = new List<int>();
    private Priority_Queue.SimplePriorityQueue<Node> OpenPQueue = new SimplePriorityQueue<Node>();
    
    public List<int> Closed = new List<int>();

    public int Target;

    public bool PathFound;
    public List<Node> Path = new List<Node>();

    public List<int>PathLocs = new List<int>();

    public bool debug = false;
    public void FindPath(int Start, int Target)
    {
        if(debug)
        Debug.Log("From " + Start + " , setting target as: "+ Target);
        
        this.Target = Target;
        reset();
        // PathFound = false;
        var FirstNode = new Node(Level.instance.GetTileFromKey(Start), null,0,
            Level.instance.GetDistance(Start, Target));
        
        Open.Enqueue(FirstNode);
        OpenPQueue.Enqueue(FirstNode, FirstNode.f);
        // OpenObjects.Add(Start);
        PathFound = false;
        while (!Progress())
        {
            PrintQueues();
            // Debug.Log("Waiting for processing");
            // await Task.Yield();
        } 
        // Debug.Log("Processing Finished");
                
    }

    public void PrintQueues()
    {
        // Debug.Log("Open list of length:" + Open.Count);
        // Debug.Log(String.Join(",",OpenLocations.ToArray()));
        // Debug.Log("Closed list");
        // Debug.Log(String.Join(",",Closed.ToArray()));
        
    }

    public void reset()
    {
        PathLocs = new List<int>();
        Open = new Queue<Node>();
        OpenPQueue = new SimplePriorityQueue<Node>();
        OpenLocations = new List<int>();
        Closed= new List<int>();
        PathFound = false;
        Path= new List<Node>();

    }

    
    //Returns true if processing finished, false if more needed
    public bool Progress()
    {
        if (PathFound)
        {
            return true;
        }
        
        
        if (OpenPQueue.Count == 0)
        {
            PathFound = false;
            return true;
        }

        Node CurrentNode = OpenPQueue.Dequeue();
        OpenLocations.Remove(CurrentNode.Tile.tileKey);
        if (CurrentNode.Tile.tileKey == Target)
        {
            PathFound = true;
            if(debug)
            Debug.Log("Found path to be:");
            StringBuilder sb = new StringBuilder();
            while (CurrentNode != null)
            {
                sb.Append(CurrentNode.Tile.tileKey + ", ");
                Path.Add(CurrentNode);
                PathLocs.Add(CurrentNode.Tile.tileKey);
                CurrentNode = CurrentNode.PrevNode;
            }

            PathLocs.Reverse();
            Path.Reverse();
            // Debug.Log(sb.ToString());
            PathFound = true;
            return true;
        }
        
        //Under construction
        if(debug)
            Debug.Log("Analyzing "+ CurrentNode.Tile.tileKey);
        Tile curTl = CurrentNode.Tile;
        foreach (Level.ELinkDirection direction in Enum.GetValues(typeof(Level.ELinkDirection)))
        {
            //Adds neighbors of current node to open list
            
            //Check if
            // It is connected in that direction
            // The connection is a tile
            // It is not in closed list
            // Debug.Log("Analyzing "+ direction.ToString()+ " of "+ curTl.tileKey);
            if (!Level.instance.hasNeighborInDirection(curTl.tileKey,direction))
            {
                // Debug.Log("Direction onto index breaks dimensions");
                continue;
            }
            var targetnebber = Level.instance.DirectionOntoLocation(curTl.tileKey, direction);
            // Debug.Log("Index is "+ targetnebber);
            if (Level.instance.IsTileOccupied(targetnebber))
            {
                // Debug.Log("Neighbor is occupied");
                continue;
            }

            if (!Level.instance.tileTraversible[targetnebber])
            {
                // Debug.Log("Neighbor is untraversible");
                continue;
            }
            if (Closed.Contains(targetnebber))
            {
                // Debug.Log("Neighbor is already closed");
                continue;
            }
            
            if (!OpenLocations.Contains(targetnebber))
            {
                // Debug.Log("Enqueing: " + targetnebber);
                OpenLocations.Add(targetnebber);

                var newNode = new Node(Level.instance.GetTileFromKey(targetnebber), CurrentNode, CurrentNode.g+1, 
                    Level.instance.GetDistance(targetnebber, Target), Level.instance.Tiles[targetnebber].traversingCost);
                OpenPQueue.Enqueue(newNode, newNode.f);
            }
            else
            {
                // Debug.Log("Open locations already contains " + targetnebber);
            }
        }
        Closed.Add(curTl.tileKey);
        return false;
    }

}