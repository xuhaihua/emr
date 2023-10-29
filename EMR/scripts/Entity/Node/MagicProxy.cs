using System;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Rendering;
using UnityEngine;
using UnityEngine.Events;
using EMR.Struct;
using EMR.Event;
using EMR.Common;
using EMR.Plugin;



namespace EMR.Entity
{
    /// <summary>
    /// 魔法代理类
    /// </summary>
    public abstract class MagicProxy : Node
    {
        protected ISpaceCharacteristic _node = null;

        public MagicProxy(float x, float y, float z, float xAngle, float yAngle, float zAngle, float width, float height, float depth) : base(x, y, z, xAngle, yAngle, zAngle, width, height, depth)
        {
        }

        public MagicProxy() : base(0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f)
        {
        }

        #region 节点基本属性
        /// <summary>
        /// 宽度是否固定
        /// </summary>
        public override bool widthFixed
        {
            get
            {
                if(this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node).widthFixed;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                ((Node)this._node).widthFixed = value;
            }
        }

        /*
         *  尺寸位置相关属性
         */
        /// <summary>
        /// width
        /// </summary>
        public override float width
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }
                
                return ((Node)this._node).width;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                ((Node)this._node).width = value;
            }
        }

        /// <summary>
        /// 高度是否固定
        /// </summary>
        public override bool heightFixed
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node).heightFixed;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                ((Node)this._node).heightFixed = value;
            }
        }

        /// <summary>
        /// height
        /// </summary>
        public override float height
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }
                
                return ((Node)this._node).height; 
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                ((Node)this._node).height = value;
            }
        }

        public override bool depthFixed
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node).depthFixed;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                ((Node)this._node).depthFixed = value;
            }
        }

        /// <summary>
        /// 厚度
        /// </summary>
        public override float depth
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node).depth;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                ((Node)this._node).depth = value;
            }
        }

        /// <summary>
        /// 空间偏移量
        /// </summary>
        public override Vector3 offset
        {
            get
            {
                Vector3 result = new Vector3(0f, 0f, 0f);

                if (this._node != null)
                {
                    result = ((Node)this._node).offset;
                }
                return result;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                ((Node)this._node).offset = value;
            }
        }

        /// <summary>
        /// npc偏移量
        /// </summary>
        public override Vector3 npcOffset
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node).npcOffset;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                ((Node)this._node).npcOffset = value;
            }
        }

        /// <summary>
        /// 包围盒
        /// </summary>
        public override BoundBox localBounds
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node).localBounds; 
            }
        }

        public override bool hasAutoInteractionTouchable
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node).hasAutoInteractionTouchable;
            }
        }

        /// <summary>
        /// 渲染模式
        /// </summary>
        public override string renderMode
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }
                
                return ((Node)this._node).renderMode;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                ((Node)this._node).renderMode = value;
            }
        }

        /// <summary>
        /// 颜色
        /// </summary>
        public override string backgroundColor
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node).backgroundColor;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                ((Node)this._node).backgroundColor = value;
            }
        }

        /// <summary>
        /// 设置背景
        /// </summary>
        public override string backgroundImage
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node).backgroundImage;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                ((Node)this._node).backgroundImage = value;
            }
        }

        /// <summary>
        /// 灯光强度
        /// </summary>
        public override float lightIntensity
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node).lightIntensity;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                ((Node)this._node).lightIntensity = value;
            }
        }

        /// <summary>
        /// 设置悬浮灯颜色
        /// </summary>
        public override string hoverColor
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node).hoverColor;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                ((Node)this._node).hoverColor = value;
            }
        }

        /// <summary>
        /// 设置接近光center颜色
        /// </summary>
        public override string nearLightCenterColor
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }
                
                return ((Node)this._node).nearLightCenterColor;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                ((Node)this._node).nearLightCenterColor = value;
            }
        }

        /// <summary>
        /// 设置接近光middle颜色
        /// </summary>
        public override string nearLightMiddleColor
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node).nearLightMiddleColor;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                ((Node)this._node).nearLightMiddleColor = value;
            }
        }

        /// <summary>
        /// 设置接近光outline颜色
        /// </summary>
        public override string nearLightOuterColor
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node).nearLightOuterColor;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                ((Node)this._node).nearLightOuterColor = value;
            }
        }

        /// <summary>
        /// 边框宽度
        /// </summary>
        public override float borderWidth
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node).borderWidth;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                ((Node)this._node).borderWidth = value;
            }
        }

        /// <summary>
        /// 边框半径
        /// </summary>
        public override float borderRadius
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node).borderRadius;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                ((Node)this._node).borderRadius = value;
            }
        }


        /// <summary>
        /// 锚集合
        /// </summary>
        public override List<Anchor> anchors
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node).anchors;
            }
        }

        /// <summary>
        /// 水平刷新
        /// </summary>
        public override void horizontalFresh()
        {
            if (this._node == null)
            {
                throw new NodeException("The magic node does not have its child node");
            }

            ((Node)this._node).horizontalFresh();
        }

        /// <summary>
        /// 自身水平对齐方式
        /// </summary>
        public override string horizontal
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node).horizontal;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                ((Node)this._node).horizontal = value;
            }
        }

        /// <summary>
        /// 自身水平浮动方式
        /// </summary>
        public override string horizontalFloat
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }
                
                return ((Node)this._node).horizontal;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                ((Node)this._node).horizontal = value;
            }
        }

        /// <summary>
        /// 垂直浮动刷新
        /// </summary>
        public override void verticalFresh()
        {
            if (this._node == null)
            {
                throw new NodeException("The magic node does not have its child node");
            }

            ((Node)this._node).verticalFresh();
        }

        /// <summary>
        /// 自身垂直对齐方式
        /// </summary>
        public override string vertical
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }
                
                return ((Node)this._node).vertical;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                ((Node)this._node).vertical = value;
            }
        }

        /// <summary>
        /// 自身垂直浮动
        /// </summary>
        public override string verticalFloat
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node).vertical;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                ((Node)this._node).vertical = value;
            }
        }

        public string contentHorizontal
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                string result = "none";

                if (this._node is PanelNode)
                {
                    result = ((PanelNode)this._node).contentHorizontal;
                }

                if (this._node is SpaceNode)
                {
                    result = ((SpaceNode)this._node).contentHorizontal;
                }

                return result;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                if (this._node is PanelNode)
                {
                    ((PanelNode)this._node).contentHorizontal = value;
                }

                if (this._node is SpaceNode)
                {
                    ((SpaceNode)this._node).contentHorizontal = value;
                }
            }
        }


        public float horizontalInterval
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                float result = 0f;

                if (this._node is PanelNode)
                {
                    result = ((PanelNode)this._node).horizontalInterval;
                }

                if (this._node is SpaceNode)
                {
                    result = ((SpaceNode)this._node).horizontalInterval;
                }

                return result;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                if (this._node is PanelNode)
                {
                    ((PanelNode)this._node).horizontalInterval = value;
                }

                if (this._node is SpaceNode)
                {
                    ((SpaceNode)this._node).horizontalInterval = value;
                }
            }
        }

        public bool horizontalLeftInterval
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                bool result = false;

                if (this._node is PanelNode)
                {
                    result = ((PanelNode)this._node).horizontalLeftInterval;
                }

                if (this._node is SpaceNode)
                {
                    result = ((SpaceNode)this._node).horizontalLeftInterval;
                }

                return result;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                if (this._node is PanelNode)
                {
                    ((PanelNode)this._node).horizontalLeftInterval = value;
                }

                if (this._node is SpaceNode)
                {
                    ((SpaceNode)this._node).horizontalLeftInterval = value;
                }
            }
        }


        public bool horizontalRightInterval
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                bool result = false;

                if (this._node is PanelNode)
                {
                    result = ((PanelNode)this._node).horizontalRightInterval;
                }

                if (this._node is SpaceNode)
                {
                    result = ((SpaceNode)this._node).horizontalRightInterval;
                }

                return result;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                if (this._node is PanelNode)
                {
                    ((PanelNode)this._node).horizontalRightInterval = value;
                }

                if (this._node is SpaceNode)
                {
                    ((SpaceNode)this._node).horizontalRightInterval = value;
                }
            }
        }

        public bool verticalTopInterval
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                bool result = false;

                if (this._node is PanelNode)
                {
                    result = ((PanelNode)this._node).verticalTopInterval;
                }

                if (this._node is SpaceNode)
                {
                    result = ((SpaceNode)this._node).verticalTopInterval;
                }

                return result;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                if (this._node is PanelNode)
                {
                    ((PanelNode)this._node).verticalTopInterval = value;
                }

                if (this._node is SpaceNode)
                {
                    ((SpaceNode)this._node).verticalTopInterval = value;
                }
            }
        }

        public bool verticalBottomInterval
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                bool result = false;

                if (this._node is PanelNode)
                {
                    result = ((PanelNode)this._node).verticalBottomInterval;
                }

                if (this._node is SpaceNode)
                {
                    result = ((SpaceNode)this._node).verticalBottomInterval;
                }

                return result;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                if (this._node is PanelNode)
                {
                    ((PanelNode)this._node).verticalBottomInterval = value;
                }

                if (this._node is SpaceNode)
                {
                    ((SpaceNode)this._node).verticalBottomInterval = value;
                }
            }
        }


        public string contentVertical
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                string result = "none";

                if (this._node is PanelNode)
                {
                    result = ((PanelNode)this._node).contentVertical;
                }

                if (this._node is SpaceNode)
                {
                    result = ((SpaceNode)this._node).contentVertical;
                }

                return result;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                if (this._node is PanelNode)
                {
                    ((PanelNode)this._node).contentVertical = value;
                }

                if (this._node is SpaceNode)
                {
                    ((SpaceNode)this._node).contentVertical = value;
                }
            }
        }

        public float verticalInterval
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                float result = 0f;

                if (this._node is PanelNode)
                {
                    result = ((PanelNode)this._node).verticalInterval;
                }

                if (this._node is SpaceNode)
                {
                    result = ((SpaceNode)this._node).verticalInterval;
                }

                return result;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                if (this._node is PanelNode)
                {
                    ((PanelNode)this._node).verticalInterval = value;
                }

                if (this._node is SpaceNode)
                {
                    ((SpaceNode)this._node).verticalInterval = value;
                }
            }
        }

        public string contentForward
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                string result = "none";

                if (this._node is SpaceNode)
                {
                    result = ((SpaceNode)this._node).contentForward;
                }

                return result;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                if (this._node is SpaceNode)
                {
                    ((SpaceNode)this._node).contentForward = value;
                }
            }
        }

        public float forwardInterval
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                float result = 0f;

                if (this._node is SpaceNode)
                {
                    result = ((SpaceNode)this._node).forwardInterval;
                }

                return result;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                if (this._node is SpaceNode)
                {
                    ((SpaceNode)this._node).forwardInterval = value;
                }
            }
        }

        /// <summary>
        /// 样式
        /// </summary>
        public override string stylesheet
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node).stylesheet.Trim();
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                ((Node)this._node).stylesheet = value.Trim();
            }
        }

        /// <summary>
        /// 是否有Collider
        /// </summary>
        public override bool hasCollider
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node).hasCollider;
            }
        }

        /// <summary>
        /// 是否有Collider
        /// </summary>
        public override bool hasInteractionTouchable
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node).hasInteractionTouchable;
            }
        }

        /*
         * npc相关
         */
        /// <summary>
        /// npc
        /// </summary>
        public override NPC npc
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node).npc;
            }
        }

        /// <summary>
        /// NPC psth
        /// </summary>
        public override string npcPath
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node).npcPath;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                ((Node)this._node).npcPath = value;
            }
        }

        /// <summary>
        /// 是否允许节点被操纵
        /// </summary>
        public override bool enableManipulator
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node).enableManipulator;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                ((Node)this._node).enableManipulator = value;
            }
        }



        /// <summary>
        /// 事件中的Collider组件是否自动添加和回收
        /// </summary>
        public override bool collider
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node).collider;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                ((Node)this._node).collider = value;
            }
        }

        /// <summary>
        /// 触摸事件需要的InteractionTouchable组件是否自动添加和回收
        /// </summary>
        public override bool interactionTouchableAuto
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node).interactionTouchableAuto;
            }

            set
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                ((Node)this._node).interactionTouchableAuto = value;
            }
        }

        #endregion

        #region 事件
        /// <summary>
        /// 按下事件
        /// </summary>
        public override DownEvent onDown
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }
                
                return ((Node)this._node)?.onDown;
            }
        }

        /// <summary>
        /// 释放事件
        /// </summary>
        public override UpEvent onUp
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node)?.onUp;
            }
        }

        /// <summary>
        /// 单击事件
        /// </summary>
        public override ClickEvent onClick
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node)?.onClick;
            }
        }

        /// <summary>
        /// 拖动事件
        /// </summary>
        public override DraggedEvent onDragged
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node)?.onDragged;
            }
        }

        /// <summary>
        /// 触摸开始事件
        /// </summary>
        public override TouchStartedEvent onTouchStarted
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node)?.onTouchStarted;
            }
        }

        /// <summary>
        /// 触摸更新事件
        /// </summary>
        public override TouchUpdateEvent onTouchUpdate
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node)?.onTouchUpdate;
            }
        }

        /// <summary>
        /// 触摸完成事件
        /// </summary>
        public override TouchCompletedEvent onTouchCompleted
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node)?.onTouchCompleted;
            }
        }

        /// <summary>
        /// 插入节点前事件
        /// </summary>
        public override InsertEvent onInsert
        {
            get
            {
                throw new NodeException("OnInset event cannot be used on magic nodes");
            }
        }

        /// <summary>
        /// 插入节点完成事件
        /// </summary>
        public override InsertedEvent onInserted
        {
            get
            {
                throw new NodeException("OnInserted event cannot be used on magic nodes");
            }
        }

        /// <summary>
        /// 焦点进入事件
        /// </summary>
        public override FocusEnterEvent onFocusEnter
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node)?.onFocusEnter;
            }
        }

        /// <summary>
        /// 焦点改变事件
        /// </summary>
        public override FocusChangedEvent onFocusChanged
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node)?.onFocusChanged;
            }
        }

        /// <summary>
        /// 焦点退出事件
        /// </summary>
        public override FocusExitEvent onFocusExit
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                return ((Node)this._node)?.onFocusExit;
            }
        }

        /// <summary>
        /// 节点尺寸改变开始事件
        /// </summary>
        public BoundScaleStartedEvent onBoundScaleStarted
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                if (!(this._node is ISizeBoundsEventNode))
                {
                    throw new NodeException("The child node under the magic node does not have an onBoundScaleStarted event");
                }

                return ((ISizeBoundsEventNode)this._node).onBoundScaleStarted;
            }
        }

        /// <summary>
        /// 节点尺寸改变开始事件
        /// </summary>
        public BoundScaleEndedEvent onBoundScaleEnded
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                if (!(this._node is ISizeBoundsEventNode))
                {
                    throw new NodeException("The child node under the magic node does not have an onBoundScaleEnded event");
                }

                return ((ISizeBoundsEventNode)this._node).onBoundScaleEnded;
            }
        }

        /// <summary>
        /// 碰撞进入事件类
        /// </summary>
        public CollisionEnterEvent onCollisionEnter
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                if (!(this._node is ICollisionEventFeature))
                {
                    throw new NodeException("The child node under the magic node does not have an onCollisionEnter event");
                }

                return ((ICollisionEventFeature)this._node).onCollisionEnter;
            }
        }

        /// <summary>
        /// 碰撞中事件类
        /// </summary>
        public CollisionStayEvent onCollisionStay
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                if (!(this._node is ICollisionEventFeature))
                {
                    throw new NodeException("The child nodes under the magic node do not have an onCollisionStay event");
                }

                return ((ICollisionEventFeature)this._node).onCollisionStay;

            }
        }

        /// <summary>
        /// 碰撞退出事件类
        /// </summary>
        public CollisionExitEvent onCollisionExit
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                if (!(this._node is ICollisionEventFeature))
                {
                    throw new NodeException("The child node under the magic node does not have an onCollisionExit event");
                }

                return ((ICollisionEventFeature)this._node).onCollisionExit;
            }
        }

        /// <summary>
        /// 节点操纵开始事件
        /// </summary>
        public ManipulationStartedEvent onManipulationStarted
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                if (!(this._node is IManipulationEventFeature))
                {
                    throw new NodeException("The child node under the magic node does not have an onManipulationStarted event");
                }

                return ((IManipulationEventFeature)this._node).onManipulationStarted;
            }
        }

        /// <summary>
        /// 节点操纵结束事件
        /// </summary>
        public ManipulationEndedEvent onManipulationEnded
        {
            get
            {
                if (this._node == null)
                {
                    throw new NodeException("The magic node does not have its child node");
                }

                if(!(this._node is IManipulationEventFeature))
                {
                    throw new NodeException("The child node under the magic node does not have an onManipulationEnded event");
                }

                return ((IManipulationEventFeature)this._node).onManipulationEnded;
            }
        }
        #endregion

        #region 基本方法
        /*-----------------------------------------定义节点事件检测相关方法开始----------------------------------------*/
        /// <summary>
        /// 检查节点是否包含Pointer事件
        /// </summary>
        /// <returns></returns>
        public override bool checkNodeIncludePointerEvent()
        {
            if (this._node == null)
            {
                throw new NodeException("The magic node does not have its child node");
            }

            return ((Node)this._node).checkNodeIncludePointerEvent();
        }

        /// <summary>
        /// 检查节点是否包含Touch事件
        /// </summary>
        /// <returns></returns>
        public override bool checkNodeIncludeTouchEvent()
        {
            if (this._node == null)
            {
                throw new NodeException("The magic node does not have its child node");
            }

            return ((Node)this._node).checkNodeIncludeTouchEvent();
        }

        /// <summary>
        /// 检查节点是否包含Focus事件
        /// </summary>
        /// <returns></returns>
        public override bool checkNodeIncludeFocusEvent()
        {
            if (this._node == null)
            {
                throw new NodeException("The magic node does not have its child node");
            }

            return ((Node)this._node).checkNodeIncludeFocusEvent();
        }

        /// <summary>
        /// 检查节点是否不包含Pointer事件
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override bool checkNodeNotIncludePointerEvent()
        {
            if (this._node == null)
            {
                throw new NodeException("The magic node does not have its child node");
            }

            return ((Node)this._node).checkNodeNotIncludePointerEvent();
        }

        /// <summary>
        /// 检查节点是否不包含Touch事件
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override bool checkNodeNotIncludeTouchEvent()
        {
            if (this._node == null)
            {
                throw new NodeException("The magic node does not have its child node");
            }

            return ((Node)this._node).checkNodeNotIncludeTouchEvent();
        }

        /// <summary>
        /// 检查节点是否不包含Focus事件
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override bool checkNodeNotIncludeFocusEvent()
        {
            if (this._node == null)
            {
                throw new NodeException("The magic node does not have its child node");
            }

            return ((Node)this._node).checkNodeNotIncludeFocusEvent();
        }
        /*-----------------------------------------定义节点事件检测相关方法结束----------------------------------------*/


        /*---------------------------------------定义尺寸相关的行为的相关方法开始--------------------------------------*/
        public override void borderFresh()
        {
            if (this._node == null)
            {
                throw new NodeException("The magic node does not have its child node");
            }

            if (this._node != null)
            {
                ((Node)this._node).borderFresh();
            }
        }
        /*---------------------------------------定义尺寸相关的行为的相关方法结束--------------------------------------*/

        /// <summary>
        /// npc刷新
        /// </summary>
        public override void npcFresh()
        {
            if (this._node == null)
            {
                throw new NodeException("The magic node does not have its child node");
            }

            ((Node)this._node).npcFresh();
        }


        /*-----------------------------------------定义锚行为的相关方法开始-----------------------------------------*/
        /// <summary>
        /// 添加锚
        /// </summary>
        /// <param name="name">锚名称</param>
        /// <param name="anchor">要添加的锚对象</param>
        public override void addAnchor(string name, Anchor anchor)
        {
            if (this._node == null)
            {
                throw new NodeException("The magic node does not have its child node");
            }

            ((Node)this._node).addAnchor(name, anchor);
        }

        /// <summary>
        /// 获取锚
        /// </summary>
        /// <param name="name">要查询的锚的名称</param>
        /// <returns></returns>
        public override Anchor getAnchor(string name)
        {
            if (this._node == null)
            {
                throw new NodeException("The magic node does not have its child node");
            }

            return ((Node)this._node).getAnchor(name);
        }

        /// <summary>
        /// 删除锚
        /// </summary>
        /// <param name="name">要移除的锚的名称</param>
        /// <returns></returns>
        public override Anchor removeAnchor(string name)
        {
            if (this._node == null)
            {
                throw new NodeException("The magic node does not have its child node");
            }

            return ((Node)this._node).removeAnchor(name);
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
        public override BoxCollider addCollider(Vector3 center, Vector3 size, string name = "")
        {
            if (this._node == null)
            {
                throw new NodeException("The magic node does not have its child node");
            }

            return ((Node)this._node).addCollider(center, size, name);
        }

        /// <summary>
        /// 按名称查找collider
        /// </summary>
        /// <param name="name">要查找的collider名称</param>
        /// <returns></returns>
        public override BoxCollider findColliderByName(string name)
        {
            if (this._node == null)
            {
                throw new NodeException("The magic node does not have its child node");
            }

            return ((Node)this._node).findColliderByName(name);
        }

        /// <summary>
        /// 移除BoxCollider
        /// </summary>
        /// <param name="collider">要移除的collider</param>
        public override void removeCollider(BoxCollider collider)
        {
            if (this._node == null)
            {
                throw new NodeException("The magic node does not have its child node");
            }

            ((Node)this._node).removeCollider(collider);
        }

        /// <summary>
        /// 移除BoxCollider
        /// </summary>
        /// <param name="name">名称</param>
        public override void removeCollider(string name)
        {
            if (this._node == null)
            {
                throw new NodeException("The magic node does not have its child node");
            }

            ((Node)this._node).removeCollider(name);
        }

        /// <summary>
        /// 添加autoCollider
        /// </summary>
        public override void addAutoCollider()
        {
            if (this._node == null)
            {
                throw new NodeException("The magic node does not have its child node");
            }

            ((Node)this._node).addAutoCollider();
        }
        /*---------------------------------------定义Collider行为的相关方法结束--------------------------------------*/

        /// <summary>
        /// 自动添加Touchable组件
        /// </summary>
        public override void addAutoInteractionTouchable()
        {
            if (this._node == null)
            {
                throw new NodeException("The magic node does not have its child node");
            }

            ((Node)this._node).addAutoInteractionTouchable();
        }

        /// <summary>
        /// 添加Touchable
        /// </summary>
        /// <param name="center">中心点</param>
        /// <param name="size">尺寸</param>
        /// <param name="name">名称</param>
        public override void addInteractionTouchable(string name, Vector3? center = null, Vector3? size = null)
        {
            if (this._node == null)
            {
                throw new NodeException("The magic node does not have its child node");
            }

            ((Node)this._node).addInteractionTouchable(name, center, size);
        }

        /// <summary>
        /// 移除InteractionTouchable
        /// </summary>
        /// <param name="interactionTouchable">要移除的InteractionTouchable</param>
        public override void removeInteractionTouchable(object interactionTouchable)
        {
            if (this._node == null)
            {
                throw new NodeException("The magic node does not have its child node");
            }

            ((Node)this._node).removeInteractionTouchable(interactionTouchable);
        }

        /// <summary>
        /// 移除InteractionTouchable
        /// </summary>
        /// <param name="name">名称</param>
        public override void removeInteractionTouchable(string name)
        {
            if (this._node == null)
            {
                throw new NodeException("The magic node does not have its child node");
            }

            ((Node)this._node).removeInteractionTouchable(name);
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
        public override void animation(Vector3 start, Vector3 end, float time, MotionCurve curveType, AnimationAction action, AnimationCallback callback = null)
        {
            if (this._node == null)
            {
                throw new NodeException("The magic node does not have its child node");
            }

            ((Node)this._node).animation(start, end, time, curveType, action, callback);
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
        public override void animation(float start, float end, float time, MotionCurve curveType, AnimationActionFloat action, AnimationCallback callback = null)
        {
            if (this._node == null)
            {
                throw new NodeException("The magic node does not have its child node");
            }

            ((Node)this._node).animation(start, end, time, curveType, action, callback);
        }
        /*----------------------------------------定义动画行为的相关方法结束----------------------------------------*/
        #endregion

        #region 节点数据结构基本操作
        /// <summary>
        /// 插入子节点
        /// </summary>
        /// <param name="current">插入的节点</param>
        /// <param name="refNode">参照节点，该节点必须为直接子节点</param>
        /// <returns></returns>
        public override Node insertBefore(Node current, Node refNode)
        {
            // 返回插入结果
            throw new NodeException("Magic node cannot insert Node");
        }

        /// <summary>
        /// 插入子节点
        /// </summary>
        /// <param name="current">插入的节点</param>
        /// <param name="refComponent">参照组件，该节点必须为直接子节点</param>
        /// <returns></returns>
        public override Node insertBefore(Node current, Component refComponent)
        {
            // 返回插入结果
            throw new NodeException("Magic node cannot insert Node");
        }

        /// <summary>
        /// 插入元素
        /// </summary>
        /// <param name="current">要插入的组件</param>
        /// <param name="refElement">参照节点</param>
        /// <returns></returns>
        public override Component insertComponent(Component component, Node refNode)
        {
            // 返回插入结果
            throw new NodeException("Magic node cannot insert components");
        }

        /// <summary>
        /// 插入元素
        /// </summary>
        /// <param name="current">要插入的组件</param>
        /// <param name="refElement">参照节点</param>
        /// <returns></returns>
        public override Component insertComponent(Component component, Component refComponent)
        {
            // 返回插入结果
            throw new NodeException("Magic node cannot insert components");
        }

        /// <summary>
        /// 插入元素
        /// </summary>
        /// <param name="current">要插入的元素</param>
        /// <param name="refElement">参照元素</param>
        /// <returns></returns>
        public override Element insertElement(Element current, Element refElement)
        {
            throw new NodeException("Magic node cannot insert Node");
        }
        #endregion
    }
}
