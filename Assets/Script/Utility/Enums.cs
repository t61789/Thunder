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
        /// 将UI相对与锚点的位置设为0
        /// </summary>
        PositionMiddleOfAnchor = 1,
        /// <summary>
        /// 将锚点设置为0和1
        /// </summary>
        FillAnchor = PositionMiddleOfAnchor << 1,
        /// <summary>
        /// 将锚点设置为0.5
        /// </summary>
        MiddleAnchor = FillAnchor << 1,
        /// <summary>
        /// 将Offset设置为0
        /// </summary>
        FillSize = MiddleAnchor << 1,
        /// <summary>
        /// 充满父容器并居中
        /// </summary>
        FillParent = FillAnchor | PositionMiddleOfAnchor | FillSize,
        /// <summary>
        /// 在父容器居中
        /// </summary>
        CenterParent = MiddleAnchor | PositionMiddleOfAnchor,
    }
}