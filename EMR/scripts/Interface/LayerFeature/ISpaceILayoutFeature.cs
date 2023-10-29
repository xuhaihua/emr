namespace EMR
{
    /// <summary>
    /// 该节点具有空间布局能力
    /// </summary>
    public interface ISpaceLayoutFeature
    {
        /// <summary>
        /// 内容对齐刷新
        /// </summary>
        void contentAlignFresh();

        /// <summary>
        /// 水平fudon刷新
        /// </summary>
        void horizontalFresh();

        /// <summary>
        /// 垂直浮动刷新
        /// </summary>
        void verticalFresh();

        /// <summary>
        /// z轴浮动刷新
        /// </summary>
        void forwardFresh();
    }
}