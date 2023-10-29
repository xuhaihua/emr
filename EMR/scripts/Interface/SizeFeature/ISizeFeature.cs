using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EMR.Struct;

namespace EMR
{
    /// <summary>
    /// 该节点具有尺寸功能
    /// </summary>
    public interface ISizeFeature
    {
        /// <summary>
        /// 宽甸度
        /// </summary>
        float width { get; set; }

        /// <summary>
        /// 高度
        /// </summary>
        float height { get; set; }

        /// <summary>
        /// 深度
        /// </summary>
        float depth { get; set; }

        /// <summary>
        /// 节点包围盒
        /// </summary>
        BoundBox localBounds { get; }

        /// <summary>
        /// 尺寸刷新
        /// </summary>
        void sizeFresh();

        void unlockSizeSetedTag();
    }
}
