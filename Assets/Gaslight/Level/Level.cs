using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Behaviors;
using Gaslight.Characters.Descriptors;
using LevelCreation;
using Tiles;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using Gaslight.Characters.Logic;
#region Character related Descriptors

[Serializable]
public struct DirectiveDetail
{
    public string key;
    public string value;
}
[Serializable]
public struct CharacterLevelDescriptor
{
    public int index;
    public string name;
    public string baseCharacter;
    public EFaction faction;
    public string behavior;
    public BehaviorTarget[] BehaviorTargets;
}

public struct CharacterLevelDescriptorArray
{
    public CharacterLevelDescriptor[] root;
}
#endregion

#region Level related Descriptors
[Serializable]
public struct TileDescriptor
{
    //Should it know its number?????????
    //SHOULD IT KNOW ITS NUMBER?????????
    //AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
    //Need to sync with DB model, so it should.
    //Add a method to convert between tiles and their descriptors
    public int index;
    public string baseString;
    public string[] decoStrings;
}


[Serializable]
public struct RowDescriptor
{
    public TileDescriptor[] tiles;
}

[Serializable]
public struct LevelDescriptor
{
    public RowDescriptor[] rows;
}
#endregion
[RequireComponent(typeof(BoxCollider))]
public class Level : MonoBehaviour
{
    #region Level specific init-ers
    public static Level instance;
    public  static float Size = .9f;
    public float padding = .1f;
    

    private GameObject[] _tile3DObjectsBase = new GameObject[] { };
    private List<GameObject>[] _tile3DObjectsDeco = new List<GameObject>[] { };
    
    public bool[] tileTraversible = new bool[] { };
    public GameObject TileDisplayPrefab = default;
    public TileDisplay[] TileDisplays;

    public LevelDescriptor levelDescriptor = default;
    public bool getLevelDescriptorsFromPlayerPrefs = false;
    [SerializeField] private BaseTileDB baseTileDB;
    [SerializeField] private DecoTileDB decoTileDB;

    
    public bool getCharacterDescriptorsFromPlayerPrefs = false;
    [SerializeField]
    private List<CharacterLevelDescriptor> CharacterLevelDescriptors = default;
    [SerializeField] private CharacterAssetDB characterAssetDB = default;

    [SerializeField] private DirectiveManager directiveManager = default;
    [SerializeField]
    public Tile[] Tiles;
    //A dictionary will make this far easier
    //However, we do not live in a world where unity serializes dicts
    [SerializeField]
    public List<(string name, SimpleCharacter character)> characters;

    void ErrorOnDuplicateCharacters()
    {
        //N^2 but will worry about optimization later
        var duplicates = characters.GroupBy(p => p.name)
            .Where(p => p.Count() > 1)
            .Select(p=>p.Key).ToList();
        if (duplicates.Count > 0)
        {
            Debug.LogWarning("Found duplicates " + String.Join(", ",duplicates.ToArray()));
        }
    }
    private Vector2 _boundsStart;
    private Vector2 _boundsEnd;
    
    [HideInInspector]
    public static int LWidth, LHeight = 0;

    public UnityEvent LevelGenerated;

    #endregion
    
    public String[] CellStringList; 
    private void Awake()
    {
        if (instance != null && instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        {
            instance = this; 
        }
        if (getLevelDescriptorsFromPlayerPrefs)
        {
            LoadLevelDescriptorFromPlayerPrefs();
        }
        
        CalculateDimensions();
        InitLists();
        
    }

    private void OnValidate()
    {
        if (started)
        {
            Regenerate();
            SetBoxCollider();
        }
    }
    private IEnumerator GetCharacters(string url)
        {
            using (var webRequest = UnityWebRequest.Get(url))
            {
                yield return webRequest.SendWebRequest();
                if (webRequest.isNetworkError || webRequest.isHttpError)
                {
                    Debug.Log("Error:" + webRequest.error);
                }
                else
                {
                    var text = webRequest.downloadHandler.text;
                    Debug.Log("received" + text);
                    // text = text.Substring(1, text.Length-2);
                    // Debug.Log("After trim" + webRequest.downloadHandler.text);
                    PlayerPrefs.SetString("characters", text);
                }
                
            };
        }

    void Start()
    {
        // var tokenizedRows = levelString.Split(';');
        
        // FillTraversibleWithStrings(tokenizedRows);
        // FillCellStringList(levelString);
        DescriptorModeStart();
        // pathFinder.gameLevel = this;
        started = true;
        CalculateBounds();
        SetBoxCollider();

        // Debug.Log(GetStrKernel(0, 0, 3, 3));
        Regenerate();
        
        if (getCharacterDescriptorsFromPlayerPrefs)
        {
            //Then get it from the playerprefs 
            //For now just get characters
            var CharacterDescriptorsJSON = PlayerPrefs.GetString("character", "");
            if (CharacterDescriptorsJSON == "")
            {
                Debug.LogWarning("Could not find saved characters in PlayerPrefs, loading from scene string");
            }
            var temp = JsonUtility.FromJson<CharacterLevelDescriptorArray>(CharacterDescriptorsJSON);
            // Debug.Log(temp.root.Length);
            CharacterLevelDescriptors= temp.root.ToList(); 
        }
        SetAndSpawnCharacters();
        LevelGenerated.Invoke();
    }

    public void LoadLevelDescriptorFromPlayerPrefs()
    {
        Debug.LogWarning("Loading from playerprefs");
        var LevelDescriptorsJSON = PlayerPrefs.GetString("level", "");
        if (LevelDescriptorsJSON == "")
        {
            Debug.LogWarning("Could not find saved level in PlayerPrefs, loading from scene string");
        }

        var temp = JsonUtility.FromJson<LevelDescriptor>(LevelDescriptorsJSON);
        // Debug.Log(temp.root.Length);
        levelDescriptor = temp;
    }
    [ContextMenu("Test")]
    public void PrintLevelToJSON()
    {
        Debug.Log(LevelToJSON()); 
    }
    public string LevelToJSON()
    {
        return JsonUtility.ToJson(LevelToDescriptor());
    }
    //Returns a level descriptor structure
    public LevelDescriptor LevelToDescriptor()
    {
        var temp = new LevelDescriptor();
        var rows = new RowDescriptor[LHeight];
        temp.rows = rows;
        for (int y = 0; y < LHeight; y++)
        {
            rows[y] = new RowDescriptor();
            rows[y].tiles = new TileDescriptor[LWidth];
            for (int x = 0; x < LWidth; x++)
            {
                var descriptor = Tiles[(y * LWidth) + x].Descriptor();
                rows[y].tiles[x] = descriptor;
            }
        }

        return temp;
    }

    void DescriptorModeStart()
    {
      FillTraversibleWithDescriptor();  
    }
    
    #region Spawning
    private void SetAndSpawnCharacters()
    {
        characters = new List<(string name, SimpleCharacter character)>();
        foreach (var cld in CharacterLevelDescriptors)
        {
            if (isLocationValid(cld.index))
            {
                //Init character
                var newCharacter = characterAssetDB.GetCharacter(cld.baseCharacter);
                if (newCharacter == null)
                {
                    Debug.LogError("Character could not be inited");
                    continue;
                }

                newCharacter.transform.position = CoordToWorld(TileCoordinate.IndexToCoord(cld.index));
                newCharacter.transform.rotation = Quaternion.identity;
                newCharacter.transform.name = cld.name;
                newCharacter.MyTile = Tiles[cld.index];
                newCharacter.faction = cld.faction;
                newCharacter.passiveDirective = new ForegoDirective();
                newCharacter.behavior = BehaviorFactory.NewBehaviorByName("default_enemy_behavior",cld.BehaviorTargets.ToList());
                newCharacter.behavior.Invoker = newCharacter;
                newCharacter.behavior.Initialize();
                characters.Add((cld.name, newCharacter.GetComponent<SimpleCharacter>()));
                
            }
            else
            {
                Debug.Log(cld.index+ " is an invalid location");
            }
        }
    }

    private void CalculateDimensions()
    {
        LHeight = levelDescriptor.rows.Length;
        LWidth = 0;
        foreach (var row in levelDescriptor.rows)
        {
            //In a row
            //Length of the row is equal to the number of comma separated things in it
            if (row.tiles.Length > LWidth)
            {
                LWidth = row.tiles.Length;
            }
        }
    }
    #region vestige
    // private void CalculateDimensions(String input){
    //  //Set the height to the length of the string
    //         LHeight = input.Split(";").Length -1;
    //         
    //         //Set the width to the longest string in the level
    //         LWidth = 0;
    //         foreach (var row in input.Split(";"))
    //         {
    //             //In a row
    //             //Length of the row is equal to the number of comma separated things in it
    //             if (row.Split(",").Length > LWidth)
    //             {
    //                 LWidth = row.Split(",").Length;
    //             }
    //         }
    // }
    #endregion


   private void InitLists()
    {
        //Convert the level string to the matrix
        //Allocate memory
        tileTraversible = new bool[LWidth*LHeight];
        _tile3DObjectsBase = new GameObject[LWidth * LHeight];
        _tile3DObjectsDeco = new List<GameObject>[LWidth * LHeight];
        TileDisplays = new TileDisplay[LWidth * LHeight];
        for (var i = 0; i < LHeight; i++)
        {
            //Make them all empty
            for (int j = 0; j < LWidth; ++j)
            {
                tileTraversible[i * LWidth + j] = false;
                _tile3DObjectsBase[i * LWidth + j] = null;
                _tile3DObjectsDeco[i * LWidth + j] = new List<GameObject>();
            }
        }
    }

   private void FillTraversibleWithDescriptor()
   {
       for(int i=0; i<LHeight;++i)
       {
           var row = levelDescriptor.rows[i];

           for (int j = 0; j < LWidth; ++j)
           {
               var TileDescriptor = row.tiles[j];
               BaseTileAsset curLA;
               //Traversible by default, turn false if you find at least a single blocking element
               tileTraversible[i * LWidth + j] = true;
               if (!baseTileDB.GetFromList(TileDescriptor.baseString, out curLA))
               {
                   Debug.LogError("Invalid LA name:_" + TileDescriptor.baseString 
                               + "_Candidates are :_" + String.Join(",",baseTileDB.ValidCandidates()));
                   continue;
               }
               
               if (!curLA.traversible)
               {
                   // Debug.Log(single_name + " is blocking");
                   tileTraversible[i * LWidth + j] = false;
                   break;
               }
           }
       }
   }
    
    private void InitTileListsWithDescriptors()
    {
        Tiles = new Tile[LHeight*LWidth];
        for (int y = 0; y < LHeight; ++y)
        {
            for (int x = 0; x < LWidth; ++x)
            {
                var newTile =  new Tile()
                {
                    tileKey =  y*LWidth+x,
                };

                //Set baseString of tile only on first iteration
                bool baseStringSetFlag = false;
                var tileDescriptor = levelDescriptor.rows[y].tiles[x];
                newTile.baseString = tileDescriptor.baseString;
                newTile.decoKeys = new();
                if (tileDescriptor.decoStrings != null)
                {
                    newTile.decoKeys.AddRange(tileDescriptor.decoStrings);
                }
                Tiles[y * LWidth + x] = newTile;

            }
        }
    }
    
    #region vestige
    // private void InitTileLists()
    // {
    //     Tiles = new Tile[LHeight*LWidth];
    //     for (int y = 0; y < LHeight; ++y)
    //     {
    //         for (int x = 0; x < LWidth; ++x)
    //         {
    //             var newTile =  new Tile()
    //             {
    //                 tileKey =  y*LWidth+x,
    //                 traversible = true
    //             };
    //
    //             //Set baseString of tile only on first iteration
    //             bool baseStringSetFlag = false;
    //             foreach (var key in CellStringList[y*LWidth+x].Trim().Split("+"))
    //             {
    //                 if (!baseStringSetFlag)
    //                 { 
    //                     //The first one is always the base
    //                     newTile.baseString = key.Trim();
    //                     baseStringSetFlag = true;
    //                 }
    //                 else
    //                 {
    //                     newTile.decoKeys.Add(key.Trim());
    //                 }
    //                 
    //             }
    //             Tiles[y * LWidth + x] = newTile;
    //
    //         }
    //     }
    //
    // }
    //
    #endregion
    public void ToggleDisplays()
    {
        foreach (var tileDisplay in TileDisplays)
        {
            tileDisplay.enabled = !tileDisplay.enabled;
        }
    }
    private void SpawnTileDisplayAt(int index)
    {
        TileDisplays[index] = GameObject.Instantiate(TileDisplayPrefab, 
            CoordToWorld(TileCoordinate.IndexToCoord(index))+ new Vector3(0f,.1f,0f),
            Quaternion.Euler(new Vector3(90f,0f,0f)), parent:this.transform).GetComponent<TileDisplay>();
        TileDisplays[index].index = index;
    }
    private void Regenerate()
    {
        if (Application.isPlaying)
        {
            CreateLevel();

        }
    }

    private bool started = false;

    private void CalculateBounds()
    {
        _boundsStart = new Vector2(-Size/2f, -Size/2f);
        _boundsEnd = new Vector2((LWidth - 1) * (Size + padding) + ((Size + padding)/ 2f), (LHeight-1) * (Size + padding) +
            (Size + padding) / 2f);
    }
    

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(new Vector3(_boundsStart.x, 0f, _boundsStart.y), Vector3.one*5f);
        Gizmos.color = Color.green;
        Gizmos.DrawCube(new Vector3(_boundsEnd.x, 0f, _boundsEnd.y), Vector3.one *5f);
    }

    private void SetBoxCollider()
    {
        this.GetComponent<BoxCollider>().center =
            new Vector3((_boundsEnd.x + _boundsStart.x) / 2f, 0f, (_boundsStart.y + _boundsEnd.y) / 2f);
        this.GetComponent<BoxCollider>().size =
            new Vector3((Size + padding) * (LWidth), 4f, (Size + padding) * (LHeight) + Size/2f);
    }

    public Vector3 CoordToWorld(TileCoordinate tc)
    {
        return new Vector3((Size + padding) * tc.x, 0f, (Size + padding) * tc.y);
    }

    public Vector3 CoordToWorld(int index)
    {
        var temp = TileCoordinate.IndexToCoord(index);
        return CoordToWorld(temp);
    }
    
    public void CreateLevel()
    {
        InitTileListsWithDescriptors();

        for (int y = 0; y < LHeight; ++y)
        {
            for (int x = 0; x < LWidth; ++x)
            {
                RefreshTile(x, y);
                SpawnTileDisplayAt(y*LWidth+x);
            }
        }
    }
    

    //Reads values from the Tile object present at x and y
    //Instantiates debug surface
    //Instantiates 3D models for base and deco
    public void RefreshTile(int x, int y)
    {

        if (_tile3DObjectsDeco[y * LWidth + x]!=null)
        {
            foreach (var o in _tile3DObjectsDeco[y * LWidth + x])
            {
                GameObject.Destroy(o);
            }
        }
        if (_tile3DObjectsBase[y * LWidth + x]!=null)
        {
            GameObject.Destroy(_tile3DObjectsBase[y*LWidth+x]);
        }

        var newCoord = TileCoordinate.xy(x, y);
        
        Vector3 squarePosition = CoordToWorld(newCoord);
        var tile = Tiles[y*LWidth+x];
        //Spawn base, code block to avoid scoping requiredLA
        {
            if (baseTileDB.GetFromList(tile.baseString, out BaseTileAsset RequiredLA))
            {
                if(RequiredLA==null){
                    Debug.Log("Base name "+ tile.baseString +" not found");
                }
                SpawnBaseAt(y, x, RequiredLA, squarePosition);
            }
        }
        {
            foreach (var tileDecoKey in tile.decoKeys)
            {
                if (decoTileDB.GetFromList(tileDecoKey, out DecoTileAsset RequiredLA))
                {
                    SpawnDecoAt(y,x,RequiredLA, squarePosition);
                    break;       
                }
            }
            
        }

        
    }

    private void SpawnBaseAt(int y, int x, BaseTileAsset RequiredLA, Vector3 squarePosition)
    {
        Tiles[y * LWidth + x].heightOffset = RequiredLA.heightOffset;
        var newObj = GameObject.Instantiate(RequiredLA.Asset, squarePosition, Quaternion.identity);
        newObj.transform.SetParent(transform, true);
        if (_tile3DObjectsBase[y * LWidth + x] != null)
        {
            GameObject.DestroyImmediate(_tile3DObjectsBase[y*LWidth+x]);
        }
        {
            _tile3DObjectsBase[y * LWidth + x] = newObj;
        }
    }

    private void SpawnDecoAt(int y, int x, DecoTileAsset RequiredLA, Vector3 squarePosition)
    {
        var newObj = GameObject.Instantiate(RequiredLA.Asset, squarePosition, Quaternion.identity);
        newObj.transform.SetParent(transform, true);
        _tile3DObjectsDeco[y * LWidth + x].Add(newObj);
    }


    private void RefreshTile(int index)
    {
        var coord = TileCoordinate.IndexToCoord(index);
        RefreshTile(coord.x, coord.y);
    }
    

    #endregion

    
    #region Tile selection
    [HideInInspector]public bool isATileSelected = false;
    public Tile selectedTile;

    public UnityEvent TileSelected;
    public UnityEvent<int> TileDeselected;
    public void SelectTile(int index)
    {
        if (highlightedTile!=null && index == highlightedTile.index)
        {
            DehighlightTile();
        }
        DeselectTile();
        isATileSelected = true;
        selectedTile = Tiles[index];
        TileDisplays[index].setState(TileDisplay.State.Selected);
        TileSelected?.Invoke();
    }
    public void DeselectTile()
    {
        if (isATileSelected)
        {
            TileDisplays[selectedTile.tileKey].setState(TileDisplay.State.Idle);
        }
        isATileSelected = false;
        TileDeselected?.Invoke(selectedTile.tileKey);
    }

    
    public bool isLocationValid(int key)
    {
        int y = key / LWidth;
        int x = key % LWidth;
        if (key < 0 || y >= LHeight || x >= LWidth)
        {
            return false;
        }
        else
        {
            return true;
        }
        
    }

    public Tile GetTileFromKey(int key)
    {
        return Tiles[key];
    }

    public GameObject GetBaseFromKey(int key)
    {
        return _tile3DObjectsBase[key];
    }

    public List<GameObject> GetDecoFromKey(int key)
    {
        return _tile3DObjectsDeco[key];
    }

    #endregion
    #region Character selection
        public bool isACharacterSelected = false;
        public SimpleCharacter selectedCharacter;

        public bool isACharacterHighlighted = false;
        public SimpleCharacter highlightedCharacter;

        public UnityEvent OnCharacterSelected;
        public UnityEvent<string> OnCharacterDeselected;

        public bool isAnyCharacterOnTile(int index)
        {
            return GetTileOccupant(index)== null;
        }
        public void SelectCharacter(int index)
        {
            isACharacterSelected = true;
            selectedCharacter = GetTileOccupant(index);
            selectedCharacter.Select();
            OnCharacterSelected?.Invoke();
        }
        public void DeselectCharacter()
        {
            if (isACharacterSelected)
            {
                selectedCharacter.Deselect();
            }
            isACharacterSelected = false;
            OnCharacterDeselected?.Invoke(selectedCharacter.transform.name);
        }
        
    #endregion
    #region Faction Management

    public List<SimpleCharacter> GetCharactersOfFaction
    (EFaction faction)
    {
        return characters.FindAll((el) => el.character.faction == faction)
            .Select((ch)=>ch.character).ToList();
    }
    #endregion
    #region Character movement

    public bool IsTileOccupied(int index)
    {
        return characters.Any(ch => ch.character.MyTile.tileKey == index);
    }

    public SimpleCharacter GetTileOccupant(int index)
    {
        return characters.Find(ch => ch.character.MyTile.tileKey == index).character;
    }
    
    public void ChangeCharacterTile(SimpleCharacter character, int newTileIndex)
    {
        character.MyTile.CharacterExit(character);
        character.MyTile = Tiles[newTileIndex];
        character.MyTile.CharacterEnter(character);
    }
    public void ChangeCharacterTile(string charactername, int newTileIndex)
    {
        // Debug.Log("Level changing character tile to "+ newTileIndex);
        var character = characters.Find(ch => ch.name == charactername).character;
        
        ChangeCharacterTile(character, newTileIndex);
        
    }
    
    #endregion
    #region Level Editor

    public List<String> GetValidBaseTileNames()
    {
        var output = baseTileDB.BaseTileAssets
            // .FindAll(e => e.levelAssetType == LevelAssetType.LevelObjectBase)
            .Select(e=>e.Key)
            .ToList();

        {
            // Debug.Log("Valid bases\n" + String.Join(", ", output));
        }   
        return output;
    }

    public UnityEvent<int> TileChanged;
    public void ChangeTileBase(int index, String newBase)
    {
        Tiles[index].baseString = newBase;
        RefreshTile(index%LWidth, index/LWidth);
        TileChanged.Invoke(index);
        TileDisplays[index].RefreshDisplay();
    }

    private TileDisplay highlightedTile = default;
    public void HighlightTile(int index)
    {

        if (isATileSelected && selectedTile.tileKey == index)
        {
            // Debug.Log("That tile is selected");
            return;
        }
        if (highlightedTile != null)
        {
            if (highlightedTile.index == index)
            {
                return;
            }
            DehighlightTile();
        }
        {
            // Debug.Log("Highlighting "+index);
            TileDisplays[index].setState(TileDisplay.State.Highlighted);
            highlightedTile = TileDisplays[index];
        }
    }

    public void DehighlightTile()
    {
        if (highlightedTile != null)
        {
            highlightedTile.setState(TileDisplay.State.Idle);
        }
        highlightedTile = null;
    }

    
    #endregion
    
    
    #region Pathfinding helpers

    public bool debugPathfinder = false;
    public AStar pathFinder = new AStar(){debug = false};
    
    public List<int> Get4NeighboringTiles(int index)
    {
        List<int> temp= new List<int>();
        foreach(ELinkDirection dir in Enum.GetValues(typeof(ELinkDirection)))
        {
            if (isLocationValid(DirectionOntoLocation(index, ELinkDirection.Down)))
            {   
                temp.Append(DirectionOntoLocation(index, dir));
            }
            else
            {
                Debug.Log($"Location {DirectionOntoLocation(index,dir)} is not valid");
            }
            
        }
        return temp;
    }

   
        
    
    public int GetDistance(int a, int b)
    {
        if(debugPathfinder)
        Debug.Log("Distance between " + a + " and " + b + " is: " + (TileCoordinate.IndexToCoord(a ) -
                                                                     TileCoordinate.IndexToCoord(b)));
        return TileCoordinate.IndexToCoord(a) - TileCoordinate.IndexToCoord(b);
    }
    
    public enum ELinkDirection
    {
        Left,
        Right,
        Up,
        Down
    }
    public ELinkDirection DirectionFrom(int From, int To)
    {
        TileCoordinate TFrom = TileCoordinate.IndexToCoord(From);
        TileCoordinate TTo = TileCoordinate.IndexToCoord(To);
        //This does not check if it is further away
        //The contract is that if there is to be a path between them, it will either be a straight line or just adjacent
        if (TFrom.y - TTo.y >0)
        {
            return ELinkDirection.Up;
        }
        else
        {
            return ELinkDirection.Right;
        }
    }
    
    //Returns the direction added to a location
    //If the tile is the leftmost tile or rightmost tile, returns MaxValue
    public int DirectionOntoLocation(int loc, ELinkDirection direction)
    {
        TileCoordinate temp;
        switch (direction)
        {
            case ELinkDirection.Up:
                //Check if at the topmost row
                if (loc+LWidth > LWidth*LHeight-1)
                {
                    return Int32.MaxValue;
                }
                return loc + LWidth;
            case ELinkDirection.Down:
                if (loc - LWidth < 0)
                {
                    return Int32.MaxValue;
                }
                return loc - LWidth;
            case ELinkDirection.Left:
                //Check if it is leftmost
                //left is x=0
                if (loc % LWidth == 0)
                {
                    return Int32.MaxValue;
                }
                else
                {
                    return loc-1;
                }
            case ELinkDirection.Right:
                //Check if it is rightmost
                //Rightmost is x=width-1
                if (loc % LWidth == LWidth - 1)
                {
                    return Int32.MaxValue;
                }
                else return loc+1;
        }
    
        return Int32.MaxValue;
    }

    public bool hasNeighborInDirection(int loc, ELinkDirection direction)
    {
        var other = DirectionOntoLocation(loc, direction);
        if (other == Int32.MaxValue)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    #endregion

    #region Pathfinding features
    //Double this to find the distance between two tiles
    public List<int> FindPath(int start, int end, int maxTiles = Int32.MaxValue)
    {
        pathFinder.FindPath(start, end);
        if (pathFinder.PathFound == false)
        {
            Debug.Log("No valid path");
            return null;
        }
        return pathFinder.PathLocs.Take(maxTiles).ToList();
    }

    #endregion
    
}
