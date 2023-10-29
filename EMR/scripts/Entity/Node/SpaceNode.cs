                                                                                        using System;
using System.Collections.Generic;
using UnityEngine;
using EMR.Common;
using EMR.Event;
using EMR.Struct;
using EMR.Layout;
using EMR.Plugin;
using Microsoft.MixedReality.Toolkit.Input;

namespace EMR.Entity
{
    public class SpaceNode : Node, ISpaceCharacteristic, ISpaceLayoutFeature, IDocumentModelModify, IPointerEventFeature, ITouchEventFeature, IFocusEventFeature, ICollisionEventFeature,  IManipulationEventFeature, ISizeBoundsEventNode, Element
    {
        #region 基本字段
        /// <summary>
        /// 子节点水平对齐方式组件
        /// </summary>
        private SpaceAlign _contentHorizontalAlign;

        /// <summary>
        /// 子节点垂直对齐组件
        /// </summary>
        private SpaceAlign _contentVerticalAlign;

        /// <summary>
        /// 子节点前向（z轴)对齐组件
        /// </summary>
        private SpaceAlign _contentForwardAlign;

        /// <summary>
        /// 空间弯曲组件
        /// </summary>
        private SpaceBend _spaceBend;

        /// <summary>
        /// z轴浮动方式
        /// </summary>
        private AlignMode _forward = AlignMode.none;

        // 位置、尺寸是否被有效设过值逻辑标识
        private bool _hasXSeted = false;
        private bool _hasYSeted = false;
        private bool _hasZSeted = false;
        private bool _hasXAngleSeted = false;
        private bool _hasYAngleSeted = false;
        private bool _hasZAngleSeted = false;

        /// <summary>
        /// SizeBounds默认配制
        /// </summary>
        private SizeBoundsConfig _sizeConfig = new SizeBoundsConfig
        {
            colliderCenter = new Vector3(0, 0, 0),
            handleSize = 0.016f,
            boxPadding = new Vector3(0f, 0f, 0f)
        };

        NearInteractionTouchableVolume autoInteractionTouchable = null;

        /// <summary>
        /// InteractionTouchable集合
        /// </summary>
        protected Dictionary<string, NearInteractionTouchableVolume> interactionTouchableDictionary = new Dictionary<string, NearInteractionTouchableVolume>();
        #endregion

        #region 构造器
        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="x">x坐标</param>
        /// <param name="y">y坐标</param>
        /// <param name="z">z坐标</param>
        /// <param name="xAngle">x轴旋转量</param>
        /// <param name="yAngle">y轴旋转量</param>
        /// <param name="zAngle">z轴旋转量</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="depth">深度</param>
        /// <param name="npc">npc</param>
        /// <param name="rendMode">渲染模式</param>
        public SpaceNode(float x, float y, float z, float xAngle, float yAngle, float zAngle, float width, float height, float depth, CoreComponent component,  NPC npc = null, RendMode rendMode = RendMode.opaque) : base(x, y, z, xAngle, yAngle, zAngle, width, height, depth, npc)
        {
            this._component = component;

            this.widthFixed = false;
            this.heightFixed = false;
            this.depthFixed = false;

            this._x = x;
            this._y = y;
            this._z = z;

            this._xAngle = xAngle;
            this._yAngle = yAngle;
            this._zAngle = zAngle;

            this._width = width;
            this._height = height;
            this._depth = depth;

            this._onCollisionEnter = new CollisionEnterEvent(this);
            this._onCollisionStay = new CollisionStayEvent(this);
            this._onCollisionExit = new CollisionExitEvent(this);

            this._spaceBend = new SpaceBend(this);

            // 一直在每个周期的末监控（用于节占尺寸改变、平移、滚动时对布局的调整）
            this.service.lateNext(() =>
            {
                if (this.isSizeChanging)
                {
                    SpaceLayout.update(new SpaceNodeActionInfo
                    {
                        current = this,
                        currentParent = this.parent,
                        action = SpaceNodeAction.sizeChange
                    });
                }
                else if (this.isMoving)
                {
                    SpaceLayout.update(new SpaceNodeActionInfo
                    {
                        current = this,
                        currentParent = this.parent,
                        action = SpaceNodeAction.positionChange
                    });
                }

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

            this.renderMode = renderMode;

            // 添加样式
            this.styleCollect.add("x");
            this.styleCollect.add("y");
            this.styleCollect.add("z");
            this.styleCollect.add("xAngle");
            this.styleCollect.add("yAngle");
            this.styleCollect.add("zAngle");
            this.styleCollect.add("forwardFloat");
            this.styleCollect.add("contentForward");
            this.styleCollect.add("forwardInterval");
            this.styleCollect.add("forwardBackInterval");
            this.styleCollect.add("forwardFrontInterval");
            this.styleCollect.add("bendAngle");
            this.styleCollect.add("overclip");
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
        public SpaceNode(CoreComponent component) : base(0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, null)
        {
            this.widthFixed = false;
            this.heightFixed = false;
            this.depthFixed = false;

            this._component = component;

            this._spaceBend = new SpaceBend(this);

            this._onCollisionEnter = new CollisionEnterEvent(this);
            this._onCollisionStay = new CollisionStayEvent(this);
            this._onCollisionExit = new CollisionExitEvent(this);

            // 一直在每个周期的末监控（用于节占尺寸改变、平移、滚动时对布局的调整）
            this.service.lateNext(() =>
            {
                if (this.isSizeChanging)
                {
                    SpaceLayout.update(new SpaceNodeActionInfo
                    {
                        current = this,
                        currentParent = this.parent,
                        action = SpaceNodeAction.sizeChange
                    });
                }
                else if (this.isMoving)
                {
                    SpaceLayout.update(new SpaceNodeActionInfo
                    {
                        current = this,
                        currentParent = this.parent,
                        action = SpaceNodeAction.positionChange
                    });
                }
                
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
            this.styleCollect.add("contentForward");
            this.styleCollect.add("forwardInterval");
            this.styleCollect.add("bendAngle");
            this.styleCollect.add("overclip");
            this.styleCollect.add("xFixed");
            this.styleCollect.add("yFixed");
            this.styleCollect.add("zFixed");
            this.styleCollect.add("xAngleFixed");
            this.styleCollect.add("yAngleFixed");
            this.styleCollect.add("zAngleFixed");
        }
        #endregion

        #region 基本属性
        public override bool hasAutoInteractionTouchable
        {
            get
            {
                return this.autoInteractionTouchable != null;
            }
        }

        public override bool hasInteractionTouchable
        {
            get
            {
                return this.parasitifer.GetComponent<NearInteractionTouchableVolume>() != null;
            }
        }

        /*
         *  以下属性与位置、尺寸相关
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
                return this.position.y;
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
                return this.position.z;
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
                return this.size.depth;
            }

            set
            {
                this._depth = value;

                this.size.depth = value;
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


        /*
         *  以下属性与布局相关
         */
        /*-------------------------------------------定义contentHorizontal属性支撑方法开始-------------------------------------------*/
        /// <summary>
        /// 获取x轴对齐模式
        /// </summary>
        /// <returns></returns>
        public AlignMode getContentHorizontalAlignMode()
        {
            return (this._contentHorizontalAlign != null) ? this._contentHorizontalAlign.mode : AlignMode.none;
        }

        /// <summary>
        /// 设置x轴对齐模式
        /// </summary>
        /// <param name="alignMode"></param>
        public void setContentHorizontalAlignMode(AlignMode alignMode)
        {
            if (this._contentHorizontalAlign == null)
            {
                this._contentHorizontalAlign = new SpaceAlign(this, Axle.right, alignMode);
            }
            else
            {
                this._contentHorizontalAlign.mode = alignMode;
            }

            this._contentHorizontalAlign.fresh();
        }
        /*-------------------------------------------定义contentHorizontal属性支撑方法结束-------------------------------------------*/

        /// <summary>
        /// 子节点水平对齐间距
        /// </summary>
        public float horizontalInterval
        {
            get
            {
                return this._contentHorizontalAlign != null ? this._contentHorizontalAlign.interval : 0f;
            }

            set
            {
                if (this._contentHorizontalAlign == null)
                {
                    this._contentHorizontalAlign = new SpaceAlign(this, Axle.right, AlignMode.none);
                }

                this._contentHorizontalAlign.interval = value;
            }
        }

        /// <summary>
        /// 水平对齐第一个子节点是否包含左间距
        /// </summary>
        public bool horizontalLeftInterval
        {
            get
            {
                return this._contentHorizontalAlign != null ? this._contentHorizontalAlign.isStartInvterval : false;
            }

            set
            {
                if (this._contentHorizontalAlign == null)
                {
                    this._contentHorizontalAlign = new SpaceAlign(this, Axle.right, AlignMode.none);
                }

                this._contentHorizontalAlign.isStartInvterval = value;
            }
        }

        /// <summary>
        /// 水平对齐最后一个子节点是否包含右间距
        /// </summary>
        public bool horizontalRightInterval
        {
            get
            {
                return this._contentHorizontalAlign != null ? this._contentHorizontalAlign.isEndInvterval : false;
            }

            set
            {
                if (this._contentHorizontalAlign == null)
                {
                    this._contentHorizontalAlign = new SpaceAlign(this, Axle.right, AlignMode.none);
                }

                this._contentHorizontalAlign.isEndInvterval = value;
            }
        }

        /// <summary>
        /// 子节点水平方向(x轴)对齐方式
        /// </summary>
        public string contentHorizontal
        {
            get
            {
                return this._contentHorizontalAlign.ToString();
            }

            set
            {
                if (value == "none")
                {
                    this.setContentHorizontalAlignMode(AlignMode.none);
                }
                else if (value == "left")
                {
                    this.setContentHorizontalAlignMode(AlignMode.left);
                }
                else if (value == "right")
                {
                    this.setContentHorizontalAlignMode(AlignMode.right);
                }
                else if (value == "center")
                {
                    this.setContentHorizontalAlignMode(AlignMode.center);
                }
                else if (value == "between")
                {
                    this.setContentHorizontalAlignMode(AlignMode.between);
                }
            }
        }


        /*-------------------------------------------定义contentVertical属性支撑方法开始-------------------------------------------*/
        /// <summary>
        /// 获取y轴对齐模式
        /// </summary>
        /// <returns></returns>
        public AlignMode getContentVerticalAlignMode()
        {
            return (this._contentVerticalAlign != null) ? this._contentVerticalAlign.mode : AlignMode.none;
        }

        /// <summary>
        /// 设置y轴对齐模式
        /// </summary>
        /// <param name="alignMode"></param>
        public void setContentVerticalAlignMode(AlignMode alignMode)
        {
            if (this._contentVerticalAlign == null)
            {
                this._contentVerticalAlign = new SpaceAlign(this, Axle.up, alignMode);
            }
            else
            {
                this._contentVerticalAlign.mode = alignMode;
            }
            this._contentVerticalAlign.fresh();
        }
        /*-------------------------------------------定义contentVertical属性支撑方法结束-------------------------------------------*/

        /// <summary>
        /// 子节点垂直对齐间距
        /// </summary>
        public float verticalInterval
        {
            get
            {
                return this._contentVerticalAlign != null ? this._contentVerticalAlign.interval : 0f;
            }

            set
            {
                if (this._contentVerticalAlign == null)
                {
                    this._contentVerticalAlign = new SpaceAlign(this, Axle.up, AlignMode.none);
                }

                this._contentVerticalAlign.interval = value;
            }
        }

        /// <summary>
        /// 水平垂直第一个子节点是否包含左间距
        /// </summary>
        public bool verticalTopInterval
        {
            get
            {
                return this._contentVerticalAlign != null ? this._contentVerticalAlign.isEndInvterval : false;
            }

            set
            {
                if (this._contentVerticalAlign == null)
                {
                    this._contentVerticalAlign = new SpaceAlign(this, Axle.up, AlignMode.none);
                }

                this._contentVerticalAlign.isEndInvterval = value;
            }
        }

        /// <summary>
        /// 水平垂直最后一个子节点是否包含右间距
        /// </summary>
        public bool verticalBottomInterval
        {
            get
            {
                return this._contentVerticalAlign != null ? this._contentVerticalAlign.isStartInvterval : false;
            }

            set
            {
                if (this._contentVerticalAlign == null)
                {
                    this._contentVerticalAlign = new SpaceAlign(this, Axle.up, AlignMode.none);
                }

                this._contentVerticalAlign.isStartInvterval = value;
            }
        }

        /// <summary>
        /// 子节点垂直方向(y轴)对齐方式
        /// </summary>
        public string contentVertical
        {
            get
            {
                string result = "none";
                if (this._contentVerticalAlign.mode == AlignMode.left)
                {
                    result = "bottom";
                }
                else if (this._contentVerticalAlign.mode == AlignMode.right)
                {
                    result = "top";
                }
                else
                {
                    result = this._contentVerticalAlign.ToString();
                }
                return result;
            }

            set
            {
                if (value == "none")
                {
                    this.setContentVerticalAlignMode(AlignMode.none);
                }
                else if (value == "bottom")
                {
                    this.setContentVerticalAlignMode(AlignMode.left);
                }
                else if (value == "top")
                {
                    this.setContentVerticalAlignMode(AlignMode.right);
                }
                else if (value == "center")
                {
                    this.setContentVerticalAlignMode(AlignMode.center);
                }
                else if (value == "between")
                {
                    this.setContentVerticalAlignMode(AlignMode.between);
                }
            }
        }

        
        /*-------------------------------------------定义contentForward属性支撑方法开始-------------------------------------------*/
        /// <summary>
        /// 获取z轴对齐模式
        /// </summary>
        /// <returns></returns>
        public AlignMode getContentForwardAlignMode()
        {
            return (this._contentForwardAlign != null) ? this._contentForwardAlign.mode : AlignMode.none;
        }

        /// <summary>
        /// 设置z轴对齐模式
        /// </summary>
        /// <param name="alignMode"></param>
        public void setContentForwardAlignMode(AlignMode alignMode)
        {
            if (this._contentForwardAlign == null)
            {
                this._contentForwardAlign = new SpaceAlign(this, Axle.forward, alignMode);
            }
            else
            {
                this._contentForwardAlign.mode = alignMode;
            }

            this._contentForwardAlign.fresh();
        }
        /*-------------------------------------------定义contentForward属性支撑方法结束-------------------------------------------*/

        /// <summary>
        /// 子节点垂直对齐间距
        /// </summary>
        public float forwardInterval
        {
            get
            {
                return this._contentForwardAlign != null ? this._contentForwardAlign.interval : 0f;
            }

            set
            {
                if (this._contentForwardAlign == null)
                {
                    this._contentForwardAlign = new SpaceAlign(this, Axle.forward, AlignMode.none);
                }

                this._contentForwardAlign.interval = value;
            }
        }

        /// <summary>
        /// 水平垂直第一个子节点是否包含左间距
        /// </summary>
        public bool forwardBackInterval
        {
            get
            {
                return this._contentForwardAlign != null ? this._contentForwardAlign.isStartInvterval : false;
            }

            set
            {
                if (this._contentForwardAlign == null)
                {
                    this._contentForwardAlign = new SpaceAlign(this, Axle.forward, AlignMode.none);
                }

                this._contentForwardAlign.isStartInvterval = value;
            }
        }

        /// <summary>
        /// 水平垂直最后一个子节点是否包含右间距
        /// </summary>
        public bool forwardFrontInterval
        {
            get
            {
                return this._contentForwardAlign != null ? this._contentForwardAlign.isEndInvterval : false;
            }

            set
            {
                if (this._contentForwardAlign == null)
                {
                    this._contentForwardAlign = new SpaceAlign(this, Axle.forward, AlignMode.none);
                }

                this._contentForwardAlign.isEndInvterval = value;
            }
        }

        /// <summary>
        /// 子节点z轴对齐方式
        /// </summary>
        public string contentForward
        {
            get
            {
                string result = "none";
                if (this._contentForwardAlign.mode == AlignMode.left)
                {
                    result = "back";
                }
                else if (this._contentForwardAlign.mode == AlignMode.right)
                {
                    result = "forward";
                }
                else
                {
                    result = this._contentForwardAlign.ToString();
                }
                return result;
            }

            set
            {
                if (value == "none")
                {
                    this.setContentForwardAlignMode(AlignMode.none);
                }
                else if (value == "back")
                {
                    this.setContentForwardAlignMode(AlignMode.left);
                }
                else if (value == "forward")
                {
                    this.setContentForwardAlignMode(AlignMode.right);
                }
                else if (value == "center")
                {
                    this.setContentForwardAlignMode(AlignMode.center);
                }
                else if (value == "between")
                {
                    this.setContentForwardAlignMode(AlignMode.between);
                }
            }
        }

        /*---------------------------------------------定义forwardFloat属性支撑方法开始---------------------------------------------*/
        /// <summary>
        /// 获取forward浮动方式
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

                    this.parent.alignAnchor.z = -1 / 2f + depth / 2f + Space.Unit.unitToScale(computeNode, this.offset.z, Axle.forward);
                }

                if (this.getForward() == AlignMode.right)
                {
                    var depth = Space.Unit.unitToScale(computeNode, this.localBounds.size.z, Axle.forward);

                    this.parent.alignAnchor.z = 1 / 2f - depth / 2f + Space.Unit.unitToScale(computeNode, this.offset.z, Axle.forward);
                }

                this.parent.alignAnchor.fresh();
            }
        }
        /*---------------------------------------------定义forwardFloat属性支撑方法结束---------------------------------------------*/

        /// <summary>
        /// z轴浮动
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

        /// <summary>
        /// 弯曲度
        /// </summary>
        public float bendAngle
        {
            get
            {
                return this._spaceBend.bendAngle;
            }

            set
            {
                this._spaceBend.bendAngle = value;
                if (!Utils.equals(this._spaceBend.bendAngle, 0f))
                {
                    this._spaceBend.fresh();
                }
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

                var width = this.size.width;
                var height = this.size.height;
                var depth = this.size.depth;

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
         *  节点显示方式相关
         */
        private SpaceClip _clip;
        public SpaceClip clipComponent
        {
            get
            {
                return this._clip;
            }
        }

        /// <summary>
        /// 是否启用裁切
        /// </summary>
        public bool overclip
        {
            get
            {
                return this._clip == null ? false : true;
            }

            set
            {
                if(value)
                {
                    var node = this;
                    while(node.parent != null)
                    {
                        node = (SpaceNode)node.parent;

                        if(node.overclip)
                        {
                            Debug.LogError("该节点的父节点中已存在有overclip为true的节点");
                            return;
                        }

                        var isChildHaveClip = false;
                        node.ergodic((item) =>
                        {
                            if(node != item && item is SpaceNode && ((SpaceNode)item).overclip)
                            {
                                isChildHaveClip = true;

                                // 终止遍历
                                return false ;
                            }

                            return true;
                        });

                        if(isChildHaveClip)
                        {
                            Debug.LogError("该节点的子节点中已存在有overclip为true的节点");
                            return;
                        }
                    }
                }

                if (value && this._clip == null)
                {
                    this._clip = new SpaceClip(this, new ScrollConfig
                    {
                        // 晶格
                        cellWidth = 0.001f,
                        cellHeight = 0.001f,
                        cellDepth = 0.001f,

                        // EmptyScrll (滚动组件ScrollCollection的载体)的位置、尺寸
                        scrollCenter = new Vector3(0, 0, 0),
                        scrollSize = new Vector3(1, 1, 1),

                        // EmptyScrll(滚动组件ScrollCollection的载体)的碰撞体
                        colliderCenter = new Vector3(0, 0, 0),
                        colliderSize = new Vector3(1f, 1f, 1f),

                        // 裁切对象的位置、尺寸
                        clippingCenter = new Vector3(0, 0, 0),
                        clippingSize = new Vector3(1f, 1f, 1f),

                        thickness = this.depth
                    });

                    this._clip.fresh();
                }

                this._clip.scrollCollection.CanScroll = value;
                this._clip.scrollCollection.ClipBox.enabled = value;
            }
        }

        /// <summary>
        /// 是否是刚体
        /// </summary>
        public bool isRigidbody
        {
            get
            {
                var rigidbody = this.parasitifer.GetComponent<Rigidbody>();
                return rigidbody != null;
            }

            set
            {
                this.parasitifer.GetComponent<Rigidbody>();
                if(value && !this.isRigidbody)
                {
                    this.parasitifer.AddComponent<Rigidbody>();

                    this.mass = this._mass;
                    this.drag = this._drag;
                    this.angularDrag = this._angularDrag;
                    
                    this.useGravity = this._useGravity;
                    this.isKinematic = this._isKinematic;
                }

                if (!value && this.isRigidbody)
                {
                    GameObject.Destroy(this.parasitifer.GetComponent<Rigidbody>());
                }
            }
        }

        /*
         *  以下属性与节点物理模拟相关
         */

        private float _mass = 1f;
        /// <summary>
        /// 刚体质量
        /// </summary>
        public float mass
        {
            get
            {
                var rigidbody = this.parasitifer.GetComponent<Rigidbody>();
                return rigidbody != null ?  rigidbody.mass : this._mass;
            }

            set
            {
                this._mass = value;
                if(this.isRigidbody)
                {
                    this.parasitifer.GetComponent<Rigidbody>().mass = value;
                }
            }
        }

        private float _drag = 0f;
        /// <summary>
        /// 刚体阻力
        /// </summary>
        public float drag
        {
            get
            {
                var rigidbody = this.parasitifer.GetComponent<Rigidbody>();
                return rigidbody != null ? rigidbody.drag : this._drag;
            }

            set
            { 
                this._drag = value;
                if (this.isRigidbody)
                {
                    this.parasitifer.GetComponent<Rigidbody>().drag = value;
                }
            }
        }

        private float _angularDrag = 0.05f;
        /// <summary>
        /// 刚体角阻力
        /// </summary>
        public float angularDrag
        {
            get
            {
                var rigidbody = this.parasitifer.GetComponent<Rigidbody>();
                return rigidbody != null ? rigidbody.angularDrag : this._drag;
            }

            set
            {
                this._angularDrag = value;
                if (this.isRigidbody)
                {
                    this.parasitifer.GetComponent<Rigidbody>().angularDrag = value;
                }
            }
        }

        private bool _useGravity = true;
        /// <summary>
        /// 是否使用重力
        /// </summary>
        public bool useGravity
        {
            get
            {
                var rigidbody = this.parasitifer.GetComponent<Rigidbody>();
                return rigidbody != null ? (bool) rigidbody.useGravity : false;
            }

            set
            {
                this._useGravity = value;
                if (this.isRigidbody)
                {
                    this.parasitifer.GetComponent<Rigidbody>().useGravity = value;
                }
            }
        }

        private bool _isKinematic = false;
        /// <summary>
        /// 是否使用运动学
        /// </summary>
        public bool isKinematic
        {
            get
            {
                var rigidbody = this.parasitifer.GetComponent<Rigidbody>();
                return rigidbody != null ? (bool)rigidbody.isKinematic : this._isKinematic;
            }

            set
            {
                this._isKinematic = value;
                if (this.isRigidbody)
                {
                    this.parasitifer.GetComponent<Rigidbody>().isKinematic = value;
                }
            }
        }

        /*
         *  以下属性与绞链相关
         */
        /// <summary>
        /// 关节绞链
        /// </summary>
        public Vector3? joint
        {
            get
            {
                var jointComponent = this.parasitifer.GetComponent<HingeJoint>();
                return jointComponent?.anchor;
            }

            set
            {
                if (value != null)
                {
                    var jointComponent = this.parasitifer.GetComponent<HingeJoint>();
                    if (jointComponent == null)
                    {
                        jointComponent = this.parasitifer.AddComponent<HingeJoint>();
                    }
                    jointComponent.anchor = (Vector3)value;
                }
                else
                {
                    var jointComponent = this.parasitifer.GetComponent<HingeJoint>();
                    if (jointComponent != null)
                    {
                        GameObject.Destroy(jointComponent);
                    }
                }
            }
        }

        /// <summary>
        /// 关节绞链轴
        /// </summary>
        public Vector3? jointRotationAxle
        {
            get
            {
                var jointComponent = this.parasitifer.GetComponent<HingeJoint>();
                return jointComponent?.axis;
            }

            set
            {
                if (value != null)
                {
                    var jointComponent = this.parasitifer.GetComponent<HingeJoint>();
                    if (jointComponent == null)
                    {
                        jointComponent = this.parasitifer.AddComponent<HingeJoint>();
                    }

                    jointComponent.axis = (Vector3)value;
                } else
                {
                    var jointComponent = this.parasitifer.GetComponent<HingeJoint>();
                    if (jointComponent != null)
                    {
                        GameObject.Destroy(jointComponent);
                    }
                }
            }
        }

        private Node _jointConnected = null;
        /// <summary>
        /// 关节绞链连接对象
        /// </summary>
        public Node jointConnected
        {
            get
            {
                return this._jointConnected;
            }

            set
            {
                if (value != null)
                {
                    if (this.isRigidbody)
                    {
                        this.isRigidbody = true;
                    }

                    var jointComponent = this.parasitifer.GetComponent<HingeJoint>();
                    if (jointComponent == null)
                    {
                        jointComponent = this.parasitifer.AddComponent<HingeJoint>();
                    }
                    jointComponent.connectedBody = value.parasitifer.GetComponent<Rigidbody>();

                    this._jointConnectedId = value.id;
                    this._jointConnected = value;
                } else
                {
                    var jointComponent = this.parasitifer.GetComponent<HingeJoint>();
                    if (jointComponent != null)
                    {
                        jointComponent.connectedBody = null;
                    }

                    this._jointConnectedId = "";
                    this._jointConnected = null;
                }
            }
        }


        private string _jointConnectedId = "";
        /// <summary>
        /// 关节绞链连接对象Id
        /// </summary>
        public string jointConnectId
        {
            get
            {
                return this.jointConnected != null ? this.jointConnected.id : this._jointConnectedId;
            }

            set
            {
                if(value.Trim() != "" && this.component.getNodeById(value) != null)
                {
                    if(this.isRigidbody)
                    {
                        this.isRigidbody = true;
                    }

                    var spaceNode = this.component.getNodeById(value);
                    if (spaceNode != null)
                    {
                        var jointComponent = this.parasitifer.GetComponent<HingeJoint>();
                        if (jointComponent == null)
                        {
                            jointComponent = this.parasitifer.AddComponent<HingeJoint>();
                        }
                        jointComponent.connectedBody = spaceNode.parasitifer.GetComponent<Rigidbody>();

                        this._jointConnectedId = value;

                        this._jointConnected = spaceNode;
                    }
                }

                if(value.Trim() == "")
                {
                    var jointComponent = this.parasitifer.GetComponent<HingeJoint>();
                    if (jointComponent != null)
                    {
                        jointComponent.connectedBody = null;
                    }

                    this._jointConnectedId = "";
                    this._jointConnected = null;
                }
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

        // 尺寸改变组件
        private SizeBounds _sizeBounds;
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
                if (value && this._sizeBounds == null)
                {
                    EMR.Space.mainService.next(() =>
                    {
                        // 防零
                        var width = this.parasitifer.transform.localScale.x;
                        var height = this.parasitifer.transform.localScale.y;
                        var depth = this.parasitifer.transform.localScale.z;
                        if (Utils.equals(this.parasitifer.transform.localScale.x, 0f) || Utils.equals(this.parasitifer.transform.localScale.y, 0f) || Utils.equals(this.parasitifer.transform.localScale.z, 0f))
                        {
                            this.parasitifer.transform.localScale = new Vector3(100f, 100f, 100f);
                        }

                        this._sizeBounds = new SizeBounds(this, this.sizeConfig);
                        this._sizeBounds.fresh();

                        // 防零还原
                        this.parasitifer.transform.localScale = new Vector3(width, height, depth);

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
        /// 节点操纵开始事件
        /// </summary>
        protected readonly ManipulationStartedEvent _onManipulationStarted = new ManipulationStartedEvent();
        /// <summary>
        /// 节点操纵开始事件
        /// </summary>
        public virtual ManipulationStartedEvent onManipulationStarted
        {
            get
            {
                return this._onManipulationStarted;
            }
        }

        /// <summary>
        /// 节点操纵开始事件
        /// </summary>
        protected readonly ManipulationEndedEvent _onManipulationEnded = new ManipulationEndedEvent();
        /// <summary>
        /// 节点操纵结束事件
        /// </summary>
        public virtual ManipulationEndedEvent onManipulationEnded
        {
            get
            {
                return this._onManipulationEnded;
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
        /// 解锁包括子节点在内的所有节点的尺寸有效设值过逻辑标识
        /// </summary>
        public override void unlockSizeSetedTag()
        {
            this.ergodic((Node node) =>
            {
                if (node is SpaceNode)
                {
                    var spaceNode = (SpaceNode)node;
                    spaceNode._hasWidthSeted = false;
                    spaceNode._hasHeightSeted = false;
                    spaceNode._hasDepthSeted = false;
                }

                if (node is PanelRoot)
                {
                    var panelRoot = (PanelRoot)node;
                    panelRoot.unlockSizeSetedTag();
                }

                if (node is SpaceMagic)
                {
                    var spaceMagic = (SpaceMagic)node;
                    spaceMagic.unlockSizeSetedTag();
                }

                return true;
            });
        }

        /// <summary>
        /// 解锁包括子节点在内的所有节点的位置有效设值过逻辑标识
        /// </summary>
        public void unlockPositionSetedTag()
        {
            this.ergodic((Node node) =>
            {
                if (node is SpaceNode)
                {
                    var spaceNode = (SpaceNode)node;
                    spaceNode._hasXSeted = false;
                    spaceNode._hasYSeted = false;
                    spaceNode._hasZSeted = false;
                }

                if (node is PanelRoot)
                {
                    var panelRoot = (PanelRoot)node;
                    panelRoot.unlockPositionSetedTag();
                }

                if (node is SpaceMagic)
                {
                    var spaceMagic = (SpaceMagic)node;
                    spaceMagic.unlockPositionSetedTag();
                }

                return true;
            });
        }

        /// <summary>
        /// 向节点加入内容
        /// </summary>
        /// <param name="current">当前要加入的内容节点</param>
        public override void addCentent(Node current)
        {
            Space.GameObjectEntity.appendNode(current.parasitifer, this.parasitifer);
        }

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

        /// <summary>
        /// 添加autoCollider
        /// </summary>
        public override void addAutoCollider()
        {
            this.autoCollider = this.parasitifer.AddComponent<BoxCollider>();

            this.autoCollider.center = new Vector3(0f, 0f, 0f);
            this.autoCollider.size = new Vector3(1f, 1f, 1f);
        }

        /// <summary>
        /// 添加autoInteractionTouchable
        /// </summary>
        public override void addAutoInteractionTouchable()
        {
            this.autoInteractionTouchable = this.parasitifer.AddComponent<NearInteractionTouchableVolume>();

        }

        /// <summary>
        /// 销毁autoCollider
        /// </summary>
        protected override void recoveryAutoCollider()
        {
            if (!this.collider && this.autoCollider != null && EMR.Space.leftHand.getHandlerCount(this) == 0 && EMR.Space.rightHand.getHandlerCount(this) == 0 && this.checkNodeNotIncludePointerEvent() && this.checkNodeNotIncludeTouchEvent() && this.checkNodeNotIncludeFocusEvent() && this.checkNodeNotIncludeColliderEvent())
            {
                GameObject.Destroy(this.autoCollider);
                this.autoCollider = null;
            }
        }

        /// <summary>
        /// 销毁autoInteractionTouchable
        /// </summary>
        protected override void recoveryAutoInteractionTouchable()
        {
            if (this.autoInteractionTouchable != null && EMR.Space.leftHand.getHandlerCount(this) == 0 && EMR.Space.rightHand.getHandlerCount(this) == 0 && this.checkNodeNotIncludeTouchEvent())
            {
                GameObject.Destroy(this.autoInteractionTouchable);
                this.autoInteractionTouchable = null;
            }
        }

        /// <summary>
        /// 添加InteractionTouchable组件
        /// </summary>
        /// <param name="center">中心点</param>
        /// <param name="size">尺寸</param>
        /// <param name="name">名称</param>
        public override void addInteractionTouchable(string name, Vector3? center = null, Vector3? size = null)
        {

            NearInteractionTouchableVolume result = this.parasitifer.AddComponent<NearInteractionTouchableVolume>();

            if (name != null && name.Trim() != "")
            {
                interactionTouchableDictionary.Add(name, result);
            }
            else
            {
                interactionTouchableDictionary.Add(Guid.NewGuid().ToString(), result);
            }
        }

        /// <summary>
        /// 移除InteractionTouchable
        /// </summary>
        /// <param name="interactionTouchable">要移除的InteractionTouchable</param>
        public override void removeInteractionTouchable(object interactionTouchable)
        {
            NearInteractionTouchableVolume result = null;
            foreach (var item in interactionTouchableDictionary)
            {
                if (item.Value == (object)interactionTouchable)
                {
                    result = interactionTouchableDictionary[name];
                    this.interactionTouchableDictionary.Remove(item.Key);
                }
            }

            if (result != null)
            {
                GameObject.Destroy(result);
            }
        }

        /// <summary>
        /// 移除InteractionTouchable
        /// </summary>
        /// <param name="name">名称</param>
        public override void removeInteractionTouchable(string name)
        {
            NearInteractionTouchableVolume result = null;
            if (interactionTouchableDictionary.ContainsKey(name))
            {
                result = interactionTouchableDictionary[name];
                interactionTouchableDictionary.Remove(name);
            }

            if (result != null)
            {
                GameObject.Destroy(result);
            }
        }

        /// <summary>
        /// 添加渲染器
        /// </summary>
        protected override MeshRenderer addRender()
        {
            // 创建MeshRenderer
            MeshRenderer meshRenderer = this.parasitifer.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                meshRenderer = this.parasitifer.AddComponent<MeshRenderer>();
                if (this._renderMode == RendMode.opaque)
                {
                    meshRenderer.material = Resources.Load<Material>("Material/DefaultUI") as Material;
                }

                if (this._renderMode == RendMode.cutout)
                {
                    meshRenderer.material = Resources.Load<Material>("Material/DefaultUI_Cutput") as Material;
                }

                if (this._renderMode == RendMode.transparent)
                {
                    meshRenderer.material = Resources.Load<Material>("Material/DefaultUI_Transparent") as Material;
                }

                if (this._renderMode == RendMode.fade)
                {
                    meshRenderer.material = Resources.Load<Material>("Material/DefaultUI_Fade") as Material;
                }

                if (this._renderMode == RendMode.additive)
                {
                    meshRenderer.material = Resources.Load<Material>("Material/DefaultUI_Additive") as Material;
                }
            }

            // 创建Filter
            MeshFilter meshFilter = this.parasitifer.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = this.parasitifer.AddComponent<MeshFilter>();

                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                meshFilter.mesh = cube.GetComponent<MeshFilter>().mesh;

                GameObject.Destroy(cube);
            }

            return meshRenderer;
        }

        /*
         *  以下方法主要用于尺寸、位置、旋转量刷新
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
        /// 位置同步 (一般由框架内部调用，开发者一般不需要调用此方法)
        /// </summary>
        public void positionSynch()
        {
            this._x = this.position.x;
            this._y = this.position.y;
            this._z = this.position.z;
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
        /// 阻止触发onRotationChange事件
        /// </summary>
        public override bool preventRotationEvent
        {
            set
            {
                if (value == true)
                {
                    this.previousRotation.xAngle = this.rotation.xAngle;
                    this.previousRotation.yAngle = this.rotation.yAngle;
                    this.previousRotation.zAngle = this.rotation.zAngle;
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
                var colliderDepth = 1f;

                if (!Utils.equals(this.parasitifer.transform.localScale.x, 0f) && !Utils.equals(this.parasitifer.transform.localScale.y, 0f) && !Utils.equals(this.parasitifer.transform.localScale.z, 0f))
                {
                    colliderWidth = 1 + Space.Unit.unitToScale(this, this.sizeBoxPadding, Axle.right) / this.parasitifer.transform.localScale.x;
                    colliderHeigyht = 1 + Space.Unit.unitToScale(this, this.sizeBoxPadding, Axle.up) / this.parasitifer.transform.localScale.y;
                    colliderDepth = 1 + Space.Unit.unitToScale(this, this.sizeBoxPadding, Axle.forward) / this.parasitifer.transform.localScale.z;
                }

                this._sizeBounds.collider.size = new Vector3(colliderWidth, colliderHeigyht, colliderDepth);
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
         *  以下方法主要用于节点布局刷新
         */
        /// <summary>
        /// 空间弯曲刷新
        /// </summary>
        public void spaceBendFresh()
        {
            if (!Utils.equals(this._spaceBend.bendAngle, 0f))
            {
                this._spaceBend.fresh();
            }
        }

        /// <summary>
        /// 内容对齐刷新
        /// </summary>
        public void contentAlignFresh()
        {
            this._contentHorizontalAlign?.fresh();
            this._contentVerticalAlign?.fresh();
            this._contentForwardAlign?.fresh();
        }

        /*
         *  以下方法主要用于节点运动
         */
        /// <summary>
        /// 移动
        /// </summary>
        /// <param name="end">目标位置</param>
        /// <param name="time">动画时长</param>
        /// <param name="curveType">动画曲线</param>
        /// <param name="callback">动画结束回调</param>
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
                    this.ergodic((Node node) =>
                    {
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
        /// <param name="end">目标尺寸</param>
        /// <param name="time">动画时长</param>
        /// <param name="curveType">动画曲线</param>
        /// <param name="callback">动画结束回调</param>
        public void rotateTo(RotationData end, float time, MotionCurve curveType, AnimationCallback callback = null)
        {
            if (Utils.equals(this.xAngle, end.xAngle) && Utils.equals(this.yAngle, end.yAngle) && Utils.equals(this.zAngle, end.zAngle))
            {
                return;
            }

            var startVector = new Vector3(this.rotation.xAngle, this.rotation.yAngle, this.rotation.zAngle);

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

            }, callback);
        }
        #endregion

        #region 节点数据结构基本操作
        /// <summary>
        /// 插入PanelRoot节点
        /// </summary>
        /// <param name="current">当前要插入的节点</param>
        /// <param name="refNode">参照节点</param>
        /// <returns></returns>
        public PanelRoot insertBefore(PanelRoot current, Node refNode)
        {
            Node result = null;

            // 抛出节点插入前事件
            InsertEventData insertEventData = new InsertEventData
            {
                target = current,
            };
            onInsert.Invoke(insertEventData);

            // 使用默认行为插入节点
            if (!insertEventData.isPreventDefault)
            {
                var originalParent = this.parent;

                // 解锁节点尺寸有效设值过逻辑标识
                current.unlockSizeSetedTag();

                // 解锁节点位置有效设值过逻辑标识
                current.unlockPositionSetedTag();

                result = base.insertBefore(current, refNode);

                if (result != null)
                {
                    this.addCentent(current);

                    // 引发布局更新
                    SpaceLayout.update(new SpaceNodeActionInfo
                    {
                        originalParent = originalParent,
                        current = this,
                        currentParent = this.parent,
                        action = SpaceNodeAction.insert
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
            return result != null ? (PanelRoot)result : null;
        }

        /// <summary>
        /// 插入SpaceMagic节点
        /// </summary>
        /// <param name="current">当前要插入的节点</param>
        /// <param name="refNode">参照节点</param>
        /// <returns></returns>
        public SpaceMagic insertBefore(SpaceMagic current, Node refNode)
        {
            Node result = null;

            // 抛出节点插入前事件
            InsertEventData insertEventData = new InsertEventData
            {
                target = (Element)current,
            };
            onInsert.Invoke(insertEventData);

            // 使用默认值插入节点
            if (!insertEventData.isPreventDefault)
            {
                var originalParent = this.parent;

                // 解锁节点尺寸有效设值过逻辑标识
                current.unlockSizeSetedTag();

                // 解锁节点位置有效设值过逻辑标识
                current.unlockPositionSetedTag();

                result = base.insertBefore(current, refNode);

                if (result != null)
                {
                    this.addCentent(current);

                    current.position.x = 0f;
                    current.position.y = 0f;
                    current.position.z = 0f;

                    current.size.width = 1f;
                    current.size.height = 1f;
                    current.size.depth = 1f;

                    current.rotation.xAngle = 0f;
                    current.rotation.yAngle = 0f;
                    current.rotation.zAngle = 0f;

                    SpaceLayout.update(new SpaceNodeActionInfo
                    {
                        originalParent = originalParent,
                        current = this,
                        currentParent = this.parent,
                        action = SpaceNodeAction.insert
                    });
                }
            }

            // 抛出节点插入完成事件
            InsertedEventData insertedEventData = new InsertedEventData
            {
                target = (Element)current,
                isSuccess = result != null ? true : false,
            };
            onInserted.Invoke(insertedEventData);

            // 返回插入结果
            return result != null ? (SpaceMagic)result : null;
        }

        /// <summary>
        /// 插入空间节点
        /// </summary>
        /// <param name="current">当前要插入的节点</param>
        /// <param name="refNode">参照节点</param>
        /// <returns></returns>
        public SpaceNode insertBefore(SpaceNode current, Node refNode)
        {
            Node result = null;

            // 抛出节点插入前事件
            InsertEventData insertEventData = new InsertEventData
            {
                target = current,
            };
            onInsert.Invoke(insertEventData);

            // 使用默认行为添加节点
            if (!insertEventData.isPreventDefault)
            {
                var originalParent = this.parent;

                // 解锁节点尺寸有效设值过逻辑标识
                current.unlockSizeSetedTag();

                // 解锁节点位置有效设值过逻辑标识
                current.unlockPositionSetedTag();

                result = base.insertBefore(current, refNode);

                if (result != null)
                {
                    this.addCentent(current);

                    SpaceLayout.update(new SpaceNodeActionInfo
                    {
                        originalParent = originalParent,
                        current = this,
                        currentParent = this.parent,
                        action = SpaceNodeAction.insert
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
            return result != null ? (SpaceNode)result : null;
        }

        /// <summary>
        /// 插入子节点
        /// </summary>
        /// <param name="current">要插入的节点</param>
        /// <param name="refNode">参照节点</param>
        /// <returns></returns>
        public override Node insertBefore(Node current, Node refNode)
        {
            Node result = null;
            if (current is SpaceNode)
            {
                result = this.insertBefore((SpaceNode)current, refNode);
            }

            if (current is PanelRoot)
            {
                result = this.insertBefore((PanelRoot)current, refNode);
            }

            if (current is SpaceMagic)
            {
                result = this.insertBefore((SpaceMagic)current, refNode);
            }

            return result;
        }

        /// <summary>
        /// 添加PanelRoot节点
        /// </summary>
        /// <param name="node">当前要添加的节点</param>
        /// <returns></returns>
        public PanelRoot appendNode(PanelRoot node)
        {
            Node result = null;

            // 抛出节点添加前事件
            AppendEventData appendEventData = new AppendEventData
            {
                target = node
            };
            onAppend.Invoke(appendEventData);

            // 使用默认行为添加节点
            if (!appendEventData.isPreventDefault)
            {
                var originalParent = this.parent;

                // 解锁节点尺寸有效设值过逻辑标识
                node.unlockSizeSetedTag();

                // 解锁节点位置有效设值过逻辑标识
                node.unlockPositionSetedTag();

                result = base.appendNode(node);
                if (result != null)
                {
                    this.addCentent(node);

                    SpaceLayout.update(new SpaceNodeActionInfo
                    {
                        originalParent = originalParent,
                        current = node,
                        currentParent = this,
                        action = SpaceNodeAction.append
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
            return result != null ? (PanelRoot)result : null;
        }

        /// <summary>
        /// 添加SpaceMagic节点
        /// </summary>
        /// <param name="node">当前要添加的节点</param>
        /// <returns></returns>
        public SpaceMagic appendNode(SpaceMagic node)
        {
            Node result = null;

            // 抛出节点添加前事件
            AppendEventData appendEventData = new AppendEventData
            {
                target = (Element)node
            };
            onAppend.Invoke(appendEventData);

            // 使用默认行业添加节点
            if (!appendEventData.isPreventDefault)
            {
                var originalParent = this.parent;

                // 解锁节点尺寸有效设值过逻辑标识
                node.unlockSizeSetedTag();

                // 解锁节点位置有效设值过逻辑标识
                node.unlockPositionSetedTag();

                result = base.appendNode(node);
                if (result != null)
                {
                    this.addCentent(node);

                    node.position.x = 0f;
                    node.position.y = 0f;
                    node.position.z = 0f;

                    node.size.width = 1f;
                    node.size.height = 1f;
                    node.size.depth = 1f;

                    node.rotation.xAngle = 0f;
                    node.rotation.yAngle = 0f;
                    node.rotation.zAngle = 0f;

                    // 引发布局更新
                    SpaceLayout.update(new SpaceNodeActionInfo
                    {
                        originalParent = originalParent,
                        current = node,
                        currentParent = this,
                        action = SpaceNodeAction.append
                    });
                }
            }

            // 拙出节点添加完成事件
            AppendedEventData appendedEventData = new AppendedEventData
            {
                target = (Element)node,
                isSuccess = result != null ? true : false,
            };
            onAppended.Invoke(appendedEventData);

            // 返回添加结果
            return result != null ? (SpaceMagic)result : null;
        }

        /// <summary>
        /// 添加空间节点
        /// </summary>
        /// <param name="node">当前要添加的节点</param>
        /// <returns></returns>
        public SpaceNode appendNode(SpaceNode node)
        {
            Node result = null;

            // 抛出节点添加前事件
            AppendEventData appendEventData = new AppendEventData
            {
                target = node
            };
            onAppend.Invoke(appendEventData);

            // 使用默认行为添加节点
            if (!appendEventData.isPreventDefault)
            {
                var originalParent = this.parent;

                // 解锁节点尺寸有效设值过逻辑标识
                node.unlockSizeSetedTag();

                // 解锁节点位置有效设值过逻辑标识
                node.unlockPositionSetedTag();

                result = base.appendNode(node);
                if (result != null)
                {
                    this.addCentent(node);

                    // 引发布局更新
                    SpaceLayout.update(new SpaceNodeActionInfo
                    {
                        originalParent = originalParent,
                        current = node,
                        currentParent = this,
                        action = SpaceNodeAction.append
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
            return result != null ? (SpaceNode)result : null;
        }

        /// <summary>
        /// 添加节点
        /// </summary>
        /// <param name="node">当前要添加的节点</param>
        /// <returns></returns>
        public override Node appendNode(Node node)
        {
            Node result = null;
            if(node is SpaceNode)
            {
                result = this.appendNode((SpaceNode)node);
            }

            if (node is SpaceMagic)
            {
                result = this.appendNode((SpaceMagic)node);
            }

            if (node is PanelRoot)
            {
                result = this.appendNode((PanelRoot)node);
            }

            return result;
        }

        /// <summary>
        /// 销毁节点
        /// </summary>
        /// <returns></returns>
        public override void destory()
        {
            // 抛出节点销毁事件
            DestoryEventData destoryEventData = new DestoryEventData
            {
                target = this,
            };
            onDestory.Invoke(destoryEventData);

            // 使用默认行为销毁节点
            if (!destoryEventData.isPreventDefault)
            {
                base.destory();

                this.npc?.destory();
            }

            // 抛出节点销毁完成事件
            DestoryedEventData destoryCompleteEventData = new DestoryedEventData
            {
                target = this,
                isSuccess = !destoryEventData.isPreventDefault,
            };
            onDestoryed.Invoke(destoryCompleteEventData);
        }
        #endregion
    }
}