using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardInputManager : InputManager
{
    public static event RotateInputHandler OnRotateInput;
    public static event ZoomInputHandler OnZoomInput;
        
    void Update()
    {


        if (Input.GetKey(KeyCode.E))
        {
            OnRotateInput?.Invoke(-1.0f);
   
        }

        if (Input.GetKey(KeyCode.Q))
        {
            OnRotateInput?.Invoke(1.0f);

            
        }
        
        if (Input.GetKey(KeyCode.Z))
        {
            OnZoomInput?.Invoke(-1.0f);
   
        }

        if (Input.GetKey(KeyCode.X))
        {
            OnZoomInput?.Invoke(1.0f);

            
        }

    }
}
