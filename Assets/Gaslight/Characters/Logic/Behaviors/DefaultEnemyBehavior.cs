using System;
using System.Collections.Generic;
using System.Linq;
using Behaviors;
using CleverCrow.Fluid.BTs.Trees;
using UnityEngine;

public class DefaultEnemyBehavior: Behavior
{
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
        Debug.Log("Default enemy behavior inited");
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


    public override void HandleCharacterTileChange(int location, SimpleCharacter otherCharacter)
    {
        Debug.Log("Handle character tile change called on " + Invoker.name);
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