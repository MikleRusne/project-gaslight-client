using System;
using System.Collections;
using System.Collections.Generic;
using Characters;
using Tiles;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;


// [ExecuteInEditMode]
public class Spawner:MonoBehaviour
{
    

    # region "Public Fields"
    public static Spawner inst;
    public GameObject PlayerPrefab;
    
    public float tileSizeOffset;
    public string Generator;
    
    public GameObject Tile;
    public GameObject Block;
    public GameObject Space;
    public Vector3 SpawnAt;
    public static float TraceDistance;


    # endregion
    void Awake()
    {
        Debug.Log("Triggered awake");
        if (inst != null)
        {
            Destroy(gameObject);
        }
        else
        {
            inst = this;
        }
    }


    # region "Instantiated Tiles"
    public enum ETileType
    {
        Invalid,
        Space,
        Tile,
        Block
    }

    public struct FInstantiatedTile
    {
        public GameObject Tile;
        public ETileType Type;

    };
    
    public static FInstantiatedTile FIT(GameObject Tile, ETileType Type)
    {
        var InstantiatedTile = new FInstantiatedTile();
        InstantiatedTile.Tile = Tile;
        InstantiatedTile.Type = Type;
        return InstantiatedTile;
    }
  
    # endregion
    
    # region "Spawners"
    
    void SpawnLevel()
    {
        if (Level)
        {
            DestroyImmediate(Level);
        }

        Level = new GameObject("Spawned");
        
    }

    [Range(0, 10f)]
    public float LinkOffset =4.1f;
    public string[] GetLevelLines()
    {
        var lines = Generator.Split(',');
        // int maxLines = 0;
        lvWidth = 0;
        lvHeight = lines.Length;
        // Debug.Log(lines);   
        foreach (var line in lines)
        {
            // Debug.Log(line);
            if (line.Length > lvWidth)
            {
                lvWidth = line.Length;
            }
        }

        return lines;
    }
    
    //To be removed
      public void SetupTileLinks( (int y, int x) location, FInstantiatedTile newTile)
        {
            
            //Check if there is a tile to the left
            //Should be the tile right with index-1
            
            //Check if the newTile is of tile type
            if (newTile.Type != ETileType.Tile)
            {
                Debug.Log("Tile at " + location + " does not need links.");
                return;
            }
    
            var newBlockLinks = newTile.Tile.GetComponent<BlockLinks>();
            newBlockLinks.MyID = location;
    
            //Check if it is leftmost tile;
            if (location.x ==0)
            {
                newBlockLinks.Neighbors[BlockLinks.ELinkDirection.Left] = FIT(null, ETileType.Invalid);
                newBlockLinks.connected[BlockLinks.ELinkDirection.Left] = false;
            }
            else
            {
                var leftTile = Instantiated[(location.y, location.x-1)];
                newBlockLinks.Neighbors[BlockLinks.ELinkDirection.Left] = leftTile.IT;
                // Debug.Log("Setting connected of "+ Tile.name + " to true");
    
                if (leftTile.IT.Type == ETileType.Tile)
                {
                    newBlockLinks.connected[BlockLinks.ELinkDirection.Left] = true;
                    leftTile.BL.Neighbors[BlockLinks.ELinkDirection.Right] = newTile;
                    // Debug.Log("Setting connected of "+ leftTile.IT.Tile.name + " to true");
                    leftTile.BL.connected[BlockLinks.ELinkDirection.Right] = true;
            }
            }
    
    
            //Check if it is at top 
            if (location.y == 0)
            {
                newBlockLinks.Neighbors[BlockLinks.ELinkDirection.Up] = FIT(null, ETileType.Invalid);
                newBlockLinks.connected[BlockLinks.ELinkDirection.Up] = false;        
            }
            else
            {
                var UpTile = Instantiated[(location.y-1, location.x)];
                newBlockLinks.Neighbors[BlockLinks.ELinkDirection.Up] = UpTile.IT;
    
                if (UpTile.IT.Type == ETileType.Tile)
                {
                    newBlockLinks.connected[BlockLinks.ELinkDirection.Up] = true;
                    UpTile.IT.Tile.GetComponent<BlockLinks>().Neighbors[BlockLinks.ELinkDirection.Down] = newTile;
                    UpTile.IT.Tile.GetComponent<BlockLinks>().connected[BlockLinks.ELinkDirection.Down] = true;
        
                }
    
            }
        }

      
      
      float TileSizeInEachDirection
      {
          get
          {
             return TraceDistance+ tileSizeOffset; 
          }
      }
      // ReSharper disable Unity.PerformanceAnalysis
            
      public void SpawnTiles(string[] lines)
        {
            
            int tileCounter = 0;
            for (int i = 0; i < lines.Length; ++i)
            {
                for (int j = 0; j < lines[i].Length; ++j)
                {
                    switch (lines[i][j])
                    {
                        case '0':
                        {
                            SpawnSpaceAt(i,j, TileSizeInEachDirection);
                        }
                            break;
    
                        case '1':
                        {
                            var InstantiatedTile = SpawnTile(i, j, TileSizeInEachDirection);
                        }
                            break;
                        case '2':

                        {
                            var InstantiatedTile = SpawnBlockAt(i, j, TileSizeInEachDirection);
                        }
                            break;   
                        case '3':
                        {
                            var newTile = SpawnTile(i, j, TileSizeInEachDirection);    
                            
                            Player = Instantiate(PlayerPrefab, newTile.Tile.transform.GetChild(5).transform.position, Quaternion.identity).GetComponent<Character>();
                            // Player.StartingTile = newTile.Tile;
                            CameraPivot = GameObject.Find("Camera Pivot");
                            CameraPivot.GetComponent<FollowPlayer>().Player = Player.transform;
                        }
                            break;
                    }
    
                    tileCounter++;
                }
            }
        }

      public GameObject CameraPivot;
      private FInstantiatedTile SpawnBlockAt(int i, int j, float TileSizeInEachDirection)
      {
          //Add a block, figure out how to set the mesh later
          var newTile = Instantiate(Block,
              new Vector3(j * (TileSizeInEachDirection), 0.0f, -i * (TileSizeInEachDirection)),
              Quaternion.identity, Level.transform);
          newTile.transform.name = "Block" + i + ":" + j;
          var BL = newTile.GetComponent<BlockLinks>();
          BL.MyID = (i, j);
          BL.LinkOffset = LinkOffset;
          var InstantiatedTile = FIT(newTile, ETileType.Block);
          Instantiated[(i, j)]= (InstantiatedTile, newTile.GetComponent<BlockLinks>());
          return InstantiatedTile;
      }

      private FInstantiatedTile SpawnTile(int i, int j, float TileSizeInEachDirection)
      {
          var newTile = Instantiate(Tile,
              new Vector3(j * (TileSizeInEachDirection), 0.0f, -i * (TileSizeInEachDirection)),
              Quaternion.identity, Level.transform);
          newTile.transform.name = "Tile" + i + ":" + j;
          var BasicInfo = newTile.GetComponent<Basic>();
          var BlockLinks = newTile.GetComponent<BlockLinks>();
          BlockLinks.MyID = (i, j);
          //Check if there is a block to the left
          BlockLinks.LinkOffset = LinkOffset;


          FInstantiatedTile InstantiatedTile = FIT(newTile, ETileType.Tile);
          SetupTileLinks((i, j), InstantiatedTile);
          Instantiated[(i, j)]= (InstantiatedTile, BlockLinks);
          return InstantiatedTile;
      }

      private FInstantiatedTile SpawnSpaceAt(int i, int j, float TileSizeInEachDirection)
        {
            var newTile = Instantiate(Tile,
                new Vector3(j * (TileSizeInEachDirection), 0.0f, -i * (TileSizeInEachDirection)),
                Quaternion.identity, Level.transform);
            FInstantiatedTile InstantiatedTile = FIT(newTile, ETileType.Space);
            SetupTileLinks((i, j), InstantiatedTile);
            var BL = newTile.GetComponent<BlockLinks>();
            BL.MyID = (i, j);
            BL.LinkOffset = LinkOffset; 
            Instantiated.Add((i, j), (InstantiatedTile, BL));
            return InstantiatedTile;
        }

        // public bool isOccupied()
        public void Spawn()
        {
            SpawnLevel();
            var lines = GetLevelLines();
            SpawnTiles(lines);
        }
       
    # endregion
    
    //Change to list later, no need for dictionary
    //Besides, dictionary will cause trouble later on with debugging
    public Dictionary<(int y, int x), (FInstantiatedTile IT, BlockLinks BL)> Instantiated = new Dictionary<(int, int), (FInstantiatedTile IT, BlockLinks BL)>();
        
    

    
    private GameObject Level;
    public Character Player;

    public int lvWidth=0;
    public int lvHeight=0;

    # region "Pathfinding helpers"
    public static int ManhattanDistance((int y, int x) a, (int y, int x) b)
    {
        return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
    }
     public static BlockLinks.ELinkDirection DirectionFrom((int y, int x) From, (int y, int x) To)
     {
         //This does not check if it is further away
         //The contract is that if there is to be a path between them, it will either be a straight line or just adjacent
         if (From.y - To.y >0)
         {
             return BlockLinks.ELinkDirection.Up;
         }
         else
         {
             return BlockLinks.ELinkDirection.Right;
         }
     }
    public (FInstantiatedTile IT, BlockLinks BL) GetTileFromLocation((int y, int x) a)
    {
        if (a.y >= lvHeight || a.y < 0 || a.x < 0 || a.x >= lvWidth)
        {
            return (FIT(null, ETileType.Invalid), null);
        }
        else
        {
            return Instantiated[(a.y, a.x)];
        }
    }
     bool islocationinbounds((int y, int x) a)
        {
            if (a.y >= lvHeight || a.y < 0 || a.x < 0 || a.x >= lvWidth)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    
        public (int, int) DirectionOntoLocation((int y, int x) loc, BlockLinks.ELinkDirection direction)
        {
            switch (direction)
            {
                case BlockLinks.ELinkDirection.Up:
                    return (loc.y - 1, loc.x);
                case BlockLinks.ELinkDirection.Down:
                    return (loc.y + 1, loc.x);
                case BlockLinks.ELinkDirection.Left:
                    return (loc.y, loc.x-1);
                case BlockLinks.ELinkDirection.Right:
                    return (loc.y, loc.x + 1);
            }
    
            return (Int32.MinValue, Int32.MinValue);
        }
        public (bool success, FInstantiatedTile IT) GetBLInDirection((int, int) from, BlockLinks.ELinkDirection direction)
        {
            if (islocationinbounds(from))
            {
                var TargetLoc = DirectionOntoLocation(from, direction);
                if (islocationinbounds(TargetLoc))
                {
                    return (true, GetTileFromLocation(TargetLoc).IT);
                }
            }
            return (false, FIT(null, ETileType.Invalid));
        }
        
    #endregion


    #region Tile modification
        //Previous implementation does not account for leaving tiles
      //Two versions overcomplicate the process
      //This one is currently made only for recalculation, not for precalculation
      public void AllDirectionLinks((int y, int x) location)
      {
          var Tile = Instantiated[location];
          //Check in each direction
         foreach (BlockLinks.ELinkDirection direction in Enum.GetValues(typeof(BlockLinks.ELinkDirection)))
         {
             //Check if it exists
             var target = DirectionOntoLocation(location, direction);
             if (islocationinbounds(target))
             {
                 switch (Instantiated[location].IT.Type)
                 {
                     case ETileType.Tile:
                         Tile.BL.connected[direction] = true;
                         Tile.BL.Neighbors[direction] = Instantiated[target].IT;
                         break;
                     case ETileType.Block:
                         Tile.BL.connected[direction] = false;
                         Tile.BL.Neighbors[direction] = Instantiated[target].IT;
                         break;
                     case ETileType.Invalid:
                         Tile.BL.connected[direction] = false;
                         break;
                     case ETileType.Space:
                         Tile.BL.connected[direction] = false;

                         break;
                     
                 }
             }
             else
             {
                 Tile.BL.connected[direction] = false;
             }
         }
      }

      public UnityEvent<(int, int)> TileToBlock;
    public void ChangeToBlock((int y, int x) Target)
    {
        if (Instantiated[Target].IT.Type == ETileType.Block)
        {
            return;
        }
        
        //It is a tile
        //Check if it is occupied
        if (Instantiated[Target].BL.isOccupied)
        {
            Debug.Log("That tile is occupied");
            return;
        }
        Destroy(Instantiated[Target].IT.Tile);
        var newBlock = SpawnBlockAt(Target.y, Target.x, TileSizeInEachDirection);
        
        foreach (BlockLinks.ELinkDirection direction in Enum.GetValues(typeof(BlockLinks.ELinkDirection)))
        {
            var nebberloc = DirectionOntoLocation(Target, direction);
            if (islocationinbounds(nebberloc))
            {
                AllDirectionLinks(nebberloc);
                Instantiated[nebberloc].BL.VerifyLinks();
            };
        }
        TileChangedEvent.Invoke(Target, ETileType.Block);
        
    }

    public void ChangeToTile((int y, int x) Target)
    {
         if (Instantiated[Target].IT.Type == ETileType.Tile)
        {
            return;
        }
        
        //It is a block
        Destroy(Instantiated[Target].IT.Tile);
        var newTile = SpawnTile(Target.y, Target.x, TileSizeInEachDirection);
        AllDirectionLinks(Target);
        foreach (BlockLinks.ELinkDirection direction in Enum.GetValues(typeof(BlockLinks.ELinkDirection)))
        {
            var nebberloc = DirectionOntoLocation(Target, direction);
            if (islocationinbounds(nebberloc))
                {
                    AllDirectionLinks(nebberloc);
                    Instantiated[nebberloc].BL.VerifyLinks();
            };
        }
        TileChangedEvent.Invoke(Target, ETileType.Tile);
    
                
    } 
    #endregion

    public UnityEvent<(int, int), ETileType> TileChangedEvent;     
    
   // Update is called once per frame
    void Start()
    {
        if (Application.isPlaying)
        {
            GameObject previousPlayer = GameObject.Find("Player(Clone)");
            Object.Destroy(previousPlayer);
            GameObject previousLevel = GameObject.Find("Spawned");
            Object.Destroy(previousLevel);
            Spawn();
            // PlayerGO.GetComponent<Character>().StartingTile = Instantiated[0];
            // GameObject Player = Instantiate(PlayerGO, Instantiated[0].transform.position, Quaternion.identity);
        }
        
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public List<string> DisplayInstantiated()
    {
        
        List<string> temp = new List<string>();
        if (Instantiated.Count < 1)
        {
            return temp;
        }
        for (int i = 0; i < lvHeight; ++i)
        {
            for (int j = 0; j < lvHeight; ++j)
            {
                if (Instantiated[(i, j)].IT.Type == ETileType.Tile)
                {
                    temp.Add(Instantiated[(i, j)].IT.Tile.transform.name+ ": " + Instantiated[(i, j)].IT.Tile.GetComponent<BlockLinks>().DisplayNeighbors());
                }
            }
        }

        return temp;
    }
 
}
