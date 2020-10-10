using System;
using UnityEngine;

namespace Thunder.Tool
{
    public class CompareVec : IComparable
    {
        public Vector3 Vec;

        public CompareVec(Vector3 v)
        {
            Vec = v;
        }

        public int CompareTo(object obj)
        {
            Vector3 next;
            try
            {
                next = ((CompareVec) obj).Vec;
            }
            catch
            {
                return 0;
            }

            if (Vec.x > next.x)
                return 1;
            if (Vec.x < next.x)
                return -1;
            if (Vec.y < next.y)
                return 1;
            if (Vec.y > next.y)
                return -1;
            return 0;
        }
    }
}