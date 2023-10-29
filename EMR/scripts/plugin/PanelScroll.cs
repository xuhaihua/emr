using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using EMR.Entity;
using EMR.Common;
using EMR.Struct;
using EMR.Event;
using EMR.Layout;


namespace EMR.Plugin
{
    /// <summary>
    /// 滚动组件状态
    /// </summary>
    public enum ScrollCompontentState
    {
        normal,
        init,
        initFinish,
        ready,
        readyFinish
    }

    /// <summary>
    /// 滚动配制结构体
    /// </summary>
    public class ScrollConfig
    {
        /// <summary>
        /// 晶格宽度
        /// </summary>
        public float cellWidth;

        /// <summary>
        /// 晶格高度
        /// </summary>
        public float cellHeight;

        /// <summary>
        /// 晶格深度
        /// </summary>
        public float cellDepth;

        /// <summary>
        /// // EmptyScrll (滚动组件ScrollCollection的载体)的位置
        /// </summary>
        public Vector3 scrollCenter;

        /// <summary>
        /// // EmptyScrll (滚动组件ScrollCollection的载体)的尺寸
        /// </summary>
        public Vector3 scrollSize;

        /// <summary>
        /// EmptyScrll(滚动组件ScrollCollection的载体)的碰撞体位置
        /// </summary>
        public Vector3 colliderCenter;

        /// <summary>
        /// EmptyScrll(滚动组件ScrollCollection的载体)的碰撞体尺寸
        /// </summary>
        public Vector3 colliderSize;

        /// <summary>
        /// 裁切对象的位置
        /// </summary>
        public Vector3 clippingCenter;

        /// <summary>
        /// 裁切对象的尺寸
        /// </summary>
        public Vector3 clippingSize;

        public float thickness;
    }

    /// <summary>
    /// 本类主要用于间接为node提供滚动支持
    /// </summary>
    public class PanelScroll : IPlugin
    {
        // 滚动所作用的节点
        private PanelNode _node;

        // 配制
        private ScrollConfig _config;

        // 滚动条组件
        public PanelScrollingCollection scrollCollection;

        // ScrollingCollection组件寄生对象
        private GameObject _emptyScroll;

        // 滚动交互碰撞器
        private BoxCollider _scrollBoxCollider;

        // 裁切对象
        private GameObject _clippingBounds;

        // 滚动容器对象
        private GameObject _scrollContainer;

        public ScrollCompontentState state = ScrollCompontentState.init;

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="node">节点</param>
        /// <param name="config">滚动配制</param>
        public PanelScroll(PanelNode node, ScrollConfig config)
        {
            // 滚动作用的节点
            this._node = node;

            // 滚动的基本配制
            this._config = config;


            // 滚动准备就绪处理事件方法
            this._node.onScrollReady.AddListener((ScrollReadyEventData eventData) =>
            {
                PanelLayout.update(new PanelNodeActionInfo
                {
                    current = this._node,
                    action = PanelNodeAction.scrollReady
                });

                this.state = ScrollCompontentState.readyFinish;
            });
        }

        public GameObject scrollContainer
        {
            get
            {
                return this._scrollContainer;
            }
        }

        public BoxCollider scrollBoxCollider
        {
            get
            {
                return this.scrollCollection.ScrollingCollider;
            }
        }

        public NearInteractionTouchable scrollingTouchable
        {
            get
            {
                return this.scrollCollection.ScrollingTouchable;
            }
        }

        /// <summary>
        /// 滚动方向
        /// </summary>
        public PanelScrollingCollection.ScrollDirectionType scrollDirection
        {
            get
            {
                return this.scrollCollection.ScrollDirection;
            }

            set
            {
                
                if (this.scrollCollection != null)
                {
                    this.scrollCollection.ScrollDirection = value;

                    if (this.scrollDirection == PanelScrollingCollection.ScrollDirectionType.UpAndDown)
                    {
                        // 计算每页行数 (当垂直滚动时
                        this.scrollCollection.TiersPerPage = (int)(Space.Unit.unitToScale(this._node, this._node.height, Axle.up) / this._config.cellHeight);

                        // 计算每页列数 (当水平滚动时)
                        this.scrollCollection.CellsPerTier = 1;
                    }
                    else
                    {
                        // 计算每页行数 (当垂直滚动时
                        this.scrollCollection.TiersPerPage = 1;

                        // 计算每页列数 (当水平滚动时)
                        this.scrollCollection.CellsPerTier = (int)(Space.Unit.unitToScale(this._node, this._node.width, Axle.right) / this._config.cellWidth);
                    }
                }
            }
        }


        /// <summary>
        /// 是否可以滚动
        /// </summary>
        public bool canScroll
        {
            get
            {
                return this.scrollCollection.CanScroll;
            }

            set
            {
                this.scrollCollection.CanScroll = value;
            }
        }

        public ClippingBox clipBox
        {
            get
            {
                return this.scrollCollection.ClipBox;
            }
        }


        /// <summary>
        /// 刷新（正式为当前节点添加滚动组件ScrollCollection，调整及跟踪组件初始化的相关参数等）
        /// </summary>
        public void fresh()
        {
            // 创建一个空游戏对象用作ScrollingCollection组件的寄生体 (该游戏对象将作为滚动对象的子对象存在)
            this._emptyScroll = new GameObject();
            this._emptyScroll.name = "EmptyScroll";

            this._emptyScroll.transform.parent = this._node.parasitifer.transform;

            // EmptyScroll中心位置、尺寸使其与父对象对齐 (EmptyScroll覆盖的区域将只显示父对象)
            this._emptyScroll.transform.localPosition = this._config.scrollCenter;
            this._emptyScroll.transform.localScale = this._config.scrollSize;


            // 向EmptyScroll游戏对象添加ScrollingCollection组件
            this.scrollCollection = this._emptyScroll.AddComponent<PanelScrollingCollection>();
            this.scrollCollection._node = this._node;
            this.scrollCollection.DisableClippedGameObjects = false;

            // 设置默认滚动方向
            this.scrollCollection.ScrollDirection = PanelScrollingCollection.ScrollDirectionType.UpAndDown;

            // 页面设置
            this.pagtionInit();

            // 从下一周期开始一直监控ScrollingCollection组件的各参数情况直到该组件稳定为止，止时滚动所需的结构已创建完成且isScrollInitFinish属性被设置为true，但此时组件还未处于真正的稳定状态，其中一些子对象的参数还并不稳定，isScrollReady属性仍未设置所有它为false
            this._node.service.next(() =>
            {
                return this.waitInitFinish();
            });

            this.scrollCollection.DisableClippedGameObjects = false;
        }

        /// <summary>
        /// 设置分页
        /// </summary>
        /// <param name="rowNumber">每页多少行</param>
        /// <param name="cellNumber">每页多少列</param>
        private void pagtionInit()
        {
            // 设置ScrollCollection组件配制
            this.scrollCollection.config = this._config;

            // 设置列尺寸
            this.scrollCollection.CellWidth = this._config.cellWidth;
            this.scrollCollection.CellHeight = this._config.cellHeight;
            this.scrollCollection.CellDepth = this._config.cellDepth;

        }

        /// <summary>
        /// 等待ScrollCollection组件初始化完成
        /// </summary>
        /// <returns></returns>
        private bool waitInitFinish()
        {
            bool result = false;

            //空滚动对象的碰撞体组件 (滚动容器对象定为一个空对象)
            if (this._scrollBoxCollider == null && this._emptyScroll != null)
            {
                this._scrollBoxCollider = this.scrollCollection.ScrollingCollider;
            }

            // 获取裁切对象
            if (this._clippingBounds == null && this._emptyScroll != null)
            {
                this._clippingBounds = this.scrollCollection.ClippingObject;
            }
            
            if (this._scrollContainer == null && this._emptyScroll != null)
            {
                this._scrollContainer = this.scrollCollection.ScrollContainer;
            }

            if (this._emptyScroll != null && this._scrollContainer != null && this._scrollBoxCollider != null && this._clippingBounds != null)
            {
                this.state = ScrollCompontentState.initFinish;

                /*
                    以下代码用于向Container对象向添加子对象
                 */
                // 将子节点加入列表
                List<GameObject> list = new List<GameObject>();
                for (var i = 0; i < this._node.parasitifer.transform.childCount; i++)
                {
                    var gameObject = this._node.parasitifer.transform.GetChild(i).gameObject;
                    if (gameObject != this.scrollCollection.ScrollContainer && gameObject != this._clippingBounds && gameObject != this._emptyScroll)
                    {
                        list.Add(gameObject);
                    }
                }

                this._emptyScroll.transform.localEulerAngles = new Vector3(0f, 0f, 0f);

                // 向container加入子节点
                foreach (var item in list)
                {
                    var orginLocalAngle = item.transform.localEulerAngles;

                    this.scrollCollection.AddContent(item);

                    item.transform.localEulerAngles = orginLocalAngle;
                }

                // 下一循环周期执行开始等待准备就绪
                this._node.service.next(() =>
                {
                    return this.waitScrollReady();
                });

                result = true;
            }

            return result;
        }

        /// <summary>
        /// 初始时需要完成的第一次刷新(已保证所有参数已设置入组件内的各对象中)
        /// </summary>
        /// <returns></returns>
        private bool waitScrollReady()
        {
            bool result = false;

            if (this.state == ScrollCompontentState.initFinish && Utils.equals(this._clippingBounds.transform.localPosition.x, 0))
            {
                this.state = ScrollCompontentState.ready;

                // 执行滚动准备就绪事件
                this._node.onScrollReady.Invoke(new ScrollReadyEventData
                {
                    target = this._node,
                });

                result = true;
            }

            // 设置相关对象的相数
            else if (this.state == ScrollCompontentState.initFinish)
            {
                this._clippingBounds.transform.localPosition = this._config.clippingCenter;
                this._clippingBounds.transform.localScale = this._config.clippingSize;
            }

            return result;
        }


        /// <summary>
        /// 视图裁切
        /// </summary>
        /// <param name="isContentBoundFresh">是否需要内容刷新</param>
        /// <param name="isScrollPositionReset">是否需要将滚动位置重置于起点</param>
        public void clipView(bool isContentBoundFresh, bool isScrollPositionReset)
        {
            PanelNode parent = null;
            PanelNode temp = this._node.parent != null && this._node.parent is PanelNode ? (PanelNode) this._node.parent : null;

            // 向上查找第一个带滚动组件的节点
            while (temp != null && temp is Node)
            {
                if ((temp.getOverflow() != NodeOverflow.visible))
                {
                    if(parent == null)
                    {
                        parent = temp;
                    }
                }

                temp = temp.parent != null && temp.parent is PanelNode ? (PanelNode)temp.parent : null; 
            }

            if (parent != null)
            {

                // 将parent节点的世界坐标转至子节点node节点本地坐标
                var localCoord = Space.Coordinate.worldCoordToLocalCoordForGameObject(this._node.parasitifer.transform.parent.gameObject, parent.scrollComponent.scrollCollection.ClipBox.transform.position);
                var parentPosX = localCoord.x;
                var parentPosY = localCoord.y;
                var parentPosZ = localCoord.z;

                // 将parentBounds的世界尺寸转至子节点node的本地尺寸
                var parentClipSize = parent.scrollComponent.scrollCollection.ClipBox.transform.lossyScale * 1000f;
                var parentSizeWidth = Space.Unit.unitToScale(this._node, parentClipSize.x, Axle.right);
                var parentSizeHeight = Space.Unit.unitToScale(this._node, parentClipSize.y, Axle.up);
                var parentSizeDepth = Space.Unit.unitToScale(this._node, parentClipSize.z, Axle.forward);


                // 获取node的本地坐标
                var nodePosX = this._node.parasitifer.transform.localPosition.x;
                var nodePosY = this._node.parasitifer.transform.localPosition.y;
                var nodePosZ = this._node.parasitifer.transform.localPosition.z;



                // var nodeBounds = this._node.getBound(topParent.scroll.scrollCollection.ScrollContainer);
                // 将nodeBounds单位尺寸尺寸转至node的本地比例
                var nodeBounds = this._node.localBounds;
                var nodeBoundWidth = Space.Unit.unitToScale(this._node, nodeBounds.size.x, Axle.right);
                var nodeBoundHeight = Space.Unit.unitToScale(this._node, nodeBounds.size.y, Axle.up);

                var nodeBoundDepth = 1f;
                if (!this._node.isPanel)
                {
                    nodeBoundDepth = Space.Unit.unitToScale(this._node, nodeBounds.size.z, Axle.forward);
                }

                /*
                 以下逻辑在同一坐标系下进行ClipBounds裁切运算
                 */
                // 此值为正，子节点在x轴负方向超出
                var offsetLeft = (parentPosX - parentSizeWidth / 2) - (nodePosX - nodeBoundWidth / 2);

                // 此值为负，子节点在x轴正方向超出
                var offsetRight = (parentPosX + parentSizeWidth / 2) - (nodePosX + nodeBoundWidth / 2);

                // 此值为负，子节点在y轴正方向超出
                var offsetTop = (parentPosY + parentSizeHeight / 2) - (nodePosY + nodeBoundHeight / 2);

                // 此值为正，子节点在y轴负方向超出
                var offsetBottom = (parentPosY - parentSizeHeight / 2) - (nodePosY - nodeBoundHeight / 2);

                // 此值为负，子节点在z轴正方向超出
                var offsetForward = (parentPosZ + parentSizeDepth / 2) - (nodePosZ + nodeBoundDepth / 2);

                // 此值为正，子节点在z轴负方向超出
                var offsetBack = (parentPosZ - parentSizeDepth / 2) - (nodePosZ - nodeBoundDepth / 2);


                // 计算裁切后的尺寸
                var width = nodeBoundWidth;
                if (offsetLeft > 0)
                {
                    width -= offsetLeft;
                }
                if (offsetRight < 0)
                {
                    width += offsetRight;
                }

                var height = nodeBoundHeight;
                if (offsetTop < 0)
                {
                    height += offsetTop;
                }
                if (offsetBottom > 0)
                {
                    height -= offsetBottom;
                }

                var depth = nodeBoundDepth;
                if (offsetForward < 0)
                {
                    depth += offsetForward;
                }
                if (offsetBack > 0)
                {
                    depth -= offsetBack;
                }

                // 计算裁切后裁切节点的坐标位置偏移量
                var offsetX = 0f;
                if (offsetLeft > 0)
                {
                    offsetX += offsetLeft / 2;
                }
                if (offsetRight < 0)
                {
                    offsetX += offsetRight / 2;
                }

                var offsetY = 0f;
                if (offsetTop < 0)
                {
                    offsetY += offsetTop / 2;
                }
                if (offsetBottom > 0)
                {
                    offsetY += offsetBottom / 2;
                }

                var offsetZ = 0f;
                if (offsetBack > 0)
                {
                    offsetZ += offsetBack / 2;
                }
                if (offsetForward < 0)
                {
                    offsetZ += offsetForward / 2;
                }

                
                var center = new Vector3(nodePosX, nodePosY, nodePosZ) + new Vector3(offsetX, offsetY, offsetZ);
                var unitWidth = Space.Unit.scaleToUnit(this._node, width, Axle.right);
                var unitHeight = Space.Unit.scaleToUnit(this._node, height, Axle.up);
                var unitDepth = Space.Unit.scaleToUnit(this._node, depth, Axle.forward);

                // 将坐标尺寸转至ClipBounds所在坐标系
                var boundCenter = this._clippingBounds.transform.parent.transform.InverseTransformPoint(this._node.parasitifer.transform.parent.TransformPoint(center));
                var boundWidth = Space.Unit.unitToScaleForGameObject(this._clippingBounds, unitWidth, Axle.right);
                var boundHeight = Space.Unit.unitToScaleForGameObject(this._clippingBounds, unitHeight, Axle.up);
                var boundDepth = Space.Unit.unitToScaleForGameObject(this._clippingBounds, unitDepth, Axle.forward);

                var boundSize = new Vector3(boundWidth, boundHeight, boundDepth);

                if (boundWidth < 0 || boundHeight < 0 || boundDepth < 0)
                {
                    this.scrollCollection.ScrollContainer.SetActive(false);
                }
                else
                {
                    this.scrollCollection.ScrollContainer.SetActive(true);
                }

                // 设置裁切对象
                this._clippingBounds.transform.localPosition = boundCenter;
                if (!float.IsNaN(boundSize.x) && !float.IsNaN(boundSize.y))
                {
                    this._clippingBounds.transform.localScale = boundSize;
                }

                // 滚动Collider刷新
                this.scrollColliderFresh();
            }
           
            if (isContentBoundFresh)
            {
                parent?.scrollComponent.scrollCompontentFresh(isScrollPositionReset);
                this.scrollCompontentFresh(isScrollPositionReset);
            }
        }

        /// <summary>
        /// 向滚动组件新增或删除完子对象后Content位置会改变，为保证与添加前一致，使用nextUpdate复位添加前的position
        /// </summary>
        /// <returns></returns>
        private bool nodeNumberChangeScrollFresh(float x, float y, float z)
        {
            bool result = false;

            if (Utils.equals(this._scrollContainer.transform.localPosition.x, x) && Utils.equals(this._scrollContainer.transform.localPosition.y, y) && Utils.equals(this._scrollContainer.transform.localPosition.z, z))
            {
                result = true;
            }

            this._scrollContainer.transform.localPosition = new Vector3(x, y, z);

            return result;
        }


        /// <summary>
        /// 当尺寸发生改变时需要重新校正的参数
        /// </summary>
        public void scrollCompontentFresh(bool isScrollPositionReset)
        {
            if (this.state == ScrollCompontentState.ready || this.state == ScrollCompontentState.readyFinish)
            {
                this.scrollCollection.fresh(isScrollPositionReset);
            }
        }


        /// <summary>
        /// 滚动组件Collider刷新
        /// </summary>
        public void scrollColliderFresh()
        {
            // collider世界坐标与尺寸 == ClipBox的世界坐标与尺寸
            var colliderWorldPosition = this._clippingBounds.transform.position;
            var colliderWorldSize = this._clippingBounds.transform.lossyScale;

            // 转换至本地
            var colliderLocalPosition = this._emptyScroll.transform.InverseTransformPoint(colliderWorldPosition);
            var colliderLocalWidth = Space.Unit.unitToScaleForGameObject(this._scrollContainer, colliderWorldSize.x * 1000f, Axle.right);
            var colliderLocalHeight = Space.Unit.unitToScaleForGameObject(this._scrollContainer, colliderWorldSize.y * 1000f, Axle.up);


            // 设置Collider位置与尺寸
            this._scrollBoxCollider.center = new Vector3(colliderLocalPosition.x, colliderLocalPosition.y, this._scrollBoxCollider.center.z);
            this._scrollBoxCollider.size = new Vector3(colliderLocalWidth, colliderLocalHeight, this._node.isPanel ? Space.Unit.unitToScale(this._node, Space.zero, Axle.forward) : 1f );


            // 设置Tuchable
            this.scrollCollection.ScrollingTouchable.SetBounds(new Vector2(this._scrollBoxCollider.size.x, this._scrollBoxCollider.size.y));
            this.scrollCollection.ScrollingTouchable.SetLocalCenter(new Vector3(colliderLocalPosition.x, colliderLocalPosition.y, -Space.Unit.unitToScaleForGameObject(this._scrollContainer, this._config.thickness, Axle.forward) / 2));
        }

        /// <summary>
        /// 可视区刷新
        /// </summary>
        public static void viewFresh(PanelNode node, bool isContentFresh, bool isScrollPositionReset)
        {
            if (node.scrollComponentState == ScrollCompontentState.ready || node.scrollComponentState == ScrollCompontentState.readyFinish)
            {
                node.hasViewFresh = true;
                if(isContentFresh)
                {
                    node.isContentBoundFresh = isContentFresh;
                }
                if(isScrollPositionReset)
                {
                    node.isScrollPositionReset = isScrollPositionReset;
                }
            }

            foreach (var item in node.children)
            {
                viewFresh(item, isContentFresh, isScrollPositionReset);
            }
        }

        /// <summary>
        /// 向滚动组件添加内容
        /// </summary>
        /// <param name="content"></param>
        public void addContent(GameObject content)
        {
            // 向组件添加子对象
            this.scrollCollection.AddContent(content);
        }

        /// <summary>
        /// 从滚动组件移除内容
        /// </summary>
        /// <param name="content"></param>
        public void removeContent(GameObject content)
        {
            // 保存移除前的容器坐标位置
            var x = this._scrollContainer.transform.localPosition.x;
            var y = this._scrollContainer.transform.localPosition.y;
            var z = this._scrollContainer.transform.localPosition.z;

            this.scrollCollection.RemoveItem(content);

            // 从下一周期开始对滚动组件刷新直至滚动容器的坐标与移除前一致
            this._node.service.next(() =>
            {
                return this.nodeNumberChangeScrollFresh(x, y, z);
            });
        }

        /// <summary>
        /// 释放滚动组件
        /// </summary>
        public void destory()
        {
            if (this._clippingBounds != null)
            {
                // 将子节点加入列表
                List<GameObject> list = new List<GameObject>();
                for (var i = 0; i < this._scrollContainer.transform.childCount; i++)
                {
                    list.Add(this._scrollContainer.transform.GetChild(i).gameObject);
                }

                // 向container加入子节点
                foreach (var item in list)
                {
                    item.transform.parent = this._node.parasitifer.transform;
                }

                // 禁止滚动条关闭事件循环
                this.scrollCollection.enabled = false;

                // 移除滚动容器节点
                GameObject.Destroy(this._emptyScroll);

                // 置空相关滚动变量
                this._emptyScroll = null;
                this._scrollContainer = null;
                this._scrollBoxCollider = null;
                this._clippingBounds = null;
                this.scrollCollection = null;
            }
        }
    }
}
