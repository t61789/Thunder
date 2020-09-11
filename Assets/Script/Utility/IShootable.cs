using UnityEngine;

namespace Thunder.Utility
{
    public interface IShootable
    {
        void GetShoot(Vector3 hitPos, Vector3 hitDir, float damage);
    }
}
