using System.Collections.Generic;
using System.Linq;
using System.Text;
using Characters;
using Tiles;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameActions
{
    public class DeliberateMoveDirective: Directive
    {
        public MoveAction[] SplitPathIntoActions(Character Invoker, BlockLinks[] PathArray, EFaction TurnFaction)
        {
            int TotalActions = PathArray.Length - 1;
            MoveAction[] temp = new MoveAction[TotalActions];
            locations = (from p in PathArray select p.MyID).ToArray();
            // var TurnGrens = (from p in PathArray select p.GetComponent<TurnGreenOnPlayerTurn>());
            // foreach (var turnGreenOnPlayerTurn in TurnGrens)
            // {
                // turnGreenOnPlayerTurn.isInPath = true;
                
            // }
            for (int i = 0; i < TotalActions; ++i)
            {
                temp[i] = new MoveAction(Invoker, TurnFaction, PathArray[i], PathArray[i + 1]);
                // Debug.Log(temp[i].ToString());
            }
            return temp;
        }

        private (int, int)[] locations = new (int, int)[0];

        public override void FixDirty()
        {
            Debug.Log("Fixing path");
            //If the last action is complete, then take the next one
            //Else resume from that one
            PathTo pathTo;
            // if (  ((MoveAction)Actions[ToBePerformed]).isComplete())
            // {
            //     pathTo = new PathTo()
            // }
            var PathToAction = new PathTo(Invoker, Invoker.TurnFaction,((MoveAction)Actions[ToBePerformed+1]).Start.MyID, Target);
            PathToAction.Pathfound.AddListener(OnPathFound);
        }

        void OnPathFound(bool success, BlockLinks[] path)
        {
            ToBePerformed = 0;
            if (success)
            {
                Debug.Log("Path to action found a path");
                this.Actions = SplitPathIntoActions(Invoker, path, Invoker.TurnFaction);
            }
            else
            {
                Debug.Log("Could not find path");
            }

            this.Dirty = false;
            // this.Actions = SplitPathIntoActions(Invoker, pathArray, Invoker.TurnFaction);
        }
        
        public DeliberateMoveDirective(Character Invoker, (int, int) From, (int, int) Target, Directive NextDirective = null) : base(Invoker, EDirectiveType.Movement, NextDirective)

        {
            this.From = From;
            this.Target = Target;
            Assert.AreNotEqual(Invoker, null);
            // Assert.AreNotEqual(From, null);
            var PathToAction = new PathTo(Invoker, Invoker.TurnFaction,From, Target);
            PathToAction.Pathfound.AddListener(OnPathFound);
            Assert.IsNotNull(PathToAction.Pathfound);
            Spawner.inst.TileChangedEvent.AddListener(OnTileChange);
            // this.Actions
        }

        public (int, int) Target { get; set; }

        public (int, int) From { get; set; }

        public void OnTileChange((int, int) location, Spawner.ETileType newType)
        {
            Debug.Log("Locations are: ");
            foreach (var (item1, item2) in locations)
            {
               // Debug.Log(item1+ " " + item2); 
            }
            if (locations.Contains(location))
            {
                // Debug.Log("My path might be invalid");
                //Check which of my directives are movement related
                Dirty = true;
            }
            else
            {
                Debug.Log("My path is still valid.");
            }
        }
    }
}