public enum DialogResult
{
    OK,
    Cancel
}

public enum UIInitAction
{
    /// <summary>
    /// 将UI相对与锚点的位置设为0
    /// </summary>
    PositionMiddleOfAnchor = 1,
    /// <summary>
    /// 将锚点设置为0和1
    /// </summary>
    FillAnchor = 2,
    /// <summary>
    /// 将锚点设置为0.5
    /// </summary>
    MiddleAnchor = 4,
    /// <summary>
    /// 将Offset设置为0
    /// </summary>
    FillSize = 8,
    /// <summary>
    /// 充满父容器并居中
    /// </summary>
    FillParent = FillAnchor| PositionMiddleOfAnchor | FillSize,
    /// <summary>
    /// 在父容器居中
    /// </summary>
    CenterParent = MiddleAnchor | PositionMiddleOfAnchor,
}