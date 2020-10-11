using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thunder.Utility
{
    public readonly struct GetterSetter<T>
    {
        public readonly Func<T> Get;
        public readonly Action<T> Set;

        public GetterSetter(Func<T> get, Action<T> set)
        {
            Get = get;
            Set = set;
        }
    }
}
