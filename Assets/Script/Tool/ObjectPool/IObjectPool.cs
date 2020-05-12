using System.Collections;
using UnityEngine;

namespace Tool.ObjectPool
{
    public interface IObjectPool
    {
        GameObject GetGameObject();
        void ObjectPoolReset();
        void ObjectPoolRecycle();
        void ObjectPoolDestroy();
    }
}
