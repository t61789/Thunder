using Newtonsoft.Json;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;

namespace Tool
{
    [JsonObject]
    public struct SerializableVector3
    {
        public float x;
        public float y;
        public float z;
        [JsonIgnore]
        public Vector3 inner;

        public SerializableVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            inner = Vector3.positiveInfinity;
        }

        public override string ToString()
        {
            if (!inner.Equals(Vector3.positiveInfinity))
                inner = new Vector3(x, y, z);

            return inner.ToString();
        }

        public static implicit operator Vector3(SerializableVector3 s)
        {
            if (!s.inner.Equals(Vector3.positiveInfinity) )
                return s.inner;
            else
            {
                s.inner = new Vector3(s.x,s.y,s.z);
                return s.inner;
            }
        }

        public static implicit operator SerializableVector3(Vector3 v)
        {
            return new SerializableVector3(v.x, v.y, v.z);
        }
    }
}
