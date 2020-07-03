using Newtonsoft.Json;
using UnityEngine;

namespace Thunder.Tool.SerializableStruct
{
    [JsonObject]
    public struct SerializableVector3
    {
        public float[] pos;
        [JsonIgnore]
        public Vector3 inner;

        public SerializableVector3(float x, float y, float z)
        {
            pos = new float[3];
            pos[0] = x;
            pos[1] = y;
            pos[2] = z;
            inner = Vector3.zero;
        }

        public override string ToString()
        {
            if (inner.Equals(Vector3.zero))
            {
                if (pos == null)
                    inner = Vector3.zero;
                else
                    inner = new Vector3(pos[0], pos[1], pos[2]);
            }

            return inner.ToString();
        }

        public static implicit operator Vector3(SerializableVector3 s)
        {
            if (s.inner.Equals(Vector3.zero))
            {
                if (s.pos == null)
                    s.inner = Vector3.zero;
                else
                    s.inner = new Vector3(s.pos[0], s.pos[1], s.pos[2]);
            }

            return s.inner;
        }

        public static implicit operator SerializableVector3(Vector3 v)
        {
            return new SerializableVector3(v.x, v.y, v.z);
        }
    }
}
