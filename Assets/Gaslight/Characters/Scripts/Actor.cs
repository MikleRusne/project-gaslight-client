using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Behaviors;
using Characters;
using CleverCrow.Fluid.BTs.Trees;
using Gaslight.Characters.Logic;
using UnityEditor;
using UnityEngine;
using TaskStatus = CleverCrow.Fluid.BTs.Tasks.TaskStatus;

namespace Gaslight.Characters
{
   public class Actor: SimpleCharacter
   {
      private int patrolStopIndex = 0;
      public override MovementComponent movementComponent => _movementComponent;
        private AnimatedMovementComponent _movementComponent;
        public bool isMoving => (_movementComponent == null || _movementComponent.isMoving);
        public void Awake()
        {
            base.Awake();
            this._movementComponent= GetComponent<AnimatedMovementComponent>();
        }

        public void Start()
        {
            
            this.bt = this.behavior.bt;
        } 

        public List<int> MovementPath;
        
        public async Task MoveToTile(int index)
        {
            var temp = Level.instance.FindPath(this.MyTile.tileKey, index);
            if (temp == null)
            {
                return;
            }
            else
            {
                // Debug.Log("Movement component called");
                await _movementComponent.StartMoving(temp);
            }
        }
        
        public override void OnTileSelected()
        {
            
        }

        public override void OnTileDeselected(int index)
        {
        }
    }

   [CustomEditor(typeof(Actor))]
   class ActorInspector : Editor
   {
       private int targetTile = 0;
       public override void OnInspectorGUI()
       {
           base.OnInspectorGUI();
           EditorGUILayout.LabelField("Custom controls:");
           Actor targ = (Actor)target;
           targetTile = EditorGUILayout.IntField("Target tile: ", targetTile);
           if (GUILayout.Button("Go!"))
           {
               if (!targ.isMoving)
               {
                   targ.MoveToTile(targetTile);
                       
               }
           }
       }
   }
}

