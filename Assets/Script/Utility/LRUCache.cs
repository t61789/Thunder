using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Thunder.Utility
{
    public class LRUCache<K, V>
    {
        private readonly Dictionary<K, int> _Dic = new Dictionary<K, int>();
        private readonly Node[] _Nodes;
        private int _First;
        private int _Last;

        public LRUCache(int capacity)
        {
            Assert.IsTrue(capacity > 0, "容量需要大于0");
            _Nodes = new Node[capacity];
            _Last = 0;
            _First = capacity - 1;
            _Nodes[_Last].Next = _Nodes[_First].Pre = -1;
            for (var i = 1; i < capacity; i++)
            {
                _Nodes[i - 1].Pre = i;
                _Nodes[i].Next = i - 1;
            }
        }

        public bool Contains(K key)
        {
            return _Dic.TryGetValue(key, out _);
        }

        public V Get(K key)
        {
            int result;
            if (!_Dic.TryGetValue(key, out result)) return default; // 缓存中没有key，返回空

            if (_Nodes[result].Pre == -1) return _Nodes[result].Val; // 节点是头部，直接返回值

            MoveToFirst(result); // 将节点移动到头部并返回值
            return _Nodes[_First].Val;
        }

        public void Put(K key, V value)
        {
            int node;
            if (_Dic.TryGetValue(key, out node)) // 如果已存在key，则将节点值重新设置并放置在头部
            {
                _Nodes[node].Val = value;
                MoveToFirst(node);
            }
            else
            {
                var temp = _Last;
                if (_Nodes[temp].Key != null)
                    _Dic.Remove(_Nodes[temp].Key);
                // 若前方为-1则说明缓冲区大小为1，这里不为1
                // 所以将尾部节点的数据抛弃，重新赋值后放置在首部
                // 为1则跳过这些操作
                if (_Nodes[temp].Pre != -1)
                {
                    _Last = _Nodes[temp].Pre;
                    _Nodes[_Nodes[temp].Pre].Next = -1;
                    _Nodes[temp].Pre = -1;
                    _Nodes[temp].Next = _First;
                    _Nodes[_First].Pre = temp;
                    _First = temp;
                }

                _Nodes[temp].Key = key;
                _Nodes[temp].Val = value;
                _Dic.Add(key, temp);
            }
        }

        private void MoveToFirst(int node)
        {
            if (node == _First) return;
            // 将节点移除出当前位置并恢复其附近的连接
            _Nodes[_Nodes[node].Pre].Next = _Nodes[node].Next; // 将节点前方的后方设为节点的后方

            if (_Nodes[node].Next != -1) // 不是尾部
                _Nodes[_Nodes[node].Next].Pre = _Nodes[node].Pre; // 则将后方的前方设为节点前方
            else
                _Last = _Nodes[node].Pre; // 如果是尾部，则将尾部设为节点的前方

            // 将当前节点放置在链表头部
            _Nodes[_First].Pre = node;
            _Nodes[node].Next = _First;
            _Nodes[node].Pre = -1;
            _First = node;
        }

        private struct Node
        {
            public int Pre;
            public int Next;
            public K Key;
            public V Val;
        }
    }
}