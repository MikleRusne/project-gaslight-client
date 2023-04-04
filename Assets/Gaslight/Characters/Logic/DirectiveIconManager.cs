using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gaslight.Characters.Logic
{
    [Serializable]
    public struct DirectiveIcon
    {
        public string name;
        public Sprite icon;
    }
    [CreateAssetMenu(fileName = "DirectiveIcons", menuName = "Game/DirectiveIcons")]
    public class DirectiveIconManager:ScriptableObject
    {
        public List<DirectiveIcon> icons;

        public Sprite GetIcon(String name)
        {
            return icons.Find((el) => el.name == name).icon;
        }
    }
}