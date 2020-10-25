using System.Collections.Generic;
using Thunder.Entity;
using Thunder.Utility;
using UnityEngine;
using UnityEngine.Assertions;

namespace Tool
{
    public class CampSys : IBaseSys
    {
        private readonly float[,] _FriendlinessMap = new float[GlobalSettings.CampMapSize, GlobalSettings.CampMapSize];

        private readonly Dictionary<string, int> _KeyMap = new Dictionary<string, int>();

        private int _InsertIndex;

        public CampSys()
        {
            Ins = this;

            foreach (var row in DataBaseSys.Ins["camp"])
            {
                var campName = (string) row["camp_name"];
                if (!_KeyMap.ContainsKey(campName))
                    AddCamp(campName);
                var targetName = (string) row["camp_target"];
                if (string.IsNullOrEmpty(targetName)) continue;
                if (!_KeyMap.ContainsKey(targetName))
                    AddCamp(targetName);
                SetFriendliness(campName, targetName, row["friendliness"]);
            }
        }

        public static CampSys Ins { get; private set; }

        public void OnSceneEnter(string preScene, string curScene)
        {
        }

        public void OnSceneExit(string curScene)
        {
        }

        public void OnApplicationExit()
        {
        }

        public void AddCamp(string campName)
        {
            var finding = 0;
            while (_FriendlinessMap[_InsertIndex, _InsertIndex] == GlobalSettings.CampMaxFriendliness)
            {
                _InsertIndex = (_InsertIndex + 1) % GlobalSettings.CampMapSize;
                Assert.IsTrue(++finding <= GlobalSettings.CampMapSize, "阵营表已满，不可添加阵营");
            }

            _KeyMap.Add(campName, _InsertIndex);

            _FriendlinessMap[_InsertIndex, _InsertIndex] = GlobalSettings.CampMaxFriendliness;
        }

        public void RemoveCamp(string campName)
        {
            var index = 0;
            Assert.IsTrue(_KeyMap.TryGetValue(campName, out index), $"不存在名为 {campName} 的阵营");

            for (var i = 0; i < GlobalSettings.CampMapSize; i++)
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
            return GetFriendliness(c1.Camp, c2.Camp);
        }

        public float GetFriendliness(string c1, string c2)
        {
            int i1 = 0, i2 = 0;
            Assert.IsTrue(_KeyMap.TryGetValue(c1, out i1), $"未找到名为 {c1} 的Camp");
            Assert.IsTrue(_KeyMap.TryGetValue(c2, out i2), $"未找到名为 {c2} 的Camp");
            return _FriendlinessMap[i1, i2];
        }

        public void SetFriendliness(Controller c1, Controller c2, float friendliness)
        {
            SetFriendliness(c1.Camp, c2.Camp, friendliness);
        }

        public void SetFriendliness(string c1, string c2, float friendliness)
        {
            int i1 = 0, i2 = 0;
            Assert.IsTrue(_KeyMap.TryGetValue(c1, out i1), $"未找到名为 {c1} 的Camp");
            Assert.IsTrue(_KeyMap.TryGetValue(c2, out i2), $"未找到名为 {c2} 的Camp");
            _FriendlinessMap[i1, i2] = friendliness;
        }
    }
}