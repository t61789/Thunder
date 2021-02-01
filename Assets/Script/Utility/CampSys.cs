using System.Collections.Generic;
using System.Linq;
using Framework;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;

namespace Thunder
{
    public class CampSys : IBaseSys
    {
        public static CampSys Ins { get; private set; }

        public static Settings RelationThreshold;

        private static Dictionary<string, Camp> _CampDic;

        public CampSys()
        {
            Ins = this;

            if (RelationThreshold == null)
                LoadInfo(Config.CampInfoValuePath);
        }

        public static float GetRelation(string from, string to)
        {
            return _CampDic[from].Relations.TryGetAndDefault(to, 0);
        }

        public static bool IsNeutral(string from, string to)
        {
            var relation = GetRelation(from, to);
            return relation > RelationThreshold.DislikeThreshold && relation < RelationThreshold.FriendlyThreshold;
        }

        public static bool IsFriendly(string from, string to)
        {
            var relation = GetRelation(from, to);
            return relation >= RelationThreshold.FriendlyThreshold && relation < RelationThreshold.AllyThreshold;
        }

        public static bool IsAlly(string from, string to)
        {
            var relation = GetRelation(from, to);
            return relation >= RelationThreshold.AllyThreshold;
        }

        public static bool IsDislike(string from, string to)
        {
            var relation = GetRelation(from, to);
            return relation <= RelationThreshold.DislikeThreshold && relation > RelationThreshold.HateThreshold;
        }

        public static bool IsHate(string from, string to)
        {
            var relation = GetRelation(from, to);
            return relation <= RelationThreshold.HateThreshold;
        }

        private static void LoadInfo(string campInfoValuePath)
        {
            _CampDic = new Dictionary<string, Camp>();

            var value = ValueSys.GetRawValue(campInfoValuePath);
            var info = JsonConvert.DeserializeObject<CampInfo>(value);

            var first = true;
            foreach (var camp in info.Camps)
            {
                if (first)
                {
                    first = false;
                    continue;// example
                }

                camp.Init();
                _CampDic.Add(camp.Name, camp);
            }

            RelationThreshold = info.Settings;
        }

        public void OnSceneEnter(string preScene, string curScene) { }

        public void OnSceneExit(string curScene) { }

        public void OnApplicationExit() { }

        private class CampInfo
        {
            public Settings Settings;
            public IEnumerable<Camp> Camps;
        }

        public class Settings
        {
            [JsonRequired] public float RelationMax;
            [JsonRequired] public float RelationMin;
            [JsonRequired] public float FriendlyThreshold;
            [JsonRequired] public float AllyThreshold;
            [JsonRequired] public float DislikeThreshold;
            [JsonRequired] public float HateThreshold;
        }

        public class Camp
        {
            [JsonRequired] public string Name;

            public Dictionary<string, float> Relations;

            public void Init()
            {
                Relations = Relations ?? new Dictionary<string, float>();
            }
        }
    }

    public interface ICamp
    {
        string GetCamp();
    }
}