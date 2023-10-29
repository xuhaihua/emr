using UnityEngine;
using EMR.Entity;

namespace EMR.Struct
{
    /// <summary>
    /// 本类主要用于描述节点在空间中的旋转量
    /// </summary>
    public class Rotation
    {
        private Node _node;

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="node">关联的节点</param>
        /// <param name="xAngle">x轴旋转角度 (度单为度）</param>
        /// <param name="yAngle">y轴旋转角度 (度单为度）</param>
        /// <param name="zAngle">z轴旋转角度 (度单为度）</param>
        public Rotation(Node node, float xAngle = 0, float yAngle = 0, float zAngle = 0)
        {
            this._node = node;
            this.xAngle = xAngle;
            this.yAngle = yAngle;
            this.zAngle = zAngle;
        }

        /// <summary>
        /// x的单位制坐标
        /// </summary>
        public float xAngle
        {
            get
            {
                return this._node.parasitifer.transform.localEulerAngles.x;
            }

            set
            {
                this._node.parasitifer.transform.localRotation = Quaternion.Euler(value, this._node.parasitifer.transform.localRotation.eulerAngles.y, this._node.parasitifer.transform.localRotation.eulerAngles.z);
            }
        }

        /// <summary>
        /// y的单位制坐标
        /// </summary>
        public float yAngle
        {
            get
            {
                return this._node.parasitifer.transform.localEulerAngles.y;
            }

            set
            {
                this._node.parasitifer.transform.localRotation = Quaternion.Euler(this._node.parasitifer.transform.localRotation.eulerAngles.x, value, this._node.parasitifer.transform.localRotation.eulerAngles.z);
            }
        }

        /// <summary>
        /// z的单位制坐标
        /// </summary>
        public float zAngle
        {
            get
            {
                return this._node.parasitifer.transform.localEulerAngles.z;
            }

            set
            {
                this._node.parasitifer.transform.localRotation = Quaternion.Euler(this._node.parasitifer.transform.localRotation.eulerAngles.x, this._node.parasitifer.transform.localRotation.eulerAngles.y, value);
            }
        }
    }
}