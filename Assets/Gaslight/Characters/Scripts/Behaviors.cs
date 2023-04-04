using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CleverCrow.Fluid.BTs.Tasks.Actions;
using CleverCrow.Fluid.BTs.Trees;
using Gaslight.Characters.Logic;
using UnityEditor;
using UnityEditor.Timeline.Actions;
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
    [Serializable]
    public abstract class Behavior
    {
        public List<BehaviorTarget> targets;
        public BehaviorTree bt;
        public SimpleCharacter Invoker;
        public abstract void Tick();
        public String Name => "Base";
        public Behavior()
        {
            
        }
        // Don't need this constructor
        //TODO: Remove
        public Behavior(SimpleCharacter invoker) => (Invoker) = (invoker);

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
        
        protected override TaskStatus OnUpdate()
        {
            if (patrolStops!=null && patrolStops.Count > 0)
            {
                FixDirection();
                var newDirective = new MoveDirective();
                newDirective.Invoker = Invoker;
                patrolStopsIndex = patrolStopsIndex + patrolDirection;
                newDirective.index = patrolStops[patrolStopsIndex];
                // Debug.Log("Creating new directive to move to "+ patrolStopsIndex + patrolDirection);
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
        public override void Tick()
        {
            
        }

        public override void Initialize()
        {
        }
    }
    public class DefaultEnemyBehavior: Behavior
    {
        private List<int> patrolStops;

        public void BuildTree()
        {
            this.bt = new BehaviorTreeBuilder(Invoker.gameObject)
                    .RepeatForever()
                    .Sequence()
                    .PatrolActionBuilder("Patrol 1",patrolStops, this.Invoker)
                    .Build();
        }
        public DefaultEnemyBehavior(List<BehaviorTarget> behaviorTargets)
        {
            this.targets = behaviorTargets; 
            GeneratePatrolRoutesFromTargets();
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
                     // Debug.Log(targ.key + ": "+ targ.value);
                     return Int32.Parse(
                         targ.value
                     );
                 }
             )
             .ToList();
         patrolStops = temp;
         }
        public override void Tick()
        {
            // Debug.Log("Calling tree tick");
            this.bt.Tick();
        }

        public override void Initialize()
        {
           BuildTree(); 
        }
    }

    public static class BehaviorFactory
    {
        public static Behavior NewBehaviorByName(String name, List<BehaviorTarget> behaviorTargets)
        {
            switch (name)
            {
                case "default_enemy_behavior":
                    var newBehavior = new DefaultEnemyBehavior(behaviorTargets);
                    return newBehavior;
                    break;
                default:
                    Debug.LogError("Behavior name " + name + " not found");
                    return null;
                    break;
            }
        }
    }
    //TODO: If you never need a reflective factory delete this
    // public  class ReflectiveFactory
    //     {
    //         private List<(string name, Type directive)> directivesByName;
    //
    //         public ReflectiveFactory()
    //         {
    //             var directiveTypes = Assembly.GetAssembly(typeof(GDirective)).GetTypes()
    //                 .Where(someType=> someType.IsClass && (!someType.IsAbstract) && (someType.IsSubclassOf(typeof(GDirective))));
    //             directivesByName = new();
    //             
    //             foreach (var directiveType in directiveTypes)
    //             {
    //                 var temp = Activator.CreateInstance(directiveType) as GDirective;
    //                 directivesByName.Add((temp.Name, directiveType));
    //             }
    //         }
    //
    //         [CanBeNull]
    //         public GDirective GetDirective(string directiveType)
    //         {
    //             var count = directivesByName.Count(entry => entry.name == directiveType);
    //             if (count!=1)
    //             {
    //                 
    //                 // var sb = new StringBuilder();
    //                 Debug.LogError("Name: "+ directiveType + " ,Count: " + count);
    //                 foreach (var (name, type) in directivesByName)
    //                 {
    //                    Debug.Log(name + ", " + type.ToString()); 
    //                 }
    //                 return null;
    //             }
    //
    //             var requiredType = directivesByName.Find((entry) => entry.name == directiveType).directive;
    //             var directive = Activator.CreateInstance(requiredType) as GDirective;
    //             return directive;
    //
    //         }
    //         
    //         // Never use this
    //         [CanBeNull]
    //         public GDirective GetDirective(string directiveType, (string key, string value)[] targets)
    //         {
    //             var temp = GetDirective(directiveType);
    //             temp.FillTargets(targets);
    //             return temp;
    //         }
    //     }
    
}