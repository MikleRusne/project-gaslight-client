using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCubeRenamer : MonoBehaviour
{
    void RenameCubes(GameObject overlook){
        GameObject[] cubes;
        cubes = GameObject.FindGameObjectsWithTag("Tile");
        int i = 1;
        foreach(GameObject cube in cubes){
            if (cube != overlook){
                cube.name = "Cube" + string.Format("{0:0}",i++);
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
