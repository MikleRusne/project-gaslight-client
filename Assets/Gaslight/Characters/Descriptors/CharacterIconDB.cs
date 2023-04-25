using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

[Serializable]
public struct CharacterIcon
{
    public string name;
    public Sprite icon;
}
[CreateAssetMenu(fileName= "CharacterIconDB",menuName = "Game/CharacterIconDB")]
public class CharacterIconDB : ScriptableObject
{
    public Sprite defaultIcon;
    public List<CharacterIcon> CharacterIcons;

    [CanBeNull]
    public Sprite GetIcon(string name)
    {
        if (CharacterIcons.Any((el) => el.name == name))
        {
            return CharacterIcons.Find((el) => el.name == name).icon;
        }
        else
        {
            // Debug.LogError("Could not find icon by name of "+ name + ". Valid names are");
            // CharacterIcons.ForEach(icon => Debug.LogError(icon.name));
            return defaultIcon;
        }
    }
}
