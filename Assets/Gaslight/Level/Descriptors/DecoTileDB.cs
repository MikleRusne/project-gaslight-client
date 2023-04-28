using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

    [CreateAssetMenu(fileName = "DefaultDecoTileDB", menuName = "Level/DecoTileDB")]

    public class DecoTileDB: ScriptableObject
    {
        [SerializeField]
        public List<DecoTileAsset> DecoTileAssets;
    
        //Prevents duplicates
        public void OnValidate()
        {
            if (DecoTileAssets == null)
            {
                DecoTileAssets = new List<DecoTileAsset>();
            }
            //N^2 but will worry about optimization later
            var duplicates = DecoTileAssets.GroupBy(p => p.Key).Where(p => p.Count() > 1).Select(p=>p.Key).ToList();
            if (duplicates.Count > 0)
            {
                Debug.LogWarning("Found duplicates " + String.Join(", ",duplicates.ToArray()));
            }
        }

        //Null safe
        //Returns false if item not found
        //Be careful of newline, it won't show up when you're debugging but cause comparisons to fail
        public bool GetFromList(String key, out DecoTileAsset output)
        {
            foreach (var la in DecoTileAssets)
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
            return (DecoTileAssets.Select(p => p.Key)).ToArray();
        }

    }
