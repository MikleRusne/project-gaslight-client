using System;
using Gaslight.Characters.Logic;

namespace Gaslight.Characters.Descriptors
{
    [Serializable]
    public struct CharacterDescriptor
    {
        public string name;
        public string archetype;
        public SimpleCharacter.NamedStringTrait[] stringTraitOverrides;
        public SimpleCharacter.NamedFloatTrait[] floatTraitOverrides;
        
    }

    
    [Serializable]
    public struct ArchetypeDescriptor
    {
        public string name;
        public Roles[] roles;
        public SimpleCharacter.NamedStringTrait[] stringTraits;
        public SimpleCharacter.NamedFloatTrait[] floatTraits;
    }
}