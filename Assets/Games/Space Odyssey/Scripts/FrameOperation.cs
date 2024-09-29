using SpaceOdyssey.Planets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceOdyssey.Frames
{
    public abstract class FrameOperation
    {
        public abstract void FixedUpdate();
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

        public override void FixedUpdate()
        {
            Vector3 direction = targetSlot.transform.position - planet.transform.position;
            direction.Normalize();
            planet.transform.position += direction * movementSpeed * Time.fixedDeltaTime;
        }
    }

    public class FakeMoveOperation : FrameOperation
    {
        private FrameSlot fromSlot;
        private FrameSlot toSlot;
        private float movementSpeed;
        private bool isGoingBack = false;

        public FakeMoveOperation(FrameSlot fromSlot, FrameSlot toSlot, float movementSpeed)
        {
            this.fromSlot = fromSlot;
            this.toSlot = toSlot;
            this.movementSpeed = movementSpeed;
        }

        public override bool IsOver()
        {
            if (!isGoingBack)
            {
                if (ReachedSlot(fromSlot.planet, toSlot) && ReachedSlot(toSlot.planet, fromSlot))
                {
                    Debug.Log("Will now go back!");
                    fromSlot.planet.transform.position = toSlot.transform.position;
                    toSlot.planet.transform.position = fromSlot.transform.position;
                    isGoingBack = true;
                }
                return false;
            }
            else
            {
                if (ReachedSlot(fromSlot.planet, fromSlot) && ReachedSlot(toSlot.planet, toSlot))
                {
                    Debug.Log("Finished Fake!");
                    fromSlot.planet.transform.position = fromSlot.transform.position;
                    toSlot.planet.transform.position = toSlot.transform.position;
                    return true;
                }
                return false;
            }
        }

        private bool ReachedSlot(Planet planet, FrameSlot slot)
        {
            return Vector3.Distance(planet.transform.position, slot.transform.position) < movementSpeed * Time.fixedDeltaTime;
        }

        public override void FixedUpdate()
        {
            Vector3 direction = -fromSlot.planet.transform.position + (isGoingBack? fromSlot.transform.position : toSlot.transform.position);
            direction.Normalize();
            fromSlot.planet.transform.position += direction * movementSpeed * Time.fixedDeltaTime;
            toSlot.planet.transform.position += -direction * movementSpeed * Time.fixedDeltaTime;
        }
    }
}
