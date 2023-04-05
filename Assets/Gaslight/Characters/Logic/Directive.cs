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
public abstract class GDirective
{

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
}

public class MoveDirective : GDirective
{
    private bool targetSet = false;
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

        if (Invoker is Actor)
        {
            await ((Actor)Invoker).MoveToTile(index);
        }
    }
    
}

public class AttackDirective : GDirective
{
    public bool targetSet = false;
    public int targetLocation;
    public override bool MoreConditions()
    {
        return !targetSet;
    }

    public override string Name => "attack";
    public override void AddTarget(int location)
    {
        targetLocation = location;
        targetSet = true;
    }

    public override bool IsLocationValid(int location)
    {
        //Check if there is a character there
        if (!Level.instance.isAnyCharacterOnTile(location))
        {
            return false;
        }
        var character = Level.instance.GetTileOccupant(location);
        
        if (character.faction != Invoker.faction)
        {
            return true;
        }
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
        await Task.Yield();
    }
}

[Serializable]
public class ForegoDirective : GDirective
{
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
}

public  class DirectiveFactory
    {
        private List<(string name, Type directive)> directivesByName;

        public DirectiveFactory()
        {
            var directiveTypes = Assembly.GetAssembly(typeof(GDirective)).GetTypes()
                .Where(someType=> someType.IsClass && (!someType.IsAbstract) && (someType.IsSubclassOf(typeof(GDirective))));
            directivesByName = new();
            
            foreach (var directiveType in directiveTypes)
            {
                var temp = Activator.CreateInstance(directiveType) as GDirective;
                directivesByName.Add((temp.Name, directiveType));
            }
        }

        [CanBeNull]
        public GDirective GetDirective(string directiveType)
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
            var directive = Activator.CreateInstance(requiredType) as GDirective;
            return directive;

        }
        
        // Never use this
        [CanBeNull]
        public GDirective GetDirective(string directiveType, (string key, string value)[] targets)
        {
            var temp = GetDirective(directiveType);
            temp.FillTargets(targets);
            return temp;
        }
    }
}
