using System;
using System.Collections.Generic;
using EMR.Event;
using EMR.Entity;
using EMR.Layout;
using EMR.Struct;

namespace EMR
{
    /// <summary>
    /// 组件异常类
    /// </summary>
    public class ComponentException : ApplicationException
    {
        public string error;

        public ComponentException(string msg)
        {
            error = msg;
        }
    }

    /// <summary>
    /// component遍历处理器
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    public delegate bool ComponentHandler(Component component);

    public partial class Component : EMR.CoreComponent, IDocumentModelModify, Element
    {
        /// <summary>
        /// 组件正在创建...
        /// </summary>
        internal bool isCreating = false;

        /// <summary>
        /// 组件正在新增或插入元素...
        /// </summary>
        internal bool isIncrease = false;

        /// <summary>
        /// 组件完全限定名
        /// </summary>
        internal string fullName = "";

        /// <summary>
        /// 组件允许的样式集合
        /// </summary>
        public StyleCollect styleCollect = null;

        public Component()
        {
            styleCollect = new StyleCollect(this);

            // 组件创建后将其加入component列表中
            EMR.Space.addComponentMap(this);
        }

        #region 基本属性
        /*---------------------------------------------------定义常规属性开始---------------------------------------------------*/
        private string _id = "";
        /// <summary>
        /// 组件id
        /// </summary>
        public string id
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

                    if(!this.parent.isAssembling)
                    {
                        foreach (var styleSheet in this.parent.styleSheets)
                        {
                            // 设置节点的样式
                            if (styleSheet != null)
                            {
                                // 设置该节点样式
                                styleSheet.setStyle(this, null, null, this._id, null);
                            }
                        }
                    }

                    EMR.Space.addComponentIdMap(this);
                }
            }
        }

        private string _name = "";
        /// <summary>
        /// 组件名称
        /// </summary>
        public string name
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

                    if(!this.parent.isAssembling)
                    {
                        foreach (var styleSheet in this.parent.styleSheets)
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

                    EMR.Space.addComponentNameMap(this);
                }
            }
        }

        private Node _solt = null;
        /// <summary>
        /// 内容插槽
        /// </summary>
        public virtual Node solt
        {
            get
            {
                return this._solt;
            }

            set
            {
                this._solt = value;
            }
        }

        private string _stylesheet = "";
        /// <summary>
        /// 样式
        /// </summary>
        public string stylesheet
        {
            get
            {
                return this._stylesheet;
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
                    if (this.parent != null)
                    {
                        ((CoreComponent)this.parent).setStyle(this, classNameList[i]);
                    }
                }
            }
        }
        #endregion

        #region 节点数据结构基本操作
        /// <summary>
        /// 第一个子组件
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
        /// 最后一个子组件
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
        /// 前一个兄弟组件
        /// </summary>
        public new Component previousSibling
        {
            get
            {
                var result = base.previousSibling;
                return result != null ? (Component)result : null;
            }
        }

        /// <summary>
        /// 后一个兄弟组件
        /// </summary>
        public new Component nextSibling
        {
            get
            {
                var result = base.nextSibling;
                return result != null ? (Component)result : null;
            }
        }

        /// <summary>
        /// 子组件列表
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
        /// 父元素
        /// </summary>
        public Element parentElement
        {
            get
            {
                Element result = null;

                var rootNodeChilds = this.rootNodeChilds;

                var rootNode = rootNodeChilds.Count > 0 ? rootNodeChilds[0] : null;

                var parentNode = rootNode.parent;

                // 父PanelLayer是一个组件(条件：parentNode.component != this.component代表父节点在另一个组件内）
                if (parentNode != null && parentNode.component != this.parent)
                {
                    var component = parentNode.component;

                    // while component.parent != this.component 确保节点与组件在同一视图内， component.parent != null 确保while不进入死循环
                    while (component.parent != this.parent && component.parent != null)
                    {
                        component = (CoreComponent)component.parent;
                    }

                    if (!(component is Component))
                    {
                        component = null;
                    }

                    result = (Element)component;
                }

                // 父PanelLayer是一个节点 (条件：parentNode.component == this.parent当前节点在另一个节点内）
                if (parentNode != null && parentNode.component == this.parent)
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

                var rootNodeChilds = this.rootNodeChilds;

                // 计算previousNode
                Node previousNode = null;
                foreach (var item in rootNodeChilds)
                {
                    if(item.previousSibling != null && rootNodeChilds.IndexOf(item.previousSibling) == -1)
                    {
                        previousNode = item.previousSibling;
                        break;
                    }
                }

                // 前一个PanelLayer是一个组件
                if (previousNode != null && previousNode.component != this.parent)
                {
                    var component = previousNode.component;
                    while (component.parent != this.parent && component.parent != null)
                    {
                        component = (CoreComponent)component.parent;
                    }

                    // 跨出了文档
                    if (!(component is Component))
                    {
                        component = null;
                    }

                    result = (Element)component;
                }

                // 前一个PanelLayer是一个节点 
                if (previousNode != null && previousNode.component == this.parent)
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

                var rootNodeChilds = this.rootNodeChilds;

                // 计算nextNode
                Node nextNode = null;
                foreach (var item in rootNodeChilds)
                {
                    if (item.nextSibling != null && rootNodeChilds.IndexOf(item.nextSibling) == -1)
                    {
                        nextNode = item.nextSibling;
                        break;
                    }
                }

                // 前一个PanelLayer是一个组件
                if (nextNode != null && nextNode.component != this.parent)
                {
                    var component = nextNode.component;
                    while (component.parent != this.parent && component.parent != null)
                    {
                        component = (CoreComponent)component.parent;
                    }

                    // 跨出了文档
                    if (!(component is Component))
                    {
                        component = null;
                    }

                    result = (Element)component;
                }

                // 前一个PanelLayer是一个节点 
                if (nextNode != null && nextNode.component == this.parent)
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
                foreach (var item in list)
                {
                    if (item.previousElement == null)
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

                // 计算直接节点
                foreach (var item in nodeList)
                {
                    var component = item.parent.component;

                    // 当前节点父元素是组件
                    if (component != this.parent)
                    {
                        while (component != this && component.parent != null)
                        {
                            component = (CoreComponent)component.parent;
                            if(component == this)
                            {
                                break;
                            }
                        }

                        if (component == this)
                        {
                            result.Add((Element)item);
                        }
                    }
                }

                // 计算直接组件
                foreach (var item in componentList)
                {
                    var itemRoot = item.rootNodeChilds;
                    var itemRootNode = itemRoot.Count > 0 ? itemRoot[0] : null;

                    // 当前组件父元素是组件
                    if (itemRootNode != null && itemRootNode.parent.component != this.parent)
                    {
                        var component = itemRootNode.parent.component;
                        while (component != this && component.parent != null)
                        {
                            component = (CoreComponent)component.parent;
                            if (component == this)
                            {
                                break;
                            }
                        }

                        if (component == this)
                        {
                            result.Add(item);
                        }
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
                while (element != null)
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
            Node result = null;

            // 抛出组件插入前事件
            InsertEventData insertEventData = new InsertEventData
            {
                target = (Element) current,
            };
            onInsert.Invoke(insertEventData);

            // 使用默认行为插入节点
            if (!insertEventData.isPreventDefault)
            {
                this.isIncrease = true;
                result = this.solt.insertBefore(current, refNode);
                this.isIncrease = false;
            }

            // 抛出组件插入完成事件
            InsertedEventData insertedEventData = new InsertedEventData
            {
                target = (Element) current,
                isSuccess = result != null,
            };
            onInserted.Invoke(insertedEventData);

            return result;
        }

        /// <summary>
        /// 插入子节点
        /// </summary>
        /// <param name="current">插入的节点</param>
        /// <param name="refComponent">参照组件，该组件必须为直接子组件</param>
        /// <returns></returns>
        public virtual Node insertBefore(Node current, Component refComponent)
        {
            Node result = null;

            // 抛出组件插入前事件
            InsertEventData insertEventData = new InsertEventData
            {
                target = (Element)current,
            };
            onInsert.Invoke(insertEventData);

            // 使用默认行为插入节点
            if (!insertEventData.isPreventDefault)
            {
                this.isIncrease = true;
                result = this.solt.insertBefore(current, refComponent);
                this.isIncrease = false;
            }

            // 抛出组件插入完成事件
            InsertedEventData insertedEventData = new InsertedEventData
            {
                target = (Element)current,
                isSuccess = result != null,
            };
            onInserted.Invoke(insertedEventData);

            return result;
        }

        /// <summary>
        /// 插入子组件
        /// </summary>
        /// <param name="component">要插入的组件</param>
        /// <param name="refNode">参照节点，该节点必须为直接子节点</param>
        /// <returns></returns>
        public virtual Component insertComponent(Component component, Node refNode)
        {
            Component result = null;

            // 抛出组件插入前事件
            InsertEventData insertEventData = new InsertEventData
            {
                target = component,
            };
            onInsert.Invoke(insertEventData);

            // 使用默认行为插入组件
            if (!insertEventData.isPreventDefault)
            {
                this.isIncrease = true;
                result = this.solt.insertComponent(component, refNode);
                this.isIncrease = false;
            }

            // 抛出组件插入完成事件
            InsertedEventData insertedEventData = new InsertedEventData
            {
                target = component,
                isSuccess = result != null,
            };
            onInserted.Invoke(insertedEventData);

            return result;
        }

        /// <summary>
        /// 插入子组件
        /// </summary>
        /// <param name="component">要插入的组件</param>
        /// <param name="refComponent">参照组件，该组件必须为直接子组件</param>
        /// <returns></returns>
        public virtual Component insertComponent(Component component, Component refComponent)
        {
            Component result = null;

            // 抛出组件插入前事件
            InsertEventData insertEventData = new InsertEventData
            {
                target = component,
            };
            onInsert.Invoke(insertEventData);

            // 使用默认行为插入组件
            if (!insertEventData.isPreventDefault)
            {
                this.isIncrease = true;
                result = this.solt.insertComponent(component, refComponent);
                this.isIncrease = false;
            }

            // 抛出组件插入完成事件
            InsertedEventData insertedEventData = new InsertedEventData
            {
                target = component,
                isSuccess = result != null,
            };
            onInserted.Invoke(insertedEventData);

            return result;
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
                    result = (Element) this.insertBefore((Node)current, (Node)refElement);
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
                    result = (Element) this.insertBefore((Node)current, (Component)refElement);
                }

                if (current is Component)
                {
                    result = this.insertComponent((Component)current, (Component)refElement);
                }
            }

            return result;
        }



        /// <summary>
        /// 添加节点
        /// </summary>
        /// <param name="node">要添加的节点</param>
        public virtual Node appendNode(Node node)
        {
            Node result = null;

            // 抛出组件添加前事件
            AppendEventData appendEventData = new AppendEventData
            {
                target = (Element)node
            };
            onAppend.Invoke(appendEventData);

            // 使用系统默认行为添加节点
            if (!appendEventData.isPreventDefault)
            {
                this.isIncrease = true;
                result = this.solt?.appendNode(node);
                this.isIncrease = false;
            }

            // 拙出组件添加完成事件
            AppendedEventData appendedEventData = new AppendedEventData
            {
                target = (Element)node,
                isSuccess = result != null,
            };
            onAppended.Invoke(appendedEventData);

            return result;
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public virtual Component appendComponent(Component component)
        {
            Component result = null;

            // 抛出组件添加前事件
            AppendEventData appendEventData = new AppendEventData
            {
                target = component
            };
            onAppend.Invoke(appendEventData);

            // 使用系统默认行为添加组件
            if (!appendEventData.isPreventDefault)
            {
                this.isIncrease = true;
                result = this.solt?.appendComponent(component);
                this.isIncrease = false;
            }

            // 拙出组件添加完成事件
            AppendedEventData appendedEventData = new AppendedEventData
            {
                target = component,
                isSuccess = result != null,
            };
            onAppended.Invoke(appendedEventData);

            return result;
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
                this.appendNode((Node)element);
            }

            if (element is Component)
            {
                this.appendComponent((Component)element);
            }

            return result;
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public override void destory()
        {
            // 抛出节点销毁事件
            DestoryEventData destoryEventData = new DestoryEventData
            {
                target = this,
            };
            onDestory.Invoke(destoryEventData);


            if (!destoryEventData.isPreventDefault)
            {
                this._isDestoring = true;

                var parentElement = this.parentElement;

                // 计算布局更新需要的父节点
                Node layoutUpdateParentNode = null;
                if (parentElement != null)
                {
                    if (parentElement is Node && !((Node)parentElement).isParentElementDestoring)
                    {
                        layoutUpdateParentNode = (Node)parentElement;
                    }

                    if (parentElement is Component && !((Component)parentElement).isDestoring)
                    {
                        layoutUpdateParentNode = ((Component)parentElement).solt;
                    }
                }

                // 组件销毁逻辑
                var childElements = this.childElements;
                foreach (var item in childElements)
                {
                    item.destory();
                }
                base.destory();

                // 布局更新
                if (layoutUpdateParentNode != null && layoutUpdateParentNode is PanelNode)
                {
                    Space.mainService.next(() =>
                    {
                        PanelLayout.update(new PanelNodeActionInfo
                        {
                            currentParent = (PanelNode)layoutUpdateParentNode,
                            action = PanelNodeAction.nodeDestory
                        });

                        return true;
                    });
                }
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
        public void ergodic(ComponentHandler componentHandler)
        {
            componentHandler((Component)this);
            foreach (Component item in this.children)
            {
                item.ergodic(componentHandler);
            }
        }
        #endregion
    }
}
