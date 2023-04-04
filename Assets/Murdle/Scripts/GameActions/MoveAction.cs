using System.Threading.Tasks;
using Characters;
using Tiles;
using UnityEngine;

namespace GameActions
{
    public class MoveAction : GameAction
    {
        // private readonly Character Invoker;
        public BlockLinks Start;
        public BlockLinks Target;
        private bool complete;
        private Quaternion StartRotation;
        // public int cost;
        public MoveAction(Character Invoker, EFaction InvokingFaction, BlockLinks Start, BlockLinks Target, int cost = 1): base(Invoker, InvokingFaction, cost)
        {
            this.StartRotation = Invoker.transform.rotation;
            this.Start = Start;
            this.Target = Target;
            complete = false;
        }

        
        
        // ReSharper disable Unity.PerformanceAnalysis
        public override async Task Execute()
        {
            Invoker.characterMovementState = ECharacterMovementState.Moving;
            Invoker.MovementComponent.MovementFinished.AddListener(Invoker.OnMovementFinished);
            var StartTile =  Spawner.inst.GetTileFromLocation(Start.MyID).IT.Tile;
            var NextTile =  Spawner.inst.GetTileFromLocation(Target.MyID).IT.Tile;
            // Invoker.NextTile = NextTile;
            // await Invoker.MovementComponent.StartMoving(StartTile.transform.Find("ChLocation").transform.position,
            //     StartRotation ,
            //     NextTile.transform.Find("ChLocation").transform.position);
            // Invoker.NextTile = null;
            Invoker.MovementComponent.MovementFinished.RemoveListener(Invoker.OnMovementFinished);

            complete = true;
            
        }

        public override bool isComplete()
        {
            return complete;
        }

        public override string ToString()
        {
            return "Action moves " + Invoker.transform.name + " from " + Start.transform.name + " to " + Target.transform.name;
        }

        public override bool isViable()
        {
            return true;
        }
    }
}