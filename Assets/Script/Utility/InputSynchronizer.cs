using Thunder.Sys;
using UnityEngine;

namespace Thunder.Utility
{
    /// <summary>
    ///     用于在Update中读取数据后传递到FixedUpdate中
    /// </summary>
    public struct InputSynchronizer
    {
        private ControlInfo _Info;

        public void Set(ControlInfo value)
        {
            if (value.Down) _Info.Down = true;
            if (value.Stay) _Info.Stay = true;
            if (value.Up) _Info.Up = true;
            if (value.Axis != Vector3.zero) _Info.Axis = value.Axis;
        }

        public ControlInfo Get()
        {
            var result = _Info;
            _Info = new ControlInfo();
            return result;
        }
    }
}