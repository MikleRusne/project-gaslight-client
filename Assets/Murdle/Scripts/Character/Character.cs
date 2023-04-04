using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameActions;
using Tiles;
using UnityEngine;






namespace Characters
{
    [Serializable]
    public class CharacterStats
    {
        [SerializeField] public float health;
        [SerializeField] public float speed;
        [SerializeField] public float armor;

        [SerializeField, Range(0.0f, 1.0f)] public float bravery;
        [SerializeField, Range(0.0f, 1.0f)] public float curiosity;
        [SerializeField, Range(0.0f, 1.0f)] public float visibility;
        [SerializeField, Range(0.0f, 1.0f)] public float perception;
    }

    #region State Enumerator
    public enum ECharacterMovementState
    {
        Moving,
        Stationary
    }
    
    #endregion

    
    public class Character : MonoBehaviour
{
    // [HideInInspector]
    
    #region Main components

    public Tile CurrentTile;
    public Level gameLevel = default;
    [SerializeField]
    public Tile StartingTile;
    public MovementComponent MovementComponent { get; set; }


    public ECharacterMovementState characterMovementState = ECharacterMovementState.Stationary;
    
    #endregion

    public LinkedList<Directive> CurrentDirectives = new LinkedList<Directive>();
    void Start()
    {
        if(StartingTile!= null)
        {
            CurrentTile = StartingTile;
            MovementComponent = GetComponent<MovementComponent>();
        
            this.transform.position+= new Vector3(0f,CurrentTile.heightOffset,0.0f);
        }

        if (gameLevel == null)
        {
            Debug.LogError("Level not set for me: " + transform.name);
        }
        if (StartingTile == null)
        {
            Debug.LogError("Starting tile not set for me: " + transform.name);
        }
        Orchestrator.inst.TurnMade.AddListener(OnTurnMade);
    }

    void PushMyNextAction()
    {
        if (CurrentDirectives.Count > 0)
        {
            var DirectiveToBeChallenged = CurrentDirectives.First.Value;
            if (DirectiveToBeChallenged.Dirty)
            {
                DirectiveToBeChallenged.FixDirty();
            }
            GameAction MyNextAction = DirectiveToBeChallenged.GetNextAction();
            DirectiveToBeChallenged.IncrementAction();
            Orchestrator.inst.EnqueueAction(MyNextAction);
            //If it is complete now then remove
            if (DirectiveToBeChallenged.isComplete)
            {
                CurrentDirectives.RemoveFirst();
            }
        }
    }
    
    #region Event listeners
    void OnTurnMade(bool PlayerTurn, int TotalTicks)
    {
        //Push the action on the queue to the orchestrator

        switch (TurnFaction)
        {
            case EFaction.Player:
                if(PlayerTurn) {PushMyNextAction(); }
                break;
            case EFaction.Enemy:
                if(!PlayerTurn) {PushMyNextAction();}
                break;
            case EFaction.Chaos:
            case EFaction.Neutral:
                PushMyNextAction();
                break;
        }
       
    }
    
    public void OnMovementFinished()
    {
        characterMovementState = ECharacterMovementState.Stationary;
    }
   #endregion 
    
   # region Pathfinding and movement
   // async Task<bool> MoveOnPath()
   //  {
   //      //Get next tile
   //      //Move to the next tile
   //      NextTile = (GameObject) Path.Current;
   //      CharacterState = ECharacterState.Moving;
   //      await MovementComponent.StartMoving(this.transform.position, this.transform.rotation,
   //          NextTile.transform.GetChild(5).transform.position);
   //      if (Path == null || !Path.MoveNext())
   //      {
   //          // Debug.Log("Path is null or no more steps");
   //          MovementComponent.MovementFinished.RemoveListener(OnMovementFinished);
   //          return false;
   //      }
   //      return true;
   //  }
    
    

    public Tile[] PathList;

    #endregion
    //If already moving, complete current move, then go to new target
    
    public void MoveTo(int targetloc)
    {
        // Tile Target = Spawner.inst.GetTileFromLocation(targetloc).IT;
        // if (Target.Type != Spawner.ETileType.Tile)
        // {
            // return;
        // }

        
        switch (characterMovementState)
        {
            case ECharacterMovementState.Stationary:
                // Debug.Log("Moving from "+ CurrentTile.name + " to " + Target.Tile.name);
                // Path = null;
                // CurrentDirectives.AddFirst(new DeliberateMoveDirective(this, CurrentBlockLinks.MyID, targetloc));
                // PendingActions = new LinkedList<GameAction>();
                break;
            case ECharacterMovementState.Moving:
                // PendingActions = new LinkedList<GameAction>();
                // PendingActions.AddFirst(new PathAndMoveToAction(this, TurnFaction, targetloc));
                break;
        }
        
    }
    
    
    public EFaction TurnFaction = EFaction.Chaos;

    public (int, int)[] loclist = new (int, int)[0];
    MoveAction[] SplitPathIntoActions(BlockLinks[] PathArray)
    {
        int TotalActions = PathArray.Length - 1;
        loclist = (from p in PathArray
            select p.MyID).ToArray();
        MoveAction[] temp = new MoveAction[TotalActions];
        for (int i = 0; i < TotalActions; ++i)
        {
            temp[i] = new MoveAction(this, TurnFaction, PathArray[i], PathArray[i + 1]);
            // Debug.Log(temp[i].ToString());
        }
        return temp;
    }
    
    
    // async Task MoveToGoal()
    // {
    //     // Debug.Log("Moving to goal");
    //     if (!Path.MoveNext()|| !Path.MoveNext())
    //     {
    //         Debug.Log("Path empty");
    //         return;
    //     }
    //
    //     MovementComponent.MovementFinished.AddListener(OnMovementFinished);
    //     bool isPathLeft = await MoveOnPath();
    //     while (isPathLeft)
    //     {
    //         isPathLeft = await MoveOnPath();
    //     }
    //     
    //     // Pathfinder = CurrentTile.GetComponent<AStar>();
    // }

    public void OnTileChange((int, int) location, Spawner.ETileType newType)
    {
        if (loclist.Contains(location))
        {
            Debug.Log("My path might be invalid");
            //Check which of my directives are movement related
            foreach (Directive currentDirective in CurrentDirectives)
            {
                if (currentDirective.DirectiveType == EDirectiveType.Movement)
                {
                    currentDirective.MarkDirty();
                } 
            }
        }
        else
        {
            Debug.Log("My path is still valid.");
        }
    }
}

}
