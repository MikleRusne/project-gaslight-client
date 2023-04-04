using System;
using System.Collections;
using System.Collections.Generic;
using Tiles;
using UnityEngine;

[ExecuteInEditMode]
public class DiscoverBlocks : MonoBehaviour
{

    // [Range(0.001f,30.0f)] 
    // public float TraceDistance = 5.0f;
    // // bool HitDetected= false;
    
    public Color FrontColor;
    public Color BackColor;
    public Color LeftColor;
    public Color RightColor;

    private float TraceDistance;
    private BlockLinks m_BlockLinks;
    Vector3 forward  ;
    Vector3 back;
    Vector3 left ;
    Vector3 right;
    Mesh qoob;

    private Dictionary<string, (bool wasHit, Vector3 hitLocation)> Hits = new Dictionary<string, (bool wasHit, Vector3 hitLocation)>();
    void Start()
    {
        m_BlockLinks = this.GetComponent<BlockLinks>();
        TraceDistance = m_BlockLinks.TraceDistance;
        forward = transform.forward;
        back= -forward;
        left = Vector3.left;
        right= -left;
        Hits = new Dictionary<string, (bool wasHit, Vector3 hitLocation)>
        {
            {"left", (false, Vector3.zero)},
            {"right", (false, Vector3.zero)},
            {"forward", (false, Vector3.zero)},
            {"down", (false, Vector3.zero)}
        };
    }

    // Update is called once per frame
    void Update()
    {
        TraceDistance = m_BlockLinks.TraceDistance;
        RaycastHit hit;
        bool LeftHitDetected = Physics.Linecast(this.transform.position + left,this.transform.position + left*TraceDistance, out hit);
        if (LeftHitDetected)
        {
            Hits["left"]=  (true, hit.point); 
        }
        else
        {
            Hits["left"] = (false, Vector3.zero);
        }
        bool rightHitDetected = Physics.Linecast(this.transform.position + right,this.transform.position + right*TraceDistance, out hit);
        if (rightHitDetected)
        {
            Hits["right"]=  (rightHitDetected, hit.point);
            
            // Hits.Add("right" , (rightHitDetected, Vector3.zero));
        }
        else
        {
            Hits["right"] = (rightHitDetected, Vector3.zero);
        }
        // Debug.DrawBox(this.transform.position + forward,this.transform.position + forward*TraceDistance, Color.black);
        bool forwardHitDetected = Physics.Linecast(this.transform.position + forward,this.transform.position + forward*TraceDistance, out hit);
        if (forwardHitDetected)
        {
            Hits["forward"] = (forwardHitDetected, hit.point);
        }
        else
        {
            Hits["forward"] = (forwardHitDetected, Vector3.zero);
        }
        bool backHitDetected = Physics.Linecast(this.transform.position + back,this.transform.position + back*TraceDistance, out hit);
        if (backHitDetected)
        {
            Hits["back"]=  (backHitDetected, hit.point);
            
        }
        else
        {
            Hits["back"] = (backHitDetected, Vector3.zero);
        }
    }

    void OnDrawGizmos()
    {
        float size = 0.5f;
        if(Hits.ContainsKey("left") & Hits["left"].wasHit){
            Gizmos.color = LeftColor;
            Gizmos.DrawCube(Hits["left"].hitLocation, new Vector3(size,size,size));
        }
        if(Hits.ContainsKey("right") & Hits["right"].wasHit){
            Gizmos.color = RightColor;
            Gizmos.DrawCube(Hits["right"].hitLocation, new Vector3(size,size,size));
        }
        if(Hits.ContainsKey("forward") & Hits["forward"].wasHit){
            Gizmos.color = FrontColor;
            Gizmos.DrawCube(Hits["forward"].hitLocation, new Vector3(size,size,size));
        }
        if(Hits.ContainsKey("back") & Hits["back"].wasHit){
            Gizmos.color = BackColor;
            Gizmos.DrawCube(Hits["back"].hitLocation, new Vector3(size,size,size));
        }

        

    }
}
