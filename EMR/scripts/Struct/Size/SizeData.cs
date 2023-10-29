using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EMR.Struct
{
    /// <summary>
    /// Size的数据表示形式
    /// </summary>
    public class SizeData
    {
        public float width;
        public float height;
        public float depth;

        public SizeData(float width, float height, float depth)
        {
            this.width = width;
            this.height = height;
            this.depth = depth;
        }

        public SizeData clone()
        {
            return new SizeData(this.width, this.height, this.depth);
        }
    }
}
