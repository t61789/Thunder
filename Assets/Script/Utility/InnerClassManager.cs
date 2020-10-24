using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Thunder.Tool;

namespace Thunder.Utility
{
    public class InnerClassManager
    {
        private readonly Type[] _Interfaces = {
            typeof(IHostDestroyed),
            typeof(IHostAwaked)
        };

        private readonly LRUCache<Type, MethodInfo[]> _AvaliableMethodsCache =
            new LRUCache<Type, MethodInfo[]>(40);
        
        private readonly object _Host;
        private readonly string[] _RequireMethods;
        private readonly object[] _Inners;

        public InnerClassManager(object host)
        {
            _Host = host;
            _Inners = GetInners(_Host);
            _RequireMethods = (from ifs in _Interfaces
                select ifs.GetMethods().First().Name).ToArray();
        }

        public void InvokeDestroyed()
        {
            Invoke(typeof(IHostDestroyed));
        }

        public void InvokeAwaked()
        {
            Invoke(typeof(IHostAwaked));
        }

        private void Invoke(Type interfaceType)
        {
            int index = _Interfaces.FindIndex(x => x == interfaceType);
            foreach (var inner in _Inners)
                GetAvaliableMethods(inner.GetType())[index]?.Invoke(inner, new[] { _Host });
        }

        private MethodInfo[] GetAvaliableMethods(Type innerType)
        {
            if (_AvaliableMethodsCache.TryGet(innerType, out var result))
                return result;

            result = new MethodInfo[_Interfaces.Length];
            int count = 0;
            foreach (var methodName in _RequireMethods)
            {
                result[count] = innerType.GetMethod(methodName);
                count++;
            }
            _AvaliableMethodsCache.Put(innerType, result);
            return result;
        }

        private object[] GetInners(object host)
        {
            const BindingFlags flag = BindingFlags.Instance |
                                      BindingFlags.NonPublic |
                                      BindingFlags.Public;
            var temp =
                from fields in host.GetType().GetFields(flag)
                where fields.FieldType.GetInterfaces()
                    .FindIndex(x => _Interfaces.FindIndex(y => y == x) != -1) != -1
                select fields.GetValue(host);
            var temp1 =
                from prop in host.GetType().GetProperties(flag)
                where prop.PropertyType.GetInterfaces()
                    .FindIndex(x => _Interfaces.FindIndex(y => y == x) != -1) != -1
                select prop.GetValue(host);
            return temp.Concat(temp1).ToArray();
        }
    }
}
