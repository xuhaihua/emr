using UnityEngine;

namespace EMR
{
    /// <summary>
    /// 该节点具有空间位置能力
    /// </summary>
    public interface ISpacePositionFeature
    {
        /// <summary>
        /// x
        /// </summary>
        float x { get; set; }

        /// <summary>
        /// y
        /// </summary>
        float y { get; set; }

        /// <summary>
        /// z
        /// </summary>
        float z { get; set; }

        /// <summary>
        /// 节点偏移量
        /// </summary>
        Vector3 offset { get; set; }

        /// <summary>
        /// 位置刷新
        /// </summary>
        void positionFresh();

        void unlockPositionSetedTag();
    }
}
