using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using UnityEngine;

namespace EMR.Event
{
    /// <summary>
    /// 空间网格移除 EventData类
    /// </summary>
    public class RemovedSpatialMeshEventData : EventData
    {
        public int id;

        /// <summary>
        /// 移除的mesh
        /// </summary>
        public Mesh mesh;

        /// <summary>
        /// 原始的事件数据
        /// </summary>
        public MixedRealitySpatialAwarenessEventData<SpatialAwarenessMeshObject> original;
    }

    /// <summary>
    /// 空间网格移除 事件类
    /// </summary>
    public class RemovedSpatialMeshEvent : EMREvent<RemovedSpatialMeshEventData>
    {
    }
}
