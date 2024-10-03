using SpaceOdyssey.Planets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceOdyssey
{
    [CreateAssetMenu(fileName = "MergeData", menuName = "Merge Data")]
    public class MergeData : ScriptableObject
    {
        public PlanetData planet1;
        public PlanetData planet2;
        public FrameOperation resultOperation;
        public ParticleSystem startVFX;

        public bool Check(PlanetData planetA, PlanetData planetB)
        {
            return (planet1.IsSynonym(planetA) && planet2.IsSynonym(planetB)) ||
               (planet2.IsSynonym(planetA) && planet1.IsSynonym(planetB));
        }
    }
}
