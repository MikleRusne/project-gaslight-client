using System.Collections.Generic;
using System.Threading.Tasks;
using Characters;
using Tiles;
using UnityEngine.Events;

namespace GameActions
{
    public class PathTo: GameAction
    {
        public UnityEvent<bool, BlockLinks[]> Pathfound = new UnityEvent<bool, BlockLinks[]>();
        public (int, int) Target;
        public (int, int) From;
        public bool complete = false;
        public bool success = false;
        public AStar Pathfinder;
        public PathTo(Character Invoker, EFaction Faction, (int, int) From, (int, int) Target) : base(Invoker, Faction)
        {
            this.From = From;
            this.Target = Target;
            // this.Pathfinder = Invoker.Pathfinder;
            Pathfinder.ProcessingFinished.AddListener(OnProcessingFinished);
            Pathfinder.FindPath(From.Item1, Target.Item2);
        }

        public BlockLinks[] PathList = new BlockLinks[0];
        public void OnProcessingFinished(bool Success)
        {
            success = Success;
            if (!Success)
            {
                Pathfound.Invoke(false, null); 
                return;
            }
            Pathfinder.ProcessingFinished.RemoveListener(OnProcessingFinished);
            // this.PathList = Pathfinder.PrintPath();
            complete = true;
            Pathfound.Invoke(true, PathList);
        }
        
        
        // ReSharper disable Unity.PerformanceAnalysis
        public override async Task Execute()
        {
            // Invoker.MoveTo(Target);
            while (!complete)
            {
                await Task.Yield();
            }
            Pathfound.Invoke(success, PathList);
        }

        public override string ToString()
        {
            return "Recalculation to " + Target;
        }

        public override bool isComplete()
        {
            return false;
        }
        
        
    }
}