using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Gaslight.Characters;
using UnityEditor.Compilation;
using UnityEngine;
using Debug = UnityEngine.Debug;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Assembly = System.Reflection.Assembly;

namespace Gaslight.Characters.Logic
{
    [Serializable]
    public enum Roles
    {
        Role1,
        Role2,
        BasicMovement
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
    public abstract bool IsLocationValid(SimpleCharacter invoker, int location);
    public abstract void StepCondition();
    public bool AllConditionsFulfillled;
    
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
    
    public override bool IsLocationValid(SimpleCharacter invoker, int location)
    {
        return true;
    }

    public override void StepCondition()
    {
        
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

[Serializable]
public class ForegoDirective : GDirective
{
    public override bool IsLocationValid(SimpleCharacter invoker, int location)
    {
        return true;
    }

    public override void StepCondition()
    {
        
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

    public static class GDirectiveFactory{
        public static GDirective CreateDirective(string directiveType, (string key, string value)[] targets)
        {
            switch (directiveType)
            {
                default:
                    throw new ArgumentException("Invalid directive type: " + directiveType);
            }
        }
    }

    public  class ReflectiveFactory
    {
        private List<(string name, Type directive)> directivesByName;

        public ReflectiveFactory()
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
