using SpaceOdyssey.Frames;
using SpaceOdyssey.Patterns;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceOdyssey
{
    public class CrossDestroyOperation : FrameOperation
    {
        //[SerializeField] private ParticleSystem explosionVFX;
        public override void Initialize(FrameSlot[,] slots, V2 frameSize, FrameSlot originSlot)
        {
            /*explosionVFX.transform.position = originSlot.transform.position;
            explosionVFX.Play();*/
            for (int y = 0; y < frameSize.y; y++)
            {
                FrameLogic.I.DestroySlotContent(slots[originSlot.x, y]);
            }
            for (int x = 0; x < frameSize.x; x++)
            {
                FrameLogic.I.DestroySlotContent(slots[x, originSlot.y]);
            }
        }

        public override void Tic()
        {

        }

        public override bool IsOver()
        {
            return true; //Instantanious
        }
    }
}
