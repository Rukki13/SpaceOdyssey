using SpaceOdyssey.Frames;
using SpaceOdyssey.Patterns;
using UnityEngine;

namespace SpaceOdyssey
{
    public abstract class FrameOperation : MonoBehaviour
    {
        public abstract void Initialize(FrameSlot[,] slots, V2 frameSize, FrameSlot originSlot);
        public abstract void Tic();
        public abstract bool IsOver();
    }
}
