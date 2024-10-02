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
            if (planet == null)
                return true;
            if (Vector3.Distance(planet.transform.position, targetSlot.transform.position) < movementSpeed * Time.fixedDeltaTime)
            {
                planet.transform.position = targetSlot.transform.position;
                return true;
            }
            return false;
        }

        public override void FixedUpdate()
        {
            if (planet == null)
                return;
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
            if (isGoingBack == false)
            {
                if (ReachedSlot(fromSlot.planet, toSlot) && ReachedSlot(toSlot.planet, fromSlot))
                {
                    isGoingBack = true;
                }
                return false;
            }
            else
            {
                if (ReachedSlot(fromSlot.planet, fromSlot) && ReachedSlot(toSlot.planet, toSlot))
                {
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
            if (!isGoingBack)
            {
                MovePlanet(fromSlot.planet, toSlot);
                MovePlanet(toSlot.planet, fromSlot);
            }
            else
            {
                MovePlanet(toSlot.planet, toSlot);
                MovePlanet(fromSlot.planet, fromSlot);
            }

        }

        private void MovePlanet(Planet planet, FrameSlot slot)
        {
            Vector3 direction = planet.transform.position - slot.transform.position;
            planet.transform.position -= direction * movementSpeed * Time.fixedDeltaTime;
        }
    }
}
