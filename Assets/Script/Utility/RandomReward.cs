using System;
using System.Collections.Generic;
using Framework;

namespace Thunder
{
    public class RandomRewardGenerator
    {
        private readonly List<RewardUnit> _RewardList;
        private readonly int _SumWeight;
        private static Dictionary<string, RandomRewardGenerator> _RewardDic;

        public ItemGroup Generate()
        {
            int rand = Tools.RandomInt(1, _SumWeight);
            int sum = 0;
            foreach (var rewardUnit in _RewardList)
            {
                sum += rewardUnit.Weight;
                if (rand <= sum)
                    return rewardUnit.ItemGroup;
            }

            return _RewardList[0].ItemGroup;
        }

        public static RandomRewardGenerator GetGenerator(string key)
        {
            return _RewardDic[key];
        }

        public static void ConstractDic(Table rewardTable)
        {
            const string itemGroupStr = "item_group";
            const string keyStr = "key";
            const string weightStr = "weight";
            _RewardDic = new Dictionary<string, RandomRewardGenerator>();
            var tempList = new List<RewardUnit>();
            string curKey = null;
            foreach (var row in rewardTable)
            {
                string key = row[keyStr];
                if (curKey != key)
                {
                    _RewardDic.Add(curKey, new RandomRewardGenerator(tempList));
                    curKey = key;
                    tempList.Clear();
                }

                if (ItemGroup.TryParse(row[itemGroupStr], out var itemGroup))
                    throw new Exception("数据转换失败");
                tempList.Add(new RewardUnit(itemGroup, row[weightStr]));
            }
            _RewardDic.Add(curKey, new RandomRewardGenerator(tempList));
        }

        private RandomRewardGenerator(IEnumerable<RewardUnit> list)
        {
            _RewardList = new List<RewardUnit>();
            _SumWeight = 0;
            foreach (var unit in list)
            {
                _SumWeight += unit.Weight;
                _RewardList.Add(unit);
            }
        }
    }

    public struct RewardUnit
    {
        public ItemGroup ItemGroup;
        public int Weight;

        public RewardUnit(ItemGroup itemGroup, int weight)
        {
            ItemGroup = itemGroup;
            Weight = weight;
        }
    }
}
