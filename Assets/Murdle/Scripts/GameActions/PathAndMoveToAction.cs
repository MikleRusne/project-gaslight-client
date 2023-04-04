using System.Threading.Tasks;
using Characters;
using Tiles;

namespace GameActions
{
    public class PathAndMoveToAction : GameAction 
    {
        public override bool isViable()
        {
            return true;
        }

        public (int, int) Target;
        public bool complete = false;
        public AStar Pathfinder;
        public (int, int) From;
        public PathAndMoveToAction(Character Invoker, EFaction Faction, (int, int) From, (int, int) Target) : base(Invoker, Faction)
        {
            this.From = From;
            this.Target = Target;
            // this.Pathfinder = Invoker.Pathfinder;
        }
        public void OnProcessingFinished(bool Success)
        {
            if (!Success)
            {
                return;
            }
            Pathfinder.ProcessingFinished.RemoveListener(OnProcessingFinished);
            //Gaslight
            // Invoker.PathList = Pathfinder.PrintPath();

                
        }
        // ReSharper disable Unity.PerformanceAnalysis
        public override async Task Execute()
        {
            Pathfinder.ProcessingFinished.AddListener(OnProcessingFinished);
            //Gaslight
            // Pathfinder.Initialize(From, Target);
            // Invoker.MoveTo(Target);
            complete = true;
            await Task.Yield();
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