using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Behaviors;
using Characters;
using CleverCrow.Fluid.BTs.Trees;
using Gaslight.Characters.Logic;
using Tiles;
using UnityEditor;
using UnityEngine;
using TaskStatus = CleverCrow.Fluid.BTs.Tasks.TaskStatus;

namespace Gaslight.Characters
{
   public class Actor: SimpleCharacter
   {
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
        }

        public int GetMaxTraversibleTilesInOneTurn()
        {
            return Mathf.FloorToInt(Level.SpeedToTileMovementFactor * GetFloatTrait("speed"));
        }

        public override (bool, float) GetPathfindingHeuristic(Node previous, int to)
        {
            bool canGetThere = true;
            // if (GetMaxTraversibleTilesInOneTurn() == previous.Step)
            // {
            //     // Debug.Log("Previous node had g of "+ previous.Step + " and "+ transform.name+" can only go "+ GetMaxTraversibleTilesInOneTurn()+ " tiles at a time so it cannot go there");
            //     canGetThere = true;
            // }
            // else
            // {
            //     canGetThere = true;
            // }
            return (canGetThere,Level.instance.ManhattanDistance(MyTile.tileKey, to));
        }

        public async Task<List<int>> GetPath(int start, int end)
        {
            var path = await Level.instance.FindPath(start, end, GetPathfindingHeuristic, false);
            return path;
        }
        public override async Task Attack(int location)
        {
            await this.GetComponent<AnimatedAttackComponent>().Attack(location);
        }

        public override async Task<bool> MoveToTile(int index)
        {
            // Debug.Log("Calling find path");
            var temp = await GetPath(MyTile.tileKey, index);
            // Trim that path to however long it can go
            if (temp == null)
            {
                Debug.LogError("Could not find a path between " + this.MyTile.tileKey+ ", "+ index);
                return false;
            }

            if (GetMaxTraversibleTilesInOneTurn() == 0)
            {
                Debug.Log("Character's traversible turns are 0");
            }
            // Debug.Log($"{transform.name} has a speed of {GetFloatTrait("speed")}, and can go " +
                      // $"{GetMaxTraversibleTilesInOneTurn()}, path is {temp.Count} long: {temp.Aggregate("",((s, i) => s+TileCoordinate.IndexToCoord(i)))+ ","}.");
            temp = temp.Take(GetMaxTraversibleTilesInOneTurn()+1).ToList();
            // Debug.Log($"path is {temp.Count} long: {temp.Aggregate("",((s, i) => s+TileCoordinate.IndexToCoord(i)))+ ","}.");
            // Debug.Log("Moving on from find path");
            
            {
                // Debug.Log("Telling movement component to move");
                await _movementComponent.StartMoving(temp);
                // Debug.Log("Moving on from movement component");
                return true;
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
        }

        public override void OnAttack(SimpleCharacter other)
        {
        }

        public override void Attack(SimpleCharacter target)
        {
        }
   }
}

