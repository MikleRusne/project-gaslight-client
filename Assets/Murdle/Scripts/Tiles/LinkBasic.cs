using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tiles
{
    [ExecuteInEditMode]
    public class LinkBasic : MonoBehaviour
    {
        public GameObject Block1;
        public GameObject Block2;

        public float xOffset;
        public Vector3 Scale = new Vector3(1,1,1);

        GameObject DisplayCube;

        void Start()
        {
            // DisplayCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            // DisplayCube = this.transform.GetChild(0).GameObject;
            // if(Block1)        this.transform.parent = Block1.transform;
            this.transform.localScale = Scale;
        }

        // Update is called once per frame
        void Update()
        {
            if(transform.hasChanged){
                if(Block1 && Block2){
                    //Debug.DrawLine(Block1.transform.position, Block2.transform.position, Color.red,0.01f);
                    // DisplayMesh.transform.parent = this.transform;
                    this.transform.position =(Block1.transform.position + Block2.transform.position)/2;
                    this.transform.LookAt(Block2.transform.position, Vector3.up);
                    float xScale = Vector3.Distance(Block1.transform.position, Block2.transform.position)/2.0f  + xOffset;
                    // this.transform.localScale = new Vector3(1, 1,xScale);
                    Vector3 newScale = new Vector3(this.transform.localScale.x,this.transform.localScale.y, xScale);
                    this.transform.localScale = newScale;
                
                
                    //TODO
                    //Add functionality to make it so it aligns at right angle to neighbours, might get fucky over time so not bothering with it for now
                }else{
                    // DisplayCube.SetActive(false);
                }
            }
        }
    }

}
