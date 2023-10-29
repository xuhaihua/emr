using EMR.Struct;

namespace EMR
{
    /// <summary>
    /// 该节点具有空间特征（空间坐标、长宽高、旋车量)
    /// </summary>
    public interface ISpaceCharacteristic : ISizeFeature, ISpacePositionFeature, IRoationFeature
    {
        /// <summary>
        /// sizeBounds组件刷新
        /// </summary>
        void sizeBoundFresh();
    }
}
