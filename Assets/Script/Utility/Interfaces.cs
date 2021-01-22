using Framework;
using UnityEngine;

namespace Thunder
{
    public interface IItem
    {
        ItemId ItemId { get; set; }
    }

    public interface IHitAble
    {
        void GetHit(Vector3 hitPos, Vector3 hitDir, float damage);
    }

    public interface IInteractive
    {
        void Interactive(ControlInfo info);
    }

    public interface IHostDestroyed
    {
        void HostDestroyed(object host);
    }

    public interface IHostAwaked
    {
        void HostAwaked(object host);
    }
}