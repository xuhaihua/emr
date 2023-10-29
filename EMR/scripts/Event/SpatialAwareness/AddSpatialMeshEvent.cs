using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using UnityEngine;

namespace EMR.Event
{
    /// <summary>
    /// 空间网格添加 EventData类
    /// </summary>
    public class AddSpatialMeshEventData : EventData
    {
        public int id;

        /// <summary>
        /// 添加的mesh
        /// </summary>
        public Mesh mesh;

        /// <summary>
        /// 原始的事件数据
        /// </summary>
        public MixedRealitySpatialAwarenessEventData<SpatialAwarenessMeshObject> original;
    }

    /// <summary>
    /// 空间网格添加 事件类
    /// </summary>
    public class AddSpatialMeshEvent : EMREvent<AddSpatialMeshEventData>
    {
    }
}
