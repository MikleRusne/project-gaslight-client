using System;
using UnityEngine;

    [Serializable]
    public class BaseTileAsset
    {
        [SerializeField]
        public GameObject Asset;
        [SerializeField]
        public String Key;

        [SerializeField] public bool traversible;
        [SerializeField] public float heightOffset;
        [SerializeField] public int traversingCost;
        void OnValidate()
        {
        
        }    
    }
    [Serializable]
    public class DecoTileAsset
    {
        [SerializeField]
        public GameObject Asset;
        [SerializeField]
        public String Key;

        void OnValidate()
        {
        
        }    
    }
