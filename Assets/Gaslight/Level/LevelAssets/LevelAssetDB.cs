using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LevelCreation
{
    
[CreateAssetMenu(fileName = "LevelAssetDB", menuName = "ScriptableObjects/Level/LevelAssetDB")]
public class LevelAssetDB : ScriptableObject
{
    [SerializeField]
    public List<LevelAsset> LevelObjects;
    
    //Prevents duplicates
    public void OnValidate()
    {
        if (LevelObjects == null)
        {
            LevelObjects = new List<LevelAsset>();
        }
        //N^2 but will worry about optimization later
        var duplicates = LevelObjects.GroupBy(p => p.Key).Where(p => p.Count() > 1).Select(p=>p.Key).ToList();
        if (duplicates.Count > 0)
        {
            Debug.LogWarning("Found duplicates " + String.Join(", ",duplicates.ToArray()));
        }
    }

    //Null safe
    //Returns false if item not found
    //Be careful of newline, it won't show up when you're debugging but cause comparisons to fail
    public bool GetFromList(String key, out LevelAsset output)
    {
        foreach (var la in LevelObjects)
        {
            if (la.Key == key)
            {
                output = la;
                return true;
            }
            else
            {
                // Debug.Log(la.Key + " and " + key +" are not equal.");
            }
            
        }
        output = null;
        return false;
    }

    public String[] ValidCandidates()
    {
        return (LevelObjects.Select(p => p.Key)).ToArray();
    }
    
}


}

