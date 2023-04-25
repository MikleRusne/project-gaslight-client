using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Cinemachine;
using UnityEditor.Search;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class CameraController : MonoBehaviour
{
    [SerializeField]private float EdgeThreshold = default;

    [SerializeField] public Transform player = default;
    [FormerlySerializedAs("follow")] public bool _follow = true;
    public Vector2 previousMousePosition = Vector2.zero;

    public Transform _camTransform = default;
    public Vector2 _boundsStart;
    public Vector2 _boundsEnd;

    public float minDistance= default;
    public float maxDistance= default;
    public float currentDistance = default;

    public float zoomSpeed = default;
    public float zoomAmount = 0f;
    [SerializeField] public bool _doZoom;

    public void HandleZoom(InputAction.CallbackContext ctx)
    {
        float inputAmount = ctx.ReadValue<float>();
        if (ctx.performed)
        {
            _doZoom = true;
            zoomAmount = inputAmount * zoomSpeed;
        }

        if (ctx.canceled)
        {
            
        }
    }
    void Awake()
    {
    }
    void Start()
    {
        
        previousMousePosition = Input.mousePosition;
        this._boundsStart = Level.instance._boundsStart;
        this._boundsEnd = Level.instance._boundsEnd;
        if(player!=null){
        
        this.transform.position = player.position;
        this.transform.rotation = player.rotation;
        }
        // VCamDefaultZOffset = _Cam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.z;
        newPosition = transform.position;
        _doPan = false;
        _newRotation = transform.rotation;
        _doOrbit = false;
    }

    [Range(0.01f, 1.0f)] public float followTransitionDuration; 
    [FormerlySerializedAs("isInFollowTransition")] public bool _isInFollowTransition = false;

    public async Task LerpToTarget(Vector3 targetPosition)
    {
        _isInFollowTransition = true;
        float myTime = 0f;
        while (myTime < followTransitionDuration)
        {
            float value = myTime / followTransitionDuration;
            this.transform.position = Vector3.Lerp(this.transform.position, targetPosition, value);
            myTime += Time.deltaTime;
            await Task.Yield();
        }
        this.transform.position = targetPosition;
        _isInFollowTransition = false;
    }
    public async Task Follow(Transform target)
    {
        this.player = target;
        //Lerp to that position in an async manner
        await LerpToTarget(player.position);
        _follow = true;
    }

    public void Unfollow()
    {
        this.player = null;
        _follow = false;
    }
    public float followLerpDistanceThreshold = 0.2f;
    [Range(0,5)]public float _movementSpeed=10f;
    public Vector3 newPosition;
    [FormerlySerializedAs("_input")] public Vector2 _panInput;

    
    public bool _doPan = false;
    public void HandlePanInput(InputAction.CallbackContext ctx)
    {
        _panInput = ctx.ReadValue<Vector2>();
        
        if (ctx.performed)
        {
            _doPan = true;
        }

        if (ctx.canceled)
        {
            _doPan = false;
        }
        

    }

    public bool _doOrbit = false;
    public Vector2 _orbitInput;
    [FormerlySerializedAs("newRotation")] public Quaternion _newRotation;
    [Range(0f,30f)] public float _rotationSpeed = 1f;

    public float camXRotation;
    public float camXRotationMin = default;
    public float camXRotationMax = default;
    public void HandleOrbitInput(InputAction.CallbackContext ctx)
    {
        _orbitInput = ctx.ReadValue<Vector2>();
        if (ctx.performed)
        {
            _doOrbit = true;
        }

        if (ctx.canceled)
        {
            _doOrbit = false;
        }
    }

    public float _mouseOrbitSpeed = 1f;
    public float _mouseOrbitDeadzone = 0.001f;
    public Vector2 newMousePosition;
    public void HandleMouseOrbit()
    {
        if (_doMouseOrbit)
        {
            newMousePosition = Input.mousePosition;
            var displacement = newMousePosition - previousMousePosition;
            var distance = displacement.sqrMagnitude;
            //Get previous mouse position
            if ( distance>_mouseOrbitDeadzone)
            {
                _doOrbit = true;
                _orbitInput.x = displacement.x;
            }
            else
            {
                _doOrbit = false;
                _orbitInput = Vector2.zero;
            }
            previousMousePosition = newMousePosition;
            
        }
    }

    public bool _doMouseOrbit = false;

    public void HandleAllowOrbitInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            _doMouseOrbit = true;
        }

        if (ctx.canceled)
        {
            _doMouseOrbit = false;
            _orbitInput = Vector2.zero;
        }
    }
    void Orbit(Vector2 direction)
    {
        // camXRotation = _camTransform.rotation.x;
        _newRotation *= Quaternion.Euler(Vector3.up * direction.x);
        // _newRotation *= Quaternion.Euler(Vector3.right * direction.y);
        // var newCamRotationX = camXRotation + direction.y;
        // newCamRotationX = Mathf.Clamp(newCamRotationX, camXRotationMin, camXRotationMax);
        // var newCamRotation = Quaternion.Euler(newCamRotationX,_camTransform.rotation.y, _camTransform.rotation.z);
        // _camTransform.localRotation =
            // Quaternion.Lerp(_camTransform.localRotation, newCamRotation, Time.deltaTime * _rotationSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, _newRotation, Time.deltaTime * _rotationSpeed);
        _newRotation = transform.rotation;
    }
    void Update()
    {
        

        if (!_isInFollowTransition)
        {
            if (_follow)
            {
                this.transform.position = player.position;
            }
            else
            {
                if (_doMouseOrbit)
                {
                    HandleMouseOrbit();
                }
                if (_doPan)
                {
                    Pan(_panInput);
                }
            }
        }
        if (_doOrbit)
        {
            Orbit(_orbitInput);
        }

        if (_doZoom)
        {
            Zoom(zoomAmount);
        }
    }

    private void Zoom(float f)
    {
        
    }

    void Pan(Vector2 direction)
    {

        newPosition += transform.right * (direction.x * _movementSpeed);
        newPosition += transform.forward * (direction.y * _movementSpeed);
        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * _movementSpeed);
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, _boundsStart.x, _boundsEnd.x),
            transform.position.y,
            Mathf.Clamp(transform.position.z, _boundsStart.y, _boundsEnd.y)
        );
        newPosition = transform.position;
    }

    public void SetFocusTarget(Transform target)
    {
        this.player = target;
    }
    
    public void Refocus(InputAction.CallbackContext ctx)
    {
        if (ctx.canceled)
        {
            if (player != null)
            {
                if (!_isInFollowTransition)
                {
                    LerpToTarget(player.position);
                }
            }
        }
    }






}
