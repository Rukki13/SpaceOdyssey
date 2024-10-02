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

        private void Awake()
        {
            I = this;
        }

        public Planet SpawnPlanet(FrameSlot slot,PlanetData planetData = null)
        {
            if (planetData == null)
            {
                int planetNumber = Random.Range(0, planetDatas.Length);
                planetData = planetDatas[planetNumber];
            }

            Planet planet = Instantiate(planetPrefab, slot.transform.position, Quaternion.identity);
            slot.planet = planet;
            planet.GetComponent<Planet>().SetIdentity(planetData);
            return planet;

        }
    }
}
