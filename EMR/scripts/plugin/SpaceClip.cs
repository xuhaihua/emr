using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using EMR.Entity;
using EMR.Common;

namespace EMR.Plugin
{
    /// <summary>
    /// 本类主要用于间接为node提供Clip支持
    /// </summary>
    public class SpaceClip : IPlugin
    {
        // 滚动所作用的节点
        private SpaceNode _node;

        // 配制
        private ScrollConfig _config;

        // 滚动条组件
        public SpaceScrollingCollection scrollCollection;

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
        public SpaceClip(SpaceNode node, ScrollConfig config)
        {
            // 滚动作用的节点
            this._node = node;

            // 滚动的基本配制
            this._config = config;
        }

        public GameObject scrollContainer
        {
            get
            {
                return this._scrollContainer;
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
            this.scrollCollection = this._emptyScroll.AddComponent<SpaceScrollingCollection>();
            this.scrollCollection._node = this._node;
            this.scrollCollection.DisableClippedGameObjects = false;

            this.scrollCollection.CanScroll = false;


            // 页面设置
            this.pagtionInit();

            // 从下一周期开始一直监控ScrollingCollection组件的各参数情况直到该组件稳定为止，止时滚动所需的结构已创建完成且isScrollInitFinish属性被设置为true，但此时组件还未处于真正的稳定状态，其中一些子对象的参数还并不稳定，isScrollReady属性仍未设置所有它为false
            this._node.service.next(() =>
            {
                return this.waitInitFinish();
            });
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

                GameObject.Destroy(this._scrollBoxCollider);

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
                this.state = ScrollCompontentState.readyFinish;

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
        /// 向滚动组件添加内容
        /// </summary>
        /// <param name="content"></param>
        public void addContent(GameObject content)
        {
            content.transform.SetParent(this._scrollContainer.transform);
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
