
using System;
using Characters;
using UnityEngine;

namespace LevelCreation{

    public enum LevelAssetType
    {
        LevelObjectBase,
        LevelObjectDeco,
        Character
    }
    [Serializable]
    public class LevelAsset
    {
        [SerializeField] public LevelAssetType levelAssetType;
        [SerializeField]
        public GameObject Asset;
        [SerializeField]
        public String Key;

        [SerializeField] public bool isBlocking;
        [SerializeField] public float tileHeightOffset =0.0f;
        void OnValidate()
        {
            if (levelAssetType == LevelAssetType.LevelObjectBase)
            {
                               
            }
            if (levelAssetType == LevelAssetType.LevelObjectDeco || levelAssetType == LevelAssetType.LevelObjectBase)
            {
            }
        }    
    }
}
