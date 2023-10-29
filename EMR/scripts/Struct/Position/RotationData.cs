using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EMR.Struct
{
    /// <summary>
    /// Rotation的数据表示形式
    /// </summary>
    public class RotationData
    {
        public float xAngle;
        public float yAngle;
        public float zAngle;

        public RotationData(float xAngle, float yAngle, float zAngle)
        {
            this.xAngle = xAngle;
            this.yAngle = yAngle;
            this.zAngle = zAngle;
        }

        public RotationData clone()
        {
            return new RotationData(this.xAngle, this.yAngle, this.zAngle);
        }
    }
}