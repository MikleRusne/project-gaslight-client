using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;
using JetBrains.Annotations;
using Assembly = System.Reflection.Assembly;

namespace Gaslight.Characters.Logic
{
    
    //TODO: Change this to a string based lookup, using enums will make it taxing for new developers
    [Serializable]
    public enum Roles
    {
        Role1,
        Role2,
        BasicMovement,
        BasicAttack
    }

    [Serializable]
    public enum DirectiveRequirement
    {
        Character,
        Tile
    }

    
    [Serializable]
public struct DirectiveTarget
{
    public string key;
    public string value;
}
[Serializable]
public abstract class Directive
{
    public abstract int actionPointCost { get; }
    public abstract void AddTarget(int location);
    public abstract bool IsLocationValid(int location);
    public abstract void StepCondition();
    public abstract bool MoreConditions();
    
    public abstract string Name {get;}
    public enum GDType
    {
        Inert,
        TargetCharacter,
        Default
    }

    public SimpleCharacter Invoker;

    public GDType type = GDType.Default;
    
    public List<DirectiveTarget> targets= new List<DirectiveTarget>();

    public abstract Task Initialize();
    public void FillTargets((string key, string value)[] targ)
    {
        if (targ == null)
        {
            Debug.Log("Targ is null");
        }

        targets.AddRange(targ.Select(
            (targe) =>
            {
                return new DirectiveTarget() { key = targe.key, value = targe.value };
            }));
    }
    public abstract Task DoAction();
    public abstract Task Visualize();
    public abstract Task EndVisualize();
}

public class ManualMoveDirective : Directive
{
    private bool targetSet = false;
    public override int actionPointCost => 2;

    public override void AddTarget(int location)
    {
        index = location;
        targetSet = true;
    }

    public override bool IsLocationValid(int i)
    {
        if (i == Invoker.MyTile.tileKey)
        {
            return false;
        }

        if (Level.instance.tileTraversible[i] == false)
        {
            return false;
        }
        if (Level.instance.isAnyCharacterOnTile(i))
        {
            return false;
        }
        //Get the manhattan distance between the character's tile and the target tile
        var manhattan = Level.instance.ManhattanDistance(Invoker.MyTile.tileKey, i);
        var speed = Invoker.isTrait("speed") ? Invoker.GetFloatTrait("speed"): 1.0f;
        if (manhattan < speed * Level.SpeedToTileMovementFactor
            // &&
            // Level.instance.GetPathDistance(Invoker.MyTile.tileKey, i, Mathf.FloorToInt(speed * 2.0f)) !=
            // Int32.MaxValue
            )
        {
            return true;
        }
        return false;
    }

    public override void StepCondition()
    {
        
    }

    public override bool MoreConditions()
    {
        return !targetSet;
    }

    public override string Name => "move";
    public int index;
    public override Task Initialize()
    {
        index = Int32.Parse(targets.Find(targ=>targ.key == "target").value);
        return Task.CompletedTask;
    }

    public async override Task DoAction()
    {
        if (Invoker is null)
        {
            Debug.LogError("Invoker null");
            
        }


        // Debug.Log("Calling move");
        await (Invoker).MoveToTile(index);
        // Debug.Log("Moving on from move");
    
    }

    public async override Task Visualize()
    {
        
    }

    public async override Task EndVisualize()
    {
    }
}

public class AttackDirective : Directive
{
    private bool attackTargetSet = false;
    private int attackTarget;
    private bool attackLocationSet = false;
    private int attackLocation;
    public override bool MoreConditions()
    {
        if (!attackTargetSet)
        {
            // Debug.Log("Set attack target");
            return true;
        }
        if (!attackLocationSet)
        {
            // Debug.Log("Set attack location");
            return true;
        }
        return false;
    }

    public override string Name => "attack";

    public override int actionPointCost => 1;

    public override void AddTarget(int location)
    {
        if (!attackTargetSet)
        {
            attackTarget = location;
            attackTargetSet = true;
            return;
        }

        if (!attackLocationSet)
        {
            attackLocation = location;
            attackLocationSet = true;
        }
    }

    public override bool IsLocationValid(int location)
    {
        if (Invoker.MyTile.tileKey == location)
        {
            return false;
        }
        var speed = Invoker.GetFloatTrait("speed");
        //Choose a target to attack first
        if (!attackTargetSet)
        {
            //Firstly, check if that location can be gotten to
            if (Level.instance.ManhattanDistance(Invoker.MyTile.tileKey, location) - 2 >
                (speed * Level.SpeedToTileMovementFactor / 2)
                ||
                Level.instance.GetPathDistance(Invoker.MyTile.tileKey, location, Level.instance.GetManhattanEvaluator(Invoker.MyTile.tileKey)) - 2 >
                (speed * Level.SpeedToTileMovementFactor / 2))
            {
                return false;
            }
            // Check if there is not a character there
             if (!Level.instance.isAnyCharacterOnTile(location))
             {
                 return false;
             }
            // Check if the character has a different faction
             var otherCharacter = Level.instance.GetCharacterOnTile(location);
             if (otherCharacter.faction == Invoker.faction)
             {
                 return false;
             }
            //
            return true;
        }

        if (!attackLocationSet)
        {
            //Check if that space has a character
            if (Level.instance.isAnyCharacterOnTile(location))
            {
                return false;
            }
            //Check if the attack location can be reached from that location
            if (Level.instance.ManhattanDistance(attackTarget, location) > 2)
            {
                return false;
            }
            if (Level.instance.GetPathDistance(attackTarget, location, Level.instance.GetManhattanEvaluator(attackTarget)) > 2)
            {
                return false;
            }
            //Check if that location can be gotten to
            if (Level.instance.ManhattanDistance(Invoker.MyTile.tileKey, location)>
                (speed * Level.SpeedToTileMovementFactor / 2)
                ||
                Level.instance.GetPathDistance(Invoker.MyTile.tileKey, location, Level.instance.GetManhattanEvaluator(Invoker.MyTile.tileKey)) >
                (speed * Level.SpeedToTileMovementFactor / 2))
            {
                return false;
            }
            
            return true;
        }
        //In a what the fuck situation, log an error
        Debug.LogError("Invalid situation in tile checker, called after conditions met");
        return false;
    }

    public override void StepCondition()
    {
    }
    public async override Task Initialize()
    {
        return;
    }

    public async override Task DoAction()
    {
        await Invoker.MoveToTile(attackLocation);
        await Invoker.Attack(attackTarget);
        await Task.Yield();
    }

    public async override Task Visualize()
    {
        
    }

    public async override Task EndVisualize()
    {
        
    }
}

[Serializable]
public class ForegoDirective : Directive
{
    public override int actionPointCost => Invoker.actionPoints;

    public override void AddTarget(int location)
    {
        
    }

    public override bool IsLocationValid(int i)
    {
        return true;
    }

    public override void StepCondition()
    {
        
    }

    public override bool MoreConditions()
    {
        return false;
    }

    public override string Name => "nothing";
    public override Task Initialize()
    {
        return Task.CompletedTask;
    }

    public override Task DoAction()
    {
        // Debug.Log("Foregoing turn");
        return Task.CompletedTask;
    }

    public async override Task Visualize()
    {
        
    }

    public override async Task EndVisualize()
    {
    }
}

public  class DirectiveFactory
    {
        private List<(string name, Type directive)> directivesByName;

        public DirectiveFactory()
        {
            var directiveTypes = Assembly.GetAssembly(typeof(Directive)).GetTypes()
                .Where(someType=> someType.IsClass && (!someType.IsAbstract) && (someType.IsSubclassOf(typeof(Directive))));
            directivesByName = new();
            
            foreach (var directiveType in directiveTypes)
            {
                var temp = Activator.CreateInstance(directiveType) as Directive;
                directivesByName.Add((temp.Name, directiveType));
            }
        }

        [CanBeNull]
        public Directive GetDirective(string directiveType)
        {
            var count = directivesByName.Count(entry => entry.name == directiveType);
            if (count!=1)
            {
                
                // var sb = new StringBuilder();
                Debug.LogError("Name: "+ directiveType + " ,Count: " + count);
                foreach (var (name, type) in directivesByName)
                {
                   Debug.Log(name + ", " + type.ToString()); 
                }
                return null;
            }

            var requiredType = directivesByName.Find((entry) => entry.name == directiveType).directive;
            var directive = Activator.CreateInstance(requiredType) as Directive;
            return directive;

        }
        
        // Never use this
        [CanBeNull]
        public Directive GetDirective(string directiveType, (string key, string value)[] targets)
        {
            var temp = GetDirective(directiveType);
            temp.FillTargets(targets);
            return temp;
        }
    }
}
