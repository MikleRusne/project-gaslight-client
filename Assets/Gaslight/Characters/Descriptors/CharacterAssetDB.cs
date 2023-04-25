using System;
using System.Collections.Generic;
using System.Linq;
using Gaslight.Characters.Descriptors;
using JetBrains.Annotations;
using UnityEngine;

namespace LevelCreation
{
    [Serializable]
    public struct CharacterAsset
    {
        public string name;
        public GameObject Base;
        public string archetype;
    }    
[CreateAssetMenu(fileName = "CharacterAssetDB", menuName = "ScriptableObjects/DB/CharacterAsset")]
public class CharacterAssetDB : ScriptableObject
{
    [SerializeField]
    public List<CharacterAsset> characterAssets = new List<CharacterAsset>();

    [SerializeField] public Archetypes archetypes;
    //Prevents duplicates
    public void OnValidate()
    {
        if (characterAssets == null)
        {
            characterAssets = new List<CharacterAsset>();
        }
        //N^2 but will worry about optimization later
        var duplicates = characterAssets.GroupBy(p => p.name)
            .Where(p => p.Count() > 1)
            .Select(p=>p.Key).ToList();
        if (duplicates.Count > 0)
        {
            Debug.LogWarning("Found duplicates " + String.Join(", ",duplicates.ToArray()));
        }
    }

    //Null safe
    //Returns false if item not found
    //Be careful of newline, it won't show up when you're debugging but cause comparisons to fail
    public bool GetFromList(String key, out CharacterAsset? output, out ArchetypeDescriptor? archetype)
    {
        foreach (var la in characterAssets)
        {
            if (la.name == key)
            {
                var requiredArchetype = archetypes.ArchetypeDescriptorByName(la.archetype);
                if (requiredArchetype != null)
                {
                    archetype = requiredArchetype;
                    output = la;
                    return true;
                }
                else
                {
                    output = null;
                    archetype = null;
                    return false;
                }
            }

        }
        output = null;
        archetype = null;
        return false;
    }

    [CanBeNull]
    public SimpleCharacter GetCharacter(String input)
    {
       //Initialize and return a whole SimpleCharacter
       
       //Find the base
       if (characterAssets == null)
       {
           Debug.LogError("character assets null in descriptor serializable object");
       }
       CharacterAsset? requiredAsset = characterAssets.Find(asset => asset.name == input);
       if (!requiredAsset.HasValue || requiredAsset.Value.Base==null)
       {
           Debug.LogError("Invalid key " + input);
           return null;
       }

       var requiredGameObject = GameObject.Instantiate(requiredAsset.Value.Base);
       // requiredGameObject.name = requiredAsset.Value.name;
       var requiredCharacter = requiredGameObject.GetComponent<SimpleCharacter>();
       if (requiredCharacter == null)
       {
           Debug.LogError(input + " has no simple character script on it.");
           return null;
       }
       //Get the required archetype
       var archetype = archetypes.archetypes.Find(descriptor => descriptor.name == requiredAsset.Value.archetype);
       requiredCharacter.FillFromArchetype(archetype);
       return requiredCharacter;
    }

}


}


