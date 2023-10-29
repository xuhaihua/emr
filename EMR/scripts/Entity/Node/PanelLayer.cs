using System.Collections.Generic;
using System;
using UnityEngine;
using EMR.Event;
using EMR.Struct;
using EMR.Common;
using EMR.Layout;

namespace EMR.Entity
{
    /// <summary>
    /// PanelLayer节点处理委托
    /// </summary>
    /// <param name="node"></param>
    public delegate bool PanelLayerHandler(PanelLayer node);

    /// <summary>
    /// 平面图层节点节点异常类
    /// </summary>
    public class PanelLayerException : ApplicationException
    {
        private string error;

        public PanelLayerException(string msg)
        {
            error = msg;
        }
    }

    /// <summary>
    /// 基本平面图层节点类（该类节点主要用于平面布局）
    /// </summary>
    public class PanelLayer: PanelNode, IPanelLayerCharacteristic, IPanelLayoutFeature
    {
        #region 基本字段定义
        /// <summary>
        /// 间隔(单位制）
        /// </summary>
        private float interval = 0.01f;

        /// <summary>
        /// 节点层高
        /// </summary>
        private int _layoutHeight = 1;

        // 位置、尺寸是否被有效设过值逻辑标识
        private bool _hasLeftSeted = false;
        private bool _hasTopSeted = false;
        #endregion

        #region 构造器
        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="left">left</param>
        /// <param name="top">top</param>
        /// <param name="right">right</param>
        /// <param name="bottom">bottom</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="depth">厚度</param>
        /// <param name="zIndex">层叠次序</param>
        /// <param name="isPanel">是否为平面</param>
        /// <param name="npc">npc</param>
        /// <param name="rendMode">渲染模式</param>
        public PanelLayer(float? left, float? top, float? right, float? bottom, float width, float height, float depth,  CoreComponent component, int zIndex = 0, bool isPanel = true, NPC npc = null, RendMode rendMode = RendMode.opaque) : base(0, 0, 0, 0, 0, 0, width, height, depth, isPanel, npc)
        {
            this._component = component;

            this.widthFixed = true;
            this.heightFixed = true;
            this.depthFixed = true;

            this._zIndex = zIndex;
            this._left = left;
            this._top = top;
            this._right = right;
            this._bottom = bottom;
            this._width = width;
            this._height = height;
            this._depth = depth;
            this.renderMode = renderMode;

            // 添加样式
            this.styleCollect.add("zIndex");
            this.styleCollect.add("left");
            this.styleCollect.add("top");
            this.styleCollect.add("right");
            this.styleCollect.add("bottom");
            this.styleCollect.add("leftFixed");
            this.styleCollect.add("topFixed");
        }

        /// <summary>
        /// 编译调用
        /// </summary>
        /// <param name="parasitifer"></param>
        public PanelLayer(CoreComponent component) : base(0, 0, 0, 0, 0, 0, 0, 0, 0, true, null)
        {
            this._component = component;

            this.widthFixed = true;
            this.heightFixed = true;
            this.depthFixed = true;

            // 添加样式
            this.styleCollect.add("zIndex");
            this.styleCollect.add("left");
            this.styleCollect.add("top");
            this.styleCollect.add("right");
            this.styleCollect.add("bottom");
            this.styleCollect.add("leftFixed");
            this.styleCollect.add("topFixed");
        }
        #endregion

        #region 基本属性
        private int _zIndex = 0;
        /// <summary>
        /// 层叠次序
        /// </summary>
        public int zIndex
        {
            get
            {
                return this._zIndex;
            }

            set
            {
                this._zIndex = value;

                // 对平面层叠次序进行排序
                this.compountCoordZ();
            }
        }

        /*--------------------------------------------------定义尺寸、位置支撑方法开始--------------------------------------------------*/
        /// <summary>
        /// 计算position x
        /// </summary>
        public void computePositionX()
        {
            // 定位x坐标
            if (this.parent != null)
            {
                if (this._left != null)
                {
                    this.position.x = Space.Coordinate.computeOriginalForCoordinateTranslation(new Vector3(-this.parent.width / 2, this.parent.height / 2, 0), new Vector3((float)_left + this.width / 2, 0, 0)).x;
                }
                else if (this._right != null)
                {
                    this.position.x = Space.Coordinate.computeOriginalForCoordinateTranslation(new Vector3(-this.parent.width / 2, this.parent.height / 2, 0), new Vector3(this.parent.width - ((float)_right + this.width / 2), 0, 0)).x;
                }
            }
        }

        /// <summary>
        /// 计算position y
        /// </summary>
        public void computePositionY()
        {
            // 定位y坐标
            if (this.parent != null)
            {
                if (_top != null)
                {
                    this.position.y = Space.Coordinate.computeOriginalForCoordinateTranslation(new Vector3(-this.parent.width / 2, this.parent.height / 2, 0), new Vector3(0, -((float)_top + this.height / 2), 0)).y;
                }
                else if (_bottom != null)
                {
                    this.position.y = Space.Coordinate.computeOriginalForCoordinateTranslation(new Vector3(-this.parent.width / 2, this.parent.height / 2, 0), new Vector3(0, -(this.parent.height - ((float)_bottom + this.height / 2)), 0)).y;
                }
            }
        }

        /// <summary>
        /// 计算宽度
        /// </summary>
        public void computeWidth()
        {
            if(this.parent != null)
            {
                this._width = Math.Abs((float)this._left - (this.parent.width - (float)this._right));
                this.size.width = this._width;
            }
        }

        /// <summary>
        /// 计算高度
        /// </summary>
        public void computeHeight()
        {
            // 计算高度
            if(this.parent != null)
            {
                this._height = Math.Abs((float)this._top - (this.parent.height - (float)this._bottom));
                this.size.height = this._height;
            }
        }
        /*--------------------------------------------------定义尺寸、位置支撑方法结束--------------------------------------------------*/
        private bool _leftFixed = true;
        /// <summary>
        /// left是否固定
        /// </summary>
        public bool leftFixed
        {
            get
            {
                return this._leftFixed;
            }

            set
            {
                if(value && !this._leftFixed)
                {
                    this._hasLeftSeted = false;
                }
                this._leftFixed = value;
            }
        }
        
        private float? _left = null;
        /// <summary>
        /// 左侧坐标
        /// </summary>
        public float? left
        {
            get
            {
                var reslut = this._left != null && this.parent != null ? this.parent.width / 2 + (this.position.x - this.size.width / 2) : this._left;

                return reslut;

            }

            set
            {
               

                this._left = value;

                // 防止在append之前计算尺寸、位置
                if (this.parent == null)
                {
                    return;
                }

                this.computePositionX();

                if (this._right != null && this._left != null)
                {
                    this.computeWidth();
                }
            }
        }
        
        private bool _topFixed = true;
        /// <summary>
        /// top是否固定
        /// </summary>
        public bool topFixed
        {
            get
            {
                return this._topFixed;
            }

            set
            {
                if (value && !this._topFixed)
                {
                    this._hasTopSeted = false;
                }
                this._topFixed = value;
            }
        }

        public static Node tcc = null;

        private float? _top = null;
        /// <summary>
        /// 顶部坐标
        /// </summary>
        public float? top
        {
            get
            {
               

                var reslut = this._top != null && this.parent != null ? this.parent.height / 2 - (this.position.y + this.size.height / 2) : this._top;

                return reslut;
            }

            set
            {
                this._top = value;

                // 防止在append之前计算尺寸、位置
                if (this.parent == null)
                {
                    return;
                }

                this.computePositionY();

                
                if (this._top != null && this._bottom != null)
                {
                    this.computeHeight();
                }
            }
        }

        private float? _right = null;
        /// <summary>
        /// 右侧坐标
        /// </summary>
        public float? right
        {
            get
            {
                var reslut = this._right != null && this.parent != null ?this.parent.width / 2 - (this.position.x + this.size.width / 2) : this._right;

                return reslut;
            }

            set
            {
                this._right = value;
                
                // 防止在append之前计算尺寸、位置
                if (this.parent == null)
                {
                    return;
                }

                this.computePositionX();

                if (this._right != null && this._left != null)
                {
                    this.computeWidth();
                }
            }
        }

        private float? _bottom = null;
        /// <summary>
        /// 底部坐标
        /// </summary>
        public float? bottom
        {
            get
            {
                var reslut = this._bottom != null && this.parent != null ? this.parent.height / 2 + (this.position.y - this.size.height / 2) : this._bottom;

                return reslut;
            }

            set
            {
                this._bottom = value;

                // 防止在append之前计算尺寸、位置
                if (this.parent == null)
                {
                    return;
                }

                this.computePositionY();

                if (this._top != null && this._bottom != null)
                {
                    this.computeHeight();
                }
            }
        }

        private float _z = 0f;
        /// <summary>
        /// z坐标
        /// </summary>
        private float z
        {
            get
            {
                return _z;
            }

            set
            {
                this._z = value;
            }
        }

        /// <summary>
        /// 宽度
        /// </summary>
        public override float width
        {
            get
            {
                var reslut = this.size.width;

                return reslut;
            }

            set
            {
                this._width = value;

                if (this._right != null && this._left != null)
                {
                    if (this.parent == null)
                    {
                        return;
                    }

                    this.computePositionX();
                    this.computeWidth();
                } else
                {
                    this.size.width = value;
                }
            }
        }

        /// <summary>
        /// 高度
        /// </summary>
        public override float height
        {
            get
            {
                var reslut = this.size.height;

                return reslut;
            }

            set
            {
                this._height = value;

                if (this._bottom != null && this._top != null)
                {
                    if (this.parent == null)
                    {
                        return;
                    }

                    this.computePositionY();
                    this.computeHeight();
                }
                else
                {
                    this.size.height = value;
                }
            }
        }

        /// <summary>
        /// 深度
        /// </summary>
        public override float depth
        {
            get
            {
                var reslut = this.isPanel ? 0 : this.size.depth;

                return reslut;
            }

            set
            {
                var num = Space.Unit.unitToScale(this, value, Axle.forward);

                this._depth = this.isPanel ? 0 : value;

                if (!float.IsNaN(num) && !float.IsInfinity(num))
                {
                    if(!this.isPanel)
                    {
                        this.size.depth = value;
                    } else
                    {
                        this.size.depth = 1000f;
                    }
                    
                }
            }
        }

        /// <summary>
        /// bounds
        /// </summary>
        public override BoundBox localBounds
        {
            get
            {
                // 节点中心点
                var x = this.position.x;
                var y = this.position.y;
                var z = this.position.z;
                var center = this.parasitifer.transform.TransformPoint(new Vector3(x, y, z));

                return new BoundBox(center, new Vector3(this.width, this.height, this.depth));
            }
        }
        #endregion

        #region 基本方法
        /// <summary>
        /// 解锁包括子节点在内的所有节点的尺寸有效设值过逻辑标识
        /// </summary>
        public override void unlockSizeSetedTag()
        {
            this.ergodic((PanelLayer node) =>
            {
                node._hasWidthSeted = false;
                node._hasHeightSeted = false;
                node._hasDepthSeted = false;

                return true;
            });
        }

        /// <summary>
        /// 解锁包括子节点在内的所有节点的位置有效设值过逻辑标识
        /// </summary>
        public void unlockPositionSetedTag()
        {
            this.ergodic((PanelLayer node) =>
            {
                node._hasLeftSeted = false;
                node._hasTopSeted = false;

                return true;
            });
        }

        /// <summary>
        /// 是否有零节点
        /// </summary>
        /// <returns></returns>
        private bool hasZeroNode()
        {
            Node node = this.parent;
            bool result = false;
            while (node != null && !(node is PanelRoot))
            {
                if (Utils.equals(node.size.width, 0f) || Utils.equals(node.size.height, 0f))
                {
                    result = true;
                    break;
                }
                node = node.parent;
            }
            return result;
        }


        /*
         * 以下方法主要用于处理z坐标
         */
        /// <summary>
        /// 层高复位(将全部节点层高设置为1)
        /// </summary>
        private void resetLayoutHeight()
        {
            this._layoutHeight = 1;

            foreach (PanelLayer item in this.children)
            {
                item.resetLayoutHeight();
            }
        }

        /// <summary>
        /// 计算层高度(包含其子节点)
        /// </summary>
        private void computeLayoutHeight()
        {
            // 层查找(在当前路径下深度定位到包含子节点的最后一层)
            foreach (PanelLayer item in this.children)
            {
                if (item.children.Count > 0)
                {
                    item.computeLayoutHeight();
                }
            }

            // 将层内节点按zIndex分组
            Dictionary<int, List<PanelLayer>> groupDictionary = new Dictionary<int, List<PanelLayer>>();
            for (var i = 0; i < this.children.Count; i++)
            {
                // 如果字典内没有对应的zIndex节点列表则创建
                var item = this.children[i];
                if (!groupDictionary.ContainsKey(item.zIndex))
                {
                    groupDictionary.Add(item.zIndex, new List<PanelLayer>());
                }

                // 获取与zIndex相关联的列表
                var list = groupDictionary[item.zIndex];

                // 将当前节点加入该列表
                list.Add(item);
            }

            // 计算分组层高
            var groupLayoutHeight = 0;
            foreach (var item in groupDictionary)
            {
                // 计算每个分组内的最高层高
                var maxLayoutHeight = 0;
                for (var i = 0; i < item.Value.Count; i++)
                {
                    var node = item.Value[i];
                    if (maxLayoutHeight < node._layoutHeight)
                    {
                        maxLayoutHeight = node._layoutHeight;
                    }
                }

                // 累加每个分组层高
                groupLayoutHeight += maxLayoutHeight;
            }

            // 设置层高
            this._layoutHeight += groupLayoutHeight;
        }

        /// <summary>
        /// 层高刷新
        /// </summary>
        public void freshLayoutHeight()
        {
            this.resetLayoutHeight();
            this.computeLayoutHeight();
        }

        /// <summary>
        /// 比较器
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private int compareHandler(int a, int b)
        {
            if (a > b)
            {
                return 1;
            }
            else if (a == b)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// 计算z坐标
        /// </summary>
        public void compountCoordZ()
        {
            // 从层叠次序计算出每个平面子节点的z坐标
            if (this.parent != null)
            {
                var parent = this.parent;

                // zIndex 列表
                List<int> zIndexSortList = new List<int>();

                // 将层内节点按zIndex分组
                Dictionary<int, List<PanelLayer>> groupDictionary = new Dictionary<int, List<PanelLayer>>();
                foreach (var item in parent.children)
                {
                    // 如果没有对应的zIndex分组则创建
                    if (!groupDictionary.ContainsKey(item.zIndex))
                    {
                        groupDictionary.Add(item.zIndex, new List<PanelLayer>());

                        // 向zIndex排序列表加入zIndex
                        zIndexSortList.Add(item.zIndex);
                    }

                    // 将节点加入对应的分组列表内
                    var list = groupDictionary[item.zIndex];
                    list.Add(item);
                }

                // z index list排序
                zIndexSortList.Sort(compareHandler);

                // 计算每个节点的z坐标
                var sumHeight = 0f;
                for (var i = 0; i < zIndexSortList.Count; i++)
                {
                    var zIndex = zIndexSortList[i];

                    // 计算当前分组层高
                    var list = groupDictionary[zIndex];

                    // 计算分组层高
                    var groupLayoutHeight = 0;
                    foreach (var item in list)
                    {
                        if (groupLayoutHeight < item._layoutHeight)
                        {
                            groupLayoutHeight = item._layoutHeight;
                        }
                    }

                    // 设置分组内每个节点的z坐标
                    foreach (var item in list)
                    {
                        var coordZ = sumHeight - interval + item.depth / 2;
                        item.z = coordZ;
                        item.position.z = coordZ;
                    }

                    sumHeight = sumHeight - groupLayoutHeight * interval;
                }
            }
        }

        /*
         *  以下方法主要用于位置、尺寸的刷新
         */
        /// <summary>
        /// 位置刷新
        /// </summary>
        public override void positionFresh()
        {
            // 只有当left为固定时或left从未被设过值时需要计算x坐标
            if (this.leftFixed || !this.leftFixed && !this._hasLeftSeted)
            {
                this.computePositionX();
            }

            // 当left为非固定时，计算left是否被已被有效设过值
            if (!this.leftFixed)
            {
                if (!this._hasLeftSeted && !this.hasZeroNode())
                {
                    this._hasLeftSeted = true;
                }
                else if (this._hasLeftSeted)
                {
                    this._left = this.left;
                }
            }

            // 只有当top为固定时或top从未被设过值时需要计算y坐标
            if (this.topFixed || !this.topFixed && !this._hasTopSeted)
            {
                this.computePositionY();
            }

            // 当top为非固定时，计算left是否被已被有效设过值
            if (!this.topFixed)
            {
                if (!this._hasTopSeted && !this.hasZeroNode())
                {
                    this._hasTopSeted = true;
                }
                else if (this._hasTopSeted)
                {
                    this._top = this.top;
                }
            }

            if ((this.isDepthChanging || !Utils.equals(this.depth, this.previousSize.depth)) && !this.isPanel)
            {
                this.compountCoordZ();
            }
            else
            {
                this.position.z = this.z;
            }
        }

        /// <summary>
        /// 尺寸刷新
        /// </summary>
        public override void sizeFresh()
        {
            // 只有当width为固定时或width从未被设过值时需要计算width
            if (this.widthFixed || !this.widthFixed && !this._hasWidthSeted)
            {
                if (this._right != null && this._left != null)
                {
                    this.computeWidth();
                }
                else
                {
                    this.size.width = this._width;
                }
            }

            // 当width为非固定时，计算width是否被已被有效设过值
            if (!this.widthFixed)
            {
                if (!this._hasWidthSeted && !this.hasZeroNode())
                {
                    this._hasWidthSeted = true;
                }
                else if (this._hasWidthSeted)
                {
                    this._width = this.width;
                }
            }

            // 只有当height为固定时或height从未被设过值时需要计算width
            if (this.heightFixed || !this.heightFixed && !this._hasHeightSeted)
            {
                if (this._top != null && this._bottom != null)
                {
                    this.computeHeight();
                }
                else
                {
                    this.size.height = this._height;
                }
            }

            // 当height为非固定时，计算height是否被已被有效设过值
            if (!this.heightFixed)
            {
                if (!this._hasHeightSeted && !this.hasZeroNode())
                {
                    this._hasHeightSeted = true;
                }
                else if (this._hasHeightSeted)
                {
                    this._height = this.height;
                }
            }

            // 只有当depth为固定时或depth从未被设过值时需要计算y坐标
            if (this.depthFixed || !this.depthFixed && !this._hasDepthSeted)
            {
                this.depth = this._depth;
            }

            // 当depth为非固定时，计算depth是否被已被有效设过值
            if (!this.depthFixed)
            {
                if (!this._hasDepthSeted && !this.hasZeroNode())
                {
                    this._hasDepthSeted = true;
                }
                else if (this._hasDepthSeted)
                {
                    this._depth = this.depth;
                }
            }
        }

        /// <summary>
        /// 位置同步
        /// </summary>
        public void positionSync()
        {
            this._left = this.left;
            this._top = this.top;
            this._right = this.right;
            this._bottom = this.bottom;
        }

        /// <summary>
        /// 尺寸同步
        /// </summary>
        public void sizeSync()
        {
            this._width = this.width;
            this._height = this.height;
            this._depth = this.depth;
        }

        /*
         *  以下方法主要用于节点运动
         */
        /// <summary>
        /// 移动
        /// </summary>
        /// <param name="endLeft">终点left</param>
        /// <param name="endTop">终点top</param>
        /// <param name="time">动画时长</param>
        /// <param name="curveType">动画曲线</param>
        /// <param name="callback">动画结束回调</param>
        public void moveTo(float endLeft, float endTop, float time, MotionCurve curveType, AnimationCallback callback = null)
        {
            var startLeft = (float)this.left;
            var startTop = (float)this.top;

            if (Utils.equals(startLeft, endLeft) && Utils.equals(startTop, endTop))
            {
                return;
            }

            var startVector = new Vector2(startLeft, startTop);

            var endX = float.IsNaN(endLeft) ? startVector.x : endLeft;
            var endY = float.IsNaN(endTop) ? startVector.y : endTop;

            var endVector = new Vector2(endX, endY);

            // 执行动画
            this.animation(startVector, endVector, time, curveType, (Vector3 data, bool isFinish) =>
            {
                this.left = data.x;
                this.top = data.y;

                if (!isFinish)
                {
                    // 在动画的过程中不触发事件
                    this.ergodic((Node node) =>
                    {
                        node.preventPositionEvent = true;
                        return true;
                    });
                }

            }, callback);
        }

        /// <summary>
        /// 缩放
        /// </summary>
        /// <param name="end">目标尺寸</param>
        /// <param name="time">动画时长</param>
        /// <param name="curveType">动画曲线</param>
        /// <param name="callback">动画结束回调</param>
        public void sizeTo(SizeData end, float time, MotionCurve curveType, AnimationCallback callback = null)
        {
            if (Utils.equals(this.width, end.width) && Utils.equals(this.height, end.height) && Utils.equals(this.depth, end.depth))
            {
                return;
            }

            var startVector = new Vector3(this.width, this.height, this.depth);

            var endWidth = float.IsNaN(end.width) ? startVector.x : end.width;
            var endHeight = float.IsNaN(end.height) ? startVector.y : end.height;
            var endDepth = float.IsNaN(end.depth) ? startVector.z : end.depth;

            var endVector = new Vector3(endWidth, endHeight, endDepth);

            // 执行动画
            this.animation(startVector, endVector, time, curveType, (Vector3 data, bool isFinish) =>
            {
                this.width = data.x;
                this.height = data.y;
                this.depth = data.z;

                if (!isFinish)
                {
                    // 在动画的过程中不触发事件
                    this.ergodic((PanelLayer node) =>
                    {
                        // 此处要对npc进行刷新因为panelNode节点有可能各维度不是等比缩放
                        if (node.npc != null)
                        {
                            ((PanelLayer)node).npcFresh();
                        }

                        node.positionFresh();

                        node.preventPositionEvent = true;
                        node.preventSizeEvent = true;

                        return true;
                    });
                }

            }, callback);
        }
        #endregion

        #region 基本数据结构操作相关
        /// <summary>
        /// 获取父节点
        /// </summary>
        public new PanelNode parent
        {
            get
            {
                var result = base.parent;
                return result != null ? (PanelNode)result : null;
            }
        }

        /// <summary>
        /// 插入子节点
        /// </summary>
        /// <param name="current">当前要插入的节点</param>
        /// <param name="refNode">参照前点</param>
        /// <returns></returns>
        public override PanelLayer insertBefore(PanelLayer current, Node refNode)
        {
            PanelNode result = null;

            // 抛出节点插入前事件
            InsertEventData insertEventData = new InsertEventData
            {
                target = current,
            };
            onInsert.Invoke(insertEventData);
            
            // 使用默认行为插入节点
            if (!insertEventData.isPreventDefault)
            {
                var oldNodeParent = current.parent;

                // 解锁节点尺寸有效设值过逻辑标识
                current.unlockSizeSetedTag();

                // 解锁节点位置有效设值过逻辑标识
                current.unlockPositionSetedTag();

                result = base.insertBefore(current, refNode);

                // 引发布局更新
                if (result != null)
                {
                    PanelLayout.update(new PanelNodeActionInfo
                    {
                        originalParent = oldNodeParent,
                        current = current,
                        currentParent = this,
                        action = PanelNodeAction.insert
                    });
                }
            }

            // 抛出节点插入完成事件
            InsertedEventData insertedEventData = new InsertedEventData
            {
                target = current,
                isSuccess = result != null ? true : false,
            };
            onInserted.Invoke(insertedEventData);

            // 返回插入结果
            return result != null ? (PanelLayer)result : null;
        }

        /// <summary>
        /// 插入子节点
        /// </summary>
        /// <param name="node">要添加的节点</param>
        /// <returns></returns>
        public override Node insertBefore(Node current, Node refNode)
        {
            if (!(current is PanelLayer))
            {
                Debug.LogError("insertBefore 输入参数类型不正确!");
                return null;
            }

            return this.insertBefore((PanelLayer)current, refNode);
        }

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="node">当前要添加的节点</param>
        /// <returns></returns>
        public override PanelLayer appendNode(PanelLayer node)
        {
            PanelNode result = null;

            // 抛出节点添加前事件
            AppendEventData appendEventData = new AppendEventData
            {
                target = node
            };
            onAppend.Invoke(appendEventData);

            // 使用系统默认行为添加节点
            if (!appendEventData.isPreventDefault)
            {
                // 节点在原结构下的父节点
                var oldNodeParent = node.parent;

                // 解锁节点尺寸有效设值过逻辑标识
                node.unlockSizeSetedTag();

                // 解锁节点位置有效设值过逻辑标识
                node.unlockPositionSetedTag();

                result = base.appendNode(node);

                // 引发布局更新
                if(result != null)
                {
                    PanelLayout.update(new PanelNodeActionInfo
                    {
                        originalParent = oldNodeParent,
                        current = node,
                        currentParent = this,
                        action = PanelNodeAction.append
                    });
                }
            }

            // 拙出节点添加完成事件
            AppendedEventData appendedEventData = new AppendedEventData
            {
                target = node,
                isSuccess = result != null ? true : false,
            };
            onAppended.Invoke(appendedEventData);

            // 返回添加结果
            return result != null ? (PanelLayer)result : null;
        }

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="node">要添加的节点</param>
        /// <returns></returns>
        public override Node appendNode(Node node)
        {
            if (!(node is PanelLayer))
            {
                Debug.LogError("appendNode 输入参数类型不正确!");
                return null;
            }

            return this.appendNode((PanelLayer)node);
        }

        /// <summary>
        /// 遍历Node
        /// </summary>
        /// <param name="node">要添加的节点</param>
        public void ergodic(PanelLayerHandler nodeHandler)
        {
            nodeHandler(this);
            foreach (PanelLayer item in this.children)
            {
                item.ergodic(nodeHandler);
            }
        }
        #endregion
    }
}
