using System;
using System.Collections.Generic;
using UnityEngine;
using EMR.Entity;
using EMR.Struct;

namespace EMR.Layout
{
    /// <summary>
    /// 对齐布局异常类
    /// </summary>
    public class AlignException : ApplicationException
    {
        private string error;

        public AlignException(string msg)
        {
            error = msg;
        }
    }

    public class SpaceAlign : ILayout
    {
        private SpaceNode _node;

        /// <summary>
        /// 作用轴
        /// </summary>
        private Axle axle = Axle.right;

        /// <summary>
        /// 间隔
        /// </summary>
        public float _interval = 0f;

        public float interval
        {
            get
            {
                return this._interval;
            }

            set
            {
                this._interval = value;
            }
        }

        // 开始是否加入间隔
        public bool isStartInvterval = false;

        // 结尾是否加入间隔
        public bool isEndInvterval = false;

        // 对齐模式
        public AlignMode mode = AlignMode.none;

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="axle">作用轴</param>
        /// <param name="interval">间距</param>
        /// <param name="mode">对齐方式</param>
        /// <param name="isStartInvterval">第一个节点是否包含左间距</param>
        /// <param name="isEndInvterval">最后一个节点是否包含右间距</param>
        public SpaceAlign(SpaceNode node, Axle axle, AlignMode mode, bool isStartInvterval = false, bool isEndInvterval = false, float interval = 0f)
        {
            this._node = node;
            this.axle = axle;
            this.mode = mode;
            this.interval = interval;
            this.isStartInvterval = isStartInvterval;
            this.isEndInvterval = isEndInvterval;
        }

        /// <summary>
        /// 计算子节点累计总长度
        /// </summary>
        /// <param name="nodes">子节点列表</param>
        /// <param name="axle">布局作用轴</param>
        /// <returns></returns>
        private float computeSumChildLength(List<ISpaceCharacteristic> nodes, Axle axle)
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
                if(axle == Axle.right)
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
        private void setLayoutPosition(List<ISpaceCharacteristic> nodes, float sumChildLength, AlignMode alignMode, Axle axle)
        {
            // 计算每个子节点的空间位置坐标
            var currentStartCoord = 0f;
            if(alignMode == AlignMode.center) { 
                if (axle == Axle.right)
                {
                    currentStartCoord = (this._node.width - sumChildLength) / 2;
                }
                if (axle == Axle.up)
                {
                    currentStartCoord = (this._node.height - sumChildLength) / 2;
                }
                if (axle == Axle.forward)
                {
                    currentStartCoord = (this._node.depth - sumChildLength) / 2;
                }
            }

            if (alignMode == AlignMode.right)
            {
                if (axle == Axle.right)
                {
                    currentStartCoord = this._node.width - sumChildLength;
                }
                if (axle == Axle.up)
                {
                    currentStartCoord = (this._node.height - sumChildLength);
                }
                if (axle == Axle.forward)
                {
                    currentStartCoord = this._node.depth - sumChildLength;
                }
            }

            for (var i = 0; i < nodes.Count; i++)
            {
                ISpaceCharacteristic item = (ISpaceCharacteristic) nodes[i];
                var itemBounds = item.localBounds;
                var interval = this.interval;

                // 累加起始位置interval
                if (i == 0 && !this.isStartInvterval)
                {
                    interval = 0;
                }

                // x轴
                if(axle == Axle.right)
                {
                    var new_coord = (currentStartCoord + interval);

                    var original_coord = Space.Coordinate.computeOriginalForCoordinateTranslation(new Vector3(-_node.width / 2, _node.height / 2, -_node.depth / 2), new Vector3(new_coord + itemBounds.size.x / 2, 0, 0)).x;
                    item.x = original_coord;

                    currentStartCoord = new_coord + itemBounds.size.x;
                }

                // y轴
                if (axle == Axle.up)
                {
                    var new_coord = currentStartCoord + interval;

                    var original_coord = Space.Coordinate.computeOriginalForCoordinateTranslation(new Vector3(-_node.width / 2, -_node.height / 2, -_node.depth / 2), new Vector3(0, new_coord + itemBounds.size.y / 2, 0)).y;
                    item.y = original_coord;

                    currentStartCoord = new_coord + itemBounds.size.y;
                }

                // z轴
                if (axle == Axle.forward)
                {
                    var new_coord = (currentStartCoord + interval);

                    var original_coord = Space.Coordinate.computeOriginalForCoordinateTranslation(new Vector3(-_node.width / 2, _node.height / 2, -_node.depth / 2), new Vector3(0, 0, new_coord + itemBounds.size.z / 2)).z;
                    item.z = original_coord;

                    currentStartCoord = new_coord + itemBounds.size.z;
                }
            }
        }

        /// <summary>
        /// 刷新
        /// </summary>
        public void fresh()
        {
            if (this.mode == AlignMode.none)
            {
                return;
            }

            var tempList = this._node.children;
            var children = new List<ISpaceCharacteristic>();

            foreach (var item in tempList)
            {
                if (item is ISpaceCharacteristic && item.horizontalFloat == "none" && item.verticalFloat == "none" && (item is SpaceNode && ((SpaceNode)item).forwardFloat == "none" || item is PanelRoot && ((PanelRoot)item).forwardFloat == "none" || item is SpaceMagic && ((SpaceMagic)item).forwardFloat == "none"))
                {
                    children.Add((ISpaceCharacteristic)item);
                }
            }

            var sumChildLength = 0f;
            var nodeLength = 0f;

            if (this.mode != AlignMode.between)
            {
                sumChildLength = this.computeSumChildLength(children, this.axle);

                var nodeList = children;
                if (this.axle == Axle.up)
                {
                    nodeList = new List<ISpaceCharacteristic>();
                    for (var i = children.Count - 1; i > -1; i--)
                    {
                        nodeList.Add(children[i]);
                    }
                }

                this.setLayoutPosition(nodeList, sumChildLength, this.mode, this.axle);
            }

            if (this.mode == AlignMode.between)
            {
                if (this.axle == Axle.right)
                {
                    nodeLength = this._node.width;
                }
                if (this.axle == Axle.up)
                {
                    nodeLength = this._node.height;
                }
                if (this.axle == Axle.forward)
                {
                    nodeLength = this._node.depth;
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
                    if (this.axle == Axle.forward)
                    {
                        sumChildLength += itemBounds.size.z;
                    }
                }

                if (sumChildLength > 0)
                {
                    var interval = 0f;

                    var nodeList = children;
                    if (this.axle == Axle.up)
                    {
                        nodeList = new List<ISpaceCharacteristic>();
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


                    // 设置各节点坐标
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
                                var original_coord = Space.Coordinate.computeOriginalForCoordinateTranslation(new Vector3(-this._node.width / 2, this._node.height / 2, -this._node.depth / 2), new Vector3(new_coord + itemBounds.size.x / 2, 0, 0)).x;

                                item.x = original_coord;

                                currentStartCoord = new_coord + itemBounds.size.x;
                            }

                            // 对齐作用在y轴上时
                            if (this.axle == Axle.up)
                            {
                                var new_coord = currentStartCoord + interval;
                                var original_coord = Space.Coordinate.computeOriginalForCoordinateTranslation(new Vector3(-this._node.width / 2, -this._node.height / 2, -this._node.depth / 2), new Vector3(0, new_coord + itemBounds.size.y / 2, 0)).y;

                                item.y = original_coord;

                                currentStartCoord = new_coord + itemBounds.size.y;
                            }

                            // 对齐作用在z轴上时
                            if (this.axle == Axle.forward)
                            {
                                var new_coord = currentStartCoord + interval;
                                var original_coord = Space.Coordinate.computeOriginalForCoordinateTranslation(new Vector3(-this._node.width / 2, this._node.height / 2, -this._node.depth / 2), new Vector3(0, 0, new_coord + itemBounds.size.z / 2)).z;

                                item.z = original_coord;

                                currentStartCoord = new_coord + itemBounds.size.z;
                            }
                        }
                    }

                    // 设置节点坐标 (子节点等于1时，相当于居中对齐)
                    if (nodeList.Count == 1)
                    {
                        // 对齐作用在x轴上时
                        if (this.axle == Axle.right)
                        {
                            nodeList[0].x = 0f;
                        }

                        // 对齐作用在y轴上时
                        if (this.axle == Axle.up)
                        {
                            nodeList[0].y = 0f;
                        }

                        // 对齐作用在y轴上时
                        if (this.axle == Axle.forward)
                        {
                            nodeList[0].z = 0f;
                        }
                    }
                }
            }
        }
    }
}
