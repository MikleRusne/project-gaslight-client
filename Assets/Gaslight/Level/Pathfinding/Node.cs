using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Tiles
{
    public class Node
    {
        public float f;
        public float g;
        public float tileCost;
        public Node PrevNode;

        public Tile Tile;
        public Vector3 position;
        public Node(Tile tile, Node prevNode, float g, float h=0.0f,float tileCost=0.0f)
        {
            this.Tile = tile;
            this.PrevNode = prevNode;
            this.g = g;
            this.f = g + h;
        }


    }

}
