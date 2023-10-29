using System;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Input;

using EMR.Common;
using EMR.Struct;
using EMR.Event;
using EMR.Layout;
using EMR.Plugin;

namespace EMR.Entity
{
    public delegate bool PanelNodeHandler(PanelNode node);

    /// <summary>
    /// 平面空间节点异常类
    /// </summary>
    public class PanelNodeException : ApplicationException
    {
        private string error;

        public PanelNodeException(string msg)
        {
            error = msg;
        }
    }

    /// <summary>
    /// 平面空间节点类
    /// </summary>
    public abstract class PanelNode : Node, IPanelCharcteristic, IDocumentModelModify, IFocusEventFeature, IPointerEventFeature, ITouchEventFeature, IScrollEventFeature, IManipulationEventFeature, Element
    {
        #region 基本字段
        // 旧滚动位置
        private PositionData _previousScrollPosition;

        // 顶点集合
        private Anchor[] _vertexs;

        // 与事件关联的InteractionTouchable
        public NearInteractionTouchable autoInteractionTouchable = null;

        /// <summary>
        /// InteractionTouchable集合
        /// </summary>
        protected Dictionary<string, NearInteractionTouchable> interactionTouchableDictionary = new Dictionary<string, NearInteractionTouchable>();
        #endregion

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
        /// <param name="depth">厚度</param>
        /// <param name="isPanel">是否平面</param>
        /// <param name="npc">npc</param>
        public PanelNode(float x, float y, float z, float xAngle, float yAngle, float zAngle, float width, float height, float depth, bool isPanel, NPC npc = null) : base(x, y, z, xAngle, yAngle, zAngle, width, height, depth, npc)
        {
            // 规定当传入时表演者为null并且厚度为0,侧认为是一个四边形
            if (isPanel)
            {
                this._isPanel = true;
                this.size.depth = 1000f;
            }

            // 滚动就绪事件
            this._onScrollReady = new ScrollReadyEvent();

            // 滚动事件
            this._onScroll = new ScrollEvent();

            // 旧滚动位置
            this._previousScrollPosition = new PositionData(0f, 0f, 0f);

            // 设置顶点集合
            this._vertexs = new Anchor[]
            {
                new Anchor(this, -0.5f, 0.5f, null),
                new Anchor(this, 0.5f, 0.5f, null),
                new Anchor(this, 0.5f, -0.5f, null),
                new Anchor(this, -0.5f, -0.5f, null),
                new Anchor(this, 0f, 0f, null)
            };

            // 生成滚动 event Emitter
            this.createScrollEventEmitter();

            // 一直在每个周期的末监控（用于节占尺寸改变、平移、滚动时对布局的调整）
            this.service.lateNext(() =>
            {
                if (this.isSizeChanging)
                {
                    PanelLayout.update(new PanelNodeActionInfo
                    {
                        current = this,
                        currentParent = (this.parent != null && this.parent is PanelNode) ? (PanelNode)this.parent : null,
                        action = PanelNodeAction.sizeChange
                    });
                } else if (this.isMoving)
                {
                    PanelLayout.update(new PanelNodeActionInfo
                    {
                        current = this,
                        currentParent = (this.parent != null && this.parent is PanelNode) ? (PanelNode)this.parent : null,
                        action = PanelNodeAction.positionChange
                    });
                } else if (this.isScrolling)
                {
                    PanelLayout.update(new PanelNodeActionInfo
                    {
                        current = this,
                        currentParent = (this.parent != null && this.parent is PanelNode) ? (PanelNode)this.parent : null,
                        action = PanelNodeAction.scroll
                    }); ;
                }

                return false;
            });

            // 一直在每个周期的末监控（用于执行节点滚动视口刷新）
            this.service.annexUpdate(() =>
            {
                // 当该节点需要视口刷新时:
                if (this._hasViewFresh)
                {
                    this?.scrollComponent.clipView(this._isContentBoundFresh, this._isScrollPositionReset);
                    this._hasViewFresh = false;
                    this._isContentBoundFresh = false;
                    this._isScrollPositionReset = false;
                } else if (this._isContentBoundFresh)
                {
                    this.scrollFresh(this._isScrollPositionReset);
                    this._isContentBoundFresh = false;
                    this._isScrollPositionReset = false;
                }

                return false;
            });

            // 添加样式
            this.styleCollect.add("overflow");
            this.styleCollect.add("color");
            this.styleCollect.add("fontSize");
            this.styleCollect.add("textHorizontal");
            this.styleCollect.add("textVertical");
            this.styleCollect.add("characterSpace");
            this.styleCollect.add("lineSpace");
            this.styleCollect.add("enableScroll");
            this.styleCollect.add("scrollDirection");
            this.styleCollect.add("horizontalLeftInterval");
            this.styleCollect.add("horizontalRightInterval");
        }

        /// <summary>
        /// 查找父滚动节点
        /// </summary>
        /// <param name="startNode"></param>
        /// <returns></returns>
        public static PanelNode findParentScrollNode(PanelNode startNode)
        {
            // 查找带滚动的父节点
            return (PanelNode)Node.findParentNodeByCondition(startNode, (Node current) =>
            {
                if (current is PanelNode && ((PanelNode)current).getOverflow() != NodeOverflow.visible)
                {
                    return true;
                }

                return false;
            });
        }

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
                return this.parasitifer.GetComponent<NearInteractionTouchable>() != null;
            }
        }

        public bool _isPanel = true;
        /// <summary>
        /// 是否平面
        /// </summary>
        public bool isPanel
        {
            get
            {
                return this._isPanel;
            }

            set
            {
                this._isPanel = value;

                if(value && this.component.isAssembling)
                {
                    this.size.depth = 1000f;
                }
            }
        }

        /// <summary>
        /// 节点所在panelRoot
        /// </summary>
        public PanelRoot panelRoot
        {
            get
            {
                PanelRoot result = null;
                PanelNode node = this;
                while (node != null)
                {
                    if ((node is PanelRoot))
                    {
                        result = (PanelRoot)node;
                        break;
                    }

                    var parent = node.parent;
                    if (!(parent is PanelNode))
                    {
                        break;
                    }
                    else
                    {
                        node = (PanelNode)parent;
                    }
                }

                return result;
            }
        }

        protected readonly List<PanelTextNode> _textNodeList = new List<PanelTextNode>();
        /// <summary>
        /// 文本节点集合
        /// </summary>
        public List<PanelTextNode> textNodeList
        {
            get
            {
                return this._textNodeList;
            }
        }

        /// <summary>
        /// 顶点
        /// </summary>
        public Anchor[] vertexs
        {
            get
            {
                return this._vertexs;
            }
        }


        /*
         *  以下属性与滚动相关
         */
        /// <summary>
        /// 滚动组件
        /// </summary>
        public PanelScroll scrollComponent
        {
            get
            {
                return this._scroll;
            }
        }

        public bool _isScrolling = false;
        /// <summary>
        /// 是否处于滚动
        /// </summary>
        public bool isScrolling
        {
            get
            {
                return this._isScrolling;
            }

            set
            {
                this._isScrolling = value;
            }
        }

        private bool _hasViewFresh = false;
        /// <summary>
        /// 是否需要视口刷新
        /// </summary>
        public bool hasViewFresh
        {
            get
            {
                return this._hasViewFresh;
            }

            set
            {
                this._hasViewFresh = value;
            }
        }

        private bool _isScrollPositionReset = false;
        /// <summary>
        /// 是否需要重置滚动位置 (滚动位置归零)
        /// </summary>
        public bool isScrollPositionReset
        {
            get
            {
                return this._isScrollPositionReset;
            }

            set
            {
                this._isScrollPositionReset = value;
            }
        }

        private bool _isContentBoundFresh = false;
        /// <summary>
        ///  是否需要刷新滚动组件的content bounds
        /// </summary>
        public bool isContentBoundFresh
        {
            get
            {
                return this._isContentBoundFresh;
            }

            set
            {
                this._isContentBoundFresh = value;
            }
        }

        /*
         * 以下属性用于定义相关显示模式
         */
        /*--------------------------------------------定义overflow属性支撑方法开始-------------------------------------------*/
        /// <summary>
        /// 获取节点溢出处理
        /// </summary>
        /// <returns></returns>
        public NodeOverflow getOverflow()
        {
            return this._overflow;
        }

        /// <summary>
        /// 设置节点溢出处理
        /// </summary>
        /// <param name="overflow"></param>
        public void setOverflow(NodeOverflow overflow)
        {
            if (overflow == NodeOverflow.hidden)
            {
                if (this.scrollComponent == null)
                {
                    this.enableScrollComponent = true;
                }
                this.scrollComponent.canScroll = false;
            }

            if (overflow == NodeOverflow.scroll)
            {
                if (this.scrollComponent == null)
                {
                    this.enableScrollComponent = true;
                }
                this.scrollComponent.canScroll = true;

                this.scrollDirection = this._scrollDirection;
            }

            if (overflow == NodeOverflow.visible)
            {
                if (this.scrollComponent != null)
                {
                    // 禁用滚动
                    this.scrollComponent.canScroll = false;

                    // 关闭裁切
                    this.scrollComponent.scrollCollection.ClipBox.enabled = false;
                }
            }

            this._overflow = overflow;
        }
        /*--------------------------------------------定义overflow属性支撑方法结束-------------------------------------------*/

        private NodeOverflow _overflow = NodeOverflow.visible;
        /// <summary>
        /// 节点溢出处理方式 (hidden | scroll | visible)
        /// </summary>
        public string overflow
        {
            get
            {
                return this._overflow.ToString();
            }
            set
            {
                // 对hidden模式处理
                if (value == "hidden")
                {
                    // 如果不存在scroll组件说明原先系统在visible模式切换到hidden中,该模式必须带有scroll组件，所以此处会在以下逻辑中使用setOverflow指令创建该组件
                    if (this._scroll == null)
                    {
                        // 通过setOverflow创建scroll组件
                        this.setOverflow(NodeOverflow.hidden);

                        // 销毁在节点上的autoCollider 然后将scroll上的collider给这个autoCollider
                        if(this.autoCollider != null)
                        {
                            GameObject.Destroy(this.autoCollider);

                            this.autoCollider = this._scroll.scrollBoxCollider;
                        }

                        // 销毁在节点上的autoInteractionTouchable 然后将scroll上的InteractionTouchable给这个autoInteractionTouchable
                        if (this.autoInteractionTouchable != null)
                        {
                            GameObject.Destroy(this.autoInteractionTouchable);

                            this.autoInteractionTouchable = this._scroll.scrollingTouchable;
                        }
                    }

                    // 如果存在scroll组件说明原先系统在非visible模式切换到hidden中，该模式已存在scroll组件，所以此处不需要通过setOverflow指令创建该组件，而是直接就复用原先的组件
                    if (this._scroll != null)
                    {
                        // 禁用滚动
                        this._scroll.canScroll = false;

                        // 打开裁切
                        this._scroll.scrollCollection.ClipBox.enabled = true;

                        this._scroll.scrollCollection.ScrollContainer.transform.localPosition = new Vector3(0f, 0f, 0f);

                        this._overflow = NodeOverflow.hidden;

                        PanelLayout.update(new PanelNodeActionInfo
                        {
                            current = this,
                            currentParent = this.parent != null && this.parent is PanelNode ? (PanelNode)this.parent : null,
                            action = PanelNodeAction.overflowChange
                        });
                    }
                }

                // 对scroll模式处理 (滚动模式中必须要启用scrollBoxCollider，所以该模式不必因为autoCollider的存在与否对scrollBoxCollider的enable属性做处理统一设置为启用，使得它保持打开状态，这样做是因为如果该碰撞体被禁用则该模式无效)
                if (value == "scroll")
                {
                    // 如果不存在scroll组件说明原先系统在visible模式切换到scroll中,该模式必须带有scroll组件，所以此处会在以下逻辑中使用setOverflow指令创建该组件
                    if (this._scroll == null)
                    {
                        // 通过setOverflow创建scroll组件
                        this.setOverflow(NodeOverflow.scroll);

                        // 销毁在节点上的autoCollider 然后将scroll上的collider给这个autoCollider
                        if (this.autoCollider != null)
                        {
                            GameObject.Destroy(this.autoCollider);

                            this.autoCollider = this._scroll.scrollBoxCollider;
                        }

                        // 销毁在节点上的autoInteractionTouchable 然后将scroll上的InteractionTouchable给这个autoInteractionTouchable
                        if (this.autoInteractionTouchable != null)
                        {
                            GameObject.Destroy(this.autoInteractionTouchable);

                            this.autoInteractionTouchable = this._scroll.scrollingTouchable;
                        }
                    }

                    // 如果存在scroll组件说明原先系统在非visible模式切换到scroll中，该模式已存在scroll组件，所以此处不需要通过setOverflow指令创建该组件，而是直接就复用原先的组件
                    if (this._scroll != null)
                    {
                        // 启用滚动
                        this._scroll.canScroll = true;

                        // 打开裁切
                        this._scroll.scrollCollection.ClipBox.enabled = true;

                        this._scroll.scrollCollection.ScrollContainer.transform.localPosition = new Vector3(0f, 0f, 0f);

                        this._overflow = NodeOverflow.scroll;

                        PanelScroll.viewFresh(this, true, true);

                        this._scroll.scrollBoxCollider.enabled = true;
                        this._scroll.scrollingTouchable.enabled = true;

                        PanelLayout.update(new PanelNodeActionInfo
                        {
                            current = this,
                            currentParent = this.parent != null && this.parent is PanelNode ? (PanelNode)this.parent : null,
                            action = PanelNodeAction.overflowChange
                        });
                    }

                    // 在scroll下使中保持boxCollider有效
                    this._scroll.scrollBoxCollider.enabled = true;
                }

                // 下理visible模式 (该模式在scroll为null的情况下不需要做任何处理，原因：不做任何处理本身就是visible模式)
                if (value == "visible")
                {
                    // 如果存在scroll组件说明原先系统在非visible模式切换到visible中，该模式已存在scroll组件，所以此处不需要通过setOverflow指令创建该组件，而是直接就复用原先的组件
                    if (this._scroll != null)
                    {
                        // 禁用滚动
                        this._scroll.canScroll = false;

                        // 关闭裁切
                        this._scroll.scrollCollection.ClipBox.enabled = false;

                        this._scroll.scrollCollection.ScrollContainer.transform.localPosition = new Vector3(0f, 0f, 0f);

                        this._overflow = NodeOverflow.visible;

                        PanelLayout.update(new PanelNodeActionInfo
                        {
                            current = this,
                            currentParent = this.parent != null && this.parent is PanelNode ? (PanelNode)this.parent : null,
                            action = PanelNodeAction.overflowChange
                        });
                    }
                }
            }
        }


        /*
         * 以下属性文本样式相关
         */
        private float _fontSize = 14f;
        /// <summary>
        /// 设置文本大小
        /// </summary>
        public float fontSize
        {
            get
            {
                return this._fontSize;
            }

            set
            {
                if (this._textNodeList.Count > 0)
                {
                    this._textNodeList[0].setSize(value);
                }
                this._fontSize = value;
            }
        }

        private string _color = "255, 255, 255";
        /// <summary>
        /// 文本颜色
        /// </summary>
        public string color
        {
            get
            {
                return this._color;
            }

            set
            {
                if (this._textNodeList.Count > 0)
                {
                    this._textNodeList[0].setColor(Utils.stringToColor(value));
                }
                this._color = value;
            }
        }

        private string _textHorizontal = "left";
        /// <summary>
        /// 文本水平对齐方式
        /// </summary>
        public string textHorizontal
        {
            get
            {
                return this._textHorizontal;
            }

            set
            {
                if (this._textNodeList.Count > 0)
                {
                    if (value == "left")
                    {
                        this._textNodeList[0].setHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Left);
                    }
                    if (value == "center")
                    {
                        this._textNodeList[0].setHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Center);
                    }
                    if (value == "flush")
                    {
                        this._textNodeList[0].setHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Flush);
                    }
                    if (value == "geometry")
                    {
                        this._textNodeList[0].setHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Geometry);
                    }
                    if (value == "justified")
                    {
                        this._textNodeList[0].setHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Justified);
                    }
                    if (value == "right")
                    {
                        this._textNodeList[0].setHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Right);
                    }
                }
                this._textHorizontal = value;
            }
        }

        private string _textVertical = "top";
        /// <summary>
        /// 文本垂直对齐方式
        /// </summary>
        public string textVertical
        {
            get
            {
                return this._textVertical;
            }

            set
            {
                if (this._textNodeList.Count > 0)
                {
                    if (value == "top")
                    {
                        this._textNodeList[0].setVerticalAlignment(TMPro.VerticalAlignmentOptions.Top);
                    }
                    if (value == "baseline")
                    {
                        this._textNodeList[0].setVerticalAlignment(TMPro.VerticalAlignmentOptions.Baseline);
                    }
                    if (value == "capline")
                    {
                        this._textNodeList[0].setVerticalAlignment(TMPro.VerticalAlignmentOptions.Capline);
                    }
                    if (value == "geometry")
                    {
                        this._textNodeList[0].setVerticalAlignment(TMPro.VerticalAlignmentOptions.Geometry);
                    }
                    if (value == "middle")
                    {
                        this._textNodeList[0].setVerticalAlignment(TMPro.VerticalAlignmentOptions.Middle);
                    }
                    if (value == "bottom")
                    {
                        this._textNodeList[0].setVerticalAlignment(TMPro.VerticalAlignmentOptions.Bottom);
                    }
                }

                this._textVertical = value;
            }
        }

        private float _characterSpace = 0f;
        /// <summary>
        /// 字符间距
        /// </summary>
        public float characterSpace
        {
            get
            {
                return this._characterSpace;
            }

            set
            {
                if (this._textNodeList.Count > 0)
                {
                    this._textNodeList[0].setCharacterSpace(value);
                }
                this._characterSpace = value;
            }
        }

        private float _lineSpace = 0f;
        /// <summary>
        /// 行间距
        /// </summary>
        public float lineSpace
        {
            get
            {
                return this._lineSpace;
            }

            set
            {
                if (this._textNodeList.Count > 0)
                {
                    this._textNodeList[0].setLineSpace(value);
                }
                this._lineSpace = value;
            }
        }

        private float _paragraphSpace = 0f;
        /// <summary>
        /// 段落间距
        /// </summary>
        public float paragraphSpace
        {
            get
            {
                return this._paragraphSpace;
            }

            set
            {
                if (this._textNodeList.Count > 0)
                {
                    this._textNodeList[0].setParagraphSpace(value);
                }
                this._paragraphSpace = value;
            }
        }

        private float _wordSpace = 0f;
        /// <summary>
        /// 字间距
        /// </summary>
        public float wordSpace
        {
            get
            {
                return this._wordSpace;
            }

            set
            {
                if (this._textNodeList.Count > 0)
                {
                    this._textNodeList[0].setWordSpace(value);
                }
                this._wordSpace = value;
            }
        }

        /// <summary>
        /// 节点文本
        /// </summary>
        public string text
        {
            get
            {
                string result = "";
                if (this._textNodeList.Count > 0)
                {
                    result = this.textNodeList[0].text;
                }
                return result;
            }

            set
            {
                if (value == "")
                {
                    if (this._textNodeList.Count > 0)
                    {
                        this.textNodeList[0].destory();
                    }

                    return;
                }

                if (this._textNodeList.Count == 0)
                {
                    this.appendNode(new PanelTextNode());
                }

                this.textNodeList[0].text = value;
               
                this.textNodeList[0].fresh();
            }
        }

        /*
         *  以下属性布局相关
         */
        /*--------------------------------------------定义contentHorizontal属性支撑方法开始-------------------------------------------*/
        /// <summary>
        /// 获取子节点水平对齐模式
        /// </summary>
        /// <returns></returns>
        public AlignMode getContentHorizontalAlignMode()
        {
            return (this._contentHorizontalAlign != null) ? this._contentHorizontalAlign.mode : AlignMode.none;
        }


        /// <summary>
        /// 设置子节点水平对齐模式
        /// </summary>
        /// <param name="alignMode"></param>
        public void setContentHorizontalAlignMode(AlignMode alignMode)
        {
            if (this._contentHorizontalAlign == null)
            {
                this._contentHorizontalAlign = new PanelAlign(this, Axle.right, alignMode);
            }
            else
            {
                this._contentHorizontalAlign.mode = alignMode;
            }

            this._contentHorizontalAlign.fresh();
        }
        /*--------------------------------------------定义contentHorizontal属性支撑方法结束-------------------------------------------*/

        /// <summary>
        /// 在X负轴上最左侧的子节点是否有开始间距
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
                    this._contentHorizontalAlign = new PanelAlign(this, Axle.right, AlignMode.none);
                }

                this._contentHorizontalAlign.isStartInvterval = value;
            }
        }

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
                    this._contentHorizontalAlign = new PanelAlign(this, Axle.right, AlignMode.none);
                }

                this._contentHorizontalAlign.interval = value;
            }
        }

        /// <summary>
        /// 在X正轴上最右侧的子节点是否有结束间距
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
                    this._contentHorizontalAlign = new PanelAlign(this, Axle.right, AlignMode.none);
                }

                this._contentHorizontalAlign.isEndInvterval = value;
            }
        }

        private PanelAlign _contentHorizontalAlign;
        /// <summary>
        /// 子节点水平对齐方式
        /// </summary>
        public string contentHorizontal
        {
            get
            {
                return this._contentHorizontalAlign.mode.ToString();
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


        /*--------------------------------------------定义contentVertical属性支撑方法开始-------------------------------------------*/
        /// <summary>
        /// 获取子节点垂直对齐方式
        /// </summary>
        public AlignMode getContentVerticalAlignMode()
        {
            return (this._contentVerticalAlign != null) ? this._contentVerticalAlign.mode : AlignMode.none;
        }

        /// <summary>
        /// 设置子节点垂直对齐方式
        /// </summary>
        /// <param name="alignMode">对齐方式</param>
        public void setContentVerticalAlignMode(AlignMode alignMode)
        {
            if (this._contentVerticalAlign == null)
            {
                this._contentVerticalAlign = new PanelAlign(this, Axle.up, alignMode);
            }
            else
            {
                this._contentVerticalAlign.mode = alignMode;
            }

            this._contentVerticalAlign.fresh();
        }
        /*--------------------------------------------定义contentVertical属性支撑方法结束-------------------------------------------*/

        private PanelAlign _contentVerticalAlign;
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
                    this._contentVerticalAlign = new PanelAlign(this, Axle.up, AlignMode.none);
                }

                this._contentVerticalAlign.interval = value;
            }
        }

        /// <summary>
        /// 在Y轴负方向上最底部的这个节点是否有结束间距
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
                    this._contentVerticalAlign = new PanelAlign(this, Axle.up, AlignMode.none);
                }

                this._contentVerticalAlign.isStartInvterval = value;
            }
        }

        /// <summary>
        /// 在Y轴正方向上最顶部的这个节点是否有开始间距
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
                    this._contentVerticalAlign = new PanelAlign(this, Axle.up, AlignMode.none);
                }

                this._contentVerticalAlign.isEndInvterval = value;
            }
        }

        /// <summary>
        /// 子节点垂直对齐方式
        /// </summary>
        public string contentVertical
        {
            get
            {
                string result = "none";
                if (this._contentVerticalAlign.mode == AlignMode.left)
                {
                    result = "bottom";
                } else if (this._contentVerticalAlign.mode == AlignMode.right)
                {
                    result = "top";
                } else
                {
                    result = this._contentVerticalAlign.mode.ToString();
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


        /*
         *  以下属性与滚动相关
         */
        private PanelScroll _scroll;
        /// <summary>
        /// 是否启用滚动
        /// </summary>
        private bool enableScrollComponent
        {
            get
            {
                return this.scrollComponent != null ? true : false;
            }

            set
            {
                // 如果没有滚动组件则创建
                if (value)
                {
                    if (this.scrollComponent == null)
                    {
                        this._scroll = new PanelScroll(this, new ScrollConfig
                        {
                            // 晶格
                            cellWidth = 0.001f,
                            cellHeight = 0.001f,
                            cellDepth = Space.zero,

                            // EmptyScrll (滚动组件ScrollCollection的载体)的位置、尺寸
                            scrollCenter = new Vector3(0, 0, 0),
                            scrollSize = new Vector3(1, 1, 1),

                            // EmptyScrll(滚动组件ScrollCollection的载体)的碰撞体
                            colliderCenter = new Vector3(0, 0, 0),
                            colliderSize = new Vector3(1, 1, Space.zero),

                            // 裁切对象的位置、尺寸
                            clippingCenter = new Vector3(0, 0, 0),
                            clippingSize = new Vector3(1, 1, 1),

                            thickness = depth
                        });

                        this._scroll.fresh();

                        // 在此处关闭滚动组件的BoxCollider和InteractTouchable，这些由节点的事件决定是否打开
                        this._scroll.scrollBoxCollider.enabled = false;
                        this._scroll.scrollingTouchable.enabled = false;
                    }
                }
                else if (this.scrollComponent != null)
                {
                    this._scroll.destory();
                    this._scroll = null;
                }
            }
        }

        /// <summary>
        /// 是否启用滚动
        /// </summary>
        public bool enableScroll
        {
            get
            {
                return this.getOverflow() != NodeOverflow.scroll;
            }
        }

        /// <summary>
        /// Scroll组件状态
        /// </summary>
        public ScrollCompontentState scrollComponentState
        {
            get
            {
                return this.getOverflow() != NodeOverflow.visible ? this.scrollComponent.state : ScrollCompontentState.normal;
            }
        }

        // <summary>
        /// 滚动位置
        /// </summary>
        public Vector3? scrollPosition
        {
            get
            {
                Vector3? result = null;

                if (this.getOverflow() != NodeOverflow.visible)
                {
                    var x = this.scrollComponent.scrollCollection.ScrollContainer.transform.localPosition.x;
                    var y = this.scrollComponent.scrollCollection.ScrollContainer.transform.localPosition.y;
                    var z = this.scrollComponent.scrollCollection.ScrollContainer.transform.localPosition.z;

                    result = new Vector3(x, y, z);
                }

                return result;
            }
        }

        /*--------------------------------------------定义scrollDirection属性支撑方法开始-------------------------------------------*/
        /// <summary>
        /// 获取滚动方向
        /// </summary>
        /// <returns></returns>
        public ScrollDrientation getScrollDirection()
        {
            ScrollDrientation result = ScrollDrientation.normal;

            if (this.getOverflow() != NodeOverflow.visible && !this.scrollComponent.scrollCollection.isBoth)
            {
                result = ScrollDrientation.upAndDown;

                if (this.scrollComponent.scrollDirection == PanelScrollingCollection.ScrollDirectionType.LeftAndRight)
                {
                    result = ScrollDrientation.leftAndRight;
                }
            }

            if (this.getOverflow() != NodeOverflow.visible && this.scrollComponent.scrollCollection.isBoth)
            {
                result = ScrollDrientation.both;
            }

            return result;
        }

        /// <summary>
        /// 设置滚动方向
        /// </summary>
        /// <param name="direction">滚动方向</param>
        public void setScrollDirection(ScrollDrientation direction)
        {
            if (this.scrollComponent != null)
            {
                if (direction == ScrollDrientation.both)
                {
                    if (this.getScrollDirection() != ScrollDrientation.both)
                    {
                        this.scrollComponent.scrollCollection.isBothScrollReady = false;
                    }

                    this.scrollComponent.scrollCollection.isBoth = true;
                }

                if (direction == ScrollDrientation.upAndDown)
                {
                    this.scrollComponent.scrollDirection = PanelScrollingCollection.ScrollDirectionType.UpAndDown;
                }

                if (direction == ScrollDrientation.leftAndRight)
                {
                    this.scrollComponent.scrollDirection = PanelScrollingCollection.ScrollDirectionType.LeftAndRight;
                }
            }
        }
        /*--------------------------------------------定义scrollDirection属性支撑方法结束-------------------------------------------*/

        private string _scrollDirection = "upAndDown";
        /// <summary>
        /// 设置滚动方向
        /// </summary>
        public string scrollDirection
        {
            get
            {
                return this._scrollDirection;
            }

            set
            {
                if (value == "upAndDown")
                {
                    this.setScrollDirection(ScrollDrientation.upAndDown);
                    this._scrollDirection = "upAndDown";
                }
                else if (value == "leftAndRight")
                {
                    this.setScrollDirection(ScrollDrientation.leftAndRight);
                    this._scrollDirection = "leftAndRight";
                }
                else if (value == "both")
                {
                    this.setScrollDirection(ScrollDrientation.both);
                    this._scrollDirection = "both";
                }
            }
        }

        /// <summary>
        /// 节点操作默认配制
        /// </summary>
        protected override ManipulatorConfig manipulatorConfig
        {
            get
            {
                return new ManipulatorConfig
                {
                    colliderCenter = new Vector3(0, 0, this.isPanel ? -Space.zero : -1 / 2f),
                    colliderSize = new Vector3(1, 1, Space.zero)
                };
            }
        }
        #endregion

        #region 事件
        private readonly ScrollReadyEvent _onScrollReady;
        /// <summary>
        /// 滚动准备就绪事件
        /// </summary>
        public ScrollReadyEvent onScrollReady
        {
            get
            {
                return this._onScrollReady;
            }
        }

        private readonly ScrollEvent _onScroll;
        /// <summary>
        /// 滚动事件
        /// </summary>
        public ScrollEvent onScroll
        {
            get
            {
                return this._onScroll;
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
        #endregion

        #region 基本方法
        /// <summary>
        /// 重写基类内容添加方法
        /// </summary>
        /// <param name="current"></param>
        public override void addCentent(Node current)
        {
            if (this.scrollComponentState == ScrollCompontentState.normal || this.scrollComponentState == ScrollCompontentState.init || this.scrollComponentState == ScrollCompontentState.initFinish)
            {
                Space.GameObjectEntity.appendNode(current.parasitifer, this.parasitifer);
            }
            else
            {
                this.scrollComponent.addContent(current.parasitifer);
            }
        }

        /// <summary>
        /// 重写基类内容移除方法
        /// </summary>
        /// <param name="node"></param>
        protected override void removeContent(Node node)
        {
            var parent = this.parent;
            if (parent != null && (((PanelNode)parent).scrollComponentState != ScrollCompontentState.normal && this.scrollComponentState == ScrollCompontentState.readyFinish))
            {
                ((PanelNode)parent).scrollComponent.removeContent(node.parasitifer);
            }
            else
            {
                // 移除该节点时销毁该节点所管理的游戏对象
                UnityEngine.Object.Destroy(node.parasitifer);
            }
        }

        /// <summary>
        /// 添加文本节点
        /// </summary>
        /// <param name="textNode">文本节点</param>
        /// <returns></returns>
        public PanelTextNode appendNode(PanelTextNode textNode)
        {
            if (this.textNodeList.Count == 0)
            {
                textNode.node = this;
                textNodeList.Add(textNode);

                // 文本刷新
                this.textAlignFresh();
            }
            else
            {
                this.text = this.text + textNode.text;
            }

            return textNode;
        }

        /*
         *  以下方法主要用于处理collider、touchable、addRender的自动添加与回收
         */
        /// <summary>
        /// 添加autoCollider
        /// </summary>
        public override void addAutoCollider()
        {
            // 当不存在scroll组件时，需要创建一个collider作为autoCollider并将其加在节点本身的游戏对象上
            if (this._scroll == null)
            {
                var depth = (this is PanelNode) ? Space.zero : EMR.Space.Unit.unitToScaleForGameObject(this.parasitifer, this.size.depth, EMR.Struct.Axle.forward);
                var center = new Vector3(0f, 0f, (this is PanelNode) ? -depth / 2 : 0f);
                var size = new Vector3(1f, 1f, (this is PanelNode) ? depth : 1f);

                this.autoCollider = this.parasitifer.AddComponent<BoxCollider>();

                this.autoCollider.center = center;
                this.autoCollider.size = size;
            }

            // 当存在scroll组件autoCollider直接引用该组件的scrollBoxCollider也就是ScrollEmpty上的collider
            if (this._scroll != null)
            {
                this.autoCollider = this._scroll.scrollBoxCollider;
                this._scroll.scrollBoxCollider.enabled = true;
            }
        }

        /// <summary>
        /// 销毁autoCollider
        /// </summary>
        protected override void recoveryAutoCollider()
        {
            // 这里的判断条件中没有加上scroll状态原因是因为scroll状态必须一定要有collider否则无法滚动
            if (!this.collider && this.autoCollider != null && EMR.Space.leftHand.getHandlerCount(this) ==0 && EMR.Space.rightHand.getHandlerCount(this) == 0 && ((this.overflow == "visible" || this.overflow == "hidden") && this.checkNodeNotIncludePointerEvent() && this.checkNodeNotIncludeTouchEvent() && this.checkNodeNotIncludeFocusEvent()))
            {
                // 当节点不存在scroll组件时说明用户从未启用过visible以外的显示方式，这时autoCollider在节点本身上，可以直接清理
                if (this._scroll == null)
                {
                    GameObject.Destroy(this.autoCollider);
                }

                // 当节点存在scroll组件时，这时清理autoCollider为禁用该碰撞体，不直接Destory是为了后续的复用
                if (this._scroll != null)
                {

                    // 如果当前节点显示方式不为滚动方式则将其enabled设置为禁用、滚动方式不作任何处理是因为即然是滚动就必须带有collider，以使指针等能与其发生碰撞，不然该模式无效
                    if (this.overflow != "scroll")
                    {
                        this._scroll.scrollBoxCollider.enabled = false;
                    }
                }

                this.autoCollider = null;
            }
        }

        /// <summary>
        /// 添加autoInteractionTouchable
        /// </summary>
        public override void addAutoInteractionTouchable()
        {
            // 当不存在scroll组件时，需要创建一个InteractionTouchable作为autoInteractionTouchable并将其加在节点本身的游戏对象上，否则scroll组件上本身就有InteractionTouchable所以不需要再创建
            if (this._scroll == null)
            {
                NearInteractionTouchable result = this.parasitifer.AddComponent<NearInteractionTouchable>();

                var depth = (this is PanelNode) ? Space.zero : EMR.Space.Unit.unitToScaleForGameObject(this.parasitifer, this.size.depth, EMR.Struct.Axle.forward);
                result.SetLocalCenter(new Vector3(0f, 0f, (this is PanelNode) ? -depth / 2 : 0f));
                result.SetBounds(new Vector2(Math.Abs(Vector3.Dot(new Vector3(1, 1, depth), result.LocalRight)), Math.Abs(Vector3.Dot(new Vector3(1, 1, depth), result.LocalUp))));

                this.autoInteractionTouchable = result;
            }

            // 当存在scroll组件autoInteractionTouchable直接引用该组件的ScrollingTouchable也就是ScrollEmpty上的InteractionTouchable
            if (this._scroll != null)
            {
                this.autoInteractionTouchable = this._scroll.scrollingTouchable;
                this.autoInteractionTouchable.enabled = true;
            }
        }

        /// <summary>
        /// 销毁autoInteractionTouchable
        /// </summary>
        protected override void recoveryAutoInteractionTouchable()
        {
            if (this.autoInteractionTouchable != null && EMR.Space.leftHand.getHandlerCount(this) == 0 && EMR.Space.rightHand.getHandlerCount(this) == 0 && (this.overflow == "visible" || this.overflow == "hidden") && this.checkNodeNotIncludeTouchEvent())
            {
                // 当节点不存在scroll组件时说明用户从未启用过visible以外的显示方式，这时autoInteractionTouchable在节点本身上，可以直接清理
                if (this._scroll == null)
                {
                    GameObject.Destroy(this.autoInteractionTouchable);
                }

                // 当节点存在scroll组件时，这时清理autoInteractionTouchable为禁用，不直接Destory是为了后续的复用
                if (this._scroll != null)
                {
                    // 如果当前节点显示方式不为滚动方式则将其enabled设置为禁用、滚动方式不作任何处理是因为即然是滚动就必须带有InteractionTouchable，以使指针等能与其发生碰撞，不然该模式无效
                    if (this.overflow != "scroll")
                    {
                        this._scroll.scrollingTouchable.enabled = false;
                    }
                }

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

            NearInteractionTouchable result = this.parasitifer.AddComponent<NearInteractionTouchable>();

            var localCeneter = center == null ? new Vector3(0f, 0f, 0f) : (Vector3)center;
            var boundSize = size == null ? new Vector3(1f, 1f, 1f) : (Vector3)size;

            result.SetLocalCenter(localCeneter);
            result.SetBounds(new Vector2(Math.Abs(Vector3.Dot(boundSize, result.LocalRight)), Math.Abs(Vector3.Dot(boundSize, result.LocalUp))));

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
            NearInteractionTouchable result = null;
            foreach (var item in interactionTouchableDictionary)
            {
                if (item.Value == (object)interactionTouchable)
                {
                    result = interactionTouchableDictionary[name];
                    this.interactionTouchableDictionary.Remove(item.Key);
                }
            }

            if(result != null)
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
            NearInteractionTouchable result = null;
            if (interactionTouchableDictionary.ContainsKey(name))
            {
                result = interactionTouchableDictionary[name];
                interactionTouchableDictionary.Remove(name);
            }

            if(result != null)
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

                GameObject temp;
                if (this.isPanel)
                {
                    temp = GameObject.CreatePrimitive(PrimitiveType.Quad);
                }
                else
                {
                    temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                }

                meshFilter.mesh = temp.GetComponent<MeshFilter>().mesh;

                GameObject.Destroy(temp);
            }

            return meshRenderer;
        }
        

        /*
         *  以下方法主要用于布局及滚动
         */
        /// <summary>
        /// 子节点对齐刷新
        /// </summary>
        public void contentAlignFresh()
        {
            this._contentHorizontalAlign?.fresh();
            this._contentVerticalAlign?.fresh();
        }

        /// <summary>
        /// 文本对齐刷新
        /// </summary>
        public void textAlignFresh()
        {
            this.textHorizontal = this._textHorizontal;
            this.textVertical = this._textVertical;
        }

        /// <summary>
        /// 刷新滚动组件 (一般由框架内部调用，开发者一般不需要调用此方法)
        /// </summary>
        /// <param name="isScrollPositionReset">是否重置滚动位置</param>
        public void scrollFresh(bool isScrollPositionReset)
        {
            this.scrollComponent?.scrollCompontentFresh(isScrollPositionReset);
        }

        /// <summary>
        /// 滚动到列或行(默认为水平方向滚动)
        /// </summary>
        /// <param name="index"></param>
        public void scrollTo(int index, ScrollDrientation scrollDrientation = ScrollDrientation.leftAndRight)
        {
            // 刷新滚动组件的contentBounds（有可能在执行该指令前用户已调整了节点的尺寸，这会使得其MaxX、MaxY发生改变)
            this.scrollFresh(false);

            var data = Space.Unit.unitToScaleForGameObject(this.scrollComponent.scrollCollection.ScrollContainer, index, Axle.right);

            if(scrollDrientation == ScrollDrientation.upAndDown)
            {
                data = Space.Unit.unitToScaleForGameObject(this.scrollComponent.scrollCollection.ScrollContainer, index, Axle.up);
            }

            this.scrollComponent.scrollCollection.MoveToTier((int)(data / 0.001));
        }

        /// <summary>
        /// 生成Scroll emitter
        /// </summary>
        private void createScrollEventEmitter()
        {
            this.service.createEmit(() =>
            {
                // 当前滚动容器位置
                PositionData currentScrollPosition = this.getOverflow() != NodeOverflow.visible ? new PositionData((float)scrollPosition?.x, (float)scrollPosition?.y, (float)scrollPosition?.z) : null;

                // 触发滚动事件
                if (currentScrollPosition != null && (!Utils.equals(currentScrollPosition.x, this._previousScrollPosition.x) || !Utils.equals(currentScrollPosition.y, this._previousScrollPosition.y) || !Utils.equals(currentScrollPosition.z, this._previousScrollPosition.z)))
                {
                    this.onScroll.Invoke(new ScrollEventData
                    {
                        target = this,
                        oldPosition = this._previousScrollPosition.clone(),
                        currentPosition = currentScrollPosition
                    });
                }
            }, () =>
            {
                PositionData currentScrollPosition = this.getOverflow() != NodeOverflow.visible ? new PositionData((float)scrollPosition?.x, (float)scrollPosition?.y, (float)scrollPosition?.z) : null;

                // 设置节点滚动行为状态
                if (currentScrollPosition != null && (!Utils.equals(currentScrollPosition.x, this._previousScrollPosition.x) || !Utils.equals(currentScrollPosition.y, this._previousScrollPosition.y) || !Utils.equals(currentScrollPosition.z, this._previousScrollPosition.z)))
                {

                    this.isScrolling = true;
                }
                else
                {
                    this.isScrolling = false;
                }
            });

            this.service.lateNext(() =>
            {
                PositionData currentScrollPosition = this.getOverflow() != NodeOverflow.visible ? new PositionData((float)scrollPosition?.x, (float)scrollPosition?.y, (float)scrollPosition?.z) : null;

                if (currentScrollPosition != null)
                {
                    this._previousScrollPosition = new PositionData(currentScrollPosition.x, currentScrollPosition.y, currentScrollPosition.z);
                }
                else
                {
                    this._previousScrollPosition = new PositionData(0f, 0f, 0f);
                }

                return false;
            });
        }
        #endregion

        #region 节点数据结构基本操作
        /// <summary>
        /// 第一个子节点
        /// </summary>
        public new PanelLayer firstChild
        {
            get
            {
                var result = base.firstChild;
                return result != null ? (PanelLayer)result : null;
            }
        }

        /// <summary>
        /// 最后一个子节点
        /// </summary>
        public new PanelLayer lastChild
        {
            get
            {
                var result = base.lastChild;
                return result != null ? (PanelLayer)result : null;
            }
        }

        /// <summary>
        /// 子节点列表
        /// </summary>
        public new List<PanelLayer> children
        {
            get
            {
                var result = base.children;
                return result.ConvertAll(t => (PanelLayer)t);
            }
        }

        /// <summary>
        /// 插入子节点
        /// </summary>
        /// <param name="icurrent">要插入子节点</param>
        /// <param name="irefNode">参考节点</param>
        /// <returns></returns>
        public virtual PanelLayer insertBefore(PanelLayer current, Node refNode)
        {
            var panelRoot = this.panelRoot;

            // 将PanelRoot处于视图正交状态
            var originalRotation = new Vector3(0f, 0f, 0f);
            if (panelRoot != null)
            {
                originalRotation = panelRoot.parasitifer.transform.eulerAngles;
                panelRoot.parasitifer.transform.eulerAngles = new Vector3(0f, 0f, 0f);
            }

            // 将节点添加至空间
            var result = base.insertBefore(current, refNode);
            if (result != null)
            {
                this.addCentent(current);
            }

            // 恢复PanelRoot的原旋转量
            if (panelRoot != null)
            {
                panelRoot.parasitifer.transform.eulerAngles = originalRotation;
            }

            // 返回添加结果
            return result != null ? (PanelLayer)result : null;
        }


        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="node">要添加的节点</param>
        /// <returns></returns>
        public virtual PanelLayer appendNode(PanelLayer node)
        {
            // 将PanelRoot处于视图正交状态
            var panelRoot = this.panelRoot;
            var originalRotation = new Vector3(0f, 0f, 0f);
            if (panelRoot != null)
            {
                originalRotation = panelRoot.parasitifer.transform.eulerAngles;
                panelRoot.parasitifer.transform.eulerAngles = new Vector3(0f, 0f, 0f);
            }

            // 将节点添加至空间
            var result = base.appendNode(node);
            if (result != null)
            {
                this.addCentent(node);
            }

            // 恢复PanelRoot的原旋转量
            if (panelRoot != null)
            {
                panelRoot.parasitifer.transform.eulerAngles = originalRotation;
            }

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
                var parent = this.parent;

                base.destory();

                // 布局更新（当父节点不处于正在销毁状态且该节点所在的组件也不在销毁状态时)
                if (!this.isParentElementDestoring && !this.component.isDestoring)
                {
                    Space.mainService.next(() =>
                    {
                        if (parent != null && parent is PanelNode)
                        {
                            PanelLayout.update(new PanelNodeActionInfo
                            {
                                currentParent = (PanelNode)parent,
                                action = PanelNodeAction.nodeDestory
                            }) ;
                        }

                        return true;
                    });
                }

                // 销毁npc
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

        /// <summary>
        /// 遍历Node
        /// </summary>
        /// <param name="node"></param>
        public void ergodic(PanelNodeHandler nodeHandler)
        {
            nodeHandler(this);
            foreach (var item in this.children)
            {
                item.ergodic(nodeHandler);
            }
        }
    }
    #endregion
}
