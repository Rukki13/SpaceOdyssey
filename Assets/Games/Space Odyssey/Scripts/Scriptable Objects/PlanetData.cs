using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceOdyssey.Planets
{
    [CreateAssetMenu(fileName = "New Planet", menuName = "Planets")]
    public class PlanetData : ScriptableObject
    {
        public Sprite skin;
        public PlanetData[] synonyms;

        public bool IsMe(PlanetData planetData)
        {
            if(this == planetData)  return true;
            
            for (int i = 0; i < synonyms.Length; i++)
            {
                if (synonyms[i] == planetData) 
                    return true;
                for (int j = 0; j < planetData.synonyms.Length; j++)
                {
                    if (planetData.synonyms[j] == planetData) 
                        return true;
                    if (synonyms[i] == planetData.synonyms[j])
                        return true;
                }
            }
            return false;
        }
    }
}
