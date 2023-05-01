using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Characters;
using Tiles;
using UnityEngine;
using UnityEngine.InputSystem;

public class PropCharacter : Character
{
    public List<(float thresh, GameObject model)> TransitionMeshes;


    public override MovementComponent movementComponent { get; }
    public override int GetMaxTraversibleTilesInOneTurn()
    {
        throw new System.NotImplementedException();
    }

    public override (bool,float) GetPathfindingHeuristic(Node previous, int to)
    {
        return (false,0);
    }

    public override async Task Attack(int location)
    {
        await Task.Yield();
    }

    public override Task<(bool oneGo, List<int> path)> GetPathTowards(int start, int end)
    {
        throw new System.NotImplementedException();
    }

    public override async Task<bool> MoveToTile(int index)
    {
        await Task.Yield();
        return false;
    }

    public override Task<bool> TraversePath(List<int> path)
    {
        throw new System.NotImplementedException();
    }

    public override void OnTileChangeSelf()
    {
        
    }

    public override void OnCharacterTileChanged(int location, Character character)
    {
        
    }

    public override void OnTileSelected()
    {

    }

    public override void OnTileDeselected(int index)
    {
    }


    void HandleTransitionMeshes(float value)
    {
        //Look for the smallest value in the transition list that is greater than
        //the value of health provided
        float minimum = float.MaxValue;
        foreach (var transitionMesh in TransitionMeshes)
        {
            if (transitionMesh.thresh < minimum)
            {
                minimum = transitionMesh.thresh;
            }
        }
        //Got the minimum, select the list with that value
        //This is assuming that no one messes with the float value in between
        //Considering how C# works, that is very possible, so adding an error
        var newModelPrefab = TransitionMeshes.Find((e) => e.thresh == minimum)
            .model;
        //Destroy the current model
        var currentModel = this.gameObject.transform.Find("model");
        //There is no need to change the model
        if (currentModel == newModelPrefab)
        {
            return;
        }
        GameObject.DestroyImmediate(currentModel);
        GameObject.Instantiate(
            newModelPrefab,
            this.transform
        );
    }
    override public void OnTraitChanged((string key, float oldValue) changed)
    {
        if (changed.key == "health")
        {
            //If the transition list is empty, do nothing
            if (TransitionMeshes.Count == 0)
            {
                
            }
            //But if the transition list is not empty, call a method to transition to another mesh
            else
            {
                HandleTransitionMeshes(GetFloatTrait("health"));
            }
        }
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
