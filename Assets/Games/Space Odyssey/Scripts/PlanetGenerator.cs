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

        public Planet SpawnPlanet(FrameSlot slot)
        {
            Planet planet = Instantiate(planetPrefab, slot.transform.position + new Vector3(0, Frame.I.slotHeight, 0), Quaternion.identity);
            slot.planet = planet;
            int planetNumber = Random.Range(0, planetDatas.Length);
            PlanetData planetData = planetDatas[planetNumber];
            planet.GetComponent<Planet>().SetIdentity(planetData);
            return planet;
        }
    }
}
