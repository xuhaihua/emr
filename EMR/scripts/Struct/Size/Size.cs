using UnityEngine;
using EMR.Entity;

namespace EMR.Struct
{
    /// <summary>
    /// 本类主要用于描述一个节点的尺寸 (节点尺寸控制被管理游戏对象的尺寸)
    /// </summary>
    public class Size
    {
        private Node _node;

        public float oldWidth = 0f;
        public float oldHeight = 0f;
        public float oldDepth = 0f;

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="node">关联的节点</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="depth">深度</param>
        public Size(Node node, float width = 0, float height = 0, float depth = 0)
        {
            this._node = node;
            this.width = width;
            this.height = height;
            this.depth = depth;
        }

        /// <summary>
        /// 宽度
        /// </summary>
        public float width
        {
            get
            {
                return Space.Unit.scaleToUnit(this._node, this._node.parasitifer.transform.localScale.x, Axle.right);
            }

            set
            {
                var num = Space.Unit.unitToScale(this._node, value, Axle.right);

                

                if (!float.IsNaN(num) && !float.IsInfinity(num))
                {
                    
                    this.oldWidth = this.width;
                    this._node.parasitifer.transform.localScale = new Vector3(num, this._node.parasitifer.transform.localScale.y, this._node.parasitifer.transform.localScale.z);
                }
            }
        }

        /// <summary>
        /// 高度
        /// </summary>
        public float height
        {
            get
            {
                return Space.Unit.scaleToUnit(this._node, this._node.parasitifer.transform.localScale.y, Axle.up);
            }

            set
            {
                var num = Space.Unit.unitToScale(this._node, value, Axle.up);

                if(!float.IsNaN(num) && !float.IsInfinity(num))
                {
                    this.oldHeight = this.height;

                    this._node.parasitifer.transform.localScale = new Vector3(this._node.parasitifer.transform.localScale.x, num, this._node.parasitifer.transform.localScale.z);
                }
            }
        }

        /// <summary>
        /// 深度
        /// </summary>
        public float depth
        {
            get
            {
                return Space.Unit.scaleToUnit(this._node, this._node.parasitifer.transform.localScale.z, Axle.forward);
            }

            set
            {
                var num = Space.Unit.unitToScale(this._node, value, Axle.forward);

                if(!float.IsNaN(num) && !float.IsInfinity(num))
                {
                    this.oldDepth = this.depth;
                    this._node.parasitifer.transform.localScale = new Vector3(this._node.parasitifer.transform.localScale.x, this._node.parasitifer.transform.localScale.y, num);
                }
            }
        }
    }
}
