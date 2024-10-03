using SpaceOdyssey.Frames;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace SpaceOdyssey.Planets
{
    public class PlanetGenerator : MonoBehaviour
    {
        public static PlanetGenerator I;

        [SerializeField] private Planet planetPrefab;
        [SerializeField] private PlanetData[] planetDatas;

        public System.Random random;

        private void Awake()
        {
            I = this;

        }

        public void Initialize(int seed )
        {
            random = new System.Random(seed);
        }
        public Planet SpawnPlanet(FrameSlot slot,PlanetData planetData = null)
        {
            if (planetData == null)
            {
                int planetNumber = random.Next(0, planetDatas.Length);
                planetData = planetDatas[planetNumber];
            }

            Planet planet = Instantiate(planetPrefab, slot.transform.position, Quaternion.identity);
            slot.planet = planet;
            planet.GetComponent<Planet>().SetIdentity(planetData);
            return planet;
        }
    }
}
