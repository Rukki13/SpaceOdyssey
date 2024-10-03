using SpaceOdyssey.Frames;
using SpaceOdyssey.Patterns;
using SpaceOdyssey.Turns;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceOdyssey.Players
{
    public class PlayerControls : MonoBehaviour
    {
        [SerializeField] private Camera camera;
        private Vector2 initialPosition;
        private Vector2 finalPosition;
        private enum State
        {
            Idle,
            Moving,
        }
        private State state = State.Idle;
        private FrameSlot selectedSlot ;

        private void Update()
        {
            if (FrameLogic.I.isOperating)
                return;
            DetectSlot();
        }
        private void DetectSlot()
        {
            if (!TurnManager.I.myTurn) 
                return;
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePosition = camera.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * 10);
                RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
                if (hit.collider != null)
                {
                    initialPosition = Input.mousePosition;

                    if (hit.collider.TryGetComponent(out FrameSlot frameSlot))
                    {
                        if (frameSlot.planet != null)
                        {
                            selectedSlot = frameSlot;
                            state = State.Moving;
                        }
                    }
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {

                if (state == State.Moving)
                {
                    V2 newSlotIndex=default;
                    finalPosition = Input.mousePosition;
                    state = State.Idle;
                    Vector2 dragDirection = finalPosition - initialPosition;
                    // Get the angle in degrees between the drag direction and the positive x-axis
                    float angle = Mathf.Atan2(dragDirection.y, dragDirection.x) * Mathf.Rad2Deg;

                    // Normalize angle to be between 0 and 360
                    if (angle < 0)
                    {
                        angle += 360;
                    }

                    if (angle >= 315 || angle < 45)
                    {
                        newSlotIndex = new V2(selectedSlot.x + 1, selectedSlot.y);
                    }
                    else if (angle >= 45 && angle < 135)
                    {
                        newSlotIndex = new V2(selectedSlot.x , selectedSlot.y + 1);
                    }
                    else if (angle >= 135 && angle < 225)
                    {
                        newSlotIndex = new V2(selectedSlot.x - 1, selectedSlot.y );
                    }
                    else if (angle >= 225 && angle < 315)
                    {
                        newSlotIndex = new V2(selectedSlot.x , selectedSlot.y - 1);
                    }
                    if (Frame.I.DoesSlotExist(newSlotIndex))
                    {
                        FrameLogic.I.SyncMovePlanet(selectedSlot, FrameLogic.I.GetFrameSlot(newSlotIndex));
                        TurnManager.I.OnFinishedTurn();
                    }
                }
            }
        }
    }
}
