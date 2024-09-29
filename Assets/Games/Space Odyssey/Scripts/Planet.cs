using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceOdyssey.Planets
{
    public class Planet : MonoBehaviour
    {
        public PlanetData data;
        public void SetIdentity(PlanetData planetData)
        {
            GetComponent<SpriteRenderer>().sprite = planetData.skin;
            data = planetData;
        }
    }
}