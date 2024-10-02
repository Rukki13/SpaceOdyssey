using SpaceOdyssey.Patterns;
using SpaceOdyssey.Planets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceOdyssey.Frames
{
    public class FrameLogic : MonoBehaviour
    {
        public static FrameLogic I;

        private FrameSlot[,] slots;
        private int height, width;

        [Header("Operations")]
        public float planetMovementSpeed;
        private List<FrameOperation> operations = new List<FrameOperation>();
        public bool isOperating => operations.Count != 0;

        [Header("Patterns")]
        [SerializeField] private PatternData[] patterns;

        private bool performOperations = false;

        public FrameSlot GetFrameSlot(V2 index)
        {
            return slots[index.x, index.y];
        }

        private void Awake()
        {
            I = this;
        }
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
                operations[i].FixedUpdate();
                if (operations[i].IsOver())
                {
                    operations.RemoveAt(i);
                    i--;
                }
            }
            if (operations.Count == 0)
                UpdateFrame();
        }


        public void UpdateFrame()
        {
            StartCoroutine(TicCoroutine());
        }

        private IEnumerator TicCoroutine()
        {
            Debug.Log("TIC()");
            performOperations = false;

            bool performPatternMatching = true;
            if (TryMovePlanets())
            {
                Debug.Log("Planets are moving, skip pattern matching.");
                performPatternMatching = false;
            }

            if (performPatternMatching)
            {
                if (TryPatternMatching())
                {
                    yield return new WaitForSeconds(0.15f); //freeze for a bit
                }
            }

            performOperations = true;

            Debug.Log("END TIC");
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
                                SpawnTopRowPlanet(slots[x, y]);
                            }
                        }
                    }
                    else if (y == height - 1)
                    {
                        SpawnTopRowPlanet(slots[x, y]);
                    }

                }
            }
            return didMovePlanets;
        }

        private bool TryPatternMatching(bool executePattern = true)
        {
            Debug.Log("TryPAtternMAtching()");
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    V2 center = new V2(x, y);
                    PatternData matchingPattern = null;
                    foreach (PatternData pattern in patterns)
                    {
                        //Debug.Log("Analysing pattern:" + pattern.name);
                        bool oneWasMissing = false;
                        foreach (V2 member in pattern.members)
                        {
                            if (!GetPlanet(member.x + center.x, member.y + center.y, out Planet planet))
                            {
                                oneWasMissing = true;
                                break;
                            }
                            if (!planet.data.IsMe(slots[x, y].planet.data))
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
                        Debug.Log("Found pattern: " + matchingPattern.name);
                        if (executePattern)
                        {
                            PlanetData result = matchingPattern.WhatIsTheResult(slots[x, y].planet.data);
                            foreach (V2 member in matchingPattern.members)
                            {
                                Destroy(slots[x + member.x, y + member.y].planet.gameObject);
                                slots[x + member.x, y + member.y].planet = null;
                            }

                            if (result != null)
                                SpawnSpecialPlanet(slots[x, y], result);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private bool GetPlanet(int x, int y, out Planet planet)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                planet = null;
                return false;
            }

            planet = slots[x, y].planet;
            if (planet == null)
                return false;
            return true;
        }


        public void SpawnTopRowPlanet(FrameSlot slot)
        {
            Planet planet = PlanetGenerator.I.SpawnPlanet(slot);
            planet.transform.position += new Vector3(0, Frame.I.slotHeight, 0);

            MoveOperation operation = new MoveOperation(planet, slot, planetMovementSpeed);
            operations.Add(operation);
        }
        public void SpawnSpecialPlanet(FrameSlot slot, PlanetData planetData)
        {
            Planet planet = PlanetGenerator.I.SpawnPlanet(slot, planetData);
        }

        public void MovePlanet(FrameSlot fromSlot, FrameSlot toSlot)
        {
            if (fromSlot.planet != null && toSlot.planet == null)//Move planet to empty slot
            {
                toSlot.planet = fromSlot.planet;
                fromSlot.planet = null;
                MoveOperation operation = new MoveOperation(toSlot.planet, toSlot, planetMovementSpeed);
                operations.Add(operation);
            }
            else if (fromSlot.planet != null && fromSlot.planet != null) //Switch slots contents
            {
                Planet planet1 = fromSlot.planet;
                fromSlot.planet = toSlot.planet;
                toSlot.planet = planet1;
                if (TryPatternMatching(false))
                {
                    MoveOperation operation1 = new MoveOperation(toSlot.planet, toSlot, planetMovementSpeed);
                    operations.Add(operation1);
                    MoveOperation operation2 = new MoveOperation(fromSlot.planet, fromSlot, planetMovementSpeed);
                    operations.Add(operation2);
                }
                else
                {
                    planet1 = fromSlot.planet;
                    fromSlot.planet = toSlot.planet;
                    toSlot.planet = planet1;
                    FakeMoveOperation operation = new FakeMoveOperation(fromSlot, toSlot, planetMovementSpeed);
                    operations.Add(operation);
                }
            }

        }
    }
}