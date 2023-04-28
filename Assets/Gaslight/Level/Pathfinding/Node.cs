using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Tiles
{
    public class Node
    {
        public float f;
        public int Step;
        public float tileCost;
        public Node PrevNode;

        public Tile Tile;
        public Vector3 position;
        public Node(Tile tile, Node prevNode, int step, float h=0.0f)
        {
            this.Tile = tile;
            this.PrevNode = prevNode;
            this.Step = step;
            this.f = step + h;
        }


    }

}
