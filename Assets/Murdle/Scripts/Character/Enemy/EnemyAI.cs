using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    public enum EHostilityState
    {
        None,
        Searching,
        Found
    }

    public enum EActionState
    {
        Idle,
        Roaming,
        Chasing
    }
    
    public class EnemyAI : MonoBehaviour
    {
        public EHostilityState HostilityState = EHostilityState.None;
        public EActionState ActionState = EActionState.Idle;
        void Start()
        {
        
        }

        void Update()
        {
        
        }
    }
    
}
