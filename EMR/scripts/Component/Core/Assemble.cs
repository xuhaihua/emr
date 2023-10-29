/*
 * 本文件内的方法主要用于组件的按装
 */

using System;
using EMR.Entity;
using EMR.Plugin;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using EMR.Common;

namespace EMR
{
    public partial class CoreComponent
    {
        // node 到 xmlNode的映射 map（主要用于在解析属性时从node跟踪到原始xml的节点，以便在从中获取到节点所要计算的相关属性信息）
        private static Dictionary<Node, XmlNode> nodeToXMLMap = new Dictionary<Node, XmlNode>();

        // component 到 xmlNode的映射 map（主要用于在解析属性时从component跟踪到原始xml的节点，以便在从中获取到component所要计算的相关属性信息）
        private static Dictionary<Component, XmlNode> componentToXMLMap = new Dictionary<Component, XmlNode>();

        // node 到 anchor的映射map
        private static Dictionary<Node, Dictionary<XmlNode, Anchor>> anchorMap = new Dictionary<Node, Dictionary<XmlNode, Anchor>>();

        // node到 CoreComponent的映射map(主要用于在文档解析过程中跟踪当前节点引用的上下文)
        private static Dictionary<Node, CoreComponent> nodeContextMap = new Dictionary<Node, CoreComponent>();

        // component到 CoreComponent的映射map(主要用于在文档解析过程中跟踪当前组件引用的上下文)
        private static Dictionary<Component, CoreComponent> componentContextMap = new Dictionary<Component, CoreComponent>();

        Node holdNode = null;

        /// <summary>
        /// 是否来自编译时的添加
        /// </summary>
        public bool isFormCompileAppend = false;

        private bool _isAssembling = false;
        /// <summary>
        /// 组件是否正在安装状态
        /// </summary>
        public bool isAssembling
        {
            get
            {
                return this._isAssembling;
            }
        }

        /// <summary>
        /// 查找节点类型
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        private Type findNodeType(string typeName)
        {
            Type result = null;

            foreach (var assembly in EMR.Space.mainService.assemblyList)
            {
                // typeName中不含级联运算符（这时先在System下查找然后在类型列表中搜寻）
                if (typeName.IndexOf(".") == -1)
                {
                    List<string> queryList = new List<string>
                    {
                        typeName
                    };

                    foreach (var spaceName in EMR.Space.mainService.typeQueryList)
                    {
                        queryList.Add(spaceName + "." + typeName);
                    }

                    foreach (var name in queryList)
                    {
                        var tempResult = assembly.GetType(name);
                        if (tempResult != null && typeof(EMR.Entity.Node).IsAssignableFrom(tempResult))
                        {
                            result = tempResult;
                            break;
                        }
                    }
                }

                // typeName中含级联运算符（这时typeName即为类型的完全限定名）
                if (typeName.IndexOf(".") != -1)
                {
                    var tempResult = assembly.GetType(typeName);
                    if (tempResult != null && typeof(EMR.Entity.Node).IsAssignableFrom(tempResult))
                    {
                        result = tempResult;
                        break;
                    }
                }

                // 当类型搜寻到时直接退出
                if (result != null)
                {
                    break;
                }
            }

            return result;
        }


        /// <summary>
        /// 查找组件类型
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        private Type findComponentType(string typeName)
        {
            Type result = null;

            foreach (var assembly in EMR.Space.mainService.assemblyList)
            {
                // typeName中不含级联运算符（这时先在System下查找然后在类型列表中搜寻）
                if (typeName.IndexOf(".") == -1)
                {
                    List<string> queryList = new List<string>
                    {
                        typeName
                    };

                    foreach (var spaceName in EMR.Space.mainService.typeQueryList)
                    {
                        queryList.Add(spaceName + "." + typeName);
                    }

                    foreach (var name in queryList)
                    {
                        var tempResult = assembly.GetType(name);
                        if (tempResult != null && (tempResult == typeof(Component) || typeof(EMR.Component).IsAssignableFrom(tempResult)))
                        {
                            result = tempResult;
                            break;
                        }
                    }
                }

                // typeName中含级联运算符（这时typeName即为类型的完全限定名）
                if (typeName.IndexOf(".") != -1)
                {
                    var tempResult = assembly.GetType(typeName);
                    if (tempResult != null && (tempResult == typeof(Component) || typeof(EMR.Component).IsAssignableFrom(tempResult)))
                    {
                        result = tempResult;
                        break;
                    }
                }

                // 当类型搜寻到时直接退出
                if (result != null)
                {
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// 装配
        /// </summary>
        /// <param name="context">上下文</param>
        public virtual void assemble()
        {
            this._isAssembling = true;

            // 加载组件xml文档并获取该文档根节点
            var documentRoot = Utils.loadXml(this.document);

            if (documentRoot == null)
            {
                if (this.document == "")
                {
                    Debug.LogError(this.GetType() + "组件的视图未定义");
                    return;
                }
                else
                {
                    Debug.LogError("加载" + this.GetType() + "   " + this.document + "   " + "组件的视图文件时错误请检查该文件是否存在或文件内容是否正确");
                    return;
                }
            }

            // 节点字典 [从xmlNode映射到Node 主要用于在文档解析过程中跟踪到要创建的节点的父节点]
            Dictionary<XmlNode, Node> nodeDictionary = new Dictionary<XmlNode, Node>();
            Dictionary<XmlNode, Component> componentDictionary = new Dictionary<XmlNode, Component>();

            if (documentRoot != null)
            {
                Utils.ergodicXml(documentRoot, (xmlNode) =>
                {
                    // Root节点不处理
                    if (xmlNode.Name == "Root")
                    {
                        return;
                    }

                    // 混合现实节点
                    Node node = null;

                    holdNode = null;
                    bool holdNModeFormComponent = true;
                    if (nodeDictionary.ContainsKey(xmlNode.ParentNode))
                    {
                        holdNode = nodeDictionary[xmlNode.ParentNode];
                        holdNModeFormComponent = false;
                    }

                    if (componentDictionary.ContainsKey(xmlNode.ParentNode))
                    {
                        holdNode = componentDictionary[xmlNode.ParentNode].solt;
                    }

                    // 向组件添加样式表
                    if (xmlNode.Name == "Stylesheet")
                    {
                        var stylesheet = new Stylesheet();

                        if (xmlNode.Attributes["path"] != null && xmlNode.Attributes["path"].Value.Trim() != "")
                        {
                            stylesheet.path = xmlNode.Attributes["path"].Value.Trim();
                        }

                        this.styleSheets.Add(stylesheet);
                    }

                    // 将锚加入anchorMap
                    if (xmlNode.Name == "Anchor")
                    {
                        if (holdNode != null)
                        {
                            if (xmlNode.Attributes["name"] != null)
                            {
                                var anchor = new Anchor(holdNode, null, null, null, 0f);

                                // 绑定关联属性
                                this.correlatorBind(anchor, xmlNode);

                                // 加入字典
                                if (!anchorMap.ContainsKey(holdNode))
                                {
                                    anchorMap.Add(holdNode, new Dictionary<XmlNode, Anchor>());
                                }

                                anchorMap[holdNode].Add(xmlNode, anchor);
                            }
                            else
                            {

                                Debug.LogError("锚必须包含name属性，" + "在" + this.GetType().ToString() + " 组件中");
                                return;
                            }
                        }
                        else
                        {
                            Debug.LogError("Anchor标签必须包囊在组件内的一个节点中");
                            return;
                        }
                    }

                    // 设置文件节点的内容
                    if (xmlNode.NodeType == XmlNodeType.Text || xmlNode.NodeType == XmlNodeType.CDATA)
                    {
                        if (holdNode != null)
                        {
                            if (holdNode is PanelNode)
                            {
                                ((PanelNode)holdNode).text = ((PanelNode)holdNode).text + xmlNode.Value.Trim();
                            }
                        }
                    }


                    var isEMRNode = false;


                    // 创建节点
                    var nodeTypeName = xmlNode.Name;
                    Type nodeType = findNodeType(nodeTypeName);

                    if (nodeType != null)
                    {
                        isEMRNode = true;

                        node = (Node)nodeType.Assembly.CreateInstance(nodeType.FullName, true, System.Reflection.BindingFlags.Default, null, new object[] { this }, null, null);

                        node.fullName = nodeType.FullName;

                        nodeDictionary.Add(xmlNode, node);
                        nodeToXMLMap.Add(node, xmlNode);
                        nodeContextMap.Add(node, this);

                        // 绑定关联属性
                        this.correlatorBind(node, xmlNode);

                        // 添加节点
                        if (xmlNode.ParentNode.Name == "Root")
                        {
                            this._rootElementChilds.Add((Element)node);
                        }
                        else
                        {
                            if (holdNode != null && holdNode is IDocumentModelModify)
                            {
                                holdNode.component.isFormCompileAppend = true;

                                // 当前holdNModeFormComponent为true代表正在向某一组件的slot添加内容，这时将该组件的isIncrease属性设置为true表示当前正在向该组件新增子节点或子组件
                                if (holdNode.component is Component && holdNModeFormComponent)
                                {
                                    var holdComponent = (Component)holdNode.component;
                                    holdComponent.isIncrease = true;
                                }

                                ((IDocumentModelModify)holdNode).appendNode(node);

                                holdNode.component.isFormCompileAppend = false;
                            }
                        }
                    }


                    // 为组件设置内容插槽
                    if (xmlNode.Name == "Slot" && this is Component)
                    {
                        if (holdNode != null)
                        {
                            ((Component)this).solt = holdNode;
                        }
                        else
                        {
                            Debug.LogError("在组件" + this + "中Slot必须被本组件内的一个节点包囊");
                            return;
                        }
                    }


                    // 不带限定符的组件名(如果名称为Component说明是一个匿名组件，直接在EMR空间中查找，如果名称不为Component，先在system中查找然后在配制范围内查找)
                    var componentTypeName = xmlNode.Name;
                    Type componentType = findComponentType(componentTypeName);

                    EMR.Component component = null;

                    // 创建Component:
                    if (componentType != null)
                    {
                        isEMRNode = true;

                        // 匿名组件
                        if (componentType == typeof(Component))
                        {
                            // 查找是否有对应类型的Compontent
                            var typeName = xmlNode.Attributes["type"] == null ? "" : xmlNode.Attributes["type"].Value.Trim();

                            // 当类型名称不为空时:
                            if (typeName != "")
                            {
                                var anonymousType = findComponentType(typeName);

                                // 当类型存在时实例化指定类型的Compontent
                                if (anonymousType != null && typeof(EMR.Component).IsAssignableFrom(anonymousType))
                                {
                                    component = (EMR.Component)anonymousType.Assembly.CreateInstance(anonymousType.FullName);
                                }
                                else
                                {
                                    Debug.LogError("不存在类型为：" + typeName + "的组件");
                                    throw new ComponentException("Component not found");
                                }
                            }

                            // 当类型名称为空时创建一个空Component
                            if (typeName == "")
                            {
                                component = new Component();
                            }
                        }

                        // 非匿名组件
                        if (typeof(EMR.Component).IsAssignableFrom(componentType))
                        {

                            component = (EMR.Component)componentType.Assembly.CreateInstance(componentType.FullName);
                        }

                        if (component != null)
                        {
                            component.fullName = componentTypeName;

                            // 设置component的视图文档
                            if (xmlNode.Attributes["document"]?.Value.Trim() != null)
                            {
                                component.document = xmlNode.Attributes["document"].Value.Trim();
                            }

                            componentToXMLMap.Add(component, xmlNode);
                            componentContextMap.Add(component, this);
                            componentDictionary.Add(xmlNode, component);

                            // 绑定关联属性
                            this.correlatorBind(component, xmlNode);

                            if (xmlNode.ParentNode.Name == "Root")
                            {
                                this._rootElementChilds.Add(component);
                            }

                            this.appendNode(component);

                            component.assemble();

                            // 组件在某一节点下
                            if (holdNode != null && !holdNModeFormComponent)
                            {
                                holdNode.isIncreaseCompontent = true;
                                holdNode.component.isFormCompileAppend = true;

                                foreach (var item in component.rootNodeChilds)
                                {
                                    ((IDocumentModelModify)holdNode).appendNode(item);
                                }

                                holdNode.component.isFormCompileAppend = false;
                                holdNode.isIncreaseCompontent = false;
                            }

                            // 组件在某一组件下
                            else if (holdNode != null && holdNModeFormComponent)
                            {
                                ((CoreComponent)holdNode.component).isFormCompileAppend = true;
                                ((Component)holdNode.component).appendComponent(component);
                                ((CoreComponent)holdNode.component).isFormCompileAppend = false;
                            }
                        }
                    }

                    if (!isEMRNode && xmlNode.NodeType != XmlNodeType.Comment && xmlNode.NodeType != XmlNodeType.Text && xmlNode.NodeType != XmlNodeType.CDATA && xmlNode.Name != "Stylesheet" && xmlNode.Name != "Slot" && xmlNode.Name != "Anchor")
                    {
                        Debug.LogWarning("在" + this.document + "中使用了不存在节点" + xmlNode.Name);
                    }
                });
            }

            // 当回退到顶层节点时：对文档进行属性和布局的渲染
            if (this.parent == null)
            {

                // 渲染所有子组件属性(在文档中先对所有组件进行属性设值)
                foreach (var item in this.children)
                {
                    ((Component)item).ergodic((component) =>
                    {
                        if (component.parent != null)
                        {
                            var context = componentContextMap[component];
                            context.renderComponent(component, componentToXMLMap[component]);
                        }

                        return true;
                    });
                }

                foreach (var item in this.rootNodeChilds)
                {
                    item.ergodic((Node node) =>
                    {
                        var xmlNode = nodeToXMLMap[node];

                        if (xmlNode.Attributes["id"] != null)
                        {
                            node.id = xmlNode.Attributes["id"].Value;
                        }

                        if (xmlNode.Attributes["name"] != null)
                        {
                            node.name = xmlNode.Attributes["name"].Value;
                        }

                        return true;
                    });
                }

                // 此刻组件的内容及结构已全部生成，组件的属性已被赋予组件上但其内容在视图上还未完成样式的初始化，触发组件的created生命周期
                foreach (var item in this.children)
                {
                    ((Component)item).ergodic((component) =>
                    {
                        component.created();

                        return true;
                    });
                }

                this.created();


                // 节点初始设置值
                foreach (var item in this.rootNodeChilds)
                {
                    item.ergodic((Node node) =>
                    {
                        var context = nodeContextMap[node];

                        // 当为魔法节点时，需要在这里对其进行刷新以初始化魔法节点的尺寸和位置（原因是因为、魔法节点不存在其自身的尺寸、位置属性所以如果不刷新魔法节点将为0)
                        if (node is SpaceMagic)
                        {
                            ((SpaceMagic)node).fresh();
                        }

                        // 对节点属性进行渲染
                        context.renderNode(node, nodeToXMLMap[node]);

                        return true;
                    });
                }

                // 刷新节点尺寸、位置
                foreach (var item in this.rootNodeChilds)
                {
                    item.ergodic((Node node) =>
                    {
                        node.sizeFresh();
                        node.positionFresh();

                        return true;
                    });
                }

                // 刷新平面节点层高
                foreach (var item in this.rootNodeChilds)
                {
                    // 刷新空间中PanelRoot下的所有PanelLayer节点的LayoutHeight
                    if (item is PanelRoot)
                    {
                        foreach (var rootChildItem in item.children)
                        {
                            if (rootChildItem is PanelLayer)
                            {
                                ((PanelLayer)rootChildItem).freshLayoutHeight();
                            }
                        }
                    }
                }

                // 刷新平面节点z坐标
                foreach (var item in this.rootNodeChilds)
                {
                    PanelNode computeCoordZParentNode = null;
                    item.ergodic((Node node) =>
                    {
                        // 计算PanelRoot下每一个PanelLayer节点的z坐标
                        if (node is PanelLayer)
                        {
                            if (node.parent != computeCoordZParentNode)
                            {
                                var elementNode = (PanelLayer)node;
                                elementNode.compountCoordZ();
                                computeCoordZParentNode = elementNode.parent;
                            }
                        }

                        return true;
                    });
                }


                // 节点布局刷新
                foreach (var item in this.rootNodeChilds)
                {
                    item.ergodic((Node node) =>
                    {
                        // 空间特征节点sizeBound插件刷新（控制手柄部分刷新）
                        if (node is ISpaceCharacteristic)
                        {
                            ((ISpaceCharacteristic)node).sizeBoundFresh();
                        }

                        // 当不为魔法节点时刷新节点的边框
                        if (!(node is SpaceMagic))
                        {
                            node.borderFresh();
                        }

                        // 具有空间布局的节点刷新它的对齐
                        if (node is ISpaceLayoutFeature)
                        {
                            ((ISpaceLayoutFeature)node).contentAlignFresh();
                        }

                        // 具有平面布局的节点刷新它的对齐
                        if (node is IPanelLayoutFeature)
                        {
                            ((IPanelLayoutFeature)node).contentAlignFresh();
                        }

                        // 水平浮动刷新
                        node.horizontalFresh();

                        // 垂直浮动刷新
                        node.verticalFresh();

                        // 具有空间布局的节点刷新它的Z轴浮动
                        if (node is ISpaceLayoutFeature)
                        {
                            ((ISpaceLayoutFeature)node).forwardFresh();
                        }

                        // SpaceNode npc 刷新
                        node.npcFresh();

                        // 文本刷新
                        if (node is PanelNode && ((PanelNode)node).textNodeList.Count > 0)
                        {
                            ((PanelNode)node).textNodeList[0].fresh();
                        }

                        // 滚动视口刷新
                        if (node is PanelNode)
                        {
                            // 可视区刷新
                            PanelScroll.viewFresh((PanelNode)node, true, true);
                        }

                        // 阻止触发节点的onSizeChange事件
                        node.preventSizeEvent = true;

                        // 阻止触发节点的onPositionChange事件
                        node.preventPositionEvent = true;

                        return true;
                    });
                }


                // 锚点处理
                foreach (var item in this.rootNodeChilds)
                {
                    item.ergodic((Node node) =>
                    {
                        var context = nodeContextMap[node];

                        // 对节点包含的锚进行属性渲染
                        if (anchorMap.ContainsKey(node))
                        {
                            context.renderAnchor(node, anchorMap[node]);
                        }

                        return true;
                    });
                }

                // 将所有组件的isAssembling设置为结束状态
                foreach (var item in this.children)
                {
                    ((Component)item).ergodic((component) =>
                    {
                        component._isAssembling = false;

                        return true;
                    });
                }
                this._isAssembling = false;

                // 此处将spaceBendFresh放在_isAssembling = false是因为该行为与节点的坐标位置有关放在此处是让他在文档解析的整个过程中只被执行一次）
                foreach (var item in this.rootNodeChilds)
                {
                    item.ergodic((node) =>
                    {
                        foreach (var anchor in node.getAnchorList())
                        {
                            if (anchor.targetId != "")
                            {
                                anchor.alignment();
                            }
                        }

                        if (node is SpaceNode)
                        {
                            var spaceNode = (SpaceNode)node;
                            spaceNode.spaceBendFresh();
                        }

                        return true;
                    });
                }

                // 此刻所有相关属性以于文档进行了绑定并且文档以完成布局，触发各文档mounted生命周期
                foreach (var item in this.children)
                {
                    ((Component)item).ergodic((component) =>
                    {
                        component.mounted();

                        return true;
                    });
                }
                this.mounted();
            }
        }
    }
}
