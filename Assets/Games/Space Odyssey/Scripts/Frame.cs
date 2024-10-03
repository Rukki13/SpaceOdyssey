using SpaceOdyssey.Patterns;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SpaceOdyssey.Frames
{
    public class Frame : MonoBehaviour
    {
        public static Frame I;

        [Min(5)] public int width;
        [Min(5)] public int height;
        public float slotHeight;

        [Header("Slots")]
        [SerializeField] private FrameSlot slotPrefab;
        public FrameSlot[,] slots;

        private void Awake()
        {
            I = this;
            Initialize();
        }

        private void Start()
        {
        }

        public bool DoesSlotExist(V2 index)
        {
            return (index.x >= 0 && index.y >= 0 && index.x < width && index.y < height);
        }

        public void Initialize()
        {
            slots = new FrameSlot[width, height];
            Vector3 boardSize = GetComponent<SpriteRenderer>().bounds.size;
            Vector3 slotSize = new Vector3(boardSize.x / width, boardSize.x / width, 1);
            slotHeight = slotSize.y;
            ForEachSlot((slot, x, y) =>
            {
                slots[x, y] = InstantiateSlot(x, y, slotSize);
            });
        }

        private FrameSlot InstantiateSlot(int x, int y, Vector3 size)
        {
            Vector3 boardSize = GetComponent<SpriteRenderer>().bounds.size;
            FrameSlot slot = Instantiate(slotPrefab);
            float padding = size.x * 0.5f;
            Vector3 offset = new Vector3(-0.5f * boardSize.x, -0.5f * boardSize.y, 0);
            slot.transform.position = transform.position + offset+ new Vector3(
                padding + size.x * x,
                padding + size.y * y,
                0
            );
            slot.Initialize(x, y, size);
            return slot;
        }

        private void ForEachSlot(UnityAction<FrameSlot, int, int> callback)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    callback(slots[x, y], x, y);
                }
            }
        }
    }
}
