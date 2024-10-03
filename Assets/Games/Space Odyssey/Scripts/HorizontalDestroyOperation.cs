using SpaceOdyssey.Frames;
using SpaceOdyssey.Patterns;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceOdyssey
{
    public class HorizontalDestroyOperation : FrameOperation
    {
        //[SerializeField] private ParticleSystem explosionVFX;

        public override void Initialize(FrameSlot[,] slots, V2 frameSize, FrameSlot originSlot)
        {
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
