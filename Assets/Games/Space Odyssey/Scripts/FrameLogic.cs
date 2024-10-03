using Photon.Pun;
using SpaceOdyssey.Patterns;
using SpaceOdyssey.Planets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceOdyssey.Frames
{
    public class FrameLogic : MonoBehaviourPun
    {
        public static FrameLogic I;
        [HideInInspector] public bool isPlaying = false;

        private FrameSlot[,] slots;
        private V2 frameSize;
        private int height, width;

        [Header("Merges")]
        [SerializeField] private MergeData[] mergeDatas;

        [Header("Animations")]
        public float planetMovementSpeed;
        private List<FrameAnimation> animations = new List<FrameAnimation>();
        private List<FrameOperation> operations = new List<FrameOperation>();
        public bool isOperating => animations.Count != 0;

        [Header("Patterns")]
        [SerializeField] private PatternData[] patterns;

        private int blockOperationsAndAnimations = 0;

        [Header("Development")]
        [SerializeField] private bool debugMode = false;

        public FrameSlot GetFrameSlot(V2 index)
        {
            return slots[index.x, index.y];
        }

        private void Awake()
        {
            I = this;
            Frame frame = GetComponent<Frame>();
            height = frame.height;
            width = frame.width;
            frameSize = new V2(width, height);
        }
        private void Start()
        {
            Frame frame = GetComponent<Frame>();
            slots = frame.slots;
            blockOperationsAndAnimations = 0;
        }

        private void Update()
        {
            if (!isPlaying)
                return;

            if (debugMode && Input.GetKeyDown(KeyCode.Space) && animations.Count == 0)
            {
                UpdateFrame();
            }
        }

        private void FixedUpdate()
        {
            if (!isPlaying)
                return;
            if (blockOperationsAndAnimations != 0)
                return;

            for (int i = 0; i < animations.Count; i++)
            {
                animations[i].Tic();
                if (animations[i].IsOver())
                {
                    animations.RemoveAt(i);
                    i--;
                }
            }

            for (int i = 0; i < operations.Count; i++)
            {
                operations[i].Tic();
                if (operations[i].IsOver())
                {
                    Destroy(operations[i].gameObject);
                    operations.RemoveAt(i);
                    i--;
                }
            }

            if (!debugMode && animations.Count == 0 && operations.Count == 0)
            {
                UpdateFrame();
            }
        }


        public void UpdateFrame()
        {
            StartCoroutine(TicCoroutine());
        }

        private IEnumerator TicCoroutine()
        {
            Debug.Log("TIC()");
            blockOperationsAndAnimations++;

            bool performPatternMatching = true;
            if (TryMovePlanets())
            {
                Debug.Log("Planets are moving, skip pattern matching.");
                performPatternMatching = false;
            }

            if (performPatternMatching)
            {
                yield return AnimatePatternMatching();
            }

            blockOperationsAndAnimations--;
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
                            MovePlanet(x, y, x, y - 1);
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

        private bool DoesPatternApply(FrameSlot centerSlot, PatternData pattern)
        {
            bool valid = true;
            foreach (V2 member in pattern.members)
            {
                if (!GetPlanet(member.x + centerSlot.x, member.y + centerSlot.y, out Planet planet))
                {
                    valid = false;
                    break;
                }
                if (!planet.data.IsSynonym(centerSlot.planet.data))
                {
                    valid = false;
                    break;
                }
            }
            return valid;
        }

        private void ApplyPattern(FrameSlot centerSlot, PatternData patternData)
        {
            //Get result planet
            PlanetData result = patternData.WhatIsTheResult(centerSlot.planet.data);
            //Destroy pattern planets
            int x = centerSlot.x, y = centerSlot.y;
            foreach (V2 member in patternData.members)
            {
                DestroySlotContent(slots[x + member.x, y + member.y]);
            }
            //Spawn result planet
            if (result != null)
                SpawnSpecialPlanet(slots[x, y], result);
        }

        public void DestroySlotContent(FrameSlot slot)
        {
            if (slot.planet == null)
                return;

            PlanetData planetData = slot.planet.data;
            bool suppressDestroyEvents = slot.planet.suppressDestroyEvents;

            Destroy(slot.planet.gameObject);
            slot.planet = null;
            if (!suppressDestroyEvents)
            {
                if (planetData.onDestroyVFX != null)
                    VFXManager.I.InstantiateVFX(planetData.onDestroyVFX, slot.transform.position);
                if (planetData.operationOnDestroy != null)
                {
                    InstantiateFrameOperation(planetData.operationOnDestroy, slot);
                }
            }
        }

        private void InstantiateFrameOperation(FrameOperation frameOperation, FrameSlot originSlot)
        {
            frameOperation = Instantiate(frameOperation);
            operations.Add(frameOperation);
            frameOperation.Initialize(slots, frameSize, originSlot);
        }

        private bool DoesMoveCreatePattern(FrameSlot from, FrameSlot to)
        {
            //Switch planets
            Planet planet1 = from.planet;
            from.planet = to.planet;
            to.planet = planet1;

            //Check for existing patterns
            bool result = DoPatternsExist();

            //Return planets to their original slots
            planet1 = from.planet;
            from.planet = to.planet;
            to.planet = planet1;

            return result;
        }

        private bool DoPatternsExist()
        {
            foreach (PatternData patternData in patterns)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (DoesPatternApply(slots[x, y], patternData))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool ApplyExistingPatterns()
        {
            bool patternsExisted = false;
            foreach (PatternData patternData in patterns)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (DoesPatternApply(slots[x, y], patternData))
                        {
                            patternsExisted = true;
                            ApplyPattern(slots[x, y], patternData);
                        }
                    }
                }
            }
            return patternsExisted;
        }

        private WaitForSeconds PrePatternMatchingWait = new WaitForSeconds(0.15f);
        private WaitForSeconds PostPatternMatchingWait = new WaitForSeconds(0.3f);
        private IEnumerator AnimatePatternMatching()
        {
            //yield return PrePatternMatchingWait;
            blockOperationsAndAnimations++;
            if (ApplyExistingPatterns())
            {
                yield return PostPatternMatchingWait;
            }
            blockOperationsAndAnimations--;
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

        private bool GetMergeDataFor(PlanetData planetA, PlanetData planetB, out MergeData mergeData)
        {
            foreach (MergeData merge in mergeDatas)
            {
                if (merge.Check(planetA, planetB))
                {
                    mergeData = merge;
                    return true;
                }
            }
            mergeData = null;
            return false;
        }

        public void SpawnTopRowPlanet(FrameSlot slot)
        {
            Planet planet = PlanetGenerator.I.SpawnPlanet(slot);
            planet.transform.position += new Vector3(0, Frame.I.slotHeight, 0);

            MoveAnimation operation = new MoveAnimation(planet, slot, planetMovementSpeed);
            animations.Add(operation);
        }
        public void SpawnSpecialPlanet(FrameSlot slot, PlanetData planetData)
        {
            if (planetData == null)
                Debug.LogError("Failed to spawn special planet: planet data was not provided.");
            Planet planet = PlanetGenerator.I.SpawnPlanet(slot, planetData);
        }

        [PunRPC]
        private void MovePlanet(int fromX, int fromY, int toX, int toY)
        {
            FrameSlot fromSlot = slots[fromX, fromY];
            FrameSlot toSlot = slots[toX, toY];
            if (fromSlot.planet != null && toSlot.planet == null)//Move planet to empty slot
            {
                toSlot.planet = fromSlot.planet;
                fromSlot.planet = null;
                MoveAnimation operation = new MoveAnimation(toSlot.planet, toSlot, planetMovementSpeed);
                animations.Add(operation);
            }
            else if (fromSlot.planet != null && toSlot.planet != null) //Switch slots contents
            {
                if (GetMergeDataFor(fromSlot.planet.data, toSlot.planet.data, out MergeData mergeData))
                {
                    fromSlot.planet.suppressDestroyEvents = true;
                    toSlot.planet.suppressDestroyEvents = true;
                    MergeAnimation mergeAnimation = new MergeAnimation(fromSlot, toSlot, planetMovementSpeed, () =>
                    {
                        DestroySlotContent(fromSlot);
                        DestroySlotContent(toSlot);
                        InstantiateFrameOperation(mergeData.resultOperation, toSlot);
                    });
                    if (mergeData.startVFX)
                    {
                        VFXManager.I.InstantiateVFX(mergeData.startVFX, toSlot.transform.position);
                    }
                    animations.Add(mergeAnimation);
                }
                else if (DoesMoveCreatePattern(fromSlot, toSlot))
                {
                    //Switch the planets' slots
                    Planet planet1 = fromSlot.planet;
                    fromSlot.planet = toSlot.planet;
                    toSlot.planet = planet1;
                    //Create the operations, and rely on the system to detect the patterns again in the next Tic()
                    MoveAnimation operation1 = new MoveAnimation(toSlot.planet, toSlot, planetMovementSpeed);
                    animations.Add(operation1);
                    MoveAnimation operation2 = new MoveAnimation(fromSlot.planet, fromSlot, planetMovementSpeed);
                    animations.Add(operation2);
                }
                else
                {
                    FakeMoveAnimation operation = new FakeMoveAnimation(fromSlot, toSlot, planetMovementSpeed);
                    animations.Add(operation);
                }
            }
        }

        public void SyncMovePlanet(FrameSlot fromSlot, FrameSlot toSlot)
        {
            if (GameManager.isAgainstBot)
                MovePlanet(fromSlot.x, fromSlot.y, toSlot.x, toSlot.y);
            else
                photonView.RPC("MovePlanet", RpcTarget.All, fromSlot.x, fromSlot.y, toSlot.x, toSlot.y);
        }
    }
}