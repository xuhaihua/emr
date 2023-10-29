using System.Collections.Generic;
using UnityEngine;
using EMR.Entity;
using EMR.Struct;

namespace EMR.Layout
{
    public class PanelAlign : ILayout
    {
        private PanelNode _node;

        /// <summary>
        /// 作用轴
        /// </summary>
        public Axle axle = Axle.right;

        /// <summary>
        /// 间隔
        /// </summary>
        private float _interval = 0f;

        /// <summary>
        /// 开始是否加入间隔
        /// </summary>
        private bool _isStartInvterval = false;

        /// <summary>
        /// 结尾是否加入间隔
        /// </summary>
        private bool _isEndInvterval = false;

        /// <summary>
        /// 对齐模式
        /// </summary>
        public AlignMode mode = AlignMode.none;

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="axle">作用轴</param>
        /// <param name="interval">间距</param>
        /// <param name="mode">对齐方式</param>
        /// <param name="isStartInvterval">第一个节点是否包含左间距</param>
        /// <param name="isEndInvterval">最后一个节点是否包含右间距</param>
        public PanelAlign(PanelNode node, Axle axle, AlignMode mode, bool isStartInvterval = false, bool isEndInvterval = false, float interval = 0f)
        {
            this._node = node;
            this.axle = axle;

            this._interval = interval;
            this.mode = mode;
            this._isStartInvterval = isStartInvterval;
            this._isEndInvterval = isEndInvterval;
        }

        /// <summary>
        /// 各子节点间距
        /// </summary>
        public float interval
        {
            get
            {
                return this._interval;
            }

            set
            {
                this._interval = value;
                this.fresh();
            }
        }

        /// <summary>
        /// 起始节点左侧是否包含间距
        /// </summary>
        public bool isStartInvterval
        {
            get
            {
                return this._isStartInvterval;
            }

            set
            {
                this._isStartInvterval = value;
                this.fresh();
            }
        }

        /// <summary>
        /// 结束节点右侧是否包含间距
        /// </summary>
        public bool isEndInvterval
        {
            get
            {
                return this._isEndInvterval;
            }

            set
            {
                this._isEndInvterval = value;
                this.fresh();
            }
        }

        /// <summary>
        /// 计算子节点累计总长度
        /// </summary>
        /// <param name="nodes">子节点列表</param>
        /// <param name="axle">布局作用轴</param>
        /// <returns></returns>
        private float computeSumChildLength(List<PanelLayer> nodes, Axle axle)
        {
            var result = 0f;

            // 计算所有子节点的总长度
            for (var i = 0; i < nodes.Count; i++)
            {
                var item = nodes[i];
                var itemBounds = item.localBounds;
                var interval = this.interval;

                // 累加起启位置interval
                if (i == 0 && !this.isStartInvterval)
                {
                    interval = 0;
                }

                // 累加结束位置interval
                if (i == nodes.Count - 1 && isEndInvterval)
                {
                    interval = interval * 2;
                }

                // x轴上节点总长度
                if (axle == Axle.right)
                {
                    result += itemBounds.size.x + interval;
                }

                // y轴上节点总长度
                if (axle == Axle.up)
                {
                    result += itemBounds.size.y + interval;
                }

                // z轴上节点总长度
                if (axle == Axle.forward)
                {
                    result += itemBounds.size.z + interval;
                }
            }

            return result;
        }

        /// <summary>
        /// 设置居中布局各节点坐标
        /// </summary>
        /// <param name="nodes">子节点</param>
        /// <param name="axle">布局作用轴</param>
        private void setLayoutPosition(List<PanelLayer> nodes, float sumChildLength, AlignMode alignMode, Axle axle)
        {
            // 计算每个子节点的空间位置坐标
            var currentStartCoord = 0f;
            if (alignMode == AlignMode.center)
            {
                if (axle == Axle.right)
                {
                    currentStartCoord = (this._node.width - sumChildLength) / 2;
                }
                if (axle == Axle.up)
                {
                    currentStartCoord = (this._node.height - sumChildLength) / 2;
                }
            }

            if(alignMode == AlignMode.right)
            {
                if (axle == Axle.right)
                {
                    currentStartCoord = this._node.width - sumChildLength;
                }
                if (axle == Axle.up)
                {
                    currentStartCoord = (this._node.height - sumChildLength);
                }
            }

            for (var i = 0; i < nodes.Count; i++)
            {
                var item = nodes[i];
                var itemBounds = item.localBounds;
                var interval = this.interval;

                // 累加起始位置interval
                if (i == 0 && !this.isStartInvterval)
                {
                    interval = 0;
                }

                // x轴
                if (axle == Axle.right)
                {
                    var new_coord = (currentStartCoord + interval);

                    var original_coord = Space.Coordinate.computeOriginalForCoordinateTranslation(new Vector3(-_node.width / 2, _node.height / 2, 0f), new Vector3(new_coord + itemBounds.size.x / 2, 0, 0)).x;

                    if(item.horizontalFloat == "none")
                    {
                        item.position.x = original_coord;
                    }

                    currentStartCoord = new_coord + itemBounds.size.x;
                }

                // y轴
                if (axle == Axle.up)
                {
                    var new_coord = currentStartCoord + interval;

                    var original_coord = Space.Coordinate.computeOriginalForCoordinateTranslation(new Vector3(-_node.width / 2, -_node.height / 2, 0f), new Vector3(0, new_coord + itemBounds.size.y / 2, 0)).y;

                    if(item.verticalFloat == "none")
                    {
                        item.position.y = original_coord;
                    }

                    currentStartCoord = new_coord + itemBounds.size.y;
                }
            }
        }

        /// <summary>
        /// 刷新
        /// </summary>
        public void fresh()
        {
            if (EMR.Common.Utils.equals(this._node.size.width, 0f) || EMR.Common.Utils.equals(this._node.size.height, 0f))
            {
                return;
            }

            if (this.mode == AlignMode.none)
            {
                return;
            }

            var childList = this._node.children;

            List<PanelLayer> children = new List<PanelLayer>();

            foreach(var item in childList)
            {
                if(item.horizontalFloat == "none" && item.verticalFloat == "none")
                {
                    children.Add(item);
                }
            }

            var sumChildLength = 0f;
            var nodeLength = 0f;

            // 处理left、right、center
            if(this.mode != AlignMode.between)
            {
                sumChildLength = this.computeSumChildLength(children, this.axle);

                var nodeList = children;
                if(this.axle == Axle.up)
                {
                    nodeList = new List<PanelLayer>();
                    for(var i = children.Count-1; i > -1; i--)
                    {
                        nodeList.Add(children[i]);
                    }
                }
                
                this.setLayoutPosition(nodeList, sumChildLength, this.mode, this.axle);

            }

            // 处理between
            if(this.mode == AlignMode.between)
            {
                if (this.axle == Axle.right)
                {
                    nodeLength = this._node.width;
                }
                if (this.axle == Axle.up)
                {
                    nodeLength = this._node.height;
                }


                foreach (var item in children)
                {
                    var itemBounds = item.localBounds;
                    if (this.axle == Axle.right)
                    {
                        sumChildLength += itemBounds.size.x;
                    }
                    if (this.axle == Axle.up)
                    {
                        sumChildLength += itemBounds.size.y;
                    }
                }

                if (sumChildLength > 0)
                {
                    var interval = 0f;

                    var nodeList = children;
                    if (this.axle == Axle.up)
                    {
                        nodeList = new List<PanelLayer>();
                        for (var i = children.Count - 1; i > -1; i--)
                        {
                            nodeList.Add(children[i]);
                        }
                    }

                    if (nodeList.Count > 1)
                    {
                        if (!isStartInvterval && !isEndInvterval)
                        {
                            interval = (nodeLength - sumChildLength) / (nodeList.Count - 1);
                        }
                        if (isStartInvterval && isEndInvterval)
                        {
                            interval = (nodeLength - sumChildLength) / (nodeList.Count + 1);
                        }
                        if (isStartInvterval == !isEndInvterval)
                        {
                            interval = (nodeLength - sumChildLength) / nodeList.Count;
                        }
                    }

                    // 设置当前初始起始坐标
                    var currentStartCoord = 0f;
                    if (!isStartInvterval && !isEndInvterval || !isStartInvterval && isEndInvterval)
                    {
                        currentStartCoord = -interval;
                    }
                    if (isStartInvterval && isEndInvterval || isStartInvterval && !isEndInvterval)
                    {
                        currentStartCoord = 0f;
                    }

                    // 设置各节点坐标 (子节点大于1时，从左侧依次排开)
                    if (nodeList.Count > 1)
                    {
                        for (var i = 0; i < nodeList.Count; i++)
                        {
                            var item = nodeList[i];
                            var itemBounds = item.localBounds;

                            // 对齐作用在x轴上时
                            if (this.axle == Axle.right)
                            {
                                var new_coord = currentStartCoord + interval;
                                var original_coord = Space.Coordinate.computeOriginalForCoordinateTranslation(new Vector3(-this._node.width / 2, this._node.height / 2, 0f), new Vector3(new_coord + (itemBounds.size.x) / 2, 0, 0)).x;

                                if (item.horizontalFloat == "none")
                                {
                                    item.position.x = original_coord;
                                }

                                currentStartCoord = new_coord + itemBounds.size.x;
                            }

                            // 对齐作用在y轴上时
                            if (this.axle == Axle.up)
                            {
                                var new_coord = currentStartCoord + interval;
                                var original_coord = Space.Coordinate.computeOriginalForCoordinateTranslation(new Vector3(-this._node.width / 2, -this._node.height / 2, 0f), new Vector3(0, new_coord + (itemBounds.size.y) / 2, 0)).y;

                                if (item.verticalFloat == "none")
                                {
                                    item.position.y = original_coord;
                                }
                                    
                                currentStartCoord = new_coord + itemBounds.size.y;
                            }
                        }
                    }

                    // 设置节点坐标 (子节点等于1时，相当于居中对齐)
                    if (nodeList.Count == 1)
                    {
                        // 对齐作用在x轴上时
                        if (this.axle == Axle.right && nodeList[0].horizontalFloat == "none")
                        {
                            nodeList[0].position.x = 0f;
                        }

                        // 对齐作用在y轴上时
                        if (this.axle == Axle.up && nodeList[0].verticalFloat == "none")
                        {
                            nodeList[0].position.y = 0f;
                        }
                    }
                }
            }

            // 对PanelLayer节点位置暂存进行同步
            foreach(var item in this._node.children)
            {
                item.positionSync();
            }
        }
    }
}
