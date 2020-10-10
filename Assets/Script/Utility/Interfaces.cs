using Thunder.Sys;
using UnityEngine;

namespace Thunder.Utility
{
    public interface IItem
    {
        int ItemId { get; }
    }

    public interface IShootable
    {
        void GetShoot(Vector3 hitPos, Vector3 hitDir, float damage);
    }

    public interface IInteractive
    {
        void Interactive(ControlInfo info);
    }
}