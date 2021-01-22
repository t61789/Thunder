using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Framework
{
    public static class ExtraFieldsDic
    {
        public static Dictionary<GameObject, ExtraFieldsForGameObject> GameObjectFields 
            = new Dictionary<GameObject, ExtraFieldsForGameObject>();

        public static ExtraFieldsForGameObject ExtraFields(this GameObject go)
        {
            if (GameObjectFields.TryGetValue(go, out var result))
            {
                return result;
            }

            var fields = new ExtraFieldsForGameObject();
            GameObjectFields.Add(go,fields);
            return fields;
        }
    }

    public partial class ExtraFieldsForGameObject
    {

    }
}
