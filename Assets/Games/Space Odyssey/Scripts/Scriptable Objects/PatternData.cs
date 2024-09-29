using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceOdyssey.Patterns
{
    [System.Serializable]
    public struct V2
    {
        public int x, y;
        public V2(int x, int y)
        {
            this.x = x; this.y = y;
        }
    }
 
    [CreateAssetMenu (fileName = "New Pattern", menuName = "Patterns")]
    public class PatternData : ScriptableObject
    {
        public V2[] members;
    }
}
