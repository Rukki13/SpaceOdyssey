using SpaceOdyssey.Frames;
using SpaceOdyssey.Planets;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace SpaceOdyssey
{
    public class FrameSlot : MonoBehaviour
    {
        public Planet planet;
        public bool IsEmpty => planet == null;
        public int x;
        public int y;
        public void Initialize(int x, int y, Vector3 size)
        {
            this.x = x;
            this.y = y;
            //transform.localScale = new Vector3(size.x / renderer.bounds.size.x, size.y / renderer.bounds.size.y, 1);
            transform.localScale = size;
        }
    }
}
