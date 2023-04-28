using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Priority_Queue;
using Tiles;
using UnityEngine;


    
// Under construction
// Uses Spawner and ManhattanDistance instead

[Serializable]
public class AStar
{
    public Color _OpenColor = default;
    public Color _ClosedColor = default;
    // public Queue<Node> Open = new Queue<Node>();
    public List<int> OpenLocations = new List<int>();
    private Priority_Queue.SimplePriorityQueue<Node> OpenPQueue = new SimplePriorityQueue<Node>();
    
    public List<int> Closed = new List<int>();
    
    public int Target;

    public bool PathFound;
    public List<Node> Path = new List<Node>();
    public int IterationsPerFrame;
    public List<int>PathLocs = new List<int>();


    //We should return path found or the closest tile to the one we want.
    //So, if at the end we cannot find a path, we should return a path to the tile that was the closest
    //Figure that out
    public async Task< (bool, int[] path)> FindPath(int Start, int Target, Func<Node,int, (bool, float)> evaluator, bool debug = false)
    {
        if(debug)
        Debug.Log("From " + Start + " , setting target as: "+ Target);
        
        this.Target = Target;
        reset();
        var FirstNode = new Node(Level.instance.GetTileFromKey(Start), null,0,
            Level.instance.ManhattanDistance(Start, Target));
        OpenPQueue.Enqueue(FirstNode, FirstNode.f);
        OpenLocations.Add(Start);
        PathFound = false;
        int iterationCounter = 0;
        while (!Progress(evaluator))
        {
            if(debug) ColorTiles();
            if (iterationCounter == IterationsPerFrame)
            {
                await Task.Yield();
                iterationCounter = 0;
            }
            else
            {
                iterationCounter++;
            }
        }

        return (PathFound, PathLocs.ToArray());
    }

    public void ColorTiles()
    {
        Level.instance.ChangeTileDisplayActivationState(OpenLocations.ToArray(), true);
        Level.instance.ChangeTileDisplayActivationState(Closed.ToArray(), true);
        Level.instance.ChangeTileDisplayColor(OpenLocations.ToArray(), _OpenColor);
        Level.instance.ChangeTileDisplayColor(Closed.ToArray(), _ClosedColor);
    }
    public void PrintQueues()
    {
        Debug.Log($"Open locations {OpenLocations.Aggregate("", ((s, i) => s+TileCoordinate.IndexToCoord(i).ToString()+","))}");
        Debug.Log($"Closed locations {Closed.Aggregate("", ((s, i) => s+TileCoordinate.IndexToCoord(i).ToString()+","))}");
    }

    public void reset()
    {
        PathLocs = new List<int>();
        OpenPQueue = new SimplePriorityQueue<Node>();
        OpenLocations = new List<int>();
        Closed= new List<int>();
        PathFound = false;
        Path= new List<Node>();
    }


    //Returns true if processing finished, false if more needed
    public bool Progress(Func<Node,int, (bool,float)> evaluator, bool debug = false)
    {
        if (PathFound)
        {
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
            if(debug)Debug.Log("Analyzing "+ direction.ToString()+ " of "+ curTl.tileKey);
            if (!Level.instance.HasValidLocationInDirection(curTl.tileKey,direction))
            {
                if(debug)Debug.Log("Direction onto index breaks dimensions");
                continue;
            }
            var targetnebber = Level.instance.DirectionOntoLocation(curTl.tileKey, direction);
            if(debug)            Debug.Log("Index is "+ targetnebber);
            if (targetnebber!= Target &&Level.instance.IsTileOccupied(targetnebber))
            {
                if(debug)Debug.Log("Neighbor is occupied");
                continue;
            }

            if (!Level.instance.tileTraversible[targetnebber])
            {
                if(debug)Debug.Log("Neighbor is untraversible");
                continue;
            }
            if (Closed.Contains(targetnebber))
            {
                if(debug)Debug.Log("Neighbor is already closed");
                continue;
            }
            
            if (!OpenLocations.Contains(targetnebber))
            {
                if(debug)Debug.Log("Enqueing: " + targetnebber);
                OpenLocations.Add(targetnebber);

                // var newNode = new Node(Level.instance.GetTileFromKey(targetnebber), CurrentNode, CurrentNode.g+1, 
                //      Level.instance.ManhattanDistance(targetnebber, Target));
                var (canGoThere, heuristic) = evaluator(CurrentNode,targetnebber);
                //Check first if we can actually go there
                if (canGoThere)
                {
                    var newNode = new Node(Level.instance.GetTileFromKey(targetnebber), CurrentNode, CurrentNode.Step+1, 
                         heuristic);
                    OpenPQueue.Enqueue(newNode, newNode.f);
                }
            }
            else
            {
                if(debug)Debug.Log("Open locations already contains " + targetnebber);
            }
        }
        Closed.Add(curTl.tileKey);
        if (OpenPQueue.Count == 0)
        {
            //The open queue is empty, and the last evaluated node
            Debug.Log("Open queue is empty");
            PrintQueues();
            while (CurrentNode != null)
            {
                Path.Add(CurrentNode);
                PathLocs.Add(CurrentNode.Tile.tileKey);
                CurrentNode = CurrentNode.PrevNode;
            }
            PathFound = false;
            PathLocs.Reverse();
            Path.Reverse();
            Debug.Break();
            return true;
        }
        return false;
    }

    public void FailingProgress()
    {
        
    }

}