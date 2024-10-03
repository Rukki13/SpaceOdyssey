using SpaceOdyssey.Planets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceOdyssey
{
    public class VFXManager : MonoBehaviour
    {
        public static VFXManager I;

        private void Awake()
        {
            I = this;
        }

        public void InstantiateVFX(ParticleSystem vfx, Vector3 position)
        {
            vfx = Instantiate(vfx, position, Quaternion.identity);
            Destroy(vfx.gameObject, vfx.main.startLifetime.constant + 0.5f);
        }
    }
}
