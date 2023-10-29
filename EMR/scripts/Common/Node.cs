using System;
using System.Collections.Generic;

namespace EMR.Common.DataStructure
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

    public delegate void NodeHandler(Node node);
    /// <summary>
    /// 树节点类
    /// </summary>
    public class Node
    {
        private Node _parent;
        private Node _left;
        private Node _right;
        private List<Node> _children = new List<Node>();

        /// <summary>
        /// 节点遍历
        /// </summary>
        /// <param name="node"></param>
        /// <param name="nodeHandler"></param>
        protected void ergodicStructNode(EMR.Common.DataStructure.Node node, NodeHandler nodeHandler)
        {
            nodeHandler(node);
            var childrens = node.children;
            foreach (var item in childrens)
            {
                ergodicStructNode(item, nodeHandler);
            }
        }

        /// <summary>
        /// 获取第一个直接子节点
        /// </summary>
        protected Node firstChild
        {
            get
            {
                Node result = this._children.Count > 0 ? this._children[0] : null;

                while (result != null && result._left != null)
                {
                    result = result._left;
                }
                return result;
            }
        }

        /// <summary>
        /// 获取最后一个直接子节点
        /// </summary>
        protected Node lastChild
        {
            get
            {
                Node result = this._children.Count > 0 ? this._children[0] : null;
                while (result != null && result._right != null)
                {
                    result = result._right;
                }
                return result;
            }
        }

        /// <summary>
        /// 父节点
        /// </summary>
        protected Node parent
        {
            get
            {
                return this._parent;
            }

            set
            {
                this._parent = value;
            }
        }

        protected Node previousSibling
        {
            get
            {
                return this._left;
            }

            set
            {
                this._left = value;
            }
        }

        protected Node nextSibling
        {
            get
            {
                return this._right;
            }

            set
            {
                this._right = value;
            }
        }

        /// <summary>
        /// 子节点列表
        /// </summary>
        protected List<Node> children
        {
            get
            {
                var result = new List<Node>();
                var current = this.firstChild;

                while (current != null)
                {
                    result.Add(current);
                    current = current._right;
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
        public Node insertBefore(Node current, Node refNode)
        {
            Node result = null;

            // 当插入节点为空时：
            if (current == null)
            {
                throw new NodeException("The inserted element cannot be empty");
            }

            // 当refNode不为直接子节点时：
            if (refNode != null && refNode._parent != this)
            {
                throw new NodeException("The reference node must be a direct child node");
            }

            // 当该节已在树内已存在时
            if(current.parent != null)
            {
                current.parent.removeChild(current);
            }

            // 如果存在有参照节点则在该节点前插入
            if (refNode != null)
            {
                // 参照节点前存在有节点
                if (refNode._left != null)
                {
                    var leftNode = refNode._left;
                    leftNode._right = current;
                    current._left = leftNode;
                    current._right = refNode;
                    refNode._left = current;
                }

                // 参照节点前不存在有节点
                if (refNode._left == null)
                {
                    refNode._left = current;
                    current._right = refNode;
                }
                result = current;
            }

            // 如果参照节点为null则在最后一个节点后添加
            if (refNode == null)
            {
                var lastNode = this.lastChild;
                if (lastNode != null)
                {
                    lastNode._right = current;
                    current._left = lastNode;
                }
                result = current;
            }

            // 添加子对象
            if (result != null)
            {
                result._parent = this;
                this._children.Add(current);
            }

            return result;
        }

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="current">需要添加的对象</param>
        /// <returns></returns>
        protected Node appendNode(Node current)
        {
            return this.insertBefore(current, null);
        }


        /// <summary>
        /// 删除子节点
        /// </summary>
        /// <param name="element">需要删除的子节点</param>
        /// <returns></returns>
        public Node removeChild(Node element)
        {
            Node result = element;

            // 当插入节点为空时：
            if (element == null)
            {
                throw new NodeException("Node cannot be empty");
            }

            // 节点不为直接子节点时：
            if (element != null && element._parent != this)
            {
                throw new NodeException("The deleted node must be a direct child of the current node");
            }

            // 从列表中删除该节点
            var index = this.children.IndexOf(element);
            this._children.RemoveAt(index);

            // 删除的节点即不是最后一个节点也不是第一个节点(指针处理)
            if (element._left != null && element._right != null)
            {
                var leftElement = element._left;
                var rightElement = element._right;
                leftElement._right = rightElement;
                rightElement._left = leftElement;
            }

            // 删除第一个节点(指针处理)
            if (element._left == null && element._right != null)
            {
                var rightElement = element._right;
                rightElement._left = null;
            }

            // 删除最后一个节点(指针处理)
            if (element._right == null && element._left != null)
            {
                var leftElement = element._left;
                leftElement._right = null;
            }

            element._left = null;
            element._right = null;
            element._parent = null;

            return result;
        }
    }
}
