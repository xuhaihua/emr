using UnityEngine;

namespace EMR
{
    /// <summary>
    /// 该节点具有位置能力
    /// </summary>
    public interface IPanelLayerPositionFeature
    {
        /// <summary>
        /// left
        /// </summary>
        float? left { get; set; }

        /// <summary>
        /// right
        /// </summary>
        float? right { get; set; }

        /// <summary>
        /// top
        /// </summary>
        float? top { get; set; }

        /// <summary>
        /// bottom
        /// </summary>
        float? bottom { get; set; }

        /// <summary>
        /// 节点偏移量
        /// </summary>
        Vector3 offset { get; set; }

        /// <summary>
        /// 位置刷新
        /// </summary>
        void positionFresh();

        void compountCoordZ();

        void unlockPositionSetedTag();
    }
}
