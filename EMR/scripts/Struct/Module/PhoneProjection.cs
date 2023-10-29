using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EMR.Struct
{
    /// <summary>
    /// 照片投影信息
    /// </summary>
    public struct PhoneProjection
    {
        /// <summary>
        /// // 相机中心点
        /// </summary>
        public Vector3 cameraPos;

        /// <summary>
        /// 相机中心到点的方向向量
        /// </summary>
        public Vector3 direction;

        public PhoneProjection(Vector3 cameraPos, Vector3 direction)
        {
            this.cameraPos = cameraPos;

            this.direction = direction;
        }
    }
}
