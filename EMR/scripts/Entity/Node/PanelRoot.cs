/*
 * PanelRoot节点是一个比较特殊的节点，它虽然是一个平面但它却在空间中，所以它继承了ISpaceCharacteristic接口拥有与SpaceNode一样的空间特征：尺寸、空间位置、旋转量同时它也拥有空间布局的能力
 */

using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UI.BoundsControlTypes;
using UnityEngine;
using EMR.Common;
using EMR.Event;
using EMR.Plugin;
using EMR.Struct;
using EMR.Layout;

namespace EMR.Entity
{
    /// <summary>
    /// PanelLayer 根节点
    /// </summary>
    public class PanelRoot : PanelNode, ISpaceCharacteristic, ISpaceLayoutFeature, ISizeBoundsEventNode, ICollisionEventFeature
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x">x坐标</param>
        /// <param name="y">y坐标</param>
        /// <param name="z">z坐标</param>
        /// <param name="xAngle">x轴旋转量</param>
        /// <param name="yAngle">y轴旋转量</param>
        /// <param name="zAngle">z轴旋转量</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="depth">厚度</param>
        /// <param name="isPanel">是否为平面</param>
        /// <param name="npc">npc</param>
        /// <param name="rendMode">渲染模式</param>
        public PanelRoot(float x, float y, float z, float xAngle, float yAngle, float zAngle, float width, float height, float depth, CoreComponent component, bool isPanel = true, NPC npc = null, RendMode rendMode = RendMode.opaque) : base(x, y, z, xAngle, yAngle, zAngle, width, height, depth, isPanel, npc)
        {
            this._component = component;

            this.widthFixed = false;
            this.heightFixed = false;
            this.depthFixed = true;

            this.renderMode = renderMode;
            
            this._width = width;
            this._height = height;
            this._depth = depth;

            this._x = x;
            this._y = y;
            this._z = z;

            this._xAngle = xAngle;
            this._yAngle = yAngle;
            this._zAngle = zAngle;

            // 一直在每个周期的末监控（用于节点旋转的调整）
            this.service.lateNext(() =>
            {
                if (this.isRotating)
                {
                    SpaceLayout.update(new SpaceNodeActionInfo
                    {
                        originalParent = this.parent,
                        current = this,
                        currentParent = this,
                        action = SpaceNodeAction.rotationChange
                    });
                }

                return false;
            });

            // 添加样式
            this.styleCollect.add("x");
            this.styleCollect.add("y");
            this.styleCollect.add("z");
            this.styleCollect.add("xAngle");
            this.styleCollect.add("yAngle");
            this.styleCollect.add("zAngle");
            this.styleCollect.add("forwardFloat");
            this.styleCollect.add("xFixed");
            this.styleCollect.add("yFixed");
            this.styleCollect.add("zFixed");
            this.styleCollect.add("xAngleFixed");
            this.styleCollect.add("yAngleFixed");
            this.styleCollect.add("zAngleFixed");
        }

        /// <summary>
        /// 编译调用
        /// </summary>
        /// <param name="parasitifer"></param>
        public PanelRoot(CoreComponent component) : base(0, 0, 0, 0, 0, 0, 0, 0, 0, true, null)
        {
            this._component = component;

            this.widthFixed = false;
            this.heightFixed = false;
            this.depthFixed = true;

            // 一直在每个周期的末监控（用于节点旋转的调整）
            this.service.lateNext(() =>
            {
                if (this.isRotating)
                {
                    SpaceLayout.update(new SpaceNodeActionInfo
                    {
                        originalParent = this.parent,
                        current = this,
                        currentParent = this,
                        action = SpaceNodeAction.rotationChange
                    });
                }

                return false;
            });

            // 添加样式
            this.styleCollect.add("x");
            this.styleCollect.add("y");
            this.styleCollect.add("z");
            this.styleCollect.add("xAngle");
            this.styleCollect.add("yAngle");
            this.styleCollect.add("zAngle");
            this.styleCollect.add("forwardFloat");
            this.styleCollect.add("xFixed");
            this.styleCollect.add("yFixed");
            this.styleCollect.add("zFixed");
            this.styleCollect.add("xAngleFixed");
            this.styleCollect.add("yAngleFixed");
            this.styleCollect.add("zAngleFixed");
        }

        #region 基本字段
        // 位置、尺寸、旋转量是否被有效设过值逻辑标识
        private bool _hasXSeted = false;
        private bool _hasYSeted = false;
        private bool _hasZSeted = false;
        private bool _hasXAngleSeted = false;
        private bool _hasYAngleSeted = false;
        private bool _hasZAngleSeted = false;

        /// <summary>
        /// 节点浮动方式
        /// </summary>
        private AlignMode _forward = AlignMode.none;

        /// <summary>
        /// SizeBounds组件默认配制
        /// </summary>
        private SizeBoundsConfig _sizeConfig = new SizeBoundsConfig
        {
            colliderCenter = new Vector3(0, 0, 0.00001f),
            flattenAxis = FlattenModeType.FlattenZ,
            handleSize = 0.016f,
            boxPadding = new Vector3(0f, 0f, 0f)
        };

        // 尺寸改变组件
        private SizeBounds _sizeBounds;
        #endregion

        #region 基本属性
        /*
         *  以下属性与节点位置、尺寸相关
         */
        private bool _xFixed = false;
        /// <summary>
        /// x坐标是否固定
        /// </summary>
        public bool xFixed
        {
            get
            {
                return this._xFixed;
            }

            set
            {
                if (value && !this._xFixed)
                {
                    this._hasXSeted = false;
                }
                this._xFixed = value;
            }
        }

        private float _x = 0f;
        /// <summary>
        /// x
        /// </summary>
        public float x
        {
            get
            {
                var result = this.position.x;

                return result;
            }

            set
            {
                this._x = value;
                this.position.x = value;
            }
        }


        private bool _yFixed = false;
        /// <summary>
        /// y坐标是否固定
        /// </summary>
        public bool yFixed
        {
            get
            {
                return this._yFixed;
            }

            set
            {
                if (value && !this._yFixed)
                {
                    this._hasYSeted = false;
                }
                this._yFixed = value;
            }
        }

        private float _y = 0f;
        /// <summary>
        /// y
        /// </summary>
        public float y
        {
            get
            {
                var result = this.position.y;
               
                return result;
            }

            set
            {
                this._y = value;
                this.position.y = value;
            }
        }


        private bool _zFixed = false;
        /// <summary>
        /// z坐标是否固定
        /// </summary>
        public bool zFixed
        {
            get
            {
                return this._zFixed;
            }

            set
            {
                if (value && !this._zFixed)
                {
                    this._hasZSeted = false;
                }
                this._zFixed = value;

                
            }
        }

        private float _z = 0f;
        /// <summary>
        /// z
        /// </summary>
        public float z
        {
            get
            {
                var result = this.position.z;

                return result;
            }

            set
            {
                this._z = value;
                this.position.z = value;
            }
        }

        /// <summary>
        /// 宽度
        /// </summary>
        public override float width
        {
            get
            {
                var result = this.size.width;

                return result;
            }

            set
            {
                this._width = value;
                this.size.width = this._width;
            }
        }

        /// <summary>
        /// 高度
        /// </summary>
        public override float height
        {
            get
            {
                var result = this.size.height;

                return result;
            }

            set
            {
                this._height = value;
                this.size.height = this._height;
            }
        }

        /// <summary>
        /// 深度
        /// </summary>
        public override float depth
        {
            get
            {
                var result = this.isPanel ? 0 : this.size.depth;

                return result;
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

        private bool _xAngleFixed = false;
        public bool xAngleFixed
        {
            get
            {
                return this._xAngleFixed;
            }

            set
            {
                if (value && !this._xAngleFixed)
                {
                    this._hasXAngleSeted = false;
                }
                this._xAngleFixed = value;
            }
        }

        private float _xAngle = 0f;
        /// <summary>
        /// x轴旋转量
        /// </summary>
        public float xAngle
        {
            get
            {
                var result = this.rotation.xAngle;

                return result;
            }

            set
            {
                this._xAngle = value;
                this.rotation.xAngle = this._xAngle;
            }
        }

        private bool _yAngleFixed = false;
        public bool yAngleFixed
        {
            get
            {
                return this._yAngleFixed;
            }

            set
            {
                if (value && !this._yAngleFixed)
                {
                    this._hasYAngleSeted = false;
                }
                this._yAngleFixed = value;
            }
        }

        private float _yAngle = 0f;
        /// <summary>
        /// y轴旋转量
        /// </summary>
        public float yAngle
        {
            get
            {
                var result = this.rotation.yAngle;

                return result;
            }

            set
            {
                this._yAngle = value;
                this.rotation.yAngle = this._yAngle;
            }
        }

        private bool _zAngleFixed = false;
        public bool zAngleFixed
        {
            get
            {
                return this._zAngleFixed;
            }

            set
            {
                if (value && !this._zAngleFixed)
                {
                    this._hasZAngleSeted = false;
                }
                this._zAngleFixed = value;
            }
        }

        private float _zAngle = 0f;
        /// <summary>
        /// z轴旋转量
        /// </summary>
        public float zAngle
        {
            get
            {
                var result = this.rotation.yAngle;

                return result;
            }

            set
            {
                this._zAngle = value;
                this.rotation.zAngle = this._zAngle;
            }
        }

        /// <summary>
        /// 包围盒
        /// </summary>
        public override BoundBox localBounds
        {
            get
            {
                // 节点中心点
                var center = this.parasitifer.transform.TransformPoint(new Vector3(this.position.x, this.position.y, this.position.z));

                // 节点旋转量
                var rotate = new Vector3(this.rotation.xAngle, this.rotation.yAngle, this.rotation.zAngle);

                // 顶点集合
                var pointList = new List<Vector3>();

                var width = this.width;
                var height = this.height;
                var depth = this.depth;

                pointList.Add(Space.Coordinate.rotate(rotate, new Vector3(-width / 2, height / 2, -depth / 2)) + center);
                pointList.Add(Space.Coordinate.rotate(rotate, new Vector3(width / 2, height / 2, -depth / 2)) + center);
                pointList.Add(Space.Coordinate.rotate(rotate, new Vector3(width / 2, height / 2, depth / 2)) + center);
                pointList.Add(Space.Coordinate.rotate(rotate, new Vector3(-width / 2, height / 2, depth / 2)) + center);

                pointList.Add(Space.Coordinate.rotate(rotate, new Vector3(-width / 2, -height / 2, -depth / 2)) + center);
                pointList.Add(Space.Coordinate.rotate(rotate, new Vector3(width / 2, -height / 2, -depth / 2)) + center);
                pointList.Add(Space.Coordinate.rotate(rotate, new Vector3(width / 2, -height / 2, depth / 2)) + center);
                pointList.Add(Space.Coordinate.rotate(rotate, new Vector3(-width / 2, -height / 2, depth / 2)) + center);

                var boundWidth = 0f;
                var boundHeight = 0f;
                var boundDepth = 0f;

                // 计算包围盒尺寸
                for (var i = 0; i < pointList.Count; i++)
                {
                    for (var j = 0; j < pointList.Count; j++)
                    {
                        var vector = pointList[i] - pointList[j];
                        if (Mathf.Abs(vector.x) > boundWidth)
                        {
                            boundWidth = Mathf.Abs(vector.x);
                        }
                        if (Mathf.Abs(vector.y) > boundHeight)
                        {
                            boundHeight = Mathf.Abs(vector.y);
                        }
                        if (Mathf.Abs(vector.z) > boundDepth)
                        {
                            boundDepth = Mathf.Abs(vector.z);
                        }
                    }
                }

                return new BoundBox(center, new Vector3(boundWidth, boundHeight, boundDepth));
            }
        }


        /*
         *  布局相关属性
         */
        /*---------------------------------------------定义forwardFloat属性支撑方法开始---------------------------------------------*/
        /// <summary>
        /// 获取自身forward对齐模式
        /// </summary>
        /// <returns></returns>
        public AlignMode getForward()
        {
            return this._forward;
        }

        /// <summary>
        /// 设置自身forward对齐模式
        /// </summary>
        /// <param name="alignMode"></param>
        public void setForward(AlignMode alignMode)
        {
            this._forward = alignMode;
            if (this.parent != null)
            {
                this.forwardFresh();
            }
        }

        /// <summary>
        /// 自已forward对齐刷新
        /// </summary>
        public void forwardFresh()
        {
            // 当parent == null时说明这个节点在顶层、 当this.parent.alignAnchor == null时有两种情况一种是没有parent节点在顶层，另一种是上一层为魔法节点但魔法节点为顶层节点，所以这两种情况都在顶层所以没有该节点的浮动布局
            if (this.parent == null || this.parent.alignAnchor == null)
            {
                return;
            }

            if (this.getForward() != AlignMode.none)
            {
                this.parent.alignAnchor.target = this;

                this.parent.alignAnchor.x = null;
                this.parent.alignAnchor.y = null;
                this.parent.alignAnchor.z = null;

                Node computeNode = this;
                if (this.parent is SpaceMagic)
                {
                    computeNode = this.parent;
                }

                if (this.getForward() == AlignMode.none)
                {
                    this.parent.alignAnchor.z = null;
                }
                if (this.getForward() == AlignMode.center || this.getForward() == AlignMode.between)
                {
                    this.parent.alignAnchor.z = 0f + Space.Unit.unitToScale(this, this.offset.z, Axle.forward);
                }
                if (this.getForward() == AlignMode.left)
                {
                    var depth = Space.Unit.unitToScale(computeNode, this.localBounds.size.z, Axle.forward);

                    this.parent.alignAnchor.z = -1 / 2f + depth / 2f + Space.Unit.unitToScale(this, this.offset.z, Axle.forward);
                }

                if (this.getForward() == AlignMode.right)
                {
                    var depth = Space.Unit.unitToScale(computeNode, this.localBounds.size.z, Axle.forward);

                    this.parent.alignAnchor.z = 1 / 2f - depth / 2f + Space.Unit.unitToScale(this, this.offset.z, Axle.forward);
                }

                this.parent.alignAnchor.fresh();
            }
        }
        /*---------------------------------------------定义forwardFloat属性支撑方法结束---------------------------------------------*/

        /// <summary>
        /// 自身前向(z轴)对齐方式
        /// </summary>
        public string forward
        {
            get
            {
                string result = "none";
                if (this._forward.ToString() == "right")
                {
                    result = "forward";
                }

                if (this._forward.ToString() == "center")
                {
                    result = "center";
                }

                if (this._forward.ToString() == "left")
                {
                    result = "back";
                }
                return result;
            }

            set
            {
                if (value == "none")
                {
                    this.setForward(AlignMode.none);
                }

                if (value == "center")
                {
                    this.setForward(AlignMode.center);
                }

                if (value == "back")
                {
                    this.setForward(AlignMode.left);
                }

                if (value == "forward")
                {
                    this.setForward(AlignMode.right);
                }
            }
        }

        /// <summary>
        /// z轴浮动
        /// </summary>
        public string forwardFloat
        {
            get
            {
                return this.forward;
            }

            set
            {
                this.forward = value;
            }
        }


        /*
         *  以下属性与SizeBounds组件相关
         */
        /// <summary>
        /// 尺寸缩放组件默认配制
        /// </summary>
        protected SizeBoundsConfig sizeConfig
        {
            get
            {
                return _sizeConfig;
            }
        }

        /// <summary>
        /// 控制柄尺寸
        /// </summary>
        public float handleSize
        {
            get
            {
                return this._sizeConfig.handleSize;
            }

            set
            {
                this._sizeConfig.handleSize = value;
            }
        }

        private float _sizeBoxPadding = 0f;
        /// <summary>
        /// padding
        /// </summary>
        public float sizeBoxPadding
        {
            get
            {
                return this._sizeBoxPadding;
            }

            set
            {
                this._sizeBoxPadding = value;
            }
        }

        /// <summary>
        /// 是否允许尺寸变动
        /// </summary>
        public bool enableSize
        {
            get
            {
                return this._sizeBounds != null ? this._sizeBounds.enable : false;
            }
            set
            {
                // 只有当size组件不存在时才去创建它
                if (value && this._sizeBounds == null)
                {
                    EMR.Space.mainService.next(() =>
                    {
                        // 防零
                        var width = this.parasitifer.transform.localScale.x;
                        var height = this.parasitifer.transform.localScale.y;
                        if (Utils.equals(this.parasitifer.transform.localScale.x, 0f) || Utils.equals(this.parasitifer.transform.localScale.y, 0f))
                        {
                            this.parasitifer.transform.localScale = new Vector3(100f, 100f, this.parasitifer.transform.localScale.z);
                        }

                        this._sizeBounds = new SizeBounds(this, this.sizeConfig);
                        this._sizeBounds.fresh();

                        // 防零还原
                        this.parasitifer.transform.localScale = new Vector3(width, height, this.parasitifer.transform.localScale.z);

                        this._sizeBounds.enable = true;
                        this.sizeBoundFresh();

                        return true;
                    });
                } else
                {
                    this._sizeBounds.enable = value;
                    this.sizeBoundFresh();
                }
            }
        }
        #endregion

        #region 事件
        /// <summary>
        /// 节点尺寸改变开始事件
        /// </summary>
        protected readonly BoundScaleStartedEvent _onBoundScaleStarted = new BoundScaleStartedEvent();
        /// <summary>
        /// 节点尺寸改变开始事件
        /// </summary>
        public virtual BoundScaleStartedEvent onBoundScaleStarted
        {
            get
            {
                return this._onBoundScaleStarted;
            }
        }

        /// <summary>
        /// 节点尺寸改变结束事件
        /// </summary>
        protected readonly BoundScaleEndedEvent _onBoundScaleEnded = new BoundScaleEndedEvent();
        /// <summary>
        /// 节点尺寸改变开始事件
        /// </summary>
        public virtual BoundScaleEndedEvent onBoundScaleEnded
        {
            get
            {
                return this._onBoundScaleEnded;
            }
        }

        /// <summary>
        /// 碰撞进入事件类
        /// </summary>
        protected readonly CollisionEnterEvent _onCollisionEnter = null;
        /// <summary>
        /// 碰撞进入事件类
        /// </summary>
        public virtual CollisionEnterEvent onCollisionEnter
        {
            get
            {
                return this._onCollisionEnter;
            }
        }

        /// <summary>
        /// 碰撞中事件类
        /// </summary>
        protected readonly CollisionStayEvent _onCollisionStay = null;
        /// <summary>
        /// 碰撞中事件类
        /// </summary>
        public virtual CollisionStayEvent onCollisionStay
        {
            get
            {
                return this._onCollisionStay;
            }
        }

        /// <summary>
        /// 碰撞退出事件类
        /// </summary>
        protected readonly CollisionExitEvent _onCollisionExit = null;
        /// <summary>
        /// 碰撞退出事件类
        /// </summary>
        public virtual CollisionExitEvent onCollisionExit
        {
            get
            {
                return this._onCollisionExit;
            }
        }
        #endregion

        #region 基本方法
        /// <summary>
        /// 是否有零节点
        /// </summary>
        /// <returns></returns>
        private bool hasZeroNode()
        {
            Node node = this.parent;
            bool result = false;
            while (node != null)
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
         *  以下方法主要用于尺寸、位置、旋转角度刷新
         */
        /// <summary>
        /// 尺寸刷新
        /// </summary>
        public override void sizeFresh()
        {
            // 只有当width为固定时或width从未被设过值时需要计算x坐标
            if (this.widthFixed || !this.widthFixed && !this._hasWidthSeted)
            {
                this.width = this._width;
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

            // 只有当height为固定时或height从未被设过值时需要计算y坐标
            if (this.heightFixed || !this.heightFixed && !this._hasHeightSeted)
            {
                this.height = this._height;
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
        /// 位置刷新
        /// </summary>
        public override void positionFresh()
        {
            // 只有当x坐标为固定时或x坐标从未被设过值时需要计算x坐标
            if (this.xFixed || !this.xFixed && !this._hasXSeted)
            {
                this.x = this._x;
            }

            // 当x坐标为非固定时，计算x坐标是否被已被有效设过值
            if (!this.xFixed)
            {
                if (!this._hasXSeted && !this.hasZeroNode())
                {
                    this._hasXSeted = true;
                }
                else if (this._hasXSeted)
                {
                    this._x = this.x;
                }
            }


            // 只有当y坐标为固定时或y坐标从未被设过值时需要计算y坐标
            if (this.yFixed || !this.yFixed && !this._hasYSeted)
            {
                this.y = this._y;
            }

            // 当y坐标为非固定时，计算y坐标是否被已被有效设过值
            if (!this.yFixed)
            {
                if (!this._hasYSeted && !this.hasZeroNode())
                {
                    this._hasYSeted = true;
                }
                else if (this._hasYSeted)
                {
                    this._y = this.y;
                }
            }


            // 只有当z坐标为固定时或z坐标从未被设过值时需要计算z坐标
            if (this.zFixed || !this.zFixed && !this._hasZSeted)
            {
                this.z = this._z;
            }

            // 当z坐标为非固定时，计算z坐标是否被已被有效设过值
            if (!this.zFixed)
            {
                if (!this._hasZSeted && !this.hasZeroNode())
                {
                    this._hasZSeted = true;
                }
                else
                {
                    this._z = this.z;
                }
            }
        }

        /// <summary>
        /// 旋转角度刷新
        /// </summary>
        public void rotationFresh()
        {
            // 只有当xAngle为固定时或xAngle从未被设过值时需要计算x坐标
            if (this.xAngleFixed || !this.xAngleFixed && !this._hasXAngleSeted)
            {
                this.xAngle = this._xAngle;
            }

            // 当xAngle为非固定时，计算xAngle是否被已被有效设过值
            if (!this.xAngleFixed)
            {
                if (!this._hasXAngleSeted && !this.hasZeroNode())
                {
                    this._hasXAngleSeted = true;
                }
                else if (this._hasXAngleSeted)
                {
                    this._xAngle = this.xAngle;
                }
            }

            // 只有当yAngle为固定时或yAngle从未被设过值时需要计算y坐标
            if (this.yAngleFixed || !this.yAngleFixed && !this._hasYAngleSeted)
            {
                this.yAngle = this._yAngle;
            }

            // 当yAngle为非固定时，计算yAngle是否被已被有效设过值
            if (!this.yAngleFixed)
            {
                if (!this._hasYAngleSeted && !this.hasZeroNode())
                {
                    this._hasYAngleSeted = true;
                }
                else if (this._hasYAngleSeted)
                {
                    this._yAngle = this.yAngle;
                }
            }

            // 只有当zAngle为固定时或zAngle从未被设过值时需要计算y坐标
            if (this.zAngleFixed || !this.zAngleFixed && !this._hasZAngleSeted)
            {
                this.zAngle = this._zAngle;
            }

            // 当zAngle为非固定时，计算zAngle是否被已被有效设过值
            if (!this.zAngleFixed)
            {
                if (!this._hasZAngleSeted && !this.hasZeroNode())
                {
                    this._hasZAngleSeted = true;
                }
                else if (this._hasZAngleSeted)
                {
                    this._zAngle = this.zAngle;
                }
            }
        }

        /// <summary>
        /// sizeBound 刷新
        /// </summary>
        public void sizeBoundFresh()
        {
            if (this.enableSize)
            {
                var colliderWidth = 1f;
                var colliderHeigyht = 1f;

                if (!Utils.equals(this.parasitifer.transform.localScale.x, 0f) && !Utils.equals(this.parasitifer.transform.localScale.y, 0f))
                {
                    colliderWidth = 1 + Space.Unit.unitToScale(this, this.sizeBoxPadding, Axle.right) / this.parasitifer.transform.localScale.x;
                    colliderHeigyht = 1 + Space.Unit.unitToScale(this, this.sizeBoxPadding, Axle.up) / this.parasitifer.transform.localScale.y;
                }

                this._sizeBounds.collider.size = new Vector3(colliderWidth, colliderHeigyht, Space.zero);
            }
        }

        /// <summary>
        /// 位置同步
        /// </summary>
        public void positionSync()
        {
            this._x = this.x;
            this._y = this.y;
            this._z = this.z;
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

        /// <summary>
        /// 旋转量同步
        /// </summary>
        public void roationSync()
        {
            this._xAngle = this.xAngle;
            this._yAngle = this.yAngle;
            this._zAngle = this.zAngle;
        }


        /*
         *  以下方法主要用于节点运动
         */
        /// <summary>
        /// 移动
        /// </summary>
        /// <param name="end">目标位置</param>
        /// <param name="time">动画时长</param>
        /// <param name="curveType">曲线类型</param>
        /// <param name="callback">动画结束回调方法</param>
        public void moveTo(PositionData end, float time, MotionCurve curveType, AnimationCallback callback = null)
        {
            if (Utils.equals(this.x, end.x) && Utils.equals(this.y, end.y) && Utils.equals(this.z, end.z))
            {
                return;
            }

            var startVector = new Vector3(this.x, this.y, this.z);

            var endX = float.IsNaN(end.x) ? startVector.x : end.x;
            var endY = float.IsNaN(end.y) ? startVector.y : end.y;
            var endZ = float.IsNaN(end.z) ? startVector.z : end.z;

            var endVector = new Vector3(endX, endY, endZ);

            // 执行动画
            this.animation(startVector, endVector, time, curveType, (Vector3 data, bool isFinish) =>
            {
                this.x = data.x;
                this.y = data.y;
                this.z = data.z;

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
        /// <param name="curveType">曲线类型</param>
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
                    this.ergodic((PanelNode node) =>
                    {
                        // 此处要对npc进行刷新因为panelNode节点有可能各维度不是等比缩放
                        if (node.npc != null)
                        {
                            if (node is PanelRoot)
                            {
                                ((PanelRoot)node).npcFresh();
                            }

                            if (node is PanelLayer)
                            {
                                ((PanelLayer)node).npcFresh();
                            }
                        }

                        node.preventPositionEvent = true;
                        node.preventSizeEvent = true;

                        return true;
                    });
                }

            }, callback);
        }

        /// <summary>
        /// 旋转
        /// </summary>
        /// <param name="end">目标旋转量</param>
        /// <param name="time">动画时长</param>
        /// <param name="curveType">曲线类型</param>
        /// <param name="callback">动画结束回调</param>
        public void rotateTo(RotationData end, float time, MotionCurve curveType, AnimationCallback callback = null)
        {
            if (Utils.equals(this.xAngle, end.xAngle) && Utils.equals(this.yAngle, end.yAngle) && Utils.equals(this.zAngle, end.zAngle))
            {
                return;
            }

            var startVector = new Vector3(this.xAngle, this.yAngle, this.zAngle);

            var endXAngle = float.IsNaN(end.xAngle) ? startVector.x : end.xAngle;
            var endYAngle = float.IsNaN(end.yAngle) ? startVector.y : end.yAngle;
            var endZAngle = float.IsNaN(end.zAngle) ? startVector.z : end.zAngle;

            var endVector = new Vector3(endXAngle, endYAngle, endZAngle);

            // 执行动画
            this.animation(startVector, endVector, time, curveType, (Vector3 data, bool isFinish) =>
            {
                this.xAngle = data.x;
                this.yAngle = data.y;
                this.zAngle = data.z;

                if (!isFinish)
                {
                    // 在动画的过程中不触发事件
                    this.ergodic((Node node) =>
                    {
                        node.preventRotationEvent = true;
                        return true;
                    });
                }

            }, callback);
        }
        #endregion

        #region 节点数据结构基本操作
        /// <summary>
        /// 解锁包括子节点在内的所有节点的尺寸有效设值过逻辑标识
        /// </summary>
        public override void unlockSizeSetedTag()
        {
            foreach(var item in this.children)
            {
                item.unlockSizeSetedTag();
            }

            this._hasWidthSeted = false;
            this._hasHeightSeted = false;
            this._hasDepthSeted = false;
        }

        /// <summary>
        /// 解锁包括子节点在内的所有节点的位置有效设值过逻辑标识
        /// </summary>
        public void unlockPositionSetedTag()
        {
            foreach (var item in this.children)
            {
                item.unlockPositionSetedTag();
            }

            this._hasXSeted = false;
            this._hasYSeted = false;
            this._hasZSeted = false;
        }

        /// <summary>
        /// 插入子节点
        /// </summary>
        /// <param name="current">当前要插入的节点</param>
        /// <param name="refNode">参照节点</param>
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

            if (!insertEventData.isPreventDefault)
            {
                var oldNodeParent = current.parent;

                // 解锁节点尺寸有效设值过逻辑标识
                current.unlockSizeSetedTag();

                // 解锁节点位置有效设值过逻辑标识
                current.unlockPositionSetedTag();

                result = base.insertBefore(current, refNode);

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

                // 抛出节点插入完成事件
                InsertedEventData insertedEventData = new InsertedEventData
                {
                    target = current,
                    isSuccess = result != null ? true : false,
                };
                onInserted.Invoke(insertedEventData);
            }

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
        /// <param name="node">要添加的节点</param>
        /// <returns></returns>
        public override PanelLayer appendNode(PanelLayer node)
        {
            PanelNode result = null;

            // 抛出节点添加前事件
            AppendEventData appendEventData = new AppendEventData
            {
                target = node,
            };
            onAppend.Invoke(appendEventData);

            if (!appendEventData.isPreventDefault)
            {
                // 节点在原结构下的父节点
                var oldNodeParent = node.parent;

                // 解锁节点尺寸有效设值过逻辑标识
                node.unlockSizeSetedTag();

                // 解锁节点位置有效设值过逻辑标识
                node.unlockPositionSetedTag();

                result = base.appendNode(node);

                if (result != null)
                {
                    node.rotation.xAngle = 0f;
                    node.rotation.yAngle = 0f;
                    node.rotation.zAngle = 0f;

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
        #endregion
    }
}
