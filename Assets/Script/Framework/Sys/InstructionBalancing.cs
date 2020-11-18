using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// 对于高消耗指令进行时间上的平均分配。<br/>
    /// 例如大量对象进行固定时间的射线检测，可能会在检测时产生性能瓶颈造成卡顿。该类的作用就是平均分配这些检测<br/>
    /// 但可能会造成第一次检测的延期，所以适用于对时间要求不那么精确的循环操作
    /// </summary>
    public class InstructionBalancing:MonoBehaviour,IBaseSys
    {
        private static readonly List<Unit> _UnitList = new List<Unit>();
        private static int _ReadyIndex;
        private static readonly HashSet<object> _AddedSet = new HashSet<object>();
        private static InstructionBalancing _Ins;

        private void Awake()
        {
            if(_Ins!=null)
                throw new InitDuplicatelyException();
            _Ins = this;
        }

        private void FixedUpdate()
        {
            CheckReady();
            Loop();
        }

        public static void AddAction(object key,Action act,float interval)
        {
            if(_AddedSet.Contains(key))
                throw new ArgumentException("已添加指定的key");

            _UnitList.Add(new Unit
            {
                Key = key,
                Act = act,
                Interval = interval,
                Counter = new SimpleCounter(Tools.RandomFloat(0,interval))
            });

            _AddedSet.Add(key);
        }

        public static void ResumeAction(object key)
        {
            SetRunning(key, true);
        }

        public static void PauseAction(object key)
        {
            SetRunning(key,false);
        }

        public static void RemoveAction(object key)
        {
            if(!_AddedSet.Contains(key))
                throw new KeyNotFoundException();

            int index = _UnitList.FindIndex(x=>x.Key==key);
            if (index < _ReadyIndex)
                _ReadyIndex--;
            _UnitList.RemoveAt(index);
            _AddedSet.Remove(key);
        }

        private static void CheckReady()
        {
            for (int i = _ReadyIndex; i < _UnitList.Count; i++)
            {
                var unit = _UnitList[i];

                if (!unit.Counter.Completed) continue;
                _UnitList[i] = _UnitList[_ReadyIndex];
                _UnitList[_ReadyIndex] = unit;
                _ReadyIndex++;
                i--;

                unit.Counter.Recount(unit.Interval);
            }
        }

        private static void Loop()
        {
            for (int i = 0; i < _ReadyIndex; i++)
            {
                var unit = _UnitList[i];
                if (unit.Counter.Completed)
                {
                    if(unit.Running)
                        unit.Act();
                    unit.Counter.Recount();
                }
            }
        }

        private static void SetRunning(object key, bool running)
        {
            if (!_AddedSet.Contains(key))
                throw new KeyNotFoundException();

            _UnitList.Find(x => x.Key == key).Running = false;
        }

        private class Unit
        {
            public object Key;
            public Action Act;
            public float Interval;
            public bool Running = true;
            public SimpleCounter Counter;
        }

        public void OnSceneEnter(string preScene, string curScene){}

        public void OnSceneExit(string curScene)
        {
            _UnitList.Clear();
            _ReadyIndex = 0;
            _AddedSet.Clear();
        }

        public void OnApplicationExit(){}
    }
}
