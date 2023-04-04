using System;
using System.Collections.Generic;
using System.Linq;
using Gaslight.Characters.Logic;
using UnityEngine;

namespace Gaslight.Characters.Descriptors
{
    
    [CreateAssetMenu(menuName = "Game/Archetypes")]
    public class Archetypes: ScriptableObject
    {
        [SerializeField] public List<ArchetypeDescriptor> archetypes;

        public ArchetypeDescriptor? ArchetypeDescriptorByName(String name)
        {
            if (archetypes.Any((arch) => arch.name == name))
            {
                return archetypes.Find((arch) => arch.name == name);
            }

            return null;
        }
    }
}