using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thunder.Utility
{
    /// <summary>
    /// 在检查条件发生变化时执行所给定的函数
    /// </summary>
    public class SwitchTrigger
    {
        public Action<bool> Trigger { get; set; }
        private bool _Pre;

        public SwitchTrigger(Action<bool> act, bool startValue = false)
        {
            Trigger = act;
            _Pre = startValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <returns>是否发生了切换事件</returns>
        public bool Check(bool condition)
        {
            var result = false;
            if (_Pre && !condition)
            {
                _Pre = false;
                Trigger?.Invoke(false);
                result = true;
            }
            else if (!_Pre && condition)
            {
                _Pre = true;
                Trigger?.Invoke(true);
                result = true;
            }

            return result;
        }

        public static implicit operator bool(SwitchTrigger t)
        {
            return t._Pre;
        }
    }
}
