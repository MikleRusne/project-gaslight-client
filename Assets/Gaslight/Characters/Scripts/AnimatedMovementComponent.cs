using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Characters;
using Tiles;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Animator), typeof(Character))]
public class AnimatedMovementComponent : MovementComponent 
{
    public Vector3 _TargetPosition;

    public Animator _animator;

    public List<int> path;

    public Character _character = default;
    public void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    //TODO: Complete this
    //Only rotates between adjacent tiles, keep in mind
    public async Task RotateToTargetUsingLevelDirection(int start, int end)
    {
        //Ask the level what is the direction between the two
        var direction = Level.instance.DirectionFrom(start, end);
        if (direction == null)
        {
            Debug.Log("Same tile this should not be happening");
            return;
        }
        Debug.Log(
            $"Direction from {TileCoordinate.IndexToCoord(start)} to {TileCoordinate.IndexToCoord(end)} is {direction}");
        
        //Play an animation to turn the character towards that direction somehow.
        var steps = Level.instance.StepsBetween(_character._facingDirection, direction.Value);
        if (steps == 0)
        {
            //Don't need to turn
            Debug.Log("Don't need to turn");
            return;
        }
        if (steps == 1)
        {
            Debug.Log("Turning left");
            _animator.SetTrigger("TurnLeft");
        }
        if (steps == -1)
        {
            Debug.Log("Turning right");
            _animator.SetTrigger("TurnRight");
        }
        if (steps == 2)
        {
            Debug.Log("Turning backwards");
            _animator.SetTrigger("TurnBackward");
        }
        Quaternion targetRotation= Quaternion.Euler(0.0f, Level.instance.RotationBetween(
            direction.Value), 0.0f);
        
        // stop = false;
        float mytime = 0;
        while (mytime <= _rotationDuration)
        {
            float value = _rotationCurve.Evaluate(mytime/MovementDuration);
            mytime += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(_character.transform.rotation, targetRotation, value);
            await Task.Yield();
        }
        _character.transform.rotation = targetRotation;
        _animator.ResetTrigger("TurnRight");
        _animator.ResetTrigger("TurnLeft");
        _animator.ResetTrigger("TurnBackward");

        //Face the character in that direction
        if (_character != null)
        {
            _character._facingDirection = direction.Value;
        }
    }
    public async Task RotateToTarget(Vector3 StartPosition, Vector3 TargetPosition)
    {
        // Quaternion TargetRotation = Quaternion.LookRotation(StartPosition - TargetPosition, Vector3.up);
        // TargetRotation = Quaternion.Euler(0, TargetRotation.eulerAngles.y, 0);
        //
        // float factor = 0;
        // //Rotate towards target at a certain speed
        // Vector3 tF = this.transform.up;
        // Vector3 distance = TargetPosition - StartPosition;
        // float dot = Vector3.Dot(tF, distance);
        // float angle = Mathf.Acos(dot / (tF.magnitude * distance.magnitude)) *Mathf.Rad2Deg -90;
        // Vector3 Cross = Vector3.Cross(StartPosition, TargetPosition);
        //
        // float difference = Quaternion.Angle(StartRotation, TargetRotation);
        //
        //
        // // if (angle <= _AngleThreshold)
        // // {
        // //     // Debug.Log("Angle too small");
        // //     transform.rotation = TargetRotation;
        // //     return;
        // // }
        //
        // float step = 0;
        // float FullSpeed = difference*RotationSpeed;
        // while (factor<=1)
        // {
        //     factor += FullSpeed*Time.deltaTime;
        //     step += RotationSpeed*Time.deltaTime;
        //     transform.rotation = Quaternion.RotateTowards(_StartRotation, TargetRotation, factor);
        //     await Task.Yield();
        // }
        //
        transform.LookAt(_TargetPosition);

    }

    public bool stopping = false;
    async Task MoveTarget(bool stop)
    {
        // stop = false;
        float mytime = 0;
        _animator.ResetTrigger(StopWalkID);
        _animator.SetTrigger("Walk");
        while (mytime <= MovementDuration)
        {
            if (stop)
            {
                float value = StopMovementCurve.Evaluate(mytime/MovementDuration);
                transform.position = Vector3.Lerp(_StartPosition, _TargetPosition, value);
                mytime += Time.deltaTime;
                await Task.Yield();
            }
            else
            {
                float value = RegularMovementCurve.Evaluate(mytime/MovementDuration);
                mytime += Time.deltaTime;
                transform.position = Vector3.Lerp(_StartPosition, _TargetPosition, value);
                await Task.Yield();
            }
        }
        transform.position = _TargetPosition;
        _animator.ResetTrigger("Walk");
        _animator.SetTrigger(StopWalkID);
    }

    public AnimationCurve StopMovementCurve;
    public AnimationCurve RegularMovementCurve;
    // public [Range(1f, 5f)] private float MovementSpeed;
    public float MovementDuration;
    private static readonly int StopWalkID = Animator.StringToHash("Stop walk");
    [SerializeField] private AnimationCurve _rotationCurve;
    [SerializeField] private float _rotationDuration;

    public override async Task StartMoving(List<int> Path)
    {
        if (!isMoving)
        {
            isMoving = true;
            this.path = Path;
            foreach (var tuple in path.Skip(1).Select((value,index)=>(value, index)))
            {
                // Debug.Log("Movement component: Moving to "+ index);
                _StartPosition = this.transform.position;
                _TargetPosition = Level.instance.CoordToWorld(tuple.value);
                bool stop = false;
                
                if (tuple.index == path.Count - 1)
                {
                    Debug.Log("At the second last tile so we should stop");
                    stop = true;
                }
                else
                {
                    // var direction = Level.instance.DirectionFrom(tuple.value,
                    //     path[(tuple.index)+1]);
                    // if (direction == null)
                    // {
                    //     continue;
                    // }
                    // if(direction!=_character._facingDirection)
                    // {
                    //     Debug.Log($"Character faces {_character._facingDirection} but the direction is {direction} so we have to stop");
                    //     Debug.Break();
                    //     stop = true;
                    // }

                }

                await AsyncMove(stop, _character.MyTile.tileKey, tuple.value);
                Level.instance.ChangeCharacterTile(this.transform.name, tuple.value);
                // Debug.Break();
            }

            isMoving = false;
        }
    }

    async Task AsyncMove(bool stop, int start, int end)
    {
        await RotateToTargetUsingLevelDirection(start, end);
        stopping = stop;
        //Waiting a frame then turning off the trigger
        await Task.Yield();
        // await RotateToTarget(_StartPosition, _TargetPosition, _StartRotation);
        await MoveTarget(stop);
    }

    public Vector3 _StartPosition { get; set; }
}

