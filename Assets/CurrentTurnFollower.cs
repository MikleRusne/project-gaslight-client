using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentTurnFollower : MonoBehaviour
{
    public static Vector3 offset;
    public Transform target;
    //TODO: Add animations, such as enable/disable transitions, and rotation when they're active or something
    void Update()
    {
        if(target!=null)
        this.transform.position = target.position + offset;
    }
}
