using SpaceOdyssey.Planets;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace SpaceOdyssey.Patterns
{
    [System.Serializable]
    struct PatternResultData
    {
        public PlanetData ingredient;
        public PlanetData resultPlanet;
    }

    [System.Serializable]
    public struct V2
    {
        public int x, y;
        public V2(int x, int y)
        {
            this.x = x; this.y = y;
        }
    }

    [CreateAssetMenu(fileName = "New Pattern", menuName = "Patterns")]
    public class PatternData : ScriptableObject
    {
        public V2[] members;
        [SerializeField] private PatternResultData[] results;

        public PlanetData WhatIsTheResult(PlanetData planetInPattern)
        {
            for (int i = 0; i < results.Length; i++)
            {
                if (results[i].ingredient == null)
                    return results[i].resultPlanet;
                if (results[i].ingredient.IsMe(planetInPattern))
                    return results[i].resultPlanet;
            }
            return null;
        }
    }
}
