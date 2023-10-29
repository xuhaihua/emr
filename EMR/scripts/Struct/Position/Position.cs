using UnityEngine;
using EMR.Entity;

namespace EMR.Struct
{
    /// <summary>
    /// 本类主要用于描述一个节点的空间的位置
    /// </summary>
    public class Position
    {
        private Node _node;
        public float oldX = 0f;
        public float oldY = 0f;
        public float oldZ = 0f;

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="node">关联的节点</param>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <param name="z">z</param
        public Position(Node node, float x = 0, float y = 0, float z = 0)
        {
            this._node = node;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// x的单位制坐标
        /// </summary>
        public float x
        {
            get
            {
                float result = 0f;
                float offsetX = Space.Unit.unitToScale(this._node, this._node.offset.x, Axle.right);
                if (!float.IsNaN(offsetX) && !float.IsInfinity(offsetX)) {

                    result = Space.Unit.scaleToUnit(this._node, this._node.parasitifer.transform.localPosition.x - offsetX, Axle.right);
                }

                return result;
            }

            set
            {
                float offsetX = Space.Unit.unitToScale(this._node, this._node.offset.x, Axle.right);

                if (!float.IsNaN(offsetX) && !float.IsInfinity(offsetX))
                {
                    this.oldX = this.x;
                    this._node.parasitifer.transform.localPosition = new Vector3(Space.Unit.unitToScale(this._node, value,  Axle.right), this._node.parasitifer.transform.localPosition.y, this._node.parasitifer.transform.localPosition.z) + new Vector3(offsetX, 0, 0);
                }
            }
        }

        /// <summary>
        /// y的单位制坐标
        /// </summary>
        public float y
        {
            get
            {
                float result = 0f;
                float offsetY = Space.Unit.unitToScale(this._node, this._node.offset.y, Axle.up);
                if (!float.IsNaN(offsetY) && !float.IsInfinity(offsetY))
                {
                    result = Space.Unit.scaleToUnit(this._node, this._node.parasitifer.transform.localPosition.y - offsetY, Axle.up);
                }

                return result;
            }

            set
            {
                float offsetY = Space.Unit.unitToScale(this._node, this._node.offset.y, Axle.up);

                if (!float.IsNaN(offsetY) && !float.IsInfinity(offsetY))
                {
                    this.oldY = this.y;
                    this._node.parasitifer.transform.localPosition = new Vector3(this._node.parasitifer.transform.localPosition.x, Space.Unit.unitToScale(this._node, value, Axle.up), this._node.parasitifer.transform.localPosition.z) + new Vector3(0, offsetY, 0);
                }
            }
        }

        /// <summary>
        /// z的单位制坐标
        /// </summary>
        public float z
        {
            get
            {
                float result = 0f;
                float offsetZ = Space.Unit.unitToScale(this._node, this._node.offset.z, Axle.forward);
                if (!float.IsNaN(offsetZ) && !float.IsInfinity(offsetZ))
                {

                    result = Space.Unit.scaleToUnit(this._node, this._node.parasitifer.transform.localPosition.z - offsetZ, Axle.forward);
                }

                return result;
            }

            set
            {
                float offsetZ = Space.Unit.unitToScale(this._node, this._node.offset.z, Axle.forward);

               
                if (!float.IsNaN(offsetZ) && !float.IsInfinity(offsetZ))
                {
                    this.oldZ = this.z;

                    this._node.parasitifer.transform.localPosition = new Vector3(this._node.parasitifer.transform.localPosition.x, this._node.parasitifer.transform.localPosition.y, Space.Unit.unitToScale(this._node, value, Axle.forward)) + new Vector3(0, 0, offsetZ);
                }
            }
        }
    }
}