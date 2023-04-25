using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Behaviors;
using Gaslight.Characters.Logic;
using LevelCreation;
using Tiles;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

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
    public string iconName;
    public string baseCharacter;
    public EFaction faction;
    public string behavior;
    public BehaviorTargets BehaviorTargets;
    public SimpleCharacter.NamedFloatTrait[] FloatTraitOverrides;
    public SimpleCharacter.NamedStringTrait[] StringTraitOverrides;
}

public struct CharacterLevelDescriptorArray
{
    public CharacterLevelDescriptor[] root;
}
#endregion

#region Level related Descriptors

[Serializable]
public struct BaseTileDescriptor
{
    public string name;
    public float yRotation;
}

[Serializable]
public struct DecorationDescriptor
{
    public string name;
    public float yRotation;
}
[Serializable]
public struct TileDescriptor
{
    //Should it know its number?????????
    //SHOULD IT KNOW ITS NUMBER?????????
    //AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
    //Need to sync with DB model, so it should.
    //Add a method to convert between tiles and their descriptors
    public int index;
    public BaseTileDescriptor baseTileDescriptor;
    public DecorationDescriptor[] decorations;
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

[Serializable]
public struct RuntimeLevelCharDescriptor
{
    public string name;
    public SimpleCharacter character;

    public RuntimeLevelCharDescriptor(string name, SimpleCharacter character)
    {
        this.name = name;
        this.character = character;
    }
}
[RequireComponent(typeof(BoxCollider)), ExecuteInEditMode]
public class Level : MonoBehaviour
{
    #region Level specific init-ers

    public bool _levelGenerated = false;
    public static Level instance;
    public  static float Size = .9f;
    public float padding = .1f;
    //Defines how many tiles a character can move with respect to 1 point of speed
    public static float SpeedToTileMovementFactor = 4.0f;

    public  GameObject[] _tile3DObjectsBase = new GameObject[] { };
    public List<GameObject>[] _tile3DObjectsDeco = new List<GameObject>[] { };
    
    public bool[] tileTraversible = new bool[] { };
    public GameObject TileDisplayPrefab = default;
    public TileDisplay[] TileDisplays;
    public BoxCollider[] _tileDisplayColliders;

    public LevelDescriptor levelDescriptor = default;
    public bool getLevelDescriptorsFromPlayerPrefs = false;
    [SerializeField] private BaseTileDB baseTileDB;
    [SerializeField] private DecoTileDB decoTileDB;

    
    public bool getCharacterDescriptorsFromPlayerPrefs = false;
    [SerializeField]
    private List<CharacterLevelDescriptor> CharacterLevelDescriptors = default;
    [SerializeField] private CharacterAssetDB characterAssetDB = default;
    [SerializeField] private CharacterIconDB characterIcons = default;
    [SerializeField]
    public Tile[] Tiles;
    //A dictionary will make this far easier
    //However, we do not live in a world where unity serializes dicts
    [SerializeField]
    public List<RuntimeLevelCharDescriptor> characters;

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
    public Vector2 _boundsStart;
    public Vector2 _boundsEnd;
    
    [HideInInspector]
    public static int LWidth, LHeight = 0;

    public UnityEvent LevelGenerated;

    #endregion
    
    public String[] CellStringList; 
    private async Task Awake()
    {
        if (instance != null && instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        {
            instance = this; 
        }

        if (!_levelGenerated)
        {
            if (getLevelDescriptorsFromPlayerPrefs)
            {
                await LoadLevelDescriptorFromPlayerPrefs();
            }

            if (getCharacterDescriptorsFromPlayerPrefs)
            {
                await LoadCharacterDescriptorsFromPlayerPrefs();
            }
            
            await CreateLevel();
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

    void InitCharacters()
    {
        if (Application.isPlaying)
        {
            if (characters == null)
            {
                Debug.LogError("Runtime character information null");
            }
            foreach (var cld in CharacterLevelDescriptors)
            {
                var requiredCharacter = characters.Find((charDescriptor => charDescriptor.character.name == cld.name)).character;
                if (requiredCharacter == null)
                {
                    Debug.LogError("Could not find character " + cld.name + ". Characters are " + characters.Aggregate("", (s, descriptor) => s+ descriptor.name + ","));
                    continue;
                }
                requiredCharacter.passiveDirective = new ForegoDirective();
                requiredCharacter.behavior = BehaviorFactory.NewBehaviorByName("default_enemy_behavior",cld.BehaviorTargets.root);
                requiredCharacter.behavior.Invoker = requiredCharacter;
                requiredCharacter.behavior.Initialize();
            }
        }   
    }

    

    async void Start()
    {
        if (Application.isPlaying)
        {
            await CalculateDimensions();
            TurnOffAllColliders();
            InitCharacters();
            LevelGenerated.Invoke();
        }
    }

    [ContextMenu("Load level descriptor from player prefs")]
    public async void LoadFromDescriptors()
    {
        await LoadLevelDescriptorFromPlayerPrefs();
        await LoadCharacterDescriptorsFromPlayerPrefs();
    }
    public Task LoadLevelDescriptorFromPlayerPrefs()
    {
        // Debug.LogWarning("Loading from playerprefs");
        var LevelDescriptorsJSON = PlayerPrefs.GetString("level", "");
        if (LevelDescriptorsJSON == "")
        {
            Debug.LogWarning("Could not find saved level in PlayerPrefs, loading from scene string");
        }

        var temp = JsonUtility.FromJson<LevelDescriptor>(LevelDescriptorsJSON);
        // Debug.Log(temp.root.Length);
        levelDescriptor = temp;
        return Task.CompletedTask;
    }
    
        
    public Task LoadCharacterDescriptorsFromPlayerPrefs()
    {
        // Debug.LogWarning("Loading from playerprefs");
        var CharacterDescriptorsJSON = PlayerPrefs.GetString("character", "");
        if (CharacterDescriptorsJSON == "")
        {
            Debug.LogWarning("Could not find saved level in PlayerPrefs, loading from scene string");
        }

        var temp = JsonUtility.FromJson<CharacterLevelDescriptorArray>(CharacterDescriptorsJSON);
        // Debug.Log(temp.root.Length);
        CharacterLevelDescriptors = temp.root.ToList();
        return Task.CompletedTask;
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
    private async Task SetAndSpawnCharacters()
    {
        characters = new List<RuntimeLevelCharDescriptor>();
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

                newCharacter.icon = characterIcons.GetIcon(cld.iconName);
                newCharacter.transform.position = CoordToWorld(TileCoordinate.IndexToCoord(cld.index));
                newCharacter.transform.rotation = Quaternion.identity;
                newCharacter.transform.name = cld.name;
                newCharacter.name = cld.name;
                newCharacter.MyTile = Tiles[cld.index];
                newCharacter.faction = cld.faction;
                var levelCharDescriptor = new RuntimeLevelCharDescriptor();
                levelCharDescriptor.name = cld.name;
                levelCharDescriptor.character = newCharacter;
                
                characters.Add(levelCharDescriptor);
            }
            else
            {
                Debug.Log(cld.index+ " is an invalid location");
            }

            await Task.Yield();
        }
    }

    private Task CalculateDimensions()
    {
        Level.LHeight = levelDescriptor.rows.Length;
        Level.LWidth = 0;
        foreach (var row in levelDescriptor.rows)
        {
            //In a row
            //Length of the row is equal to the number of comma separated things in it
            if (row.tiles.Length > LWidth)
            {
                LWidth = row.tiles.Length;
            }
        }

        width = LWidth;
        height = LHeight;
        return Task.CompletedTask;
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


   private Task InitLists()
    {
        //Allocate memory
        tileParents = new GameObject[LWidth * LHeight];
        tileTraversible = new bool[LWidth*LHeight];
        _tile3DObjectsBase = new GameObject[LWidth * LHeight];
        _tile3DObjectsDeco = new List<GameObject>[LWidth * LHeight];
        TileDisplays = new TileDisplay[LWidth * LHeight];
        _tileDisplayColliders = new BoxCollider[LWidth*LHeight];
        for (var i = 0; i < LHeight; i++)
        {
            //Make them all empty
            for (int j = 0; j < LWidth; ++j)
            {
                tileTraversible[i * LWidth + j] = false;
                _tile3DObjectsBase[i * LWidth + j] = null;
                _tile3DObjectsDeco[i * LWidth + j] = new List<GameObject>();
                tileParents[i * LWidth + j] = new GameObject($"Tile{i * LWidth + j} {i},{j}");
                tileParents[i * LWidth + j].transform.parent = this.transform;
                tileParents[i * LWidth + j].isStatic = true;
            }
        }

        return Task.CompletedTask;
    }

   public GameObject[] tileParents { get; set; }

   private Task FillTraversibleWithDescriptor()
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
               if (!baseTileDB.GetFromList(TileDescriptor.baseTileDescriptor.name, out curLA))
               {
                   Debug.LogError("Invalid LA name:_" + TileDescriptor.baseTileDescriptor.name 
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

       return Task.CompletedTask;
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
                try
                {
                    var tileDescriptor = levelDescriptor.rows[y].tiles[x];
                    newTile.baseTileDescriptor = tileDescriptor.baseTileDescriptor;
                    // newTile.decorations = tileDescriptor.decorations.ToList();
                    if (tileDescriptor.decorations != null)
                    {
                        newTile.decorations.AddRange(tileDescriptor.decorations);
                    }

                    Tiles[y * LWidth + x] = newTile;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Could not access level descriptor y:{y}, x:{x}");
                    Debug.Log($"Level descriptor total rows: {levelDescriptor.rows.Length}");
                    Debug.Log($"Cells in that row: {levelDescriptor.rows[y].tiles.Length}");
                }
                
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
    
    private void SpawnTileDisplayAt(int index)
    {
        TileDisplays[index] = GameObject.Instantiate(TileDisplayPrefab, 
            CoordToWorld(TileCoordinate.IndexToCoord(index))+ new Vector3(0f,.1f,0f),
            Quaternion.Euler(new Vector3(90f,0f,0f)), parent:this.transform).GetComponent<TileDisplay>();
        TileDisplays[index].transform.name = $"Display {index}";
        TileDisplays[index].transform.SetParent(tileParents[index].transform, true);
        TileDisplays[index].index = index;
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
        Gizmos.DrawCube(new Vector3(_boundsStart.x, 0f, _boundsStart.y), Vector3.one*0.5f);
        Gizmos.color = Color.green;
        Gizmos.DrawCube(new Vector3(_boundsEnd.x, 0f, _boundsEnd.y), Vector3.one *0.5f);
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

    public int currentBuildIndex = 0;
    [ContextMenu("Create Level")]
    public async Task CreateLevel()
    {
        DestroyChildren();
        LoadFromDescriptors();
        await CalculateDimensions();
        await InitLists();
        await FillTraversibleWithDescriptor();
        
        CalculateBounds();
        SetBoxCollider();
        InitTileListsWithDescriptors();
        width = LWidth;
        height = LHeight;
        
        for (int y = 0; y < LHeight; ++y)
        {
            for (int x = 0; x < LWidth; ++x)
            {
                try
                {
                    RefreshTile(x, y);
                    SpawnTileDisplayAt(y * LWidth + x);
                    if (TileDisplays[y * LWidth + x]!=null)
                    {
                        _tileDisplayColliders[y * LWidth + x] =
                            TileDisplays[y * LWidth + x].GetComponent<BoxCollider>();
                    }
                    else
                    {
                        Debug.LogError($"Tile display object at {y*LWidth+x} is null");
                    }
                    currentBuildIndex = y * LWidth + x;
                    // Debug.Log("Building index " + currentBuildIndex);
                }catch (Exception e)
                {
                    Debug.LogError($"Exception occurred at {y},{x}: {e}");
                }


            }
                await Task.Yield();
        }
        Debug.Log("Level generation done");
        await SetAndSpawnCharacters();
        this.TurnOffAllDisplays();
        _levelGenerated = true;
        Lightmapping.BakeAsync();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }
    [ContextMenu("Destroy children")]
    private void DestroyChildren()
    {
        if (tileParents != null)
        {
            foreach (var tileParent in tileParents)
            {
                if (tileParent == null)
                {
                    continue;
                }
                for (var i = tileParent.transform.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(tileParent.transform.GetChild(i).gameObject);
                }
                DestroyImmediate(tileParent);
            }
        }

        characters?.ForEach(descriptor =>
        {
            if (descriptor.character != null)
            {
                DestroyImmediate(descriptor.character.gameObject);
            }
        });
        characters = null;
        //Destroy all children objects
        for (var i = this.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(this.transform.GetChild(i).gameObject);
        }

        _levelGenerated = false;
    }


    //Reads values from the Tile object present at x and y
    //Instantiates debug surface
    //Instantiates 3D models for base and deco
    public void RefreshTile(int x, int y)
    {
        // try
        // {

        if (_tile3DObjectsBase[y * LWidth + x]!=null)
        {
            Debug.Log($"Destroying base tile object at {y},{x}");
            GameObject.DestroyImmediate(_tile3DObjectsBase[y*LWidth+x]);
        }
            // if (_tile3DObjectsDeco[y * LWidth + x] != null)
            // {
            //     foreach (var o in _tile3DObjectsDeco[y * LWidth + x])
            //     {
            //         GameObject.Destroy(o);
            //     }
            // }

        // }
        // catch (Exception e)
        // {
        //     
        //     Debug.LogError("x: " + x + " y: " + y + " index: " + (y * LWidth + x)+  " out of bounds");
        // }

        var newCoord = TileCoordinate.xy(x, y);
        
        Vector3 squarePosition = CoordToWorld(newCoord);
        var tile = Tiles[y*LWidth+x];
        //Spawn base, code block to avoid scoping requiredLA
        Transform baseTransform = null;
        if (baseTileDB.GetFromList(tile.baseTileDescriptor.name, out BaseTileAsset requiredBaseTileAsset))
        {
            if(requiredBaseTileAsset==null){
                Debug.LogError("Base name "+ tile.baseTileDescriptor.name +" not found");
            }
            baseTransform= SpawnBaseAt(y, x, tile.baseTileDescriptor.yRotation, requiredBaseTileAsset, squarePosition);
        }
        {
            foreach (var decoration in tile.decorations)
            {
                if (decoTileDB.GetFromList(decoration.name, out DecoTileAsset RequiredLA))
                {
                    SpawnDecoAt(y,x,decoration.yRotation,RequiredLA, squarePosition, baseTransform);
                    break;       
                }
            }
            
        }

        
    }

    private Transform SpawnBaseAt(int y, int x, float yRotation, BaseTileAsset RequiredLA, Vector3 squarePosition)
    {
        Tiles[y * LWidth + x].heightOffset = RequiredLA.heightOffset;
        var newObj = GameObject.Instantiate(RequiredLA.Asset, squarePosition, Quaternion.identity);
        newObj.transform.rotation = Quaternion.Euler(0.0f,yRotation, 0.0f);
        newObj.transform.SetParent(tileParents[y*LWidth+x].transform, true);
        if (_tile3DObjectsBase[y * LWidth + x] != null)
        {
            GameObject.DestroyImmediate(_tile3DObjectsBase[y*LWidth+x]);
        }
        {
            _tile3DObjectsBase[y * LWidth + x] = newObj;
        }
        return newObj.transform;
    }

    private void SpawnDecoAt(int y, int x, float yRotation, DecoTileAsset RequiredLA, Vector3 squarePosition, Transform parent)
    {
        var newObj = GameObject.Instantiate(RequiredLA.Asset, squarePosition, Quaternion.identity);
        newObj.transform.rotation = Quaternion.Euler(0.0f, yRotation, 0.0f);
        newObj.transform.SetParent(parent, true);
        _tile3DObjectsDeco[y * LWidth + x].Add(newObj);
    }


    private void RefreshTile(int index)
    {
        var coord = TileCoordinate.IndexToCoord(index);
        RefreshTile(coord.x, coord.y);
    }
    

    #endregion

    
    #region Tile selection

    public void TurnOffAllDisplays()
    {
        // Debug.Log("Turning off displays");

        foreach (var tileDisplay in TileDisplays)
        {
            tileDisplay.gameObject.SetActive(false);
        }
    }
    public void TurnOffAllColliders()
    {
        // Debug.Log("Turning off displays");

        foreach (var tileCollider in _tileDisplayColliders)
        {
            if (tileCollider != null)
            {
                tileCollider.enabled = false;
                
            }
        }
    }
    [ContextMenu("Toggle displays")]
    public void ToggleDisplays()
    {
        foreach (var tileDisplay in TileDisplays)
        {
            tileDisplay.gameObject.SetActive(!tileDisplay.gameObject.activeInHierarchy);
        }
    }
    public void TurnOnAllDisplays()
    {
        Debug.Log("Turning on displays");
        foreach (var tileDisplay in TileDisplays)
        {
            tileDisplay.gameObject.SetActive(true);
        }
    }

    public void ChangeTileDisplayStateWithPredicate(Func<Tile, bool> pred, bool state)
    {
        foreach (var tileDisplay in TileDisplays)
        {
            if (pred(Tiles[tileDisplay.index]))
            {
                tileDisplay.gameObject.SetActive(state);
            }
        }
    }
    public void ChangeTileSelectionColliderState(Func<Tile, bool> pred, bool state)
    {
        for (int i = 0; i < Tiles.Length; i++)
        {
            if (pred(Tiles[i]) == true)
            {
                _tileDisplayColliders[i].enabled = state;
            }
        }
    }

    public void TurnOffSelectionColliderAt(int index)
    {
        if (isLocationValid(index))
        {
            _tileDisplayColliders[index].enabled = false;
        }
    }
    public void TurnOnSelectionColliderAt(int index)
    {
        if (isLocationValid(index))
        {
            _tileDisplayColliders[index].enabled = true;
        }
    }
    public void ChangeTileDisplaySelectionState(int[] locations, TileDisplay.State newState)
    {
        foreach (var location in locations)
        {
            TileDisplays[location].setState(newState);
        }
    }
    public void ChangeTileDisplayActivationState(int[] locations, bool newState)
    {
        foreach (var location in locations)
        {
            TileDisplays[location].gameObject.SetActive(newState);
        }
    }
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
            return GetCharacterOnTile(index)!= null;
        }
        public void SelectCharacter(int index)
        {
            isACharacterSelected = true;
            selectedCharacter = GetCharacterOnTile(index);
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

    //Get the character on a provided tile index
    //Get Character On tile
    public SimpleCharacter GetCharacterOnTile(int index)
    {
        return characters.Find(ch => ch.character.MyTile.tileKey == index).character;
    }
    public UnityEvent<int, SimpleCharacter> CharacterChangedTile;
    public void ChangeCharacterTile(SimpleCharacter character, int newTileIndex)
    {
        character.MyTile.CharacterExit(character);
        character.MyTile = Tiles[newTileIndex];
        character.MyTile.CharacterEnter(character);
        CharacterChangedTile.Invoke(newTileIndex, character);
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
        Tiles[index].baseTileDescriptor.name = newBase;
        RefreshTile(index%LWidth, index/LWidth);
        TileChanged.Invoke(index);
        TileDisplays[index].RefreshDisplay();
    }

    public void ChangeTileRotation(int index, float newY)
    {
        Tiles[index].baseTileDescriptor.yRotation = newY;
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
    [SerializeField] private int height;
    [SerializeField] private int width;

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

    public int ManhattanDistance(int a, int b)
    {
        var aTileCoordinate = TileCoordinate.IndexToCoord(a);
        var bTileCoordinate = TileCoordinate.IndexToCoord(b);
        var distance = TileCoordinate.manhattan(aTileCoordinate,bTileCoordinate);
        return distance;
    }
    
    public int GetPathDistance(int a, int b, int maxTiles =0)
    {
        var path = FindPath(a, b, maxTiles);
        if (path == null)
        {
            return Int32.MaxValue;
        }
        else
        {
            return path.Count;
        }
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
            // Debug.Log("No valid path");
            return null;
        }
        return pathFinder.PathLocs.Take(maxTiles).ToList();
    }

    #endregion
    
}
