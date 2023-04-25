using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Cinemachine;
using UnityEditor.Search;
using UnityEngine.InputSystem;

public class OldCameraController : MonoBehaviour
{
    
    [SerializeField] private CinemachineVirtualCamera _Cam = default;

    [SerializeField] private float MoveSpeed = 1f;
    [SerializeField]private float EdgeThreshold = default;
    [SerializeField]private float minFOV = 40f;
    [SerializeField]private float maxFOV = 90f;
    [SerializeField] private float zoomSpeed = 1f;
    [SerializeField]public float SnapOrbitDeadZone;
    private int SnapState;
    public float TimeSinceLastSnap = 0f;
    public float SnapDelay = default;

    [SerializeField, Range(0.01f, 1f)] private float rotationSpeed = 1f;
    [SerializeField] public Transform player = default;
    public bool follow = true;
    private Vector2 previousMousePosition = Vector2.zero;
    
    void Start()
    {
        this.transform.position = player.position;
        this.transform.rotation = player.rotation;
        VCamDefaultZOffset = _Cam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.z;
    }

    [Range(0.01f, 1.0f)] public float followTransitionDuration; 
    public bool isInFollowTransition = false;

    public async Task LerpToTarget()
    {
        float myTime = 0f;
        while (myTime < followTransitionDuration)
        {
            float value = myTime / followTransitionDuration;
            this.transform.position = Vector3.Lerp(this.transform.position, this.player.position, value);
            myTime += Time.deltaTime;
            await Task.Yield();
        }
        this.transform.position = this.player.position;
    }
    public async Task Follow(Transform target)
    {
        isInFollowTransition = true;
        this.player = target;
        //Lerp to that position in an async manner
        await LerpToTarget();
        isInFollowTransition = false;
    }

    public float followLerpDistanceThreshold = 0.2f;
    // Update is called once per frame
    Vector3 MoveDir;
    private float VerticalMovement;
    private float yRotateDir;
    private float zRotateDir;
    private float FOVDelta;
    void Update()
    {
        if (isInFollowTransition)
        {
            return;
        }
        if (follow)
        {
            this.transform.position = player.position;
            return;
        }
        MoveDir = Vector3.zero;
        yRotateDir = 0f;
        VerticalMovement = 0f;
        FOVDelta = 0f;
        //Movement
        HandleKBInput();
        // HandleEdgeScroll();
        HandlePan();
        
        //Rotation
        // HandleQEOrbit();
        // HandleMouseOrbit();
        // HandleMouseSnapOrbit();
        MoveDir = MoveDir.normalized;
        VerticalMovement *= Time.deltaTime * rotationSpeed;
        var VerticalPosition = transform.position.y + VerticalMovement;
        
        var MovementVector = (transform.forward*MoveDir.z + transform.right * MoveDir.x) * (MoveSpeed * Time.deltaTime);
        //XZ movement
        transform.position += new Vector3(MovementVector.x,
            VerticalPosition,
            MovementVector.z);
        
        transform.position = new Vector3(transform.position.x, Mathf.Clamp(VerticalPosition, HeightMin, HeightMax), transform.position.z);

        // yRotateDir *= rotationSpeed;
        // transform.eulerAngles += new Vector3(0f, yRotateDir, 0f);
        HandleScrollZoom();
        
        // ChangeFOV
        FOVDelta *= zoomSpeed;
        var newFOV = Mathf.Clamp(_Cam.m_Lens.FieldOfView+(FOVDelta* Time.deltaTime), minFOV, maxFOV) ;
        // newFOV = Mathf.Lerp(_Cam.m_Lens.FieldOfView, newFOV, Time.deltaTime);
        _Cam.m_Lens.FieldOfView = newFOV;
        // Debug.Log(_Cam.m_Lens.FieldOfView);
        HandleFocusOnPlayer();
        
        previousMousePosition = Input.mousePosition;
        if (SnapState == 0)
        {
            TimeSinceLastSnap += Time.deltaTime;
        }else if (TimeSinceLastSnap > SnapDelay)
        {
            if (SnapState == 1)
            {
                transform.eulerAngles += new Vector3(0f, 15f, 0f);
            }
            else
            {
                transform.eulerAngles += new Vector3(0f, -15f, 0f);
            }
            
            TimeSinceLastSnap =0f;
        }

        HandleOrtho(Time.deltaTime);
    }

    public float HeightMin { get; set; }
    public float HeightMax { get; set; }

    private float VCamDefaultZOffset = default;
    public void ToggleOrtho()
    {
        orthoView = !orthoView;
    }

    public void SetOrtho(bool targ)
    {
        orthoView = targ;
    }
    public bool orthoView = false;
    [SerializeField,Range(0.001f,10f)] public float orthoToggleSpeed = 0.0f; 
    void HandleOrtho(float deltaTime)
    {
        //If orthoview is true, then lerp the cinemachine follow z to 0
        if (orthoView)
        {
            Vector3 curOffset = _Cam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset;
            float newZ = Mathf.Lerp(curOffset.z, 0, deltaTime*orthoToggleSpeed);
            _Cam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset =
                new Vector3(curOffset.x, curOffset.y, newZ);
        }
        else
        {
            Vector3 curOffset = _Cam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset;
            float newZ = Mathf.Lerp(curOffset.z, VCamDefaultZOffset, deltaTime*orthoToggleSpeed);
            _Cam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset =
                new Vector3(curOffset.x, curOffset.y, newZ);
        }
    }

    private void HandleFocusOnPlayer()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            transform.position = player.position;
        }
    }

    private void HandleQEOrbit()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            yRotateDir -= rotationSpeed ;
        }
        if (Input.GetKey(KeyCode.E))
        {
            yRotateDir += rotationSpeed;
        }
    }

    private void HandleMouseSnapOrbit()
    {
        SnapState = 0;
        if (Input.GetMouseButton(1))
        {
            var MousePositionDelta = previousMousePosition - (Vector2)Input.mousePosition;
            //Use the y to orbit
            // VerticalMovement = MousePositionDelta.y * MoveSpeed/2;
            if (Mathf.Abs(MousePositionDelta.x) < SnapOrbitDeadZone)
            {
                Debug.Log("Ignored: " + MousePositionDelta.x);
                SnapState = 0;
            }
            else
            {
                if (MousePositionDelta.x > 0)
                {
                    SnapState = 1;
                }
                else
                {
                    SnapState = -1;
                }
            }

        }
    }


    private void HandleMouseOrbit(){
        if (Input.GetMouseButtonDown(2))
        {
            previousMousePosition = Input.mousePosition;
        }
        // Debug.Log("Orbitin");
        var MousePositionDelta = previousMousePosition - (Vector2)Input.mousePosition;
        //Use the y to orbit
        VerticalMovement = MousePositionDelta.y * MoveSpeed/2;
        yRotateDir = MousePositionDelta.x * rotationSpeed;
        previousMousePosition = Input.mousePosition;
    }

    private bool panUp, panDown, panLeft, panRight = false;
    void HandleKBInput()
    {
        if (panUp) MoveDir.z += 1f;
        if (panDown) MoveDir.z -= 1f;
        if (panLeft)
        {
            MoveDir.x -= 1f;
            
        }
        if (panRight) MoveDir.x += 1f;
    }

    public void Pan(InputAction.CallbackContext ctx)
    {
        // Debug.Log("Panning");   
        var delta = ctx.ReadValue<Vector2>();
        var movementVector = new Vector3(delta.x, 0f, delta.y);
        if (delta.x > 0.1)
        {
            panRight = true;
            panLeft = false;
            // Debug.Log("Panning right");
        }
        else if (delta.x<-0.1)
        {
            panLeft = true;
            panRight = false;
            // Debug.Log("Panning left");
        }
        else
        {
            panLeft = false;
            panRight = false;
        }
        
        if (delta.y > 0.1)
        {
            panUp = true;
            panDown = false;
            // Debug.Log("Panning right");
        }
        else if (delta.y<-0.1)
        {
            panDown = true;
            panUp = false;
            // Debug.Log("Panning left");
        }
        else
        {
            panDown = false;
            panUp = false;
        }
    }
    void HandleEdgeScroll()
    {
        Vector2 MousePosition = Input.mousePosition;
        if (MousePosition.x < EdgeThreshold)
        {
            MoveDir.x -= 1f;
        }

        if (MousePosition.x > (Screen.width - EdgeThreshold))
        {
            MoveDir.x += 1f;
        }
        
        if (MousePosition.y < EdgeThreshold)
        {
            MoveDir.z -= 1f;
        }

        if (MousePosition.y > (Screen.height - EdgeThreshold))
        {
            MoveDir.z += 1f;
        }
    }

    void HandlePan()
    {
        if (Input.GetMouseButtonDown(1))
        {
            previousMousePosition = Input.mousePosition;
        }
        if (!Input.GetMouseButton(1))
        {
            return;
        }
        var MousePositionDelta = previousMousePosition - (Vector2) Input.mousePosition;
        previousMousePosition = (Vector2)Input.mousePosition;
        MoveDir.x = MousePositionDelta.x;
        MoveDir.z = MousePositionDelta.y;
    }

    void HandleScrollZoom()
    {
        var mouseDelta = Input.mouseScrollDelta.y;
        // Debug.Log(mouseDelta);
        FOVDelta -= mouseDelta;
    }
}
