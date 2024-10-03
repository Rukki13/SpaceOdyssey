using SpaceOdyssey.Frames;
using SpaceOdyssey.Patterns;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceOdyssey
{
    public class VerticalDestroyOperation : FrameOperation
    {
        //[SerializeField] private ParticleSystem explosionVFX;

        public override void Initialize(FrameSlot[,] slots, V2 frameSize, FrameSlot originSlot)
        {
            
            for (int y = 0; y < frameSize.y; y++)
            {
                FrameLogic.I.DestroySlotContent(slots[originSlot.x, y]);
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
