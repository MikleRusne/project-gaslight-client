using System;
using System.Collections;
using System.Collections.Generic;
using Characters;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles
{
    public class BlockLinks : MonoBehaviour
    {

        public bool isOccupied;
        public Character OccupiedBy;
    public enum ELinkDirection
    {
        Left,
        Right,
        Up,
        Down
    }

   

    // public static Array directionenums = Enum.GetValues(typeof(ELinkDirection));
    
    public Dictionary<ELinkDirection, GameObject> LinkPrefabs = new Dictionary<ELinkDirection, GameObject>();

    public Dictionary<ELinkDirection, TextMesh> TextMeshes = new Dictionary<ELinkDirection, TextMesh>();
    public Dictionary<ELinkDirection, Spawner.FInstantiatedTile> Neighbors =
        new Dictionary<ELinkDirection, Spawner.FInstantiatedTile>();

    public Dictionary<ELinkDirection, bool> connected =
        new Dictionary<ELinkDirection, bool>()
        {
            { ELinkDirection.Left , false},
            {ELinkDirection.Right, false},
            { ELinkDirection.Down , false},
            {ELinkDirection.Up, false}
        };

    public (int y, int x) MyID;
    public Vector3 CharacterPosition;


    public GameObject[] GetNeighbors
    {
        get
        {
            //Make an array of all Tile neighbors
            var NeighborList = new List<GameObject>();
            foreach(ELinkDirection direction in Enum.GetValues(typeof(ELinkDirection)))
            {
                if (Neighbors[direction].Type == Spawner.ETileType.Tile)
                {
                    NeighborList.Add(Neighbors[direction].Tile);
                }
            }

            return NeighborList.ToArray();
        }
    }

    Vector3 forward  ;
    Vector3 back;
    Vector3 left ;
    Vector3 right;


    
    [Range(0, 10f)]
    public float LinkOffset =4.1f;

    [SerializeField] public GameObject LinkPrefab;
    
    public float TraceDistance;

    private Dictionary<ELinkDirection, Vector3> Directions = new Dictionary<ELinkDirection, Vector3>
    {
        {ELinkDirection.Up, new Vector3(0,0,1)},
        {ELinkDirection.Down, new Vector3(0,0,-1)},
        {ELinkDirection.Left, new Vector3(-1,0,0)},
        {ELinkDirection.Right, new Vector3(1,0,0)}
    };

    void Awake()
    {
        CharacterPosition = transform.Find("ChLocation").transform.position;
    }
    
    
    void Start()
    {

        foreach (ELinkDirection direction in Enum.GetValues(typeof(ELinkDirection)))
        {
            var nLinkPrefab =Instantiate(LinkPrefab,this.transform.position + CalculateLinkPosition(direction), Quaternion.identity);
            // nLinkPrefab.transform.localPosition = CalculateLinkPosition(direction);
            LinkPrefabs[direction] = nLinkPrefab;
            
            nLinkPrefab.transform.name = this.name + "in" + direction.ToString();
            nLinkPrefab.transform.parent = this.transform;
            nLinkPrefab.SetActive(false);
            // TextMeshes[direction] = transform.Find(direction.ToString() + "Neighbour").GetComponent<TextMesh>();
            // TextMeshes[direction].text = "lmao";

        }
        VerifyLinks();
    }


    public bool ConLeft;
    public bool ConRight;
    public bool ConUp;
    public bool ConDown;

    Vector3 CalculateLinkPosition(ELinkDirection direction)
    {
        return Directions[direction]*LinkOffset + new Vector3(0,1f,0)*0.3f;
    }
    public void VerifyLinks()
    {
        //Check each neighbor, if it is tile, then turn on linkprefabs
        //Somehow rotate it towards the direction they're pointing in
        //Should happen out of frame so, easy just use point to
        foreach(ELinkDirection direction in Enum.GetValues(typeof(ELinkDirection)))
        {
            if (connected[direction] && Neighbors[direction].Type == Spawner.ETileType.Tile)
            {
                // Debug.Log("Setting " + direction.ToString() + " to true");
                LinkPrefabs[direction].SetActive(true);
                LinkPrefabs[direction].transform.localPosition = CalculateLinkPosition(direction);
                LinkPrefabs[direction].transform.LookAt(Neighbors[direction].Tile.transform.position);

            }
            else
            {
                // Debug.Log("Setting " + direction.ToString() + " to false");
                LinkPrefabs[direction].SetActive(false);
                
            }
        }
        
        
    }
    
    
    void Update()
    {
        
        // VerifyLinks();
    }

    public static string LD2S(ELinkDirection direction)
    {
        switch (direction)
        {
            case ELinkDirection.Left:
                return "Left";
            case ELinkDirection.Right:
                return "Right";
            case ELinkDirection.Up:
                return "Up";
            case ELinkDirection.Down:
                return "Down";
        }

        return null;
    }

    public string DisplayNeighbors()
    {
        // Debug.Log("Displauy neighbors called on " + transform.name);
        List<string> temp = new List<string>();
        foreach (ELinkDirection direction in Enum.GetValues(typeof(ELinkDirection)))
        {
            if (connected[direction])
            {
                string temp2 = direction.ToString() + " neighbor is of type " + Neighbors[direction].Type +
                               " and has name " + Neighbors[direction].Tile.transform.name;
                temp.Add(temp2);
                // Debug.Log(temp2);
            }
        }

        return String.Join("\n ", temp);
    }




}
    
}

