using System.Collections.Generic;
using Thunder.Entity;
using Thunder.Utility;
using UnityEngine;
using UnityEngine.Assertions;

namespace Thunder.PublicScript
{
    public class CampManager
    {
        private readonly float[,] _FriendlinessMap = new float[GlobalSettings.CampMapSize, GlobalSettings.CampMapSize];

        private readonly Dictionary<string,int> _KeyMap = new Dictionary<string, int>();

        private int _InsertIndex;

        public void AddCamp(string campName)
        {
            int finding = 0;
            while (_FriendlinessMap[_InsertIndex, _InsertIndex] == GlobalSettings.CampMaxFriendliness)
            {
                _InsertIndex = _InsertIndex + 1 % GlobalSettings.CampMapSize;
                Assert.IsTrue(++finding <= GlobalSettings.CampMapSize,"阵营表已满，不可添加阵营");
            }
            _KeyMap.Add(campName, _InsertIndex);

            _FriendlinessMap[_InsertIndex, _InsertIndex] = GlobalSettings.CampMaxFriendliness;
        }

        public void RemoveCamp(string campName)
        {
            Assert.IsTrue(_KeyMap.TryGetValue(campName,out int index),$"不存在名为 {campName} 的阵营");

            for (int i = 0; i < GlobalSettings.CampMapSize; i++)
            {
                _FriendlinessMap[i, index] = 0;
                _FriendlinessMap[index, i] = 0;
            }

            _KeyMap.Remove(campName);
        }

        public bool IsHostile(Controller c1, Controller c2)
        {
            return GetFriendliness(c1, c2) < -GlobalSettings.CampNeutralValue;
        }

        public bool IsNeutral(Controller c1, Controller c2)
        {
            return Mathf.Abs(GetFriendliness(c1, c2)) < GlobalSettings.CampNeutralValue;
        }

        public bool IsAlly(Controller c1, Controller c2)
        {
            return GetFriendliness(c1, c2) > GlobalSettings.CampNeutralValue;
        }

        public float GetFriendliness(Controller c1, Controller c2)
        {
            Assert.IsTrue(_KeyMap.TryGetValue(c1.Camp,out int i1),$"未找到名为 {c1.Camp} 的Camp");
            Assert.IsTrue(_KeyMap.TryGetValue(c2.Camp, out int i2), $"未找到名为 {c2.Camp} 的Camp");
            return _FriendlinessMap[i1, i2];
        }

        public CampManager()
        {
            foreach (var item in Sys.Stable.DataBase["camp"].Select(null, null).Rows)
                _Camps.Add((string)item[0], new CampUnit((string)item[0]));
            Sys.Stable.DataBase.DeleteTable(null, null, "camp");

            foreach (var item in Sys.Stable.DataBase["camp_hostile"].Select(null, null).Rows)
            {
                _Camps[(string)item[0]].AddHostile((string)item[1]);
                _Camps[(string)item[1]].AddHostile((string)item[0]);
            }
            Sys.Stable.DataBase.DeleteTable(null, null, "camp_hostile");

            foreach (var item in Sys.Stable.DataBase["camp_ally"].Select(null, null).Rows)
            {
                _Camps[(string)item[0]].AddAlly((string)item[1]);
                _Camps[(string)item[1]].AddAlly((string)item[0]);
            }
            Sys.Stable.DataBase.DeleteTable(null, null, "camp_ally");
        }
    }
}
