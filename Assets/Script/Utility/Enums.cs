using System;

namespace Thunder.Utility
{
    public enum DialogResult
    {
        Ok,
        Cancel
    }

    [Flags]
    public enum UiInitType
    {
        /// <summary>
        ///     将UI相对与锚点的位置设为0
        /// </summary>
        PositionMiddleOfAnchor = 1,

        /// <summary>
        ///     将锚点设置为0和1
        /// </summary>
        FillAnchor = PositionMiddleOfAnchor << 1,

        /// <summary>
        ///     将锚点设置为0.5
        /// </summary>
        MiddleAnchor = FillAnchor << 1,

        /// <summary>
        ///     将Offset设置为0
        /// </summary>
        FillSize = MiddleAnchor << 1,

        /// <summary>
        ///     充满父容器并居中
        /// </summary>
        FillParent = FillAnchor | PositionMiddleOfAnchor | FillSize,

        /// <summary>
        ///     在父容器居中
        /// </summary>
        CenterParent = MiddleAnchor | PositionMiddleOfAnchor
    }

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
    }

    public enum GameType
    {
        FlyingSaucer,
        SpotShooting
    }
}