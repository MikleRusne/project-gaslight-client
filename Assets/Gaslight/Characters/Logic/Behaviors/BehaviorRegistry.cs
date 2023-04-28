
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct BehaviorField
{
    public string name;
    public GameObject prefab;
}
[CreateAssetMenu(menuName = "Behaviors/Registry", fileName = "default registry")]
public class BehaviorRegistry : ScriptableObject
{
    public List<BehaviorField> behaviors;

    public bool isBehavior(string name)
    {
        return behaviors.Any(field => field.name == name);
    }
    public GameObject GetBehaviorPrefab(string name)
    {
        return behaviors.Find(field => field.name == name).prefab;
    }
}