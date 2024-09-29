using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceOdyssey.Planets
{
    [CreateAssetMenu(fileName = "New Planet", menuName = "Planets")]
    public class PlanetData : ScriptableObject
    {
        public Sprite skin;
    }
}
