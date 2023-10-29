using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EMR.Struct
{
    /// <summary>
    /// Position的数据表示形式
    /// </summary>
    public class PositionData
    {
        public float x;
        public float y;
        public float z;

        public PositionData(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// 克隆
        /// </summary>
        /// <returns></returns>
        public PositionData clone()
        {
            return new PositionData(this.x, this.y, this.z);
        }
    }
}