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
        public abstract void HandleCharacterTileChange(int location, Character otherCharacter);
        public List<BehaviorTarget> targets;
        public BehaviorTree bt;
        public Character Invoker;
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

        public Character Invoker;
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
                if (Invoker.MyTile.tileKey == patrolStops[patrolStopsIndex])
                {
                    return TaskStatus.Success;
                }
                //Check if I am on any of the patrol stops
                FixDirection();
                var newDirective = new TryMoveDirective();
                newDirective.Invoker = Invoker;
                patrolStopsIndex = patrolStopsIndex + patrolDirection;
                bool canGetToStop = false;
                var task = Level.instance.FindPath(Invoker.MyTile.tileKey, patrolStops[patrolStopsIndex],
                    Invoker.GetPathfindingHeuristic);
                task.Wait();
                var path = task.Result;
                if (path.Count <= Invoker.GetMaxTraversibleTilesInOneTurn())
                {
                    Debug.Log($"We can go from {Invoker.MyTile.tileKey} to {patrolStops[patrolStopsIndex]} in a single turn");
                }
                else
                {
                    Debug.Log($"We cannot go from {Invoker.MyTile.tileKey} to {patrolStops[patrolStopsIndex]} in a single turn. Path: {path}, max tiles: {Invoker.GetMaxTraversibleTilesInOneTurn()}");
                }
                newDirective.AddTarget(patrolStops[patrolStopsIndex]);
                Invoker.passiveDirective = newDirective;
                //Check if I can actually get to that one in the current turn
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
        public static BehaviorTreeBuilder PatrolActionBuilder (this BehaviorTreeBuilder builder,string name, List<int> patrolStops, Character invoker) {
            return builder.AddNode(new PatrolAction(patrolStops){Name = name, Invoker =  invoker});
        }
    }
    

    


    
}