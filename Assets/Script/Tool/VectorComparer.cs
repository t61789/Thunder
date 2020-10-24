using System.Collections.Generic;
using UnityEngine;

namespace Thunder.Tool
{
    public class VectorComparer : Comparer<Vector3>
    {
        public new static readonly VectorComparer Default;
        public bool Des;

        static VectorComparer()
        {
            Default = new VectorComparer(false);
        }

        public VectorComparer(bool des)
        {
            Des = des;
        }

        public override int Compare(Vector3 x, Vector3 y)
        {
            int result;
            if (x.x > y.x)
                result = 1;
            else if (x.x < y.x)
                result = -1;
            else if (x.y > y.y)
                result = 1;
            else if (x.y < y.y)
                result = -1;
            else
                result = 0;
            if (Des) result = -result;
            return result;
        }
    }
}