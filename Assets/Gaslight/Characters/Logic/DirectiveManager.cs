using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gaslight.Characters.Logic
{
    [Serializable]
    public struct RoleDirective
    {
        public Roles role;
        public string[] directives;
    }
    [CreateAssetMenu(fileName = "DirectiveManager", menuName = "Game/DirectiveManager")]
    public class DirectiveManager : ScriptableObject
    {
        [SerializeField] public List<RoleDirective> RoleDirectives;

        void OnValidate()
        {
            if (RoleDirectives == null)
            {
                RoleDirectives = new List<RoleDirective>();
            }

            ErrorOnDuplicate();
        }

        void ErrorOnDuplicate()
        {
            //N^2 but will worry about optimization later
            var duplicates = RoleDirectives
                .GroupBy(p => p.role)
                .Where(p => p.Count() > 1)
                .Select(p => p.Key).ToList();
            if (duplicates.Count > 0)
            {
                Debug.LogWarning("Found duplicates " + String.Join(", ", duplicates.ToArray()));
            }
        }

        public bool IsDirectiveValidForRole(Roles role, string directive)
        {
            if (RoleDirectives.Count(entry => entry.role == role) == 0)
            {
                Debug.Log("Role " + role.ToString() + " not found");
                return true;
            }

            var requiredRD = RoleDirectives.Find((ent) => ent.role == role);
            return requiredRD.directives.Contains(directive);
        }

        public string[] GetValidDirectivesForRole(Roles role)
        {
            if (RoleDirectives.Count(entry => entry.role == role) == 0)
            {
                Debug.Log("Role " + role.ToString() + " not found");
                return new string[] { };
            }

            var requiredRD = RoleDirectives.Find((ent) => ent.role == role);
            return requiredRD.directives;
        }
    }
}