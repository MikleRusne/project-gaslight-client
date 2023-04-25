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

        public override void OnTileChangeSelf()
        {
            
        }

        public override void OnCharacterTileChanged(int location, SimpleCharacter character)
        {
            this.behavior.HandleCharacterTileChange(location, character);
        }

        public void Start()
        {
            base.Start();
            if (this.behavior != null)
            {
                Debug.Log("Behavior loaded "+ behavior.Name);
                this.bt = this.behavior.GetBT();
            }
        } 

        public List<int> MovementPath;

        public override async Task Attack(int location)
        {
            // Debug.Log("Haven't made attack function yet😔");
            // await Task.Delay(millisecondsDelay: 2000);
            await this.GetComponent<AnimatedAttackComponent>().Attack(location);
        }

        public override async Task MoveToTile(int index)
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

        public override void OnAttacked(SimpleCharacter other)
        {
            throw new System.NotImplementedException();
        }

        public override void OnAttack(SimpleCharacter other)
        {
            throw new System.NotImplementedException();
        }

        public override void Attack(SimpleCharacter target)
        {
            throw new System.NotImplementedException();
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

