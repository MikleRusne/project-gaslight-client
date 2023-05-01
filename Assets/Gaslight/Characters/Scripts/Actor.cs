using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Behaviors;
using Characters;
using Tiles;
using UnityEngine;

namespace Gaslight.Characters
{
   public class Actor: Character
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


        public override void OnCharacterTileChanged(int location, Character character)
        {
            
            this.behavior.HandleCharacterTileChange(location, character);
        }

        public void Start()
        {
            base.Start();
        }

        public override int GetMaxTraversibleTilesInOneTurn()
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

        public async override Task<(bool oneGo,List<int> path)> GetPathTowards(int start, int end)
        {
            var path = await Level.instance.FindPath(start, end, GetPathfindingHeuristic, false);
            if (path == null)
            {
                return (false, null);
            }
            if (path.Count <= GetMaxTraversibleTilesInOneTurn() + 1)
            {
                return (true, path);
            }
            path = path.Take(GetMaxTraversibleTilesInOneTurn()+1).ToList();

            if (GetMaxTraversibleTilesInOneTurn() == 0)
            {
                Debug.Log("Character's traversible turns are 0");
            }
            
            return (false, path);
        }
        public override async Task Attack(int location)
        {
            await this.GetComponent<AnimatedAttackComponent>().Attack(location);
        }


        public override async Task<bool> MoveToTile(int index)
        {
            // Debug.Log("Calling find path");
            var temp = await GetPathTowards(MyTile.tileKey, index);
            // Trim that path to however long it can go
            
            if (temp.path == null)
            {
                Debug.LogError("Could not find a path between " + this.MyTile.tileKey+ ", "+ index);
                return false;
            }

            // Debug.Log("Telling movement component to move");
            await _movementComponent.StartMoving(temp.path);
            // Debug.Log("Moving on from movement component");
            return true;
        }

        public override async Task<bool> TraversePath(List<int> path)
        {
            await _movementComponent.StartMoving(path);
            return true;
        }

        public override void OnTileSelected()
        {
            
        }

        public override void OnTileDeselected(int index)
        {
        }

        public override void OnAttacked(Character other)
        {
        }

        public override void OnAttack(Character other)
        {
        }

        public override void Attack(Character target)
        {
        }
   }
}

