namespace EMR
{
    /// <summary>
    /// 该节点具有角度旋转能力
    /// </summary>
    public interface IRoationFeature
    {
        /// <summary>
        /// x轴旋转量
        /// </summary>
        float xAngle { get; set; }

        /// <summary>
        /// y轴旋转量
        /// </summary>
        float yAngle { get; set; }

        /// <summary>
        /// z轴旋转量
        /// </summary>
        float zAngle { get; set; }

        /// <summary>
        /// 旋转量刷新
        /// </summary>
        void rotationFresh();
    }
}