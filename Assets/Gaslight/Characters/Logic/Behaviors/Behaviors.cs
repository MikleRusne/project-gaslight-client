using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CleverCrow.Fluid.BTs.Tasks.Actions;
using CleverCrow.Fluid.BTs.Trees;
using Gaslight.Characters.Logic;
using UnityEngine;
using TaskStatus = CleverCrow.Fluid.BTs.Tasks.TaskStatus;

namespace Behaviors
{
    [Serializable]
    public struct BehaviorTarget
    {
        public string key;
        public string value;
    }
   

    public abstract class Behavior : MonoBehaviour
    {
        public abstract void HandleCharacterTileChange(int location, SimpleCharacter otherCharacter);
        public List<BehaviorTarget> targets;
        public BehaviorTree bt;
        public SimpleCharacter Invoker;
        public abstract void Tick();
        public String Name => "Base";
        // Don't need this constructor
        //TODO: Remove

        public abstract void Initialize();
    }

    public class PatrolAction : ActionBase
    {
        private int patrolDirection = 1;
        private int patrolStopsIndex = -1;

        public SimpleCharacter Invoker;
        public List<int> patrolStops;
        public PatrolAction(List<int> patrolStops)
        {
            this.patrolStops = patrolStops;
        }
        private void FixDirection()
        {
            if (patrolStopsIndex >= patrolStops.Count - 1 && patrolDirection==1)
            {
                patrolDirection = -1;
            } else if (patrolDirection == -1 && patrolStopsIndex == 0)
            {
                patrolDirection = 1;
            }
            
        }

        public bool isAtCurrentStopIndex()
        {
            if (Invoker.MyTile.tileKey == patrolStops[patrolStopsIndex])
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        protected override TaskStatus OnUpdate()
        {
            if (patrolStops!=null && patrolStops.Count > 0)
            {
                FixDirection();
                var newDirective = new ManualMoveDirective();
                newDirective.Invoker = Invoker;
                patrolStopsIndex = patrolStopsIndex + patrolDirection;
                newDirective.AddTarget(patrolStops[patrolStopsIndex]);
                Debug.Log("Creating new directive to move to "+ patrolStops[patrolStopsIndex]);
                this.Invoker.passiveDirective = newDirective;
                return TaskStatus.Success;
            }
            else
            {
                // Debug.Log("Patrol empty");
                return TaskStatus.Failure;
            }
        }
    }
    public static class BehaviorTreeBuilderExtensions {
        public static BehaviorTreeBuilder PatrolActionBuilder (this BehaviorTreeBuilder builder,string name, List<int> patrolStops, SimpleCharacter invoker) {
            return builder.AddNode(new PatrolAction(patrolStops){Name = name, Invoker =  invoker});
        }
    }
    public class NoBehavior : Behavior
    {

        public override void HandleCharacterTileChange(int location, SimpleCharacter otherCharacter)
        {
            
        }

        public override void Tick()
        {
            
        }

        public override void Initialize()
        {
        }
    }

    


    
}