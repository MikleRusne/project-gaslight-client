using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Characters;
using UnityEditor;
using UnityEngine;
[RequireComponent(typeof(Animator))]
public class AnimatedMovementComponent : MovementComponent 
{
    public Vector3 TargetPosition;

    public Animator _animator;

    public List<int> path;
    
    public void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public float AngleThreshold = 45;
    
    async Task RotateToTarget()
    {
        Quaternion TargetRotation = Quaternion.LookRotation(StartPosition - TargetPosition, Vector3.up);
        TargetRotation = Quaternion.Euler(0, TargetRotation.eulerAngles.y, 0);
        
        float factor = 0;
        //Rotate towards target at a certain speed
        Vector3 tF = this.transform.up;
        Vector3 distance = TargetPosition - StartPosition;
        float dot = Vector3.Dot(tF, distance);
        float angle = Mathf.Acos(dot / (tF.magnitude * distance.magnitude)) *Mathf.Rad2Deg -90;
        Vector3 Cross = Vector3.Cross(StartPosition, TargetPosition);
        
        float difference = Quaternion.Angle(StartRotation, TargetRotation);
       

        if (angle <= AngleThreshold)
        {
            // Debug.Log("Angle too small");
            transform.rotation = TargetRotation;
            return;
        }

        float step = 0;
        float FullSpeed = difference*RotationSpeed;
        while (factor<=1)
        {
            factor += FullSpeed*Time.deltaTime;
            step += RotationSpeed*Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(StartRotation, TargetRotation, factor);
            await Task.Yield();
        }

        transform.LookAt(TargetPosition);

    }

    public float RotationSpeed = 5f;
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
                transform.position = Vector3.Lerp(StartPosition, TargetPosition, value);
                mytime += Time.deltaTime;
                await Task.Yield();
            }
            else
            {
                float value = RegularMovementCurve.Evaluate(mytime/MovementDuration);
                mytime += Time.deltaTime;
                transform.position = Vector3.Lerp(StartPosition, TargetPosition, value);
                await Task.Yield();
            }
        }
        transform.position = TargetPosition;
        _animator.ResetTrigger("Walk");
        _animator.SetTrigger(StopWalkID);
    }

    public AnimationCurve StopMovementCurve;
    public AnimationCurve RegularMovementCurve;
    // public [Range(1f, 5f)] private float MovementSpeed;
    public float MovementDuration;
    private static readonly int StopWalkID = Animator.StringToHash("Stop walk");

    public override async Task StartMoving(List<int> Path)
    {
        if (!isMoving)
        {
            isMoving = true;
            this.path = Path;
            foreach (var index in path.Skip(1))
            {
                // Debug.Log("Movement component: Moving to "+ index);
                StartPosition = this.transform.position;
                TargetPosition = Level.instance.CoordToWorld(index);
                if (index == path.Count - 1)
                {
                    await AsyncMove(true);
                }
                else
                {
                    await AsyncMove(false);
                }
                Level.instance.ChangeCharacterTile(this.transform.name, index);
            }

            isMoving = false;
        }
        else
        {
        }
    }

    async Task AsyncMove(bool stop)
    {
        await RotateToTarget();
        await MoveTarget(stop);
    }

    public Quaternion StartRotation { get; set; }
    public Vector3 StartPosition { get; set; }
}

