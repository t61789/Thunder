using System;

namespace Thunder.Utility
{
    public class Check
    {
        public static void IsNull(object arg, string message = null)
        {
            if (arg != null)
                throw new Exception(message);
        }

        public static void IsNotNull(object arg, string message = null)
        {
            if (arg == null)
                throw new Exception(message);
        }

        public static void IsTrue(bool arg, string message = null)
        {
            if (!arg)
                throw new Exception(message);
        }

        public static void IsFalse(bool arg, string message = null)
        {
            if (arg)
                throw new Exception(message);
        }
    }
}
