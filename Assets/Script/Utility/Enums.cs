using System;

namespace Thunder.Utility
{
    [Flags]
    public enum PickupItemAction
    {
        /// <summary>
        ///     需要瞄准物品按下互动键后拾取
        /// </summary>
        Directed = 0b1,

        /// <summary>
        ///     玩家进入拾取触发范围内自动拾取
        /// </summary>
        UnDirected = 0b10,

        /// <summary>
        ///     进入触发范围或是互动均可
        /// </summary>
        All = Directed | UnDirected
    }

    [Flags]
    public enum ItemFlag
    {
        Weapon = 0x1,
        Packageable = 0x2,
        Stackable = 0x4,
        Food = 0x8
    }

    public enum GameType
    {
        FlyingSaucer,
        SpotShooting
    }

    public enum CtrlMode
    {
        Stay,
        Switch
    }
}