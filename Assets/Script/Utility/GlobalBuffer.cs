using System;
using Thunder.GameMode;
using Thunder.PublicScript;

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

        //public static bool Add(string key, object value,bool cover=false ,bool stable = false)
        //{
        //    if (cover)
        //        buffer.Remove(key);

        //    if (buffer.TryGetValue(key, out _))
        //        return false;

        //    buffer.Add(key, new ObjUnit(value, stable));
        //    return true;
        //}

        //public static object Get(string key)
        //{
        //    if (buffer.TryGetValue(key, out ObjUnit result))
        //    {
        //        if (!result.stable)
        //            buffer.Remove(key);
        //        return result.obj;
        //    }

        //    return null;
        //}

        //public static bool Remove(string key)
        //{
        //    return buffer.Remove(key);
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

        public static (LevelManager.LevelParam levelParam, Action<BaseGameMode> init) battleSceneParam;
    }
}
