using SpaceOdyssey.Planets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SpaceOdyssey.Frames
{
    public abstract class FrameAnimation
    {
        public abstract void Tic();
        public abstract bool IsOver();

    }

    public class MoveAnimation : FrameAnimation
    {
        private Planet planet;
        private FrameSlot targetSlot;
        private float movementSpeed;
        public MoveAnimation(Planet planet, FrameSlot targetSlot, float movementSpeed)
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

        public override void Tic()
        {
            if (planet == null)
                return;
            Vector3 direction = targetSlot.transform.position - planet.transform.position;
            direction.Normalize();
            planet.transform.position += direction * movementSpeed * Time.fixedDeltaTime;
        }
    }

    public class FakeMoveAnimation : FrameAnimation
    {
        private FrameSlot fromSlot;
        private FrameSlot toSlot;
        private float movementSpeed;
        private bool isGoingBack = false;

        public FakeMoveAnimation(FrameSlot fromSlot, FrameSlot toSlot, float movementSpeed)
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

        public override void Tic()
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

    public class MergeAnimation : FrameAnimation
    {
        private FrameSlot fromSlot;
        private FrameSlot toSlot;
        private float movementSpeed;
        private UnityAction callback;

        public MergeAnimation(FrameSlot fromSlot, FrameSlot toSlot, float movementSpeed, UnityAction callback)
        {
            this.fromSlot = fromSlot;
            this.toSlot = toSlot;
            this.movementSpeed = movementSpeed;
            this.callback = callback;
        }

        public override bool IsOver()
        {
            if (ReachedTarget(fromSlot.planet, toSlot.planet))
            {
                callback();
                return true;
            }
            return false;
        }

        private bool ReachedTarget(Planet from, Planet to)
        {
            return Vector3.Distance(from.transform.position, to.transform.position) < movementSpeed * Time.fixedDeltaTime;
        }

        public override void Tic()
        {
            MovePlanet(fromSlot.planet, toSlot);
        }

        private void MovePlanet(Planet planet, FrameSlot slot)
        {
            Vector3 direction = slot.planet.transform.position - planet.transform.position;
            planet.transform.position += direction * movementSpeed * Time.fixedDeltaTime;
        }
    }
}
