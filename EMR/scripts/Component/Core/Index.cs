/*
 * 本文件内的方法主要用于元素的索引
 */

using EMR.Entity;
using System.Collections.Generic;

namespace EMR
{
    public partial class CoreComponent
    {
        #region 节点引方法
        /// <summary>
        /// 查找组件内的所有节点
        /// </summary>
        /// <returns></returns>
        protected List<Node> getNodes()
        {
            List<Node> result = new List<Node>();

            foreach (var rootChild in this.rootNodeChilds)
            {
                rootChild.ergodic((node) =>
                {
                    if (node.component == this)
                    {
                        result.Add(node);
                    }

                    return true;
                });
            }
            return result;
        }

        /// <summary>
        /// 查找组件内的所有节点
        /// </summary>
        /// <typeparam name="T">所要查询的节点类型</typeparam>
        /// <returns></returns>
        protected List<T> getNodes<T>() where T : Node
        {
            List<T> result = new List<T>();

            List<Node> list = this.getNodes();
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
        /// 按Id查找组件内的节点
        /// </summary>
        /// <param name="id">所要查询的节点id</param>
        /// <returns></returns>
        public Node getNodeById(string id)
        {
            Node result = null;

            if (id != "" && id != null)
            {
                var list = this.getNodes();
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
        /// 按Id查找组件内的节点
        /// </summary>
        /// <typeparam name="T">要查找的节点类型</typeparam>
        /// <param name="id">节点id</param>
        /// <returns></returns>
        protected T getNodeById<T>(string id) where T : Node
        {
            T reslut = null;
            var node = this.getNodeById(id);
            if (typeof(T) == node?.GetType())
            {
                reslut = (T)node;
            }

            return reslut;
        }

        /// <summary>
        /// 按名称查找组件内的节点
        /// </summary>
        /// <param name="name">要查询的节点名称</param>
        /// <returns></returns>
        public List<Node> getNodesByName(string name)
        {
            List<Node> result = new List<Node>();

            if (name != "" && name != null)
            {
                List<Node> list = this.getNodes();

                foreach (var node in list)
                {
                    if (node.name == name)
                    {
                        result.Add(node);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 按名称查找组件内的节点
        /// </summary>
        /// <typeparam name="T">所要查询的节点类型</typeparam>
        /// <param name="name">要查询的节点名称</param>
        /// <returns></returns>
        protected List<T> getNodesByName<T>(string name, bool isDeep = true) where T : Node
        {
            List<T> result = new List<T>();

            var list = this.getNodesByName(name);
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
        /// <summary>
        /// 查找所有子组件
        /// </summary>
        /// <returns></returns>
        protected List<Component> getComponents()
        {
            List<Component> result = new List<Component>();

            foreach (Component item in this.children)
            {
                result.Add(item);
            }

            return result;
        }

        /// <summary>
        /// 按名称查询组件
        /// </summary>
        /// <typeparam name="T">要查询的组件类型</typeparam>
        /// <param name="isDeep">是否包含子组件</param>
        /// <returns></returns>
        protected List<T> getComponents<T>() where T : Component
        {
            List<T> result = new List<T>();

            var list = this.getComponents();
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
        protected Component getComponentById(string id)
        {
            Component result = null;

            if (id != "" && id != null)
            {
                List<Component> list = this.getComponents();
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
        protected T getComponentById<T>(string id) where T : Component
        {
            T reslut = null;
            var compontent = this.getComponentById(id);
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
        protected List<Component> getComponentsByName(string name)
        {
            List<Component> result = new List<Component>();

            if (name != "" && name != null)
            {
                var list = this.getComponents();

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
        /// <param name="isDeep">是否包含子组件</param>
        /// <returns></returns>
        protected List<T> getComponentsByName<T>(string name) where T : Component
        {
            List<T> result = new List<T>();

            var list = this.getComponentsByName(name);
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
        /// 获取所有元素
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<Element> getElements()
        {
            List<Element> result = new List<Element>();

            var nodeList = this.getNodes();

            foreach (var item in nodeList)
            {
                result.Add((Element)item);
            }

            var componentList = this.getComponents();

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
        public Element getElementById(string id)
        {
            Element result;
            result = (Element)this.getNodeById(id);

            if (result == null)
            {
                result = this.getComponentById(id);
            }

            return result;
        }

        /// <summary>
        /// 按名称查找元素
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<Element> getElementsByName(string name)
        {
            List<Element> result = new List<Element>();

            var nodeList = this.getNodesByName(name);

            foreach (var item in nodeList)
            {
                result.Add((Element)item);
            }

            var componentList = this.getComponentsByName(name);

            foreach (var item in componentList)
            {
                result.Add(item);
            }

            return result;
        }
        #endregion
    }
}
