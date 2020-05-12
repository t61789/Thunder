using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;

namespace Tool
{
    public struct SerializableVector3 : IXmlSerializable
    {
        public float x;
        public float y;
        public float z;

        public SerializableVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.Read();
            reader.ReadStartElement("x");
            x = reader.ReadContentAsFloat();
            reader.ReadEndElement();
            reader.ReadStartElement("y");
            y = reader.ReadContentAsFloat();
            reader.ReadEndElement();
            reader.ReadStartElement("z");
            z = reader.ReadContentAsFloat();
            reader.ReadEndElement();
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteElementString("x", x.ToString());
            writer.WriteElementString("y", y.ToString());
            writer.WriteElementString("z", z.ToString());
        }

        public static implicit operator SerializableVector3(Vector3 vector)
        {
            return new SerializableVector3(vector.x, vector.y, vector.z);
        }

        public static implicit operator Vector3(SerializableVector3 svector)
        {
            return new Vector3(svector.x, svector.y, svector.z);
        }
    }
}
