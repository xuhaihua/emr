using UnityEngine;

namespace EMR.Struct
{
    /// <summary>
    /// 包围盒结构体
    /// </summary>
    public struct BoundBox
    {
        public Vector3 center;
        public Vector3 size;

        public BoundBox(Vector3 center, Vector3 size)
        {
            this.center = center;
            this.size = size;
        }
    }
}