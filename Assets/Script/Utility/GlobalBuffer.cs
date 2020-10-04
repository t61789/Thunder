namespace Thunder.Utility
{
    public class GlobalBuffer
    {
        #region Dictionary
        //private struct ObjUnit
        //{
        //    public object obj;
        //    public bool stable;

        //    public ObjUnit(object obj, bool stable)
        //    {
        //        this.obj = obj;
        //        this.stable = stable;
        //    }
        //}

        //private static readonly Dictionary<string, ObjUnit> buffer = new Dictionary<string, ObjUnit>();

        //public static bool Add(string Key, object value,bool cover=false ,bool stable = false)
        //{
        //    if (cover)
        //        buffer.Remove(Key);

        //    if (buffer.TryGetValue(Key, out _))
        //        return false;

        //    buffer.Add(Key, new ObjUnit(value, stable));
        //    return true;
        //}

        //public static object Get(string Key)
        //{
        //    if (buffer.TryGetValue(Key, out ObjUnit result))
        //    {
        //        if (!result.stable)
        //            buffer.Remove(Key);
        //        return result.obj;
        //    }

        //    return null;
        //}

        //public static bool Remove(string Key)
        //{
        //    return buffer.Remove(Key);
        //}

        //public static void Clear()
        //{
        //    buffer.Clear();
        //}

        //public static string GetInfo()
        //{
        //    StringBuilder sb = new StringBuilder();
        //    foreach (var item in buffer)
        //    {
        //        sb.Append('(');
        //        sb.Append(item.Key);
        //        sb.Append(',');
        //        sb.Append(item.Value);
        //        sb.Append(')');
        //    }
        //    return sb.ToString();
        //}
        #endregion
    }
}
