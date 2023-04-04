using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LevelCreation
{
    [CreateAssetMenu(fileName = "DefaultBaseTileDB", menuName = "Level/BaseTileDB")]
    public class BaseTileDB: ScriptableObject
    {
        [SerializeField]
        public List<BaseTileAsset> BaseTileAssets;
    
        //Prevents duplicates
        public void OnValidate()
        {
            if (BaseTileAssets == null)
            {
                BaseTileAssets = new List<BaseTileAsset>();
            }
            //N^2 but will worry about optimization later
            var duplicates = BaseTileAssets.GroupBy(p => p.Key).Where(p => p.Count() > 1).Select(p=>p.Key).ToList();
            if (duplicates.Count > 0)
            {
                Debug.LogWarning("Found duplicates " + String.Join(", ",duplicates.ToArray()));
            }
        }

        //Null safe
        //Returns false if item not found
        //Be careful of newline, it won't show up when you're debugging but cause comparisons to fail
        public bool GetFromList(String key, out BaseTileAsset output)
        {
            foreach (var la in BaseTileAssets)
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
            return (BaseTileAssets.Select(p => p.Key)).ToArray();
        }

    }
}