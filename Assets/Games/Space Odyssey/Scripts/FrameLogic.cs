using SpaceOdyssey.Patterns;
using SpaceOdyssey.Planets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceOdyssey.Frames
{
    public class FrameLogic : MonoBehaviour
    {
        private FrameSlot[,] slots;
        private int height, width;

        [Header("Operations")]
        public float planetMovementSpeed;
        private List<FrameOperation> operations = new List<FrameOperation>();

        [Header("Patterns")]
        [SerializeField] private PatternData[] patterns;

        private bool performOperations = false;

        private void Start()
        {
            Frame frame = GetComponent<Frame>();
            slots = frame.slots;
            height = frame.height;
            width = frame.width;
            performOperations = true;
        }

        private void FixedUpdate()
        {
            if (!performOperations)
                return;
            
            for (int i = 0; i < operations.Count; i++)
            {
                operations[i].Update();
                if (operations[i].IsOver())
                {
                    operations.RemoveAt(i);
                    i--;
                }
            }
            if (operations.Count == 0)
            {
                StartCoroutine(TicCoroutine());
            }
        }

        private IEnumerator TicCoroutine()
        {
            performOperations = false;

            bool performPatternMatching = true;
            if (TryMovePlanets())
            {
                performPatternMatching = false;
            }

            if (performPatternMatching)
            {
                if (TryPatternMatching())
                {
                    yield return new WaitForSeconds(0.15f);
                }
            }

            performOperations = true;
        }

        private bool TryMovePlanets()
        {
            bool didMovePlanets = false;
            for (int y = 1; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!slots[x, y].IsEmpty)
                    {
                        if (slots[x, y - 1].IsEmpty)
                        {
                            MovePlanet(slots[x, y], slots[x, y - 1]);
                            didMovePlanets = true;
                            if (y == height - 1)
                            {
                                SpawnPlanet(slots[x, y]);
                            }
                        }
                    }
                    else if (y == height - 1)
                    {
                        SpawnPlanet(slots[x, y]);
                    }

                }
            }
            return didMovePlanets;
        }

        private bool TryPatternMatching()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    V2 center = new V2(x, y);
                    PatternData matchingPattern = null;
                    foreach (PatternData pattern in patterns)
                    {
                        bool oneWasMissing = false;
                        foreach (V2 member in pattern.members)
                        {
                            if (!GetPlanet(member.x + center.x, member.y + center.y, out Planet planet))
                            {
                                oneWasMissing = true;
                                break;
                            }
                            if (planet.data != slots[x, y].planet.data)
                            {
                                oneWasMissing = true;
                                break;
                            }
                        }
                        if (!oneWasMissing)
                        {
                            matchingPattern = pattern;
                            break;
                        }
                    }
                    if (matchingPattern != null)
                    {
                        foreach (V2 member in matchingPattern.members)
                        {
                            Destroy(slots[x + member.x, y + member.y].planet.gameObject);
                            slots[x + member.x, y + member.y].planet = null;
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private bool GetPlanet(int x, int y, out Planet planet)
        {
            if (x < 0 || x >= width || y < 0 || y >= height - 1)
            {
                planet = null;
                return false;
            }

            planet = slots[x, y].planet;
            if (planet == null)
                return false;
            return true;
        }


        public void SpawnPlanet(FrameSlot slot)
        {
            Planet planet = PlanetGenerator.I.SpawnPlanet(slot);

            MoveOperation operation = new MoveOperation(planet, slot, planetMovementSpeed);
            operations.Add(operation);
        }

        private void MovePlanet(FrameSlot fullSlot, FrameSlot emptySlot)
        {
            emptySlot.planet = fullSlot.planet;
            fullSlot.planet = null;
            MoveOperation operation = new MoveOperation(emptySlot.planet, emptySlot, planetMovementSpeed);
            operations.Add(operation);
        }
    }

}