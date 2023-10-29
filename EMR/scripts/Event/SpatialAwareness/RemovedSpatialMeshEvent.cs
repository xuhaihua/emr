using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using UnityEngine;

namespace EMR.Event
{
    /// <summary>
    /// 空间网格更新 EventData类
    /// </summary>
    public class UpdatedSpatialMeshEventData : EventData
    {
        public int id;

        /// <summary>
        /// 更新的mesh
        /// </summary>
        public Mesh mesh;

        /// <summary>
        /// 原始的事件数据
        /// </summary>
        public MixedRealitySpatialAwarenessEventData<SpatialAwarenessMeshObject> original;
    }

    /// <summary>
    /// 空间网格更新 事件类
    /// </summary>
    public class UpdatedSpatialMeshEvent : EMREvent<UpdatedSpatialMeshEventData>
    {
    }
}
