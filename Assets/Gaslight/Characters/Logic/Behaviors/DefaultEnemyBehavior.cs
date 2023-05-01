using System;
using System.Collections.Generic;
using System.Linq;
using Behaviors;
using CleverCrow.Fluid.BTs.Trees;
using UnityEngine;

public class DefaultEnemyBehavior: Behavior
{
    public bool _debugDetection = false;
    public List<int> patrolStops;
    public void BuildTree()
    {
        this.bt = new BehaviorTreeBuilder(Invoker.gameObject)
            .RepeatForever()
            .Sequence()
            .PatrolActionBuilder("Patrolling",patrolStops, this.Invoker)
            .Build();
    }
    public void InitTargets(List<BehaviorTarget> behaviorTargets)
    {
        this.targets = behaviorTargets; 
        GeneratePatrolRoutesFromTargets();
        // Debug.Log("Default enemy behavior inited");
    }
    //TODO: Add the initializing system from directives into this
    public void GeneratePatrolRoutesFromTargets()
    {  
        var prefix_string = "patrol_target_";
        var temp = targets
            .FindAll(targ =>
            {
                return targ.key.StartsWith(prefix_string);

            })
            .OrderBy((a) =>
            {
                return a.key;
            })
            .Select((targ) =>
                {
                    return Int32.Parse(
                        targ.value
                    );
                }
            )
            .ToList();
        patrolStops = temp;
    }
    public bool _drawDetectionGizmos;
    public Vector3 _castStart;
    public Vector3 _castEnd;
    public Color _lineColor;
    [SerializeField]public RaycastHit _detectionHit;
    public void OnDrawGizmos()
    {
        if (_drawDetectionGizmos)
        {
            float size = 0.5f;
            Color prevColor = Gizmos.color;
            Gizmos.color = Color.red;
            Gizmos.DrawCube(_castStart, new Vector3(size,size,size));
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(_castEnd, new Vector3(size,size,size));
                
            Gizmos.color = _lineColor;
            Gizmos.DrawLine(_castStart,_castEnd);
            Gizmos.color = prevColor;
        }
    }


    public override void HandleCharacterTileChange(int location, Character otherCharacter)
    {
        // Debug.Log("Handle character tile change called on " + Invoker.name);
        if (otherCharacter != this.Invoker)
            {
                //Do a raycast to that actor
                var requiredLayerMask = Level.instance.DetectionLayerMask;
                _drawDetectionGizmos = true;
                _castStart = transform.position + Invoker._detectionRaycastOffset;
                _castEnd = otherCharacter.transform.TransformPoint(otherCharacter._boundingBox.center);
                if (Physics.Linecast(_castStart, _castEnd,
                        layerMask: requiredLayerMask,hitInfo:out _detectionHit))
                {
                    var heading = (otherCharacter.transform.position - this.transform.position);
                    heading.y = 0.0f;
                    heading.Normalize();
                    var dotProduct = Vector3.Dot(this.transform.forward, heading);
                    Debug.DrawRay(transform.position, this.transform.forward*5.0f,Color.blue);
                    _lineColor = Color.yellow;
                    _castEnd = _detectionHit.point;
                    if (_detectionHit.transform.CompareTag("character"))
                    {
                        if(_debugDetection)
                            Debug.Log($"{Invoker.name} detected {_detectionHit.transform.name} with dotProduct: {dotProduct}");
                    }
                    
                    // Debug.Break();
                }
                
            }
            else
            {
                if(_debugDetection)
                Debug.Log($"{Invoker.name} trying to detect other characters");
                //I am the same otherCharacter that moved, I need to raycast every other character
                Level.instance.characters.ForEach(descriptor =>
                {
                    if (descriptor.name == transform.name)
                    {
                        return;
                    }
                    var req_character = descriptor.character;
                    
                    var requiredLayerMask = Level.instance.DetectionLayerMask;
                    _drawDetectionGizmos = true;
                    _castStart = transform.position + Invoker._detectionRaycastOffset;
                    _castEnd = req_character.transform.TransformPoint(otherCharacter._boundingBox.center);
                    if (Physics.Linecast(_castStart, _castEnd,
                            layerMask: requiredLayerMask,hitInfo:out _detectionHit))
                    {

                        var heading = (req_character.transform.position - this.transform.position);
                        heading.y = 0.0f;
                        heading.Normalize();
                        _lineColor = Color.green;
                        _castEnd = _detectionHit.point;
                        var dotProduct = Vector3.Dot(this.transform.forward, heading);
                        if (_detectionHit.transform.CompareTag("character"))
                        {
                            if(_debugDetection)
                            Debug.Log($"{Invoker.name} detected {_detectionHit.transform.name} with dotProduct: {dotProduct}");
                        }
                        Debug.DrawRay(transform.position, this.transform.forward*5.0f,Color.blue);
                        // Debug.Break();
                    }
                });
            }
    }

    public override void Tick()
    {
        // Debug.Log("Calling tree tick");
        this.bt.Tick();
    }

    public override void Initialize()
    {
        InitTargets(targets);
        BuildTree(); 
    }
}