using System;
using System.Collections.Generic;
using System.Linq;
using Tool;

namespace Thunder.Utility
{
    public class ItemCombiner
    {
        private readonly Package _Package;
        private readonly CombineExpression[] _Expressions;
        private readonly Dictionary<ItemId, List<int>> _OutputIndexer;

        public ItemCombiner(Package package, Table expressionTable)
        {
            _Package = package;
            _Expressions = ReadExpressions(expressionTable);
            _OutputIndexer = CreateOutputIndexer(_Expressions);
        }

        /// <summary>
        /// 查找所有能够合成出指定物品的所有合成规则
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        public CombineExpression[] FindExpressions(ItemId output)
        {
            return FindExpressions(new[] { output });
        }

        /// <summary>
        /// 查找所有能够合成出指定物品的所有合成规则
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        public CombineExpression[] FindExpressions(ItemId[] output)
        {
            if (output == null || output.Length == 0) return new CombineExpression[0];
            var enum1 =
                from index in _OutputIndexer[output[0]]
                select _Expressions[index];
            if (output.Length == 1) return enum1.ToArray();
            var avaliableList = enum1.ToList();

            for (int i = 0; i < avaliableList.Count; i++)
            {
                int pOut = 0;
                for (int j = 0; j < _Expressions[i].Output.Length && pOut < output.Length; j++)
                    if (_Expressions[i].Output[j].Id == output[pOut].Id)
                        pOut++;
                    else if (_Expressions[i].Output[j].Id > output[pOut].Id)
                        break;
                if (pOut == output.Length) continue;
                avaliableList.RemoveAt(i);
                i--;
            }
            return avaliableList.ToArray();
        }

        /// <summary>
        /// 判断背包中的物品是否满足合成规则的需求
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public bool ExpressionAvaliable(CombineExpression expression)
        {
            return expression.Input.All(item => _Package.GetItemNum(item.Id) >= item.Count);
        }

        /// <summary>
        /// 应用合成规则
        /// </summary>
        /// <param name="expression"></param>
        /// <returns>无法添加进背包的物品（背包已满或是不可打包物品）</returns>
        public ItemGroup[] ApplyExpression(CombineExpression expression)
        {
            foreach (var costItem in expression.Input)
                _Package.CostItem(costItem, out _);

            return (
                from createdItem in expression.Output
                let overflow = _Package.AddItem(createdItem, out _)
                where overflow > 0
                select (createdItem.Id, overflow))
                .Select(dummy => (ItemGroup)dummy).ToArray();
        }

        private static CombineExpression[] ReadExpressions(Table expressionTable)
        {
            const string outstr = "out";

            int maxInputNum = 0;
            int maxOutputNum = 0;
            foreach (var field in expressionTable.Fields)
                if (field.StartsWith(outstr))
                    maxInputNum++;
                else
                    maxOutputNum++;

            var len = maxInputNum + maxOutputNum;

            var expressionList = new List<CombineExpression>();
            var unitList = new List<ItemGroup>();

            foreach (var row in expressionTable)
            {
                var expression = new CombineExpression();
                unitList.Clear();
                for (int i = 0; i < len; i++)
                {
                    if (i == maxOutputNum)
                    {
                        expression.Output = unitList.ToArray();
                        unitList.Clear();
                    }

                    var strs = ((string)row[i]).Split('|');

                    int id = int.Parse(strs[0]);
                    if (id == 0) continue;
                    string add = strs[2];
                    if (string.IsNullOrEmpty(add))
                        add = null;
                    var itemId = new ItemId(id, add);

                    int count = int.Parse(strs[1]);

                    unitList.Add(new ItemGroup(itemId, count));
                }

                expression.Input = unitList.ToArray();
                Array.Sort(expression.Output);
                Array.Sort(expression.Input);

                expressionList.Add(expression);
            }

            return expressionList.ToArray();
        }

        private static Dictionary<ItemId, List<int>> CreateOutputIndexer(IList<CombineExpression> expressions)
        {
            var result = new Dictionary<ItemId, List<int>>();
            for (int i = 0; i < expressions.Count; i++)
            {
                int len = expressions[i].Output.Length;
                for (int j = 0; j < len; j++)
                {
                    var id = expressions[i].Output[j].Id;
                    if (!result.TryGetValue(id, out var list))
                    {
                        list = new List<int>();
                        result.Add(id, list);
                    }
                    list.Add(i);
                }
            }

            foreach (var list in result.Values)
                list.Sort();
            return result;
        }

        // todo 拥有的物品可以作为什么规则的输入
    }

    public struct CombineExpression
    {
        public ItemGroup[] Output;
        public ItemGroup[] Input;
    }
}
