using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  Gaslight.UI
{
    public class CharacterUIController : MonoBehaviour
    {
        [Foldout("Health bar")]
        public GameObject SegmentFilledPrefab;
        [Foldout("Health bar")]
        public GameObject SegmentEmptyPrefab;
        [Foldout("Health bar")]
        public GameObject HealthBarLayoutGroup;

        [Foldout("Health bar")]
        public int _health;
        [Foldout("Health bar")]
        public int _maxHealth;


        [Foldout("Action bar")] public int _actionPoints;
        [Foldout("Action bar")] public int _maxActionPoints;
        [Foldout("Action bar")] public GameObject _actionPointsLayoutGroup;
        [Foldout("Action bar")] public GameObject _actionPointsEmptyPrefab;
        [Foldout("Action bar")] public GameObject _actionPointsFilledPrefab;
        
        
        public Character Invoker;

        void Start()
        {
            if (Invoker != null)
            {
                CreateHealthbar(GetHealthbarValuesFromInvoker());
                CreateActionPointsbar(GetActionbarValuesFromInvoker());
                Invoker.HealthChanged.AddListener(OnHealthChanged);
                Invoker.ActionPointsChanged.AddListener(OnActionPointsChanged);
                Invoker.FloatTraitChanged.AddListener(OnFloatTraitChanged);
            }
        }

        private void OnHealthChanged(int arg0)
        {
            CreateHealthbar(GetHealthbarValuesFromInvoker());
        }

        private void OnActionPointsChanged(int old)
        {
            CreateActionPointsbar(GetActionbarValuesFromInvoker());
        }

        public void CreateHealthbar((int health, int maxHealth) values)
        {
            CreateHealthbar(values.health, values.maxHealth);
        }

        public void CreateHealthbar(int health, int maxHealth)
        {
            if (HealthBarLayoutGroup == null || SegmentFilledPrefab == null || SegmentEmptyPrefab == null)
            {
                Debug.LogError("Health bar layout group, segment filled prefab or segment empty prefab not set up correctly");
                return;
            }
            // Debug.Log("Making health bar");
            _health = health;
            _maxHealth = maxHealth;
            //Empty the health bar layout group
            for (int i = HealthBarLayoutGroup.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(HealthBarLayoutGroup.transform.GetChild(i).gameObject);
            }
            //Create new ones
            //Empties first
            for (int i = 0; i < maxHealth - health; i++)
            {
                var newEmptySegment = Instantiate(SegmentEmptyPrefab, HealthBarLayoutGroup.transform, false);
            }
            for (int i = 0; i < health; i++)
            {
                var newFilledSegment = Instantiate(SegmentFilledPrefab, HealthBarLayoutGroup.transform, false);
            }
        }
        public void CreateActionPointsbar((int ap, int maxap) values)
        {
            CreateActionPointsbar(values.ap, values.maxap);
        }

        public void CreateActionPointsbar(int ap, int maxap)
        {
            if (_actionPointsLayoutGroup == null || _actionPointsFilledPrefab == null || _actionPointsEmptyPrefab == null)
            {
                Debug.LogError("AP bar layout group, AP filled prefab or AP empty prefab not set up correctly");
                return;
            }
            // Debug.Log("Making AP bar");
            _actionPoints = ap;
            _maxActionPoints = maxap;
            //Empty the health bar layout group
            for (int i = _actionPointsLayoutGroup.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(_actionPointsLayoutGroup.transform.GetChild(i).gameObject);
            }
            //Create new ones
            //Empties first
            for (int i = 0; i < maxap - ap; i++)
            {
                var newEmptySegment = Instantiate(_actionPointsEmptyPrefab, _actionPointsLayoutGroup.transform, false);
            }
            for (int i = 0; i < ap; i++)
            {
                var newFilledSegment = Instantiate(_actionPointsFilledPrefab, _actionPointsLayoutGroup.transform, false);
            }
        }

        public (int health, int maxHealth) GetHealthbarValuesFromInvoker()
        {
            int charHealth = Invoker.health;
            int charMaxHealth = Mathf.FloorToInt(Invoker.GetFloatTrait("max health"));
            return (charHealth, charMaxHealth);
        }
        public (int ap, int maxap) GetActionbarValuesFromInvoker()
        {
            int charAp = Invoker.actionPoints;
            int charMaxAP = Mathf.FloorToInt(Invoker.GetFloatTrait("max action points"));
            return (charAp, charMaxAP);
        }
        public void OnFloatTraitChanged((string name, float oldValue) trait)
        {
        }
    }

}
