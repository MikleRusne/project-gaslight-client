using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tiles
{

    [Serializable]
    public struct TileCoordinate
    {
        public int x;
        public int y;

        public static int CoordToIndex(TileCoordinate _tc)
        {
            return (_tc.y * Level.LWidth) + _tc.x;
        }


        //Assumes it is within bounds
        public static TileCoordinate PositionToCoord(Vector3 position)
        {

            var newCoord = new TileCoordinate();
            var X = Mathf.Clamp(
                Mathf.Round((position.x / (Level.Size + Level.instance.padding))),
                0f,
                Level.LWidth - 1);
            // Debug.Log("X before rounding: " + X);
            var x = Mathf.RoundToInt(X);
            var y = Mathf.RoundToInt(
                Mathf.Clamp(
                    Mathf.Round((position.z / (Level.Size + Level.instance.padding)) + 0.25f),
                    0f,
                    Level.LHeight - 1)
            );
            newCoord.x = x;
            newCoord.y = y;
            return newCoord;
        }
        public static TileCoordinate IndexToCoord(int i)
        {
            return new TileCoordinate()
            {
                x = (i % Level.LWidth),
                y = (i / Level.LWidth)
            };
        }
        
        public override string ToString()
        {
            return this.x + ", " + this.y;
        }
        public static TileCoordinate xy(int X, int Y) => new TileCoordinate() { x = X, y = Y };

        public int index()
        {
            return this.y * Level.LWidth + this.x;
        }

        public static int operator -(TileCoordinate a, TileCoordinate b) => Math.Abs(b.x - a.x) + Math.Abs(b.y - a.y);

        
    };
    
    
    [Serializable]
    public class Tile
    {
        [SerializeField]
        public int tileKey;
        // public bool traversible;
        public int traversingCost=0;
        public float heightOffset;
        public string baseString;
        public List<String> decoKeys = new List<String>();
        public TileDescriptor Descriptor()
        {
            var temp = new TileDescriptor()
            {
                index = tileKey,
                baseString = this.baseString,
                decoStrings = decoKeys.ToArray()
            };
            return temp;
        }
        public void CharacterEnter(SimpleCharacter character)
        {
            
        }

        public void CharacterExit(SimpleCharacter character)
        {
            
        }

    }
    
    
}