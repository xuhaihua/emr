using System;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Rendering;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;
using UnityEngine.Events;
using EMR.Struct;
using EMR.Event;
using EMR.Common;
using EMR.Plugin;

namespace EMR.Entity
{
    /// <summary>
    /// 节点异常类
    /// </summary>
    public class NodeException : ApplicationException
    {
        private string error;

        public NodeException(string msg)
        {
            error = msg;
        }
    }

    /// <summary>
    /// 节点处理委托
    /// </summary>
    /// <param name="node">要处理的节点</param>
    public delegate bool NodeHandler(Node node);

    /// <summary>
    /// 动画回调委托 （主要用于动画结束时)
    /// </summary>
    public delegate void AnimationCallback();

    /// <summary>
    /// 动画行为委托
    /// </summary>
    /// <param name="data">空间向量数据</param>
    /// <param name="isFinish">动画是否已结束</param>
    public delegate void AnimationAction(Vector3 data, bool isFinish);

    /// <summary>
    /// 动画行为委托
    /// </summary>
    /// <param name="data">当前运动量</param>
    /// <param name="m">运动参量</param>
    public delegate void AnimationActionFloat(float data, bool isFinish);


    /// <summary>
    /// 空间节点
    /// 本类主要用于在空间中创建一个节点实体，该节点实体在创建的同时也向SpaceEventEngine注册相关的emitter
    /// </summary>
    public abstract class Node : EMR.Common.DataStructure.Node
    {

        #region 基本字段定义
        public delegate bool FindNodeConditionHander(Node node);

        public NodeService service;

        /// <summary>
        /// 节点id
        /// </summary>
        private string _id = "";

        /// <summary>
        /// 节点名称
        /// </summary>
        private string _name = "";

        /// <summary>
        /// 缩主 节点所管理的游戏对象
        /// </summary>
        private GameObject _parasitifer;

        /// <summary>
        /// 偏移量
        /// </summary>
        public Vector3 _offset = new Vector3(0, 0, 0);

        /// <summary>
        /// 位置
        /// </summary>
        public Position position;
        protected PositionData previousPosition;

        /// <summary>
        /// 旋转量
        /// </summary>
        public Rotation rotation;
        protected RotationData previousRotation;

        /// <summary>
        /// 尺寸
        /// </summary>
        public Size size;
        protected SizeData previousSize;

        /// <summary>
        /// 节点水平浮动方式
        /// </summary>
        private AlignMode _horizontal = AlignMode.none;

        /// <summary>
        /// 节点垂直浮动方式
        /// </summary>
        private AlignMode _vertical = AlignMode.none;

        /// <summary>
        /// 当前是否正在移动
        /// </summary>
        private bool _isMoving = false;

        /// <summary>
        /// 当前尺寸是否处于改变状态
        /// </summary>
        private bool _isSizeChanging = false;

        /// <summary>
        /// 当前是否处于旋转状态
        /// </summary>
        private bool _isRotating = false;

        /// <summary>
        /// 所在组件
        /// </summary>
        protected CoreComponent _component = null;

        /// <summary>
        /// 对齐锚（用于浮动布局）
        /// </summary>
        private Anchor _alignAnchor;

        /// <summary>
        /// 锚集合
        /// </summary>
        private Dictionary<string, Anchor> _anchorCollection = new Dictionary<string, Anchor>();

        /// <summary>
        /// collider集合
        /// </summary>
        protected Dictionary<string, BoxCollider> boxColliderDictionary = new Dictionary<string, BoxCollider>();

        /// <summary>
        /// npc
        /// </summary>
        protected NPC _npc = null;


        // 事件碰撞体自动（当为true时系统将为pointer事件自动添加一个专属的collider否则事件碰撞由点自身碰撞体决定）
        private bool _collider = false;

        // 近指针触摸自动（当为true时系统将为触摸事件自动添加一个专属的InteractionTouchable组件）
        private bool _interactionTouchableAuto = true;

        // 与事件关联的collider
        public BoxCollider autoCollider = null;

        /// <summary>
        /// 节点正在添加或插入组件
        /// </summary>
        public bool isIncreaseCompontent = false;

        protected bool _hasWidthSeted = false;
        protected bool _hasHeightSeted = false;
        protected bool _hasDepthSeted = false;

        /// <summary>
        /// 标识当前元素是否正在处于创建阶段
        /// </summary>
        public bool isCreating = false;

        /// <summary>
        /// 标识当前父元素是否正在处于销毁阶段
        /// </summary>
        protected bool _isParentElementDestoring = false;

        /// <summary>
        /// 节点完全限定名
        /// </summary>
        public string fullName = "";
        #endregion

        public StyleCollect styleCollect = null;

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
        public Node(float x, float y, float z, float xAngle, float yAngle, float zAngle, float width, float height, float depth, NPC npc = null)
        {
            // 尺寸异常
            if (width < 0 || height < 0)
            {
                throw new NodeException("节点尺寸不能小于0");
            }

            this._parasitifer = new GameObject();

            this._npc = npc;

            this._onDown = new DownEvent(this);                                                    // 按下事件
            this._onDragged = new DraggedEvent(this);                                              // 拖动事件
            this._onUp = new UpEvent(this);                                                        // 释放事件
            this._onClick = new ClickEvent(this);                                                  // 单击事件

            this._onTouchStarted = new TouchStartedEvent(this);                                    // 触摸开始事件
            this._onTouchUpdate = new TouchUpdateEvent(this);                                      // 触摸更新事件
            this._onTouchCompleted = new TouchCompletedEvent(this);                                // 触摸完成事件

            this._onFocusEnter = new FocusEnterEvent(this);                                        // 焦点进入事件
            this._onFocusChanged = new FocusChangedEvent(this);                                    // 焦点改变事件
            this._onFocusExit = new FocusExitEvent(this);                                          // 焦点退出事件

            // 节点所管理的游戏对象尺寸、位置、旋转量
            this.position = new Position(this, x, y, z);
            this.size = new Size(this, width, height, depth);
            this.rotation = new Rotation(this, xAngle, yAngle, zAngle);

            // 初始化postion size rotation 旧值
            this.previousPosition = new PositionData(0f, 0f, 0f);
            this.previousSize = new SizeData(0f, 0f, 0f);
            this.previousRotation = new RotationData(0f, 0f, 0f);

            // 向游戏对象添加SpaceEventEngine组件
            var eventEngine = this.parasitifer.AddComponent<EventEngine>();
            eventEngine.node = this;

            this.service = new NodeService(MixedRealityToolkit.Instance, Guid.NewGuid().ToString(), 10, null);
            EMR.Space.mainService.addService(service);

            // 对齐锚(用作自身节点的对齐)
            this._alignAnchor = new Anchor(this, null, null, null);

            // 创建postion size rotation emitter 
            this._createPositionEmitter();
            this._createSizeEmit();
            this._createRotationEmit();

            // 节点创建后将基加入表演者查询map中
            EMR.Space.addParasitiferMap(this);

            // 一直在每个周期的末监控（用于执行相关销毁）
            if (this is PanelNode || this is SpaceNode)
            {
                this.service.annexUpdate(() =>
                {
                    this.recoveryAutoCollider();

                    this.recoveryAutoInteractionTouchable();

                    return false;
                });
            }

            styleCollect = new StyleCollect(this);

            // 将属性加入样式集合
            this.styleCollect.add("offset");
            this.styleCollect.add("width");
            this.styleCollect.add("height");
            this.styleCollect.add("depth");
            this.styleCollect.add("backgroundColor");
            this.styleCollect.add("backgroundImage");
            this.styleCollect.add("borderWidth");
            this.styleCollect.add("borderRadius");
            this.styleCollect.add("horizontalFloat");
            this.styleCollect.add("verticalFloat");
            this.styleCollect.add("contentHorizontal");
            this.styleCollect.add("horizontalInterval");
            this.styleCollect.add("contentVertical");
            this.styleCollect.add("verticalInterval");
            this.styleCollect.add("lightIntensity");
            this.styleCollect.add("hoverColor");
            this.styleCollect.add("widthFixed");
            this.styleCollect.add("heightFixed");
            this.styleCollect.add("depthFixed");
            this.styleCollect.add("npcPath");
            this.styleCollect.add("npcOffset");
        }

        /// <summary>
        /// 按条件查找父节点
        /// </summary>
        /// <param name="startNode">起始节点</param>
        /// <param name="nodeConditionHander">查询条件</param>
        public static Node findParentNodeByCondition(Node startNode, FindNodeConditionHander nodeConditionHander)
        {
            Node result = null;
            var tempNode = startNode.parent;
            while (tempNode != null && tempNode is Node)
            {
                if (nodeConditionHander(tempNode))
                {
                    result = tempNode;
                    break;
                }

                tempNode = tempNode.parent;
            }
            return result;
        }

        #region 节点基本属性
        /// <summary>
        /// 节点id
        /// </summary>
        public virtual string id
        {
            get
            {
                return this._id;
            }

            set
            {
                if (this._id == "" && value != "")
                {
                    this._id = value;

                    if(!this.component.isAssembling)
                    {
                        foreach (var styleSheet in this.component.styleSheets)
                        {
                            // 设置节点的样式
                            if (styleSheet != null)
                            {
                                // 设置该节点样式
                                styleSheet.setStyle(this, null, null, this._id, null);
                            }
                        }
                    }
                    
                    EMR.Space.addNodeIdMap(this);
                }
            }
        }

        /// <summary>
        /// 节点名称
        /// </summary>
        public virtual string name
        {
            get
            {
                return this._name;
            }

            set
            {
                if (this._name == "" && value != "")
                {
                    this._name = value;
                    this.parasitifer.transform.name = value;

                    if(!this.component.isAssembling)
                    {
                        foreach (var styleSheet in this.component.styleSheets)
                        {
                            // 设置节点的样式
                            if (styleSheet != null)
                            {
                                // 设置节点的样式
                                if (styleSheet != null)
                                {
                                    // 设置该节点样式
                                    styleSheet.setStyle(this, null, this._name, null, null);
                                }
                            }
                        }
                    }
                    
                    EMR.Space.addNodeNameMap(this);
                }
            }
        }

        public bool isParentElementDestoring
        {
            get
            {
                return this._isParentElementDestoring;
            }
        }

        /// <summary>
        /// 宿主 (源)
        /// </summary>
        public GameObject parasitifer
        {
            get
            {
                return this._parasitifer;
            }
        }

        /// <summary>
        /// 所在组件
        /// </summary>
        public CoreComponent component
        {
            get
            {
                return this._component;
            }
        }

        private bool _widthFixed = true;
        /// <summary>
        /// 宽度是否固定
        /// </summary>
        public virtual bool widthFixed
        {
            get
            {
                return this._widthFixed;
            }

            set
            {
                if (value && !this._widthFixed)
                {
                    this._hasWidthSeted = false;
                }
                this._widthFixed = value;
            }
        }

        /*
         *  尺寸位置相关属性
         */
        protected float _width = 0f;
        /// <summary>
        /// width
        /// </summary>
        public virtual float width
        {
            get
            {
                return _width;
            }

            set
            {
                this._width = value;
                this.size.width = value;

            }
        }

        private bool _heightFixed = true;
        /// <summary>
        /// 高度是否固定
        /// </summary>
        public virtual bool heightFixed
        {
            get
            {
                return this._heightFixed;
            }

            set
            {
                if (value && !this._heightFixed)
                {
                    this._hasHeightSeted = false;
                }
                this._heightFixed = value;
            }
        }

        protected float _height = 0f;
        /// <summary>
        /// height
        /// </summary>
        public virtual float height
        {
            get
            {
                return this._height;
            }

            set
            {
                this._height = value;
                this.size.height = value;
            }
        }

        protected bool _depthFixed = true;
        public virtual bool depthFixed
        {
            get
            {
                return this._depthFixed;
            }

            set
            {
                if (value && !this._depthFixed)
                {
                    this._hasDepthSeted = false;
                }
                this._depthFixed = value;
            }
        }

        protected float _depth = 0f;
        /// <summary>
        /// 厚度
        /// </summary>
        public virtual float depth
        {
            get
            {
                return this._depth;
            }

            set
            {
                this._depth = value;
                this.size.depth = value;
            }
        }

        /// <summary>
        /// 空间偏移量
        /// </summary>
        public virtual Vector3 offset
        {
            get
            {
                return this._offset;
            }

            set
            {
                var x = this.position.x;
                var y = this.position.y;
                var z = this.position.z;

                this._offset = value;

                this.position.x = x;
                this.position.y = y;
                this.position.z = z;
            }
        }

        private Vector3 _npcOffset = new Vector3(0f, 0f, 0f);
        /// <summary>
        /// npc偏移量
        /// </summary>
        public virtual Vector3 npcOffset
        {
            get
            {
                return this._npcOffset;
            }

            set
            {
                this._npcOffset = value;

                if(this.npc != null)
                {
                    this.npc.offset = value;
                }
            }
        }

        /// <summary>
        /// 是否正在移动
        /// </summary>
        public virtual bool isMoving
        {
            get
            {
                return this._isMoving;
            }
            set
            {
                this._isMoving = value;
            }
        }

        /// <summary>
        /// 是否正在旋转
        /// </summary>
        public virtual bool isRotating
        {
            get
            {
                return this._isRotating;
            }

            set
            {
                this._isRotating = value;
            }
        }

        /// <summary>
        /// 是否正在改变尺寸
        /// </summary>
        public virtual bool isSizeChanging
        {
            get
            {
                return this._isSizeChanging;
            }

            set
            {
                this._isSizeChanging = value;
            }
        }

        private bool _isDepthChanging = false;
        /// <summary>
        /// 是否深度正在改变尺寸
        /// </summary>
        public virtual bool isDepthChanging
        {
            get
            {
                return this._isDepthChanging;
            }
        }

        /// <summary>
        /// 包围盒
        /// </summary>
        public virtual BoundBox localBounds
        {
            get
            {
                return new BoundBox(new Vector3(float.NaN, float.NaN, float.NaN), new Vector3(float.NaN, float.NaN, float.NaN));
            }
        }

        public virtual bool hasAutoInteractionTouchable
        {
            get
            {
                return false;
            }
        }


        /*
         * 基础样式属性
         */
        /// <summary>
        /// 材质
        /// </summary>
        protected virtual Material material
        {
            get
            {
                Material result = this.parasitifer.GetComponent<MaterialInstance>()?.Material;

                if (result == null)
                {
                    this.addRender();
                    result = this.parasitifer.AddComponent<MaterialInstance>().Material;
                }

                return result;
            }
        }

        protected RendMode _renderMode = RendMode.opaque;
        private string _renderModeString = "";
        /// <summary>
        /// 渲染模式
        /// </summary>
        public virtual string renderMode
        {
            get
            {
                return _renderModeString;
            }

            set
            {
                if (_renderModeString == "")
                {
                    if (value == RendMode.opaque.ToString())
                    {
                        this._renderMode = RendMode.opaque;
                        this._renderModeString = this.renderMode;
                    }

                    if (value == RendMode.cutout.ToString())
                    {
                        this._renderMode = RendMode.cutout;
                        this._renderModeString = this.renderMode;
                    }

                    if (value == RendMode.transparent.ToString())
                    {
                        this._renderMode = RendMode.transparent;
                        this._renderModeString = this.renderMode;
                    }

                    if (value == RendMode.fade.ToString())
                    {
                        this._renderMode = RendMode.fade;
                        this._renderModeString = this.renderMode;
                    }

                    if (value == RendMode.additive.ToString())
                    {
                        this._renderMode = RendMode.additive;
                        this._renderModeString = this.renderMode;
                    }
                }
            }
        }

        /*--------------------------------------------定义backgroundColor、backgroundImage属性支撑方法开始-------------------------------------------*/
        /// <summary>
        /// 添加渲染器
        /// </summary>
        protected virtual MeshRenderer addRender()
        {
            return null;
        }

        /// <summary>
        /// 对象Render
        /// </summary>
        protected void setRenderEnable()
        {
            Renderer render = this.parasitifer.GetComponent<Renderer>();

            if (render != null)
            {
                if (this._backgroundColor == "" && this._backgroundImage == "")
                {
                    render.enabled = false;
                }
                else
                {
                    render.enabled = true;
                }
            }
        }
        /*--------------------------------------------定义backgroundColor、backgroundImage属性支撑方法结束-------------------------------------------*/

        private string _backgroundColor = "";
        /// <summary>
        /// 颜色
        /// </summary>
        public virtual string backgroundColor
        {
            get
            {
                return this._backgroundColor;
            }

            set
            {
                this._backgroundColor = value;

                // 设置背景色
                if (value != "")
                {
                    this.material.color = Utils.stringToColor(value);
                }

                this.setRenderEnable();
            }
        }

        private string _backgroundImage = "";
        /// <summary>
        /// 设置背景
        /// </summary>
        public virtual string backgroundImage
        {
            get
            {
                return this._backgroundImage;
            }

            set
            {
                // 获取贴图资源
                var texture = value != "" ? Resources.Load<Texture2D>(value) as Texture2D : null;

                this._backgroundImage = value;

                // 设置背景
                if (texture != null)
                {
                    this.material.mainTexture = texture;
                }

                // 当背景为空颜色不为空时重置颜色
                if (value == "" && this.backgroundColor != "")
                {
                    this.backgroundColor = this._backgroundColor;
                }

                this.setRenderEnable();
            }
        }

        private float _lightIntensity = 0f;
        /// <summary>
        /// 灯光强度
        /// </summary>
        public virtual float lightIntensity
        {
            get
            {
                return this._lightIntensity;
            }

            set
            {
                this._lightIntensity = value;

                this.material.SetFloat("_FluentLightIntensity", value);

                this.setRenderEnable();
            }
        }

        private string _hoverColor = "255,255,255";
        /// <summary>
        /// 设置悬浮灯颜色
        /// </summary>
        public virtual string hoverColor
        {
            get
            {
                return this._hoverColor;
            }

            set
            {
                this._hoverColor = value;

                this.material.SetColor("_HoverColorOverride", Utils.stringToColor(value));

                this.setRenderEnable();
            }
        }

        private string _nearLightCenterColor = "255,255,255";
        /// <summary>
        /// 设置接近光center颜色
        /// </summary>
        public virtual string nearLightCenterColor
        {
            get
            {
                return this._nearLightCenterColor;
            }

            set
            {
                this._nearLightCenterColor = value;

                this.material.SetColor("_ProximityLightCenterColorOverride", Utils.stringToColor(value));

                this.setRenderEnable();
            }
        }

        private string _nearLightMiddleColor = "255,255,255";
        /// <summary>
        /// 设置接近光middle颜色
        /// </summary>
        public virtual string nearLightMiddleColor
        {
            get
            {
                return this._nearLightMiddleColor;
            }

            set
            {
                this._nearLightMiddleColor = value;

                this.material.SetColor("_ProximityLightMiddleColorOverride", Utils.stringToColor(value));

                this.setRenderEnable();
            }
        }

        private string _nearLightOuterColor = "255,255,255";
        /// <summary>
        /// 设置接近光outline颜色
        /// </summary>
        public virtual string nearLightOuterColor
        {
            get
            {
                return this._nearLightOuterColor;
            }

            set
            {
                this._nearLightOuterColor = value;

                this.material.SetColor("_ProximityLightOuterColorOverride", Utils.stringToColor(value));

                this.setRenderEnable();
            }
        }
        /*--------------------------------------------定义borderRadius、borderWidth属性支撑方法结束-------------------------------------------*/


        private float _borderWidth = 0f;
        /// <summary>
        /// 边框宽度
        /// </summary>
        public virtual float borderWidth
        {
            get
            {
                return this._borderWidth;
            }

            set
            {
                this._borderWidth = value;

                // 获取最短的边
                var isPanel = (this is PanelNode) ? ((PanelNode)this).isPanel : false;
                string minSide = "width";
                var min = this.width;
                if (Utils.noExceed(this.height, min))
                {
                    min = this.height;
                    minSide = "height";
                }
                if (!isPanel && !Utils.equals(this.depth, 0f) && Utils.noExceed(this.depth, min))
                {
                    minSide = "depth";
                }


                // 计算边框scale并设值
                if (minSide == "width")
                {
                    var w = Space.Unit.unitToScale(this, value * 2, Axle.right) / this.parasitifer.transform.localScale.x;

                    if (!float.IsNaN(w) && !float.IsInfinity(w))
                    {
                        this.material.SetFloat("_BorderWidth", w);
                    }
                }
                else if (minSide == "height")
                {
                    var h = Space.Unit.unitToScale(this, value * 2, Axle.up) / this.parasitifer.transform.localScale.y;

                    if (!float.IsNaN(h) && !float.IsInfinity(h))
                    {
                        this.material.SetFloat("_BorderWidth", h);
                    }
                }
                else if (minSide == "depth")
                {
                    var z = Space.Unit.unitToScale(this, value * 2, Axle.forward) / this.parasitifer.transform.localScale.z;

                    if (!float.IsNaN(z) && !float.IsInfinity(z))
                    {
                        this.material.SetFloat("_BorderWidth", z);
                    }
                }

                this.setRenderEnable();
            }
        }

        private float _borderRadius = 0f;
        /// <summary>
        /// 边框半径
        /// </summary>
        public virtual float borderRadius
        {
            get
            {
                return this._borderRadius;
            }

            set
            {
                // 设置是否启用圆角(圆角为0直接关闭)
                if (Utils.equals(value, 0f))
                {
                    this.material.DisableKeyword("_ROUND_CORNERS");
                }
                else
                {
                    this.material.EnableKeyword("_ROUND_CORNERS");
                }

                // 获取最短的边
                var isPanel = (this is PanelNode) ? ((PanelNode)this).isPanel : false;
                string minSide = "width";
                var min = this.width;
                if (Utils.noExceed(this.height, min))
                {
                    min = this.height;
                    minSide = "height";
                }
                if (!isPanel && !Utils.equals(this.depth, 0f) && Utils.noExceed(this.depth, min))
                {
                    minSide = "depth";
                }

                // 计算圆角scale并设值
                if (minSide == "width")
                {
                    var w = Space.Unit.unitToScale(this, value * 2, Axle.right) / this.parasitifer.transform.localScale.x;

                    if (!float.IsNaN(w) && !float.IsInfinity(w))
                    {
                        this.material.SetFloat("_RoundCornerRadius", w);
                    }
                }
                else if (minSide == "height")
                {
                    var h = Space.Unit.unitToScale(this, value * 2, Axle.up) / this.parasitifer.transform.localScale.y;

                    if (!float.IsNaN(h) && !float.IsInfinity(h))
                    {
                        this.material.SetFloat("_RoundCornerRadius", h);
                    }
                }
                else if (minSide == "depth")
                {
                    var z = Space.Unit.unitToScale(this, value * 2, Axle.forward) / this.parasitifer.transform.localScale.z;

                    if (!float.IsNaN(z) && !float.IsInfinity(z))
                    {
                        this.material.SetFloat("_RoundCornerRadius", z);
                    }
                }

                this._borderRadius = value;

                this.setRenderEnable();
            }
        }

        /// <summary>
        /// 对齐锚（用于浮动)
        /// </summary>
        public virtual Anchor alignAnchor
        {
            get
            {
                return this._alignAnchor;
            }
        }

        /// <summary>
        /// 锚集合
        /// </summary>
        public virtual List<Anchor> anchors
        {
            get
            {
                var result = new List<Anchor>();

                foreach (var item in this._anchorCollection)
                {
                    result.Add(item.Value);
                }

                return result;
            }
        }
        /*---------------------------------------------定义horizontalFloat属性支撑方法开始---------------------------------------------*/
        /// <summary>
        /// 获取节点自身水平对齐模式
        /// </summary>
        /// <returns></returns>
        protected AlignMode getHorizontal()
        {
            return this._horizontal;
        }

        /// <summary>
        /// 设置节点自身水平对齐模式
        /// </summary>
        /// <param name="horizontal"></param>
        protected void setHorizontal(AlignMode alignMode)
        {
            this._horizontal = alignMode;

            if (this.parent != null)
            {
                this.horizontalFresh();
            }
        }

        /// <summary>
        /// 水平fudon刷新
        /// </summary>
        public virtual void horizontalFresh()
        {
            // 当parent == null时说明这个节点在顶层、 当this.parent.alignAnchor == null时有两种情况一种是没有parent节点在顶层，另一种是上一层为魔法节点但魔法节点为顶层节点，所以这两种情况都在顶层所以没有该节点的浮动布局
            if (this.parent == null || this.parent.alignAnchor == null)
            {
                return;
            }

            if (this.getHorizontal() != AlignMode.none)
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

                if (this.getHorizontal() == AlignMode.none)
                {
                    this.parent.alignAnchor.x = null;
                }
                if (this.getHorizontal() == AlignMode.center || this.getHorizontal() == AlignMode.between)
                {
                    this.parent.alignAnchor.x = 0f + Space.Unit.unitToScale(this, this.offset.x, Axle.right);
                }
                if (this.getHorizontal() == AlignMode.left)
                {
                    var width = Space.Unit.unitToScale(computeNode, this.size.width, Axle.right);

                    this.parent.alignAnchor.x = -1 / 2f + width / 2f + Space.Unit.unitToScale(computeNode, this.offset.x, Axle.right);
                }
                if (this.getHorizontal() == AlignMode.right)
                {
                    var width = Space.Unit.unitToScale(computeNode, this.size.width, Axle.right);

                    this.parent.alignAnchor.x = 1 / 2f - width / 2f + Space.Unit.unitToScale(computeNode, this.offset.x, Axle.right);
                }

                this.parent.alignAnchor.fresh();
            }
        }
        /*---------------------------------------------定义horizontalFloat属性支撑方法结束---------------------------------------------*/

        /// <summary>
        /// 自身水平对齐方式
        /// </summary>
        public virtual string horizontal
        {
            get
            {
                return this.getHorizontal().ToString();
            }

            set
            {
                if (value == "none")
                {
                    this.setHorizontal(AlignMode.none);
                }

                if (value == "center")
                {
                    this.setHorizontal(AlignMode.center);
                }

                if (value == "left")
                {
                    this.setHorizontal(AlignMode.left);
                }

                if (value == "right")
                {
                    this.setHorizontal(AlignMode.right);
                }

                // 子节点对齐刷新（浮动会使节点离开内容，所以浮动后要对父节点进行对齐刷新）
                if (this is PanelLayer)
                {
                    ((PanelLayer)this.parent)?.contentAlignFresh();
                }
            }
        }

        /// <summary>
        /// 自身水平浮动方式
        /// </summary>
        public virtual string horizontalFloat
        {
            get
            {
                return this.horizontal;
            }

            set
            {
                this.horizontal = value;
            }
        }


        /*---------------------------------------------定义verticalFloat属性支撑方法开始---------------------------------------------*/
        /// <summary>
        /// 获取自身水平对齐模式
        /// </summary>
        /// <returns></returns>
        protected AlignMode getVertical()
        {
            return this._vertical;
        }

        /// <summary>
        /// 设置自身水平对齐模式
        /// </summary>
        /// <param name="alignMode"></param>
        protected void setVertical(AlignMode alignMode)
        {
            this._vertical = alignMode;
            if (this.parent != null)
            {
                this.verticalFresh();
            }
        }

        /// <summary>
        /// 垂直浮动刷新
        /// </summary>
        public virtual void verticalFresh()
        {
            // 当parent == null时说明这个节点在顶层、 当this.parent.alignAnchor == null时有两种情况一种是没有parent节点在顶层，另一种是上一层为魔法节点但魔法节点为顶层节点，所以这两种情况都在顶层所以没有该节点的浮动布局
            if (this.parent == null || this.parent.alignAnchor == null)
            {
                return;
            }

            if (this.getVertical() != AlignMode.none)
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

                if (this.getVertical() == AlignMode.none)
                {
                    this.parent.alignAnchor.y = null;
                }
                if (this.getVertical() == AlignMode.center || this.getVertical() == AlignMode.between)
                {
                    this.parent.alignAnchor.y = 0f + Space.Unit.unitToScale(this, this.offset.y, Axle.up);
                }
                if (this.getVertical() == AlignMode.left)
                {
                    var height = Space.Unit.unitToScale(computeNode, this.localBounds.size.y, Axle.up);

                    this.parent.alignAnchor.y = -1 / 2f + height / 2f + Space.Unit.unitToScale(computeNode, this.offset.y, Axle.up);
                }
                if (this.getVertical() == AlignMode.right)
                {
                    var height = Space.Unit.unitToScale(computeNode, this.localBounds.size.y, Axle.up);
                    this.parent.alignAnchor.y = 1 / 2f - height / 2f + Space.Unit.unitToScale(computeNode, this.offset.y, Axle.up);
                }

                this.parent.alignAnchor.fresh();
            }
        }
        /*---------------------------------------------定义verticalFloat属性支撑方法结束---------------------------------------------*/

        /// <summary>
        /// 自身垂直对齐方式
        /// </summary>
        public virtual string vertical
        {
            get
            {
                string result = "none";
                if (this._vertical.ToString() == "right")
                {
                    result = "top";
                }
                if (this._vertical.ToString() == "center")
                {
                    result = "center";
                }
                if (this._vertical.ToString() == "left")
                {
                    result = "bottom";
                }
                return result;
            }

            set
            {
                if (value == "none")
                {
                    this.setVertical(AlignMode.none);
                }

                if (value == "center")
                {
                    this.setVertical(AlignMode.center);
                }

                if (value == "bottom")
                {
                    this.setVertical(AlignMode.left);
                }

                if (value == "top")
                {
                    this.setVertical(AlignMode.right);
                }

                // 子节点对齐刷新（浮动会使节点离开内容，所以浮动后要对父节点进行对齐刷新）
                if (this is PanelLayer)
                {
                    ((PanelLayer)this.parent)?.contentAlignFresh();
                }
            }
        }

        /// <summary>
        /// 自身垂直浮动
        /// </summary>
        public virtual string verticalFloat
        {
            get
            {
                return this.vertical;
            }

            set
            {
                this.vertical = value;
            }
        }

        private string _stylesheet = "";
        /// <summary>
        /// 样式
        /// </summary>
        public virtual string stylesheet
        {
            get
            {
                return this._stylesheet.Trim();
            }

            set
            {
                this._stylesheet = value.Trim();

                // 获取样式名数组
                List<string> classNameList = new List<string>();
                if (value != null && this._stylesheet != "")
                {
                    var temp = this._stylesheet.Split(' ');
                    foreach (var item in temp)
                    {
                        if (item.Trim() != "")
                        {
                            classNameList.Add(item);
                        }
                    }
                }

                // 逐一对样式设置
                for (var i = 0; i < classNameList.Count; i++)
                {
                    this.component.setStyle(this, classNameList[i]);
                }
            }
        }


        /*
         *  事件相关
         */
        /// <summary>
        /// 阻止触发onPositionChange事件
        /// </summary>
        public virtual bool preventPositionEvent
        {
            set
            {
                if (value == true)
                {
                    this.previousPosition.x = this.position.x;
                    this.previousPosition.y = this.position.y;
                    this.previousPosition.z = this.position.z;
                }
            }
        }

        /// <summary>
        /// 阻止触发onSizeChange事件
        /// </summary>
        public virtual bool preventSizeEvent
        {
            set
            {
                if (value == true)
                {
                    this.previousSize.width = this.size.width;
                    this.previousSize.height = this.size.height;
                    this.previousSize.depth = this.size.depth;
                }
            }
        }


        /// <summary>
        /// 阻止旋转事件
        /// </summary>
        public virtual bool preventRotationEvent
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
        /// 是否有Collider
        /// </summary>
        public virtual bool hasCollider
        {
            get
            {
                return this.parasitifer.GetComponent<BoxCollider>() != null;
            }
        }

        /// <summary>
        /// 是否有Collider
        /// </summary>
        public virtual bool hasInteractionTouchable
        {
            get
            {
                return false;
            }
        }

        /*
         * npc相关
         */
        /// <summary>
        /// npc
        /// </summary>
        public virtual NPC npc
        {
            get
            {
                return this._npc;
            }
        }

        private string _npcPath = "";
        /// <summary>
        /// NPC psth
        /// </summary>
        public virtual string npcPath
        {
            get
            {
                return this._npcPath;
            }

            set
            {
                var isPanel = this is PanelNode ? ((PanelNode)this).isPanel : false;

                if (!isPanel && this._npc == null && value != "")
                {
                    if (value != this._npcPath)
                    {
                        if (this.npc != null)
                        {
                            NPC.npcDictionary.Remove(this.npc.parasitifer);
                            this.npc.destory();
                        }

                        this._npcPath = value;
                        this._npc = new NPC(Resources.Load<GameObject>(value), (ISizeFeature)this);

                        this.npcOffset = this._npcOffset;
                    }
                }
            }
        }

        /// <summary>
        /// 节点操作默认配制
        /// </summary>
        protected virtual ManipulatorConfig manipulatorConfig
        {
            get
            {
                return new ManipulatorConfig
                {
                    colliderCenter = new Vector3(0, 0, 0),
                    colliderSize = new Vector3(1, 1, 1)
                };
            }
        }

        private NodeManipulator _nodeManipulator;
        /// <summary>
        /// 是否允许节点被操纵
        /// </summary>
        public virtual bool enableManipulator
        {
            get
            {
                return this._nodeManipulator != null ? this._nodeManipulator.enable : false;
            }

            set
            {

                if (value && this._nodeManipulator == null)
                {
                    this._nodeManipulator = new NodeManipulator(this, this.manipulatorConfig);
                    this._nodeManipulator.fresh();
                }

                this._nodeManipulator.enable = value;
            }
        }



        /// <summary>
        /// 事件中的Collider组件是否自动添加和回收
        /// </summary>
        public virtual bool collider
        {
            get
            {
                return this._collider;
            }

            set
            {
                if(value)
                {
                    if (this.autoCollider == null)
                    {
                        this.addAutoCollider();
                    }
                }

                this._collider = value;
            }
        }

        /// <summary>
        /// 触摸事件需要的InteractionTouchable组件是否自动添加和回收
        /// </summary>
        public virtual bool interactionTouchableAuto
        {
            get
            {
                return this._interactionTouchableAuto;
            }

            set
            {
                this._interactionTouchableAuto = value;
            }
        }
        #endregion

        #region 事件
        /// <summary>
        /// 位置改变事件
        /// </summary>
        protected readonly PositionChangeEvent _onPositionChange = new PositionChangeEvent();

        /// <summary>
        /// 尺寸改变事件
        /// </summary>
        protected readonly SizeChangeEvent _onSizeChange = new SizeChangeEvent();

        /// <summary>
        /// 旋转量改变事件
        /// </summary>
        protected readonly RotationChangeEvent _onRotationChange = new RotationChangeEvent();

        protected readonly DownEvent _onDown;
        /// <summary>
        /// 按下事件
        /// </summary>
        public virtual DownEvent onDown
        {
            get
            {
                return this._onDown;
            }
        }

        protected readonly UpEvent _onUp;
        /// <summary>
        /// 释放事件
        /// </summary>
        public virtual UpEvent onUp
        {
            get
            {
                return this._onUp;
            }
        }

        protected readonly ClickEvent _onClick;
        /// <summary>
        /// 单击事件
        /// </summary>
        public virtual ClickEvent onClick
        {
            get
            {
                return this._onClick;
            }
        }

        protected readonly DraggedEvent _onDragged;
        /// <summary>
        /// 拖动事件
        /// </summary>
        public virtual DraggedEvent onDragged
        {
            get
            {
                return this._onDragged;
            }
        }

        protected readonly TouchStartedEvent _onTouchStarted;
        /// <summary>
        /// 触摸开始事件
        /// </summary>
        public virtual TouchStartedEvent onTouchStarted
        {
            get
            {
                return this._onTouchStarted;
            }
        }

        protected readonly TouchUpdateEvent _onTouchUpdate;
        /// <summary>
        /// 触摸更新事件
        /// </summary>
        public virtual TouchUpdateEvent onTouchUpdate
        {
            get
            {
                return this._onTouchUpdate;
            }
        }

        protected readonly TouchCompletedEvent _onTouchCompleted;
        /// <summary>
        /// 触摸完成事件
        /// </summary>
        public virtual TouchCompletedEvent onTouchCompleted
        {
            get
            {
                return this._onTouchCompleted;
            }
        }

        protected readonly AppendEvent _onAppend = new AppendEvent();
        /// <summary>
        /// 添加节点前事件
        /// </summary>
        public virtual AppendEvent onAppend
        {
            get
            {
                return this._onAppend;
            }
        }

        protected readonly AppendedEvent _onAppended = new AppendedEvent();
        /// <summary>
        /// 添加节点完成事件
        /// </summary>
        public virtual AppendedEvent onAppended
        {
            get
            {
                return this._onAppended;
            }
        }

        protected readonly InsertEvent _onInsert = new InsertEvent();
        /// <summary>
        /// 插入节点前事件
        /// </summary>
        public virtual InsertEvent onInsert
        {
            get
            {
                return this._onInsert;
            }
        }

        protected readonly InsertedEvent _onInserted = new InsertedEvent();
        /// <summary>
        /// 插入节点完成事件
        /// </summary>
        public virtual InsertedEvent onInserted
        {
            get
            {
                return this._onInserted;
            }
        }

        protected readonly DestoryEvent _onDestory = new DestoryEvent();
        /// <summary>
        /// 销毁节点事件
        /// </summary>
        public virtual DestoryEvent onDestory
        {
            get
            {
                return this._onDestory;
            }
        }

        protected readonly DestoryedEvent _onDestoryed = new DestoryedEvent();
        /// <summary>
        /// 销毁节点完成事件
        /// </summary>
        public virtual DestoryedEvent onDestoryed
        {
            get
            {
                return this._onDestoryed;
            }
        }


        protected readonly FocusEnterEvent _onFocusEnter;
        /// <summary>
        /// 焦点进入事件
        /// </summary>
        public virtual FocusEnterEvent onFocusEnter
        {
            get
            {
                return this._onFocusEnter;
            }
        }

        protected readonly FocusChangedEvent _onFocusChanged;
        /// <summary>
        /// 焦点改变事件
        /// </summary>
        public virtual FocusChangedEvent onFocusChanged
        {
            get
            {
                return this._onFocusChanged;
            }
        }

        protected readonly FocusExitEvent _onFocusExit;
        /// <summary>
        /// 焦点退出事件
        /// </summary>
        public virtual FocusExitEvent onFocusExit
        {
            get
            {
                return this._onFocusExit;
            }
        }
        #endregion

        #region 基本方法
        /// <summary>
        /// 添加内容方法 （通过该方法可以安全的将GameObject加入到节点层级中)
        /// </summary>
        /// <param name="current">要添加的节点</param>
        public virtual void addCentent(Node current)
        {
            Space.GameObjectEntity.appendNode(current.parasitifer, this.parasitifer);
        }

        /// <summary>
        /// 移除节点内容
        /// </summary>
        /// <param name="node"></param>
        protected virtual void removeContent(Node node)
        {
            UnityEngine.Object.Destroy(node.parasitifer);
        }

        /*-----------------------------------------定义节点事件检测相关方法开始----------------------------------------*/
        /// <summary>
        /// 检查节点是否包含Pointer事件
        /// </summary>
        /// <returns></returns>
        public virtual bool checkNodeIncludePointerEvent()
        {
            bool result = false;
            if (this is IPointerEventFeature)
            {
                var iPointerEventNode = (IPointerEventFeature)this;
                result = iPointerEventNode.onClick.listenerCount > 0 || iPointerEventNode.onDown.listenerCount > 0 || iPointerEventNode.onUp.listenerCount > 0 || iPointerEventNode.onDragged.listenerCount > 0;
            }

            return result;
        }

        /// <summary>
        /// 检查节点是否包含Touch事件
        /// </summary>
        /// <returns></returns>
        public virtual bool checkNodeIncludeTouchEvent()
        {

            bool result = false;
            if (this is ITouchEventFeature)
            {
                var iTouchEventNode = (ITouchEventFeature)this;
                result = iTouchEventNode.onTouchStarted.listenerCount > 0 || iTouchEventNode.onTouchUpdate.listenerCount > 0 || iTouchEventNode.onTouchCompleted.listenerCount > 0; ;
            }

            return result;
        }

        /// <summary>
        /// 检查节点是否包含Focus事件
        /// </summary>
        /// <returns></returns>
        public virtual bool checkNodeIncludeFocusEvent()
        {
            bool result = false;
            if (this is IFocusEventFeature)
            {
                var iFocusEventNode = (IFocusEventFeature)this;
                result = iFocusEventNode.onFocusExit.listenerCount > 0 || iFocusEventNode.onFocusChanged.listenerCount > 0 || iFocusEventNode.onFocusEnter.listenerCount > 0;
            }

            return result;
        }

        /// <summary>
        /// 检查节点是否包含Collider事件
        /// </summary>
        /// <returns></returns>
        public bool checkNodeIncludeColliderEvent()
        {
            bool result = false;
            if (this is ICollisionEventFeature)
            {
                var iColliderEventNode = (ICollisionEventFeature)this;
                result = iColliderEventNode.onCollisionEnter.listenerCount > 0 || iColliderEventNode.onCollisionExit.listenerCount > 0 || iColliderEventNode.onCollisionStay.listenerCount > 0;
            }

            return result;
        }

        /// <summary>
        /// 检查节点是否不包含Pointer事件
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public virtual bool checkNodeNotIncludePointerEvent()
        {
            bool result = false;
            if (this is IPointerEventFeature)
            {
                var iPointerEventNode = (IPointerEventFeature)this;
                result = iPointerEventNode.onClick.listenerCount == 0 && iPointerEventNode.onDown.listenerCount == 0 && iPointerEventNode.onUp.listenerCount == 0 && iPointerEventNode.onDragged.listenerCount == 0;
            }

            return result;
        }

        /// <summary>
        /// 检查节点是否不包含Touch事件
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public virtual bool checkNodeNotIncludeTouchEvent()
        {

            bool result = false;
            if (this is ITouchEventFeature)
            {
                var iTouchEventNode = (ITouchEventFeature)this;
                result = iTouchEventNode.onTouchStarted.listenerCount == 0 && iTouchEventNode.onTouchUpdate.listenerCount == 0 && iTouchEventNode.onTouchCompleted.listenerCount == 0; ;
            }

            return result;
        }

        /// <summary>
        /// 检查节点是否不包含Focus事件
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public virtual bool checkNodeNotIncludeFocusEvent()
        {
            bool result = false;
            if (this is IFocusEventFeature)
            {
                var iFocusEventNode = (IFocusEventFeature)this;
                result = iFocusEventNode.onFocusExit.listenerCount == 0 && iFocusEventNode.onFocusEnter.listenerCount == 0 && iFocusEventNode.onFocusChanged.listenerCount == 0;
            }

            return result;
        }

        /// <summary>
        /// 检查节点是否不包含Collider事件
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool checkNodeNotIncludeColliderEvent()
        {
            bool result = false;
            if (this is ICollisionEventFeature)
            {
                var iColliderEventNode = (ICollisionEventFeature)this;
                result = iColliderEventNode.onCollisionEnter.listenerCount == 0 && iColliderEventNode.onCollisionExit.listenerCount == 0 && iColliderEventNode.onCollisionStay.listenerCount == 0;
            }

            return result;
        }
        /*-----------------------------------------定义节点事件检测相关方法结束----------------------------------------*/


        /*---------------------------------------定义尺寸相关的行为的相关方法开始--------------------------------------*/
        /// <summary>
        /// 回滚当前位置
        /// </summary>
        public virtual void revertPosition()
        {
            this.position.x = this.previousPosition.x;
            this.position.y = this.previousPosition.y;
            this.position.z = this.previousPosition.z;
        }

        /// <summary>
        /// 回滚当前尺寸
        /// </summary>
        public virtual void revertSize()
        {
            this.size.width = this.previousSize.width;
            this.size.height = this.previousSize.height;
            this.size.depth = this.previousSize.depth;
        }

        /// <summary>
        /// 回滚旋转量
        /// </summary>
        public virtual void revertRotation()
        {
            this.rotation.xAngle = this.previousRotation.xAngle;
            this.rotation.yAngle = this.previousRotation.yAngle;
            this.rotation.zAngle = this.previousRotation.zAngle;
        }

        public virtual void sizeFresh()
        {
            ;
        }

        public virtual void unlockSizeSetedTag()
        {
            ;
        }

        public virtual void positionFresh()
        {
            ;
        }

        public virtual void borderFresh()
        {
            this.borderWidth = this._borderWidth;
            this.borderRadius = this._borderRadius;
        }
        /*---------------------------------------定义尺寸相关的行为的相关方法结束--------------------------------------*/

        /// <summary>
        /// npc刷新
        /// </summary>
        public virtual void npcFresh()
        {
            this._npc?.fresh();
        }


        /*-----------------------------------------定义锚行为的相关方法开始-----------------------------------------*/
        /// <summary>
        /// 添加锚
        /// </summary>
        /// <param name="name">锚名称</param>
        /// <param name="anchor">要添加的锚对象</param>
        public virtual void addAnchor(string name, Anchor anchor)
        {
            if (name == null || name.Trim() == "")
            {
                name = Guid.NewGuid().ToString();
            }

            if (this._anchorCollection.ContainsKey(name))
            {
                Debug.LogError("节点中锚的名称不能重复");
                return;
            }

            this._anchorCollection.Add(name, anchor);
        }

        /// <summary>
        /// 获取锚列表
        /// </summary>
        /// <returns></returns>
        public List<Anchor> getAnchorList()
        {
            List<Anchor> result = new List<Anchor>();
            foreach(var item in this._anchorCollection)
            {
                result.Add(item.Value);
            }
            return result;
        }

        /// <summary>
        /// 获取锚
        /// </summary>
        /// <param name="name">要查询的锚的名称</param>
        /// <returns></returns>
        public virtual Anchor getAnchor(string name)
        {
            Anchor result = null;
            if (name != null && name != "" && this._anchorCollection.ContainsKey(name))
            {
                result = this._anchorCollection[name];
            }
            return result;
        }

        /// <summary>
        /// 删除锚
        /// </summary>
        /// <param name="name">要移除的锚的名称</param>
        /// <returns></returns>
        public virtual Anchor removeAnchor(string name)
        {
            Anchor result = null;
            if (name != null && name != "" && this._anchorCollection.ContainsKey(name))
            {
                result = this._anchorCollection[name];

                this._anchorCollection.Remove(name);
            }
            return result;
        }
        /*-----------------------------------------定义锚行为的相关方法结束-----------------------------------------*/


        /*---------------------------------------定义Collider行为的相关方法开始--------------------------------------*/
        /// <summary>
        /// 添加碰撞体
        /// </summary>
        /// <param name="center">中心点</param>
        /// <param name="size">尺寸 (比例)</param>
        /// <param name="name">名称</param>
        /// <returns></returns>
        public virtual BoxCollider addCollider(Vector3 center, Vector3 size, string name)
        {
            BoxCollider result = this.parasitifer.AddComponent<BoxCollider>();

            result.center = center;
            result.size = size;

            if (name != null && name.Trim() != "")
            {
                boxColliderDictionary.Add(name, result);
            }
            
            return result;
        }

        /// <summary>
        /// 移除BoxCollider
        /// </summary>
        /// <param name="collider">要移除的collider</param>
        public virtual void removeCollider(BoxCollider collider)
        {
            BoxCollider result = null;
            foreach (var item in boxColliderDictionary)
            {
                if (item.Value == collider)
                {
                    result = item.Value;
                    this.boxColliderDictionary.Remove(item.Key);
                }
            }

            if(result != null)
            {
                GameObject.Destroy(collider);
            }
        }

        /// <summary>
        /// 移除BoxCollider
        /// </summary>
        /// <param name="name">名称</param>
        public virtual void removeCollider(string name)
        {
            BoxCollider collider = null;
            if (boxColliderDictionary.ContainsKey(name))
            {
                collider = boxColliderDictionary[name];
                boxColliderDictionary.Remove(name);
            }

            if(collider != null)
            {
                GameObject.Destroy(collider);
            }
        }

        /// <summary>
        /// 获取collider
        /// </summary>
        /// <param name="name">要获取的collider名称</param>
        /// <returns></returns>
        public virtual BoxCollider findColliderByName(string name)
        {
            BoxCollider result = null;

            if (boxColliderDictionary.ContainsKey(name))
            {
                result = boxColliderDictionary[name];
            }

            return result;
        }

        /// <summary>
        /// 添加autoCollider
        /// </summary>
        public virtual void addAutoCollider()
        {

        }

        /// <summary>
        /// 自动回收collider
        /// </summary>
        protected virtual void recoveryAutoCollider()
        {

        }
        /*---------------------------------------定义Collider行为的相关方法结束--------------------------------------*/

        /// <summary>
        /// 自动添加Touchable组件
        /// </summary>
        public virtual void addAutoInteractionTouchable()
        {

        }

        /// <summary>
        /// 自动回收Touchable组件
        /// </summary>
        protected virtual void recoveryAutoInteractionTouchable()
        {

        }

        /// <summary>
        /// 添加Touchable
        /// </summary>
        /// <param name="center">中心点</param>
        /// <param name="size">尺寸</param>
        /// <param name="name">名称</param>
        public virtual void addInteractionTouchable(string name, Vector3? center = null, Vector3? size = null)
        {

        }

        /// <summary>
        /// 移除InteractionTouchable
        /// </summary>
        /// <param name="interactionTouchable">要移除的InteractionTouchable</param>
        public virtual void removeInteractionTouchable(object interactionTouchable)
        {

        }

        /// <summary>
        /// 移除InteractionTouchable
        /// </summary>
        /// <param name="name">名称</param>
        public virtual void removeInteractionTouchable(string name)
        {

        }

        /*----------------------------------------定义动画行为的相关方法开始----------------------------------------*/
        /// <summary>
        /// 动画方法
        /// </summary>
        /// <param name="start">开始量</param>
        /// <param name="end">结束量</param>
        /// <param name="time">动画时长</param>
        /// <param name="curveType">曲线类型</param>
        /// <param name="action">动画行为[一个方法，比如修改节点的position]</param>
        /// <param name="callback">动画结束回调</param>
        public virtual void animation(Vector3 start, Vector3 end, float time, MotionCurve curveType, AnimationAction action, AnimationCallback callback = null)
        {
            var m = new Movement(start, end, time, curveType, () =>
            {
                if(callback != null)
                {
                    callback();
                }
            });

            // 在每个周期都被执行直到动画结束
            this.service.next(() =>
            {
                action(m.data, m.isFinish);
                return m.isFinish;
            });
        }

        /// <summary>
        /// 动画方法
        /// </summary>
        /// <param name="start">开始量</param>
        /// <param name="end">结束量</param>
        /// <param name="time">动画时长</param>
        /// <param name="curveType">曲线类型</param>
        /// <param name="action">动画行为[一个方法，比如修改节点的position]</param>
        /// <param name="callback">动画结束回调</param>
        public virtual void animation(float start, float end, float time, MotionCurve curveType, AnimationActionFloat action, AnimationCallback callback = null)
        {
            var m = new Movement(new Vector3(start, 0f, 0f), new Vector3(end, 0f, 0f), time, curveType, () =>
            {
                if (callback != null)
                {
                    callback();
                }
            });

            // 在每个周期都被执行直到动画结束
            this.service.next(() =>
            {
                action(m.data.x, m.isFinish);
                return m.isFinish;
            });
        }
        /*----------------------------------------定义动画行为的相关方法结束----------------------------------------*/

        /*
         * emiter
         */
        /// <summary>
        /// 创建postion emitter (用于检测及触发节点位置改变事件)
        /// </summary>
        private void _createPositionEmitter()
        {
            this.service.createEmit(() =>
            {
                // 触发空间位置改变事件
                if (!Utils.equals(this.position.x, this.previousPosition.x) || !Utils.equals(this.position.y, this.previousPosition.y) || !Utils.equals(this.position.z, this.previousPosition.z))
                {
                    this._onPositionChange.Invoke(new PositionChangeEventData
                    {
                        target = this,
                        oldPosition = this.previousPosition.clone(),
                        oldSize = this.previousSize.clone(),
                        currentPosition = new PositionData(this.position.x, this.position.y, this.position.z)
                    });
                }
            }, () =>
            {
                // 设置节点移动行为状态
                if (!Utils.equals(this.position.x, this.previousPosition.x) || !Utils.equals(this.position.y, this.previousPosition.y) || !Utils.equals(this.position.z, this.previousPosition.z))
                {
                    this.isMoving = true;
                }
                else
                {
                    this.isMoving = false;
                }
            });

            this.service.lateNext(() =>
            {
                this.previousPosition = new PositionData(this.position.x, this.position.y, this.position.z);
                return false;
            });
        }

        /// <summary>
        /// 创建size emitter (用于检测及触发节点尺寸改变事件)
        /// </summary>
        private void _createSizeEmit()
        {
            this.service.createEmit(() =>
            {
                var isSizeChange =  !Utils.equals(this.size.width, this.previousSize.width) || !Utils.equals(this.size.height, this.previousSize.height);

                if(!isSizeChange && (!(this is PanelNode) || this is PanelNode && !((PanelNode)this).isPanel))
                {
                    isSizeChange = isSizeChange || !Utils.equals(this.size.depth, this.previousSize.depth);
                }

                // 触发空间尺寸改变事件
                if (isSizeChange)
                {
                    this._onSizeChange.Invoke(new SizeChangeEventData
                    {
                        target = this,
                        oldSize = this.previousSize.clone(),
                        currentSize = new SizeData(this.size.width, this.size.height, this.size.depth)
                    });
                }
            }, () =>
            {
                // 设置节点尺寸改变行为状态
                var isSizeChange = !Utils.equals(this.size.width, this.previousSize.width) || !Utils.equals(this.size.height, this.previousSize.height);

                if (!isSizeChange && (!(this is PanelNode) || this is PanelNode && !((PanelNode)this).isPanel))
                {
                    isSizeChange = isSizeChange || !Utils.equals(this.size.depth, this.previousSize.depth);
                }

                if (isSizeChange)
                {
                    this.isSizeChanging = true;

                    // 是否为深度改变
                    if (!Utils.equals(this.size.depth, this.previousSize.depth))
                    {
                        this._isDepthChanging = true;
                    } else
                    {
                        this._isDepthChanging = false;
                    }
                }
                else
                {
                    this.isSizeChanging = false;
                }
            });

            this.service.lateNext(() =>
            {
                this.previousSize = new SizeData(this.size.width, this.size.height, this.size.depth);
                return false;
            });
        }

        /// <summary>
        /// 创建rotation emitter (用于检测及触发节点旋转改变事件)
        /// </summary>
        private void _createRotationEmit()
        {
            this.service.createEmit(() =>
            {
                // 触发旋转量改变事件
                if (!Utils.equals(this.rotation.xAngle, this.previousRotation.xAngle) || !Utils.equals(this.rotation.yAngle, this.previousRotation.yAngle) || !Utils.equals(this.rotation.zAngle, this.previousRotation.zAngle))
                {
                    this._onRotationChange.Invoke(new RotationChangeEventData
                    {
                        target = this,
                        oldRotation = this.previousRotation.clone(),
                        currentRotation = new RotationData(this.rotation.xAngle, this.rotation.yAngle, this.rotation.zAngle)
                    });
                }
            }, () =>
            {
                // 设置节点旋转行为状态
                if (!Utils.equals(this.rotation.xAngle, this.previousRotation.xAngle) || !Utils.equals(this.rotation.yAngle, this.previousRotation.yAngle) || !Utils.equals(this.rotation.zAngle, this.previousRotation.zAngle))
                {
                    this.isRotating = true;
                }
                else
                {
                    this.isRotating = false;
                }
            });

            this.service.lateNext(() =>
            {
                this.previousRotation = new RotationData(this.rotation.xAngle, this.rotation.yAngle, this.rotation.zAngle);
                return false;
            });
        }
        #endregion

        #region 节点索引
        /*----------------------------------------定义节点查询的相关方法开始----------------------------------------*/
        /// <summary>
        /// 查找所有子节点
        /// </summary>
        /// <returns></returns>
        public List<Node> getChildNodes()
        {
            List<Node> result = new List<Node>();
            this.ergodic((node) =>
            {
                if (node == this)
                {
                    return true;
                }

                if (node.component == this.component)
                {
                    result.Add(node);
                }

                return true;
            });

            return result;
        }

        /// <summary>
        /// 查找所有子节点
        /// </summary>
        /// <typeparam name="T">要查询的节点类型</typeparam>
        /// <returns></returns>
        public List<T> getChildNodes<T>() where T : Node
        {
            List<T> result = new List<T>();

            var list = this.getChildNodes();
            foreach (var item in list)
            {
                if (typeof(T) == item.GetType())
                {
                    result.Add((T)item);
                }
            }

            return result;
        }

        /// <summary>
        /// 按Id查找子节点
        /// </summary>
        /// <returns></returns>
        public Node getChildNodeById(string id)
        {
            Node result = null;

            if (id != "" && id != null)
            {
                var list = getChildNodes();
                foreach (var item in list)
                {
                    if (item.id == id)
                    {
                        result = item;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 按Id查找子节点
        /// </summary>
        /// <typeparam name="T">要查询的节点类型</typeparam>
        /// <returns></returns>
        public T getChildNodeById<T>(string id) where T : Node
        {
            T result = null;

            var node = getChildNodeById(id);

            if (typeof(T) == node?.GetType())
            {
                result = (T)node;
            }

            return result;
        }


        /// <summary>
        /// 根据name获取Node
        /// </summary>
        /// <param name="name">要查询的节点名称</param>
        /// <returns></returns>
        public List<Node> getChildNodesByName(string name)
        {
            List<Node> result = new List<Node>();

            if (name != "" && name != null)
            {
                var list = getChildNodes();
                foreach (var item in list)
                {
                    if (item.name == name)
                    {
                        result.Add(item);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 根据name获取Node
        /// </summary>
        /// <typeparam name="T">要查询的节点类型</typeparam>
        /// <param name="name">要查询的节点名称</param>
        /// <returns></returns>
        public List<T> getChildNodesByName<T>(string name) where T : Node
        {
            List<T> result = new List<T>();
            var list = this.getChildNodesByName(name);

            foreach (var item in list)
            {
                if (typeof(T) == item.GetType())
                {
                    result.Add((T)item);
                }
            }

            return result;
        }
        /*----------------------------------------定义节点查询的相关方法结束----------------------------------------*/
        #endregion

        #region 组件索引
        /*----------------------------------------定义组件查询的相关方法开始----------------------------------------*/
        /// <summary>
        /// 查找所有子组件
        /// </summary>
        /// <returns></returns>
        public List<Component> getChildComponents()
        {
            List<Component> result = new List<Component>();

            List<Component> list = new List<Component>();
            foreach (Component item in this.component.children)
            {
                list.Add(item);
            }

            foreach (var item in list)
            {
                var rootNodeChilds = item.rootNodeChilds;
                if (rootNodeChilds.Count > 0)
                {
                    var node = rootNodeChilds[0].parent;
                    while (node != null)
                    {
                        if (node == this)
                        {
                            result.Add(item);
                            break;
                        }

                        node = node.parent;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 按名称查询组件
        /// </summary>
        /// <typeparam name="T">要查询的组件类型</typeparam>
        /// <returns></returns>
        public List<T> getChildComponents<T>() where T : Component
        {
            List<T> result = new List<T>();

            var list = this.getChildComponents();
            foreach (var item in list)
            {
                if (typeof(T) == item.GetType())
                {
                    result.Add((T)item);
                }
            }

            return result;
        }

        /// <summary>
        /// 按Id查找组件
        /// </summary>
        /// <returns></returns>
        public Component getChildComponentById(string id)
        {
            Component result = null;

            if (id != "" && id != null)
            {
                var list = this.getChildComponents();
                foreach (var item in list)
                {
                    if (item.id == id)
                    {
                        result = item;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 按Id查找组件
        /// </summary>
        /// <typeparam name="T">要查询的组件类型</typeparam>
        /// <returns></returns>
        public T getChildComponentById<T>(string id) where T : Component
        {
            T result = null;

            var compontent = getChildComponentById(id);

            if (typeof(T) == compontent?.GetType())
            {
                result = (T)compontent;
            }

            return result;
        }

        /// <summary>
        /// 按名称查询组件
        /// </summary>
        /// <param name="name">要查询的组件名称</param>
        /// <returns></returns>
        public List<Component> getChildComponentsByName(string name)
        {
            List<Component> result = new List<Component>();

            if (name != "" && name != null)
            {
                var list = this.getChildComponents();

                foreach (var item in list)
                {
                    if (item.name == name)
                    {
                        result.Add(item);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 按名称查询组件
        /// </summary>
        /// <typeparam name="T">要查询的组件类型</typeparam>
        /// <param name="name">要查询的组件名称</param>
        /// <returns></returns>
        public List<T> getChildComponentsByName<T>(string name) where T : Component
        {
            List<T> result = new List<T>();

            var list = this.getChildComponentsByName(name);
            foreach (var item in list)
            {
                if (typeof(T) == item.GetType())
                {
                    result.Add((T)item);
                }
            }

            return result;
        }
        /*----------------------------------------定义组件查询的相关方法结束----------------------------------------*/
        #endregion

        #region 元素索引
        /// <summary>
        /// 获取所有元素
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<Element> getChildElements()
        {
            List<Element> result = new List<Element>();

            var nodeList = this.getChildNodes();

            foreach (var item in nodeList)
            {
                result.Add((Element)item);
            }

            var componentList = this.getChildComponents();

            foreach (var item in componentList)
            {
                result.Add(item);
            }

            return result;
        }

        /// <summary>
        /// 按Id查找元素
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Element getChildElementById(string id)
        {
            Element result;
            result = (Element)this.getChildNodeById(id);

            if (result == null)
            {
                result = this.getChildComponentById(id);
            }

            return result;
        }

        /// <summary>
        /// 按名称查找子元素
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<Element> getChildElementsByName(string name)
        {
            List<Element> result = new List<Element>();

            var nodeList = this.getChildNodesByName(name);

            foreach (var item in nodeList)
            {
                result.Add((Element)item);
            }

            var componentList = this.getChildComponentsByName(name);

            foreach (var item in componentList)
            {
                result.Add(item);
            }

            return result;
        }
        #endregion

        #region 节点数据结构基本操作
        public new Node parent
        {
            get
            {
                var result = base.parent;
                return result != null ? (Node)result : null;
            }
        }

        /// <summary>
        /// 第一个子节点
        /// </summary>
        public new Node firstChild
        {
            get
            {
                var result = base.firstChild;
                return result != null ? (Node)result : null;
            }
        }

        /// <summary>
        /// 最后一个子节点
        /// </summary>
        public new Node lastChild
        {
            get
            {
                var result = base.lastChild;
                return result != null ? (Node)result : null;
            }
        }

        /// <summary>
        /// 前一个兄弟节点
        /// </summary>
        public new Node previousSibling
        {
            get
            {
                var result = base.previousSibling;
                return result != null ? (Node)result : null;
            }
        }

        /// <summary>
        /// 后一个兄弟节点
        /// </summary>
        public new Node nextSibling
        {
            get
            {
                var result = base.nextSibling;
                return result != null ? (Node)result : null;
            }
        }

        /// <summary>
        /// 子节点列表
        /// </summary>
        public new List<Node> children
        {
            get
            {
                var result = base.children;
                return result.ConvertAll(t => (Node)t);
            }
        }



        /// <summary>
        /// 父元素
        /// </summary>
        public Element parentElement
        {
            get
            {
                Element result = null;

                var parentNode = this.parent;

                // 父PanelLayer是一个组件(条件：parentNode.component != this.component代表父节点在另一个组件内）
                if (parentNode != null && parentNode.component != this.component)
                {
                    var component = parentNode.component;

                    // while component.parent != this.component 确保节点与组件在同一视图内， component.parent != null 确保while不进入死循环
                    while (component.parent != this.component && component.parent != null)
                    {
                        component = (CoreComponent)component.parent;
                    }

                    if(!(component is Component))
                    {
                        component = null;
                    }

                    result = (Element)component;
                }

                // 父PanelLayer是一个节点 (条件：parentNode.component != this.component代表父节点在另一个组件内）
                if (parentNode != null && parentNode.component == this.component)
                {
                    result = (Element)parentNode;
                }

                return result;
            }
        }

        /// <summary>
        /// 前一个兄弟元素
        /// </summary>
        public Element previousElement
        {
            get
            {
                Element result = null;

                var previousNode = this.previousSibling;

                // 前一个PanelLayer是一个组件
                if (previousNode != null && previousNode.component != this.component)
                {
                    var component = previousNode.component;
                    while (component.parent != this.component && component.parent != null)
                    {
                        component = (CoreComponent)component.parent;
                    }

                    if (!(component is Component))
                    {
                        component = null;
                    }

                    result = (Element)component;
                }

                // 前一个PanelLayer是一个节点 
                if (previousNode != null && previousNode.component == this.component)
                {
                    result = (Element)previousNode;
                }

                return result;
            }
        }

        /// <summary>
        /// 后一个兄弟元素
        /// </summary>
        public Element nextElement
        {
            get
            {
                Element result = null;

                var nextNode = this.nextSibling;

                // 后一个PanelLayer是一个组件
                if (nextNode != null && nextNode.component != this.component)
                {
                    var component = nextNode.component;
                    while (component.parent != this.component && component.parent != null)
                    {
                        component = (CoreComponent)component.parent;
                    }

                    if (!(component is Component))
                    {
                        component = null;
                    }

                    result = (Element)component;
                }

                // 后一个PanelLayer是一个节点 
                if (nextNode != null && nextNode.component == this.component)
                {
                    result = (Element)nextNode;
                }

                return result;
            }
        }

        /// <summary>
        /// 第一个子元素
        /// </summary>
        public Element firstChildElement
        {
            get
            {
                Element result = null;

                var list = this._ChildElments;
                foreach(var item in list)
                {
                    if(item.previousElement == null)
                    {
                        result = item;
                        break;
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// 最后一个子元素
        /// </summary>
        public Element lastChildElement
        {
            get
            {
                Element result = null;

                var list = this._ChildElments;
                foreach (var item in list)
                {
                    if (item.nextElement == null)
                    {
                        result = item;
                        break;
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// 子元素列表
        /// </summary>
        private List<Element> _ChildElments
        {
            get
            {
                List<Element> result = new List<Element>();

                var nodeList = this.getChildNodes();
                var componentList = this.getChildComponents();

                foreach(var item in nodeList)
                {
                    if (item.parent == this)
                    {
                        result.Add((Element)item);
                    }
                }

                foreach (var item in componentList)
                {
                    var rootNodeChilds = item.rootNodeChilds;
                    if (rootNodeChilds.Count > 0 && rootNodeChilds[0].parent == this)
                    {
                        result.Add(item);
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// 子元素列表
        /// </summary>
        public List<Element> childElements
        {
            get
            {
                List<Element> result = new List<Element>();

                var element = this.firstChildElement;
                while(element != null)
                {
                    result.Add(element);
                    element = element.nextElement;
                }

                return result;
            }
        }


        /// <summary>
        /// 插入子节点
        /// </summary>
        /// <param name="current">插入的节点</param>
        /// <param name="refNode">参照节点，该节点必须为直接子节点</param>
        /// <returns></returns>
        public virtual Node insertBefore(Node current, Node refNode)
        {
            /*
                以下逻辑用于校验是否跨文档插入节点
             */
            // 节点插入节点
            if ((this.component is Component && !((Component)this.component).isIncrease || this.component is Room) && !this.isIncreaseCompontent && current.component != this.component)
            {
                Debug.LogError(this + "节点在插入" + current + "节点时，该节点所在的定义与" + this + "不同");
                return null;
            }

            // 组件插入节点
            if (this.component is Component &&((Component)this.component).isIncrease && !this.isIncreaseCompontent && current.component != this.component.parent)
            {
                Debug.LogError(this.component + "组件在插入" + current + "节点时，该节点所在的定义与" + this.component + "不同");
                return null;
            }
            

            var result = base.insertBefore(current, refNode);

            // 如果该节点原先在根节点中则需要从rootChilds中将其移出
            if (result != null && current.component == this.component)
            {
                current.component.updateRemoveRootElement((Element)current);
            }

            // 返回插入结果
            return result != null ? (Node)result : null;
        }

        /// <summary>
        /// 插入子节点
        /// </summary>
        /// <param name="current">插入的节点</param>
        /// <param name="refComponent">参照组件，该节点必须为直接子节点</param>
        /// <returns></returns>
        public virtual Node insertBefore(Node current, Component refComponent)
        {
            // 获取插入的参照节点
            Node refNode = null;
            var refComponentRoot = ((Component)refComponent).rootNodeChilds;
            foreach (var item in refComponentRoot)
            {
                if (item.previousElement == null)
                {
                    refNode = item;
                    break;
                }
            }

            var result = this.insertBefore(current, refNode);

            // 返回插入结果
            return result != null ? (Node)result : null;
        }

        /// <summary>
        /// 插入元素
        /// </summary>
        /// <param name="current">要插入的组件</param>
        /// <param name="refElement">参照节点</param>
        /// <returns></returns>
        public virtual Component insertComponent(Component component, Node refNode)
        {
            /*
                以下逻辑校验是否跨文档插入组件
             */
            // 节点插入组件
            if ((this.component is Component && !((Component)this.component).isIncrease || this.component is Room) && component.parent != this.component)
            {
                Debug.LogError(this + "节点在插入" + component + "组件时，该组件所在的定义文档与" + this + "节点不同");
                return null;
            }

            // 组件插入组件
            if (this.component is Component && ((Component)this.component).isIncrease && component.parent != this.component.parent)
            {
                Debug.LogError(this.component + "组件在插入" + component + "组件时，该组件所在的定义文档与" + this.component + "组件不同");
                return null;
            }

            // 抛出组件插入前事件
            InsertEventData insertEventData = new InsertEventData
            {
                target = component,
            };
            onInsert.Invoke(insertEventData);

            // 使用默认行为插入节点
            if (!insertEventData.isPreventDefault)
            {
                // 向视图插入组件的各节点
                this.isIncreaseCompontent = true;
                foreach (var item in component.rootNodeChilds)
                {
                    if (this.insertBefore(item, refNode) == null)
                    {
                        throw new ComponentException("Failed to insert node within component");
                    }
                }
                this.isIncreaseCompontent = false;
            }

            // 如果该组件原先在根节点中则需要从rootChilds中将其移出
            if (component.parent == this.component)
            {
                component.parent.updateRemoveRootElement(component);
            }

            // 抛出组件插入完成事件
            InsertedEventData insertedEventData = new InsertedEventData
            {
                target = component,

                // 此处之所以可以直接为true是因为如果插入失败则直接抛出异常，根本执行不到这
                isSuccess = true,
            };
            onInserted.Invoke(insertedEventData);

            return component;
        }

        /// <summary>
        /// 插入元素
        /// </summary>
        /// <param name="current">要插入的组件</param>
        /// <param name="refElement">参照节点</param>
        /// <returns></returns>
        public virtual Component insertComponent(Component component, Component refComponent)
        {
            // 节点插入组件
            if ((this.component is Component && !((Component)this.component).isIncrease || this.component is Room) && component.parent != this.component)
            {
                Debug.LogError(this + "节点在插入" + component + "组件时，该组件所在的定义文档与" + this + "节点不同");
                return null;
            }

            // 组件插入组件
            if (this.component is Component && ((Component)this.component).isIncrease && component.parent != this.component.parent)
            {
                Debug.LogError(this.component + "组件在插入" + component + "组件时，该组件所在的定义文档与" + this.component + "组件不同");
                return null;
            }

            // 抛出组件插入前事件
            InsertEventData insertEventData = new InsertEventData
            {
                target = component,
            };
            onInsert.Invoke(insertEventData);


            // 使用默认行为插入节点
            if (!insertEventData.isPreventDefault)
            {
                Node refNode = null;
                foreach (var item in ((Component)refComponent).rootNodeChilds)
                {
                    if (item.previousElement == null)
                    {
                        refNode = item;
                        break;
                    }
                }

                this.isIncreaseCompontent = true;
                foreach (var item in component.rootNodeChilds)
                {
                    if(this.insertBefore((Node)item, (Node)refNode) == null)
                    {
                        throw new ComponentException("Failed to insert node within component");
                    }
                }
                this.isIncreaseCompontent = false;
            }

           
            // 如果该组件原先在根节点中则需要从rootChilds中将其移出
            if (component.parent == this.component)
            {
                component.parent.updateRemoveRootElement(component);
            }

            // 抛出组件插入完成事件
            InsertedEventData insertedEventData = new InsertedEventData
            {
                target = component,

                // 此处之所以可以直接为true是因为如果插入失败则直接抛出异常，根本执行不到这
                isSuccess = true
            };
            onInserted.Invoke(insertedEventData);

            return component;
        }

        /// <summary>
        /// 插入元素
        /// </summary>
        /// <param name="current">要插入的元素</param>
        /// <param name="refElement">参照元素</param>
        /// <returns></returns>
        public virtual Element insertElement(Element current, Element refElement)
        {
            Element result = null;

            if (refElement is Node)
            {
                if (current is Node)
                {
                    result = (Element)this.insertBefore((Node)current, (Node)refElement);
                }

                if (current is Component)
                {
                    result = this.insertComponent((Component)current, (Node)refElement);
                }
            }

            if (refElement is Component)
            {
                if (current is Node)
                {
                    result = (Element)this.insertBefore((Node)current, (Component)refElement);
                }

                if (current is Component)
                {
                    result = this.insertComponent((Component)current, (Component)refElement);
                }
            }

            return result;
        }



        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public virtual Node appendNode(Node current)
        {
            /*
             以下逻辑用于校验是否跨文档添加节点
             */
            // 节点添加节点 == 如果当前节点在某一组件内(this.component is Component)且组件不在新增状态(!((Component)this.component).isIncrease)或当前节点在room内(this.component is Room)同时当前节点新增组件状态为关闭状态(!this.isIncreaseCompontent)
            if (!current.isCreating && (this.component is Component && !((Component)this.component).isIncrease || this.component is Room) && !this.isIncreaseCompontent && current.component != this.component)
            {
                Debug.LogError(this + "节点在添加" + current + "节点时，该节点所在的定义与" + this + "不同");
                return null;
            }

            // 组件添加节点 == 如果当前节点在某一组件内(this.component is Component)且组件在新增状态(((Component)this.component).isIncrease)同时当前节点添加组件状态为关闭状态(!this.isIncreaseCompontent)
            if (!current.isCreating && this.component is Component && ((Component)this.component).isIncrease && !this.isIncreaseCompontent && current.component != this.component.parent)
            {
                Debug.LogError(this.component + "组件在添加" + current + "节点时，该节点所在的定义与" + this.component + "不同");
                return null;
            }
            

            var result = base.appendNode(current);

            if (result != null && current.component == this.component)
            {
                // 如果该节点原先在根节点中则需要从rootChilds中将其移出
                current.component.updateRemoveRootElement((Element)current);
            }

            return result != null ? (Node)result : null;
        }

        /// <summary>
        /// 添加子组件
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public virtual Component appendComponent(Component component)
        {
            /*
                以下逻辑校验是否跨文档添加组件
             */
            
            // 节点添加组件
            if (!component.isCreating && (this.component is Component && !((Component)this.component).isIncrease || this.component is Room) && component.parent != this.component)
            {
                Debug.LogError(this + "节点在添加" + component + "组件时，该组件所在的定义文档与" + this + "节点不同");
                return null;
            }

            // 组件添加组件
            if (!component.isCreating && this.component is Component && ((Component)this.component).isIncrease && component.parent != this.component.parent)
            {
                Debug.LogError(this.component + "组件在添加" + component + "组件时，该组件所在的定义文档与" + this.component + "组件不同");
                return null;
            }

            // 抛出组件添加前事件
            AppendEventData appendEventData = new AppendEventData
            {
                target = component
            };
            onAppend.Invoke(appendEventData);

            // 使用系统默认行为添加节点
            if (!appendEventData.isPreventDefault)
            {
                this.isIncreaseCompontent = true;
                foreach (var item in component.rootNodeChilds)
                {
                    if (this.appendNode(item) == null)
                    {
                        throw new ComponentException("Failed to insert node within component");
                    }
                }
                this.isIncreaseCompontent = false;
            }

            // 如果该组件原先在根节点中则需要从rootChilds中将其移出
            if (component.parent == this.component)
            {
                this.component.updateRemoveRootElement(component);
            }

            // 拙出组件添加完成事件
            AppendedEventData appendedEventData = new AppendedEventData
            {
                target = component,
                isSuccess = true
            };
            onAppended.Invoke(appendedEventData);

            return component;
        }


        /// <summary>
        /// 添加元素
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public virtual Element appendElement(Element element)
        {
            Element result = null;

            if (element is Node)
            {
                result = (Element)this.appendNode((Node)element);
            }

            if (element is Component)
            {
                result = this.appendComponent((Component)element);
            }

            return result;
        }

        /// <summary>
        /// 销毁节点
        /// </summary>
        /// <returns></returns>
        public virtual void destory()
        {
            // 设置当前节点的isParentElementDestoring状态
            var parentElement = this.parentElement;
            if (parentElement != null)
            {
                if (parentElement is CoreComponent)
                {
                    this._isParentElementDestoring = ((CoreComponent)parentElement).isDestoring;
                }

                if (parentElement is Node)
                {
                    this._isParentElementDestoring = ((Node)parentElement)._isParentElementDestoring;
                }
            }

            // 销毁子元素
            var childElements = this.childElements;
            foreach(var item in childElements)
            {
                item.destory();
            }

            /*
             以下逻辑用于清理内存中与该节点相关的引用
             */
            // 删除该节点中锚在compontent中属性关联
            foreach (var item in this.anchors)
            {
                this.component.delCorrelator(anchors);
            }

            // 删除该节点在compontent中属性关联
            this.component.delCorrelator(this);

            // 如果该节点原先在根节点中则需要从rootChilds中将其移出
            this.component.updateRemoveRootElement((Element)this);

            // 删除与该节点相关的所有手势
            Space.leftHand.removeGesture(this);
            Space.rightHand.removeGesture(this);

            // 删除Node的映射
            EMR.Space.delNodeMap(this);

            // 从节点树中移除该节点
            this.parent?.removeChild(this);

            // 从主服务中移除该节点的服务
            EMR.Space.mainService.removeService(this.service);

            // 删除节点游戏对象
            this.removeContent(this);
        }

        /// <summary>
        /// 遍历Node
        /// </summary>
        /// <param name="node"></param>
        public void ergodic(NodeHandler nodeHandler)
        {
            var result = nodeHandler(this);

            if(result)
            {
                foreach (Node item in this.children)
                {
                    item.ergodic(nodeHandler);
                }
            }
        }
        #endregion
    }
}
