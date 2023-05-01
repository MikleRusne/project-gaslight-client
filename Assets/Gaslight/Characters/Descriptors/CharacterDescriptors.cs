using System;
using Gaslight.Characters.Logic;

namespace Gaslight.Characters.Descriptors
{
    [Serializable]
    public struct CharacterDescriptor
    {
        public string name;
        public string archetype;
        public Character.NamedStringTrait[] stringTraitOverrides;
        public Character.NamedFloatTrait[] floatTraitOverrides;
        
    }

    
    [Serializable]
    public struct ArchetypeDescriptor
    {
        public string name;
        public Roles[] roles;
        public Character.NamedStringTrait[] stringTraits;
        public Character.NamedFloatTrait[] floatTraits;
    }
}