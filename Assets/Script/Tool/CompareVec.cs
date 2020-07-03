using System;
using UnityEngine;

namespace Assets.Script.Tool
{
    public class CompareVec : IComparable
    {
        public Vector3 vec;
        public CompareVec(Vector3 v)
        {
            vec = v;
        }

        public int CompareTo(object obj)
        {
            Vector3 next = Vector3.zero;
            try { next = ((CompareVec)obj).vec; } catch { return 0; }

            if (vec.x > next.x)
                return 1;
            else if (vec.x < next.x)
                return -1;
            else if (vec.y < next.y)
                return 1;
            else if (vec.y > next.y)
                return -1;
            else return 0;
        }
    }
}
