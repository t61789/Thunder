using Thunder.Sys;
using UnityEngine;

namespace Thunder.Utility
{
    public interface IItem
    {
        ItemId ItemId { get; set; }
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