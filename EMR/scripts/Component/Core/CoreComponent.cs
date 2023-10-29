using System;
using System.Xml;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using EMR.Common;
using EMR.Entity;
using EMR.Event;
using EMR.Plugin;
using EMR.Struct;
using static Microsoft.MixedReality.Toolkit.UI.PrefabSpawner;
using static EMR.CoreComponent;

namespace EMR
{
    public abstract partial class CoreComponent : EMR.Common.DataStructure.Node
    {
        #region 基本字段

        /// <summary>
        /// 组件文档样式表
        /// </summary>
        internal List<Stylesheet> styleSheets = new List<Stylesheet>();

        /// <summary>
        /// 组件根节点列表
        /// </summary>
        private List<Element> _rootElementChilds = new List<Element>();
        #endregion

        /// <summary>
        /// 构造器
        /// </summary>
        public CoreComponent()
        {
            this.create();
        }

        #region 基本属性
        /// <summary>
        /// 组件所在的room;
        /// </summary>
        public Room room
        {
            get
            {
                Room result = null;
                var temp = this;
                while (temp.parent != null)
                {
                    temp = (CoreComponent)temp.parent;
                }

                if (temp is Room)
                {
                    result = (Room)temp;
                }

                return result;
            }
        }

        /// <summary>
        /// 组件的根节点集合
        /// </summary>
        public List<Node> rootNodeChilds
        {
            get
            {
                // 将当前组件的所有根节点加入结果集
                List<Node> result = new List<Node>();

                foreach (var item in this._rootElementChilds)
                {
                    if (item is Node)
                    {
                        result.Add((Node)item);
                    }

                    if (item is Component)
                    {
                        foreach (var node in ((Component)item).rootNodeChilds)
                        {
                            if (node is Node)
                            {
                                result.Add((Node)node);
                            }
                        }
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// 组件默认视图文档
        /// </summary>
        public virtual string defaultDocument
        {
            get
            {
                return "";
            }
        }

        private string _document = "";
        /// <summary>
        /// 视图文档
        /// </summary>
        public string document
        {
            get
            {
                string result;
                if(this._document == "")
                {
                    result = this.defaultDocument;
                } else
                {
                    result = this._document;
                }

                return result;
            }

            set
            {
                this._document = value;
            }
        }

        protected bool _isDestoring = false;
        /// <summary>
        /// 组件是否正在销毁
        /// </summary>
        public bool isDestoring
        {
            get
            {
                return this._isDestoring;
            }
        }
        #endregion

        #region 生命周期勾子方法
        /// <summary>
        /// 开始创建生命周期
        /// </summary>
        protected virtual void create()
        {
        }

        /// <summary>
        /// 创建完成生命周期
        /// </summary>
        protected virtual void created()
        {
        }

        /// <summary>
        /// 创建已完成并已加入空间
        /// </summary>
        protected virtual void mounted()
        {
        }

        /// <summary>
        /// 开始销毁前
        /// </summary>
        protected virtual void destoryBefore()
        {

        }

        /// <summary>
        /// 销毁完成
        /// </summary>
        protected virtual void destoryed()
        {

        }
        #endregion

        #region 组件功能方法
        /// <summary>
        /// 创建PanelRoot节点
        /// </summary>
        /// <param name="x">x轴坐标</param>
        /// <param name="y">y轴坐标</param>
        /// <param name="z">z轴坐标</param>
        /// <param name="xAngle">x轴旋转量</param>
        /// <param name="yAngle">y轴旋转量</param>
        /// <param name="zAngle">z轴旋转量</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="depth">深度</param>
        /// <param name="isPanel">是否平面</param>
        /// <param name="npc">npc</param>
        /// <param name="rendMode">渲染模式</param>
        /// <returns></returns>
        protected PanelRoot createPanelRoot(float x, float y, float z, float xAngle, float yAngle, float zAngle, float width, float height, float depth, bool isPanel = true, NPC npc = null, RendMode rendMode = RendMode.opaque)
        {
            var node = new PanelRoot(x, y, z, xAngle, yAngle, zAngle, width, height, depth, this, isPanel, npc, rendMode);
            node.fullName = node.GetType().ToString();
            this._rootElementChilds.Add(node);

            node.isCreating = true;

            var rootNodeChilds = this.rootNodeChilds;
            var rootNode = rootNodeChilds.Count > 0 ? rootNodeChilds[0] : null;
            if (rootNode != null && rootNode.parent != null)
            {
                var rootNodeParent = rootNode.parent;
                rootNodeParent.appendNode(node);
            }

            node.isCreating = false;

            // 设置tag样式
            foreach (var styleSheet in this.styleSheets)
            {
                // 设置component的样式
                if (styleSheet != null)
                {
                    string tag = node.fullName;

                    // 设置该节点样式
                    styleSheet.setStyle(node, tag, null, null, null);
                }
            }

            return node;
        }

        /// <summary>
        /// 创建PanelLayer节点
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="right"></param>
        /// <param name="bottom"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="depth"></param>
        /// <param name="zIndex"></param>
        /// <param name="isPanel"></param>
        /// <param name="npc"></param>
        /// <param name="rendMode"></param>
        /// <returns></returns>
        protected PanelLayer createPanelLayer(float? left, float? top, float? right, float? bottom, float width = 0, float height = 0, float depth = 0, int zIndex = 0, bool isPanel = true, NPC npc = null, RendMode rendMode = RendMode.opaque)
        {
            var node = new PanelLayer(left, top, right, bottom, width, height, depth, this, zIndex, isPanel, npc, rendMode);
            node.fullName = node.GetType().ToString();
            this._rootElementChilds.Add(node);

            node.isCreating = true;

            var rootNodeChilds = this.rootNodeChilds;
            var rootNode = rootNodeChilds.Count > 0 ? rootNodeChilds[0] : null;
            if(rootNode != null && rootNode.parent != null)
            {
                var rootNodeParent = rootNode.parent;
                rootNodeParent.appendNode(node);
            }

            node.isCreating = false;

            // 设置tag样式
            foreach (var styleSheet in this.styleSheets)
            {
                // 设置component的样式
                if (styleSheet != null)
                {
                    string tag = node.fullName;

                    // 设置该节点样式
                    styleSheet.setStyle(node, tag, null, null, null);
                }
            }

            return node;
        }

        /// <summary>
        /// 创建空间节点
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="xAngle"></param>
        /// <param name="yAngle"></param>
        /// <param name="zAngle"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="depth"></param>
        /// <param name="npc"></param>
        /// <param name="rendMode"></param>
        /// <returns></returns>
        protected SpaceNode createSpanceNode(float x, float y, float z, float xAngle, float yAngle, float zAngle, float width, float height, float depth, NPC npc = null, RendMode rendMode = RendMode.opaque)
        {
            var node = new SpaceNode(x, y, z, xAngle, yAngle, zAngle, width, height, depth, this, npc, rendMode);
            node.fullName = node.GetType().ToString();
            this._rootElementChilds.Add(node);

            node.isCreating = true;

            var rootNodeChilds = this.rootNodeChilds;
            var rootNode = rootNodeChilds.Count > 0 ? rootNodeChilds[0] : null;
            if (rootNode != null && rootNode.parent != null)
            {
                var rootNodeParent = rootNode.parent;
                rootNodeParent.appendNode(node);
            }

            node.isCreating = false;

            // 设置tag样式
            foreach (var styleSheet in this.styleSheets)
            {
                // 设置component的样式
                if (styleSheet != null)
                {
                    string tag = node.fullName;

                    // 设置该节点样式
                    styleSheet.setStyle(node, tag, null, null, null);
                }
            }

            return node;
        }

        /// <summary>
        /// 创建魔法节点
        /// </summary>
        /// <returns></returns>
        protected SpaceMagic createSpaceMagic()
        {
            var node = new SpaceMagic(this);
            node.fullName = node.GetType().ToString();
            this._rootElementChilds.Add((Element)node);

            node.isCreating = true;

            var rootNodeChilds = this.rootNodeChilds;
            var rootNode = rootNodeChilds.Count > 0 ? rootNodeChilds[0] : null;
            if (rootNode != null && rootNode.parent != null)
            {
                var rootNodeParent = rootNode.parent;
                rootNodeParent.appendNode(node);
            }

            node.isCreating = false;

            return node;
        }

        /// <summary>
        /// 创建组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T createComponent<T>() where T : Component
        {
            T result;
            var type = typeof(T);
            result = (T) type.Assembly.CreateInstance(type.ToString(), true, System.Reflection.BindingFlags.Default, null, null, null, null);
            result.fullName = result.GetType().ToString();

            result.isCreating = true;

            result.assemble();
            this.appendNode(result);
            this._rootElementChilds.Add(result);

            var rootNodeChilds = this.rootNodeChilds;
            var rootNode = rootNodeChilds.Count > 0 ? rootNodeChilds[0] : null;
            if (rootNode != null && rootNode.parent != null)
            {
                var rootNodeParent = rootNode.parent;
                rootNodeParent.appendComponent(result);
            }

            result.isCreating = false;

            // 设置tag样式
            foreach (var styleSheet in this.styleSheets)
            {
                // 设置component的样式
                if (styleSheet != null)
                {
                    string tag = result.fullName;

                    // 设置该节点样式
                    styleSheet.setStyle(result, tag, null, null, null);
                }
            }

            return result;
        }

        /// <summary>
        /// 创建组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public T createComponent<T>(object[] parameters) where T : Component
        {
            T result;
            var type = typeof(T);
            result = (T)type.Assembly.CreateInstance(type.ToString(), true, System.Reflection.BindingFlags.Default, null, parameters, null, null);
            result.fullName = result.GetType().ToString();

            result.isCreating = true;

            result.assemble();
            this.appendNode(result);
            this._rootElementChilds.Add(result);
            
            var rootNodeChilds = this.rootNodeChilds;
            var rootNode = rootNodeChilds.Count > 0 ? rootNodeChilds[0] : null;
            if (rootNode != null && rootNode.parent != null)
            {
                var rootNodeParent = rootNode.parent;
                rootNodeParent.appendComponent(result);
            }
            result.isCreating = false;


            // 设置tag样式
            foreach (var styleSheet in this.styleSheets)
            {
                // 设置component的样式
                if (styleSheet != null)
                {
                    string tag = result.fullName;

                    // 设置该节点样式
                    styleSheet.setStyle(result, tag, null, null, null);
                }
            }

            return result;
        }

        /// <summary>
        /// 延迟执行
        /// </summary>
        /// <param name="delayTime"></param>
        /// <param name="handle"></param>
        public void delay(int delayTime, TimerCallbackHandle handle)
        {
            EMR.Room.timeOut(delayTime, handle);
        }

        /// <summary>
        /// 下一循环周期执行
        /// </summary>
        /// <param name="handle"></param>
        public void next(CirculateTaskHandler handle)
        {
            EMR.Space.mainService.next(handle);
        }

        /// <summary>
        /// 设置样式(针对Node)
        /// </summary>
        /// <param name="node"></param>
        /// <param name="styleName"></param>
        internal void setStyle(Node node, string styleName)
        {
            if (this.styleSheets.Count > 0)
            {
                foreach (var styleSheet in this.styleSheets)
                {
                    foreach (var item in styleSheet.classStyleCollect)
                    {
                        if (item.Key == styleName)
                        {
                            item.Value.setStyle(node);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 设置样式(针对Component)
        /// </summary>
        /// <param name="node"></param>
        /// <param name="styleName"></param>
        internal void setStyle(Component component, string styleName)
        {
            if (this.styleSheets.Count > 0)
            {
                foreach (var styleSheet in this.styleSheets)
                {
                    foreach (var item in styleSheet.classStyleCollect)
                    {
                        if (item.Key == styleName)
                        {
                            item.Value.setStyle(component);
                        }
                    }
                }
            }
        }

        internal void updateRemoveRootElement(Element element)
        {
            // 如果该节点原先在根节点中则需要从_rootElementChilds中将其移出
            if (this._rootElementChilds.IndexOf(element) != -1)
            {
                this._rootElementChilds.Remove(element);
            }
        }
        #endregion

        #region 组件节点基本操作
        public new CoreComponent parent
        {
            get
            {
                var result = base.parent;
                return result != null ? (CoreComponent)result : null;
            }
        }

        /// <summary>
        /// 第一个子节点
        /// </summary>
        internal new Component firstChild
        {
            get
            {
                var result = base.firstChild;
                return result != null ? (Component)result : null;
            }
        }

        /// <summary>
        /// 最后一个子节点
        /// </summary>
        public new Component lastChild
        {
            get
            {
                var result = base.lastChild;
                return result != null ? (Component)result : null;
            }
        }

        /// <summary>
        /// 前一个兄弟节点
        /// </summary>
        public new CoreComponent previousSibling
        {
            get
            {
                var result = base.previousSibling;
                return result != null ? (CoreComponent)result : null;
            }
        }

        /// <summary>
        /// 后一个兄弟节点
        /// </summary>
        public new CoreComponent nextSibling
        {
            get
            {
                var result = base.nextSibling;
                return result != null ? (CoreComponent)result : null;
            }
        }

        /// <summary>
        /// 子节点列表
        /// </summary>
        public new List<Component> children
        {
            get
            {
                var result = base.children;
                return result.ConvertAll(t => (Component)t);
            }
        }

        /// <summary>
        /// 查找所有子组件包括自身
        /// </summary>
        /// <param name="component"></param>
        /// <param name="result"></param>
        private void findAllComponent(CoreComponent component, ref List<CoreComponent> result)
        {
            result.Add(component);
            foreach (var item in component.children)
            {
                this.findAllComponent((CoreComponent)item, ref result);
            }
        }

        /// <summary>
        /// 销毁组件
        /// </summary>
        public virtual void destory()
        {
            this.destoryBefore();

            this._isDestoring = true;

            // 销毁节点
            foreach (var item in this._rootElementChilds)
            {
                item.destory();
            }

            /*
             以下逻辑用于清理内存中与该节点相关的引用
             */
            // 删除该组件在compontent中属性关联
            this.parent?.delCorrelator(this);

            // 从当前组件所在的定义文档组件的_rootElementChilds中移除
            if (this.parent?._rootElementChilds.IndexOf((Element) this) != -1)
            {
                this.parent?._rootElementChilds.Remove((Element)this);
            }

            // 从组件树中移除
            this.parent?.removeChild(this);

            // 删除Component在Space中的映射
            if(this is Component)
            {
                EMR.Space.delComponentMap((Component)this);
            }
            

            // 触发组件销毁完成勾子函数
            this.destoryed();
        }
        #endregion
    }
}