using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class BlockNamer : MonoBehaviour
{

    void RenameTiles(GameObject overlook){
        GameObject[] tiles;
        tiles = GameObject.FindGameObjectsWithTag("Tile");
        int i = 1;
        foreach(GameObject tile in tiles){
            if (tile != overlook){
                tile.name = "Tile" + string.Format("{0:0}",i);
                ++i;
            }
        }
    }

    void OnDestory(){
        RenameTiles(this.gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        RenameTiles(null);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
