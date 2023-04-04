using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HeadSmacker : MonoBehaviour
{
    void Awake()
    {
        GetComponent<PlayerInput>().enabled = false;
        GetComponent<PlayerInput>().enabled = true;
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
