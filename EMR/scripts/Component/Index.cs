/*
 * 本文件主要用于定义组件的索引方法
 */

using System.Collections.Generic;
using EMR.Entity;

namespace EMR
{
    public partial class Component
    {
        #region 节点索引
        /// <summary>
        /// 查找组件内的所有节点
        /// </summary>
        /// <returns></returns>
        public List<Node> getChildNodes()
        {
            List<Node> result = new List<Node>();
            List<Node> list = new List<Node>();

            foreach (var rootChild in ((CoreComponent)this.parent).rootNodeChilds)
            {
                rootChild.ergodic((node) =>
                {
                    if (node.component == this.parent)
                    {
                        list.Add(node);
                    }

                    return true;
                });
            }

            var rootNodeChilds = this.rootNodeChilds;

            foreach (var item in list)
            {
                var node = item.parent;
                while (node != null)
                {
                    if (rootNodeChilds.IndexOf(node) != -1)
                    {
                        result.Add(item);
                        break;
                    }
                    node = node.parent;
                }
            }



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
                var list = this.getChildNodes();
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
                var list = this.getChildNodes();
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
        #endregion

        #region 组件索引
        public List<Component> getChildComponents()
        {
            List<Component> result = new List<Component>();
            List<Component> list = new List<Component>();

            foreach (Component item in this.parent.children)
            {
                list.Add(item);
            }

            foreach (var item in list)
            {
                var node = item.rootNodeChilds.Count > 0 ? item.rootNodeChilds[0].parent : null;
                var rootNodeChilds = this.rootNodeChilds;
                while (node != null)
                {
                    if (rootNodeChilds.IndexOf(node) != -1)
                    {
                        result.Add(item);
                        break;
                    }

                    node = node.parent;
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
        /// 根据id获取Compontent
        /// </summary>
        /// <param name="id">组件id</param>
        /// <returns></returns>
        public Component getChildComponentById(string id)
        {
            Component result = null;

            if (id != "" && id != null)
            {
                List<Component> list = this.getChildComponents();
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
        /// 根据id获取Compontent
        /// </summary>
        /// <typeparam name="T">要查询的组件类型</typeparam>
        /// <param name="id">组件id</param>
        /// <returns></returns>
        public T getChildComponentById<T>(string id) where T : Component
        {
            T reslut = null;
            var compontent = this.getChildComponentById(id);
            if (typeof(T) == compontent?.GetType())
            {
                reslut = (T)compontent;
            }

            return reslut;
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
        #endregion

        #region 元素索引
        /// <summary>
        /// 获取所有子元素
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
        /// 按Id查找子元素
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
        /// 按名称查找元素
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
    }
}