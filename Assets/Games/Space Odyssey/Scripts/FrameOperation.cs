using SpaceOdyssey.Planets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceOdyssey.Frames
{
    public abstract class FrameOperation
    {
        public abstract void Update();
        public abstract bool IsOver();

    }

    public class MoveOperation : FrameOperation
    {
        private Planet planet;
        private FrameSlot targetSlot;
        private float movementSpeed;
        public MoveOperation(Planet planet, FrameSlot targetSlot, float movementSpeed)
        {
            this.planet = planet;
            this.targetSlot = targetSlot;
            this.movementSpeed = movementSpeed;
        }

        public override bool IsOver()
        {
            if (Vector3.Distance(planet.transform.position, targetSlot.transform.position) < movementSpeed * Time.fixedDeltaTime)
            {
                planet.transform.position = targetSlot.transform.position;
                return true;
            }
            return false;
        }

        public override void Update()
        {
            Vector3 direction = targetSlot.transform.position - planet.transform.position;
            direction.Normalize();
            planet.transform.position += direction * movementSpeed * Time.fixedDeltaTime;
        }
    }
}
