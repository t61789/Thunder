using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Tool
{
    [XmlRoot("dictionary")]
    public struct SerializableDictionary<K, V> : IXmlSerializable
    {
        private Dictionary<K, V> dic;
        private XmlSerializer kSerializer;
        private XmlSerializer vSerializer;

        public SerializableDictionary(Dictionary<K, V> dic)
        {
            this.dic = dic;
            kSerializer = new XmlSerializer(typeof(K));
            vSerializer = new XmlSerializer(typeof(V));
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            dic = new Dictionary<K, V>();
            if (reader.IsEmptyElement) return;
            reader.Read();
        }

        public void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException();
        }

        public static implicit operator SerializableDictionary<K, V>(Dictionary<K, V> dic)
        {
            return new SerializableDictionary<K, V>(dic);
        }
    }
}
