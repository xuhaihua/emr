/*
 * 本文件内容方法主要是针对节点、组件的渲染（这里的渲染是指对元素属性、事件设置）
 */

using System;
using EMR.Entity;
using System.Reflection;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using EMR.Common;
using EMR.Struct;
using EMR.Event;

namespace EMR
{
    public partial class CoreComponent
    {

        /// <summary>
        /// 计算属性值
        /// </summary>
        /// <param name="current"></param>
        /// <param name="attributeName"></param>
        /// <param name="attributeValue"></param>
        /// <param name="isValueType"></param>
        /// <returns></returns>
        public object computeAttributeValue(object current, string attributeName, string attributeValue, bool isFromStyle = false)
        {
            object result = null;

            var propertyInfo = current.GetType().GetProperty(attributeName);
            if (propertyInfo == null)
            {
                var errorMessage = "在解析" + this + "组件时发生异常，因为" + current.GetType() + "中不存在属性：" + attributeName;
                if (isFromStyle)
                {
                    errorMessage = "从样式表解析" + this + "组件时发生异常，因为" + current.GetType() + "中不存在属性：" + attributeName + "请检查相应样式表内属性书写是否正确";
                }
                Debug.LogError(errorMessage);
                return null;
            }

            string propertyType = current.GetType().GetProperty(attributeName).PropertyType.FullName;

            if (propertyType != null)
            {
                if (attributeName == "offset" || attributeName == "npcOffset" || attributeName == "joint" || attributeName == "jointRotationAxle")
                {
                    var weightList = attributeValue.Split(',');
                    var x = (float)System.Convert.ToDouble(weightList[0].Trim());
                    var y = (float)System.Convert.ToDouble(weightList[1].Trim());
                    var z = (float)System.Convert.ToDouble(weightList[2].Trim());

                    return new Vector3(x, y, z);
                }

                if (propertyType == "System.Single" || propertyType.IndexOf("System.Single") != -1)
                {
                    try
                    {
                        result = System.Convert.ToSingle(attributeValue);
                    }
                    catch
                    {
                        Debug.LogError("属性的值不为Single!");
                    }
                }
                else if (propertyType == "System.Boolean")
                {
                    if (Utils.isTrue(attributeValue))
                    {
                        result = true;
                    }
                    else if (Utils.isFalse(attributeValue))
                    {
                        result = false;
                    }
                    else
                    {
                        Debug.LogError("属性布尔值不正确!");
                    }
                }
                else if (propertyType == "System.String")
                {
                    result = System.Convert.ToString(attributeValue);
                }
                else if (propertyType == "System.Int32")
                {
                    if (Utils.isInt(attributeValue))
                    {
                        try
                        {
                            result = System.Convert.ToInt32(attributeValue);
                        }
                        catch
                        {
                            Debug.LogError("属性的值不为整形!");
                        }
                    }
                }
                else if (propertyType == "System.Double")
                {
                    try
                    {
                        result = System.Convert.ToDouble(attributeValue);
                    }
                    catch
                    {
                        Debug.LogError("属性的值不为Double!");
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 获取节点属性的名-值集合
        /// </summary>
        /// <param name="current"></param>
        /// <param name="xmlNode"></param>
        /// <param name="attributes"></param>
        private void getNodeAttributeList(object current, XmlNode xmlNode, ref Dictionary<string, object> attributes)
        {
            // 计算当前节点的属性键值对
            for (var i = 0; i < xmlNode.Attributes.Count; i++)
            {
                if (!xmlNode.Attributes[i].Name.StartsWith("on") && !xmlNode.Attributes[i].Name.StartsWith("onNpc"))
                {
                    // 是否为变量类型
                    bool isValueType = xmlNode.Attributes[i].Name.StartsWith("_");

                    // 获取属性名称
                    string name = isValueType ? xmlNode.Attributes[i].Name.Substring(1, xmlNode.Attributes[i].Name.Length - 1) : xmlNode.Attributes[i].Name;

                    // 计算属性值
                    object value;

                    if(!isValueType)
                    {
                        value = this.computeAttributeValue(current, name, xmlNode.Attributes[i].Value.Trim());
                    } else
                    {
                        value = Utils.getProperty(xmlNode.Attributes[i].Value.Trim(), this);
                    }

                    // 将名-值对加入集合
                    attributes.Add(name, value);
                }
            }
        }

        #region 节点事件
        /// <summary>
        /// 获取节点事件的名-值集合
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <param name="events"></param>
        private void getNodeEventList(XmlNode xmlNode, ref Dictionary<string, MethodInfo> events)
        {
            // 计算当前节点的事件键值对
            for (var i = 0; i < xmlNode.Attributes.Count; i++)
            {
                if (xmlNode.Attributes[i].Name.StartsWith("on"))
                {
                    string name = xmlNode.Attributes[i].Name;
                    var methodInfo = Utils.getMethod(xmlNode.Attributes[i].Value.Trim(), this);
                    if (methodInfo == null)
                    {
                        Debug.LogError("在组件" + this.GetType().ToString() + "中没有找到在类型" + xmlNode.Name + "上的" + name + "事件对应的方法" + xmlNode.Attributes[i].Value.Trim());
                        continue;
                    }
                    events.Add(name, methodInfo);
                }
            }
        }

        /// <summary>
        /// 设置事件
        /// </summary>
        /// <param name="name"></param>
        /// <param name="method"></param>
        /// <param name="node"></param>
        private void setNodeEvent(string name, MethodInfo method, Node node)
        {
            if (name == "onDown" && node is IPointerEventFeature)
            {
                if (method.GetParameters()[0].ParameterType != typeof(DownEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + node.GetType().ToString() + "类型上的" + name + "事件时方法参数不正确");
                    return;
                }

                ((IPointerEventFeature)node).onDown.AddListener((DownEventData eventData) =>
                {
                    method.Invoke(this, new DownEventData[] { eventData });
                });
            }

            if (name == "onUp" && node is IPointerEventFeature)
            {
                if (method.GetParameters()[0].ParameterType != typeof(UpEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + node.GetType().ToString() + "类型上的" + name + "事件时方法参数不正确");
                    return;
                }

                ((IPointerEventFeature)node).onUp.AddListener((UpEventData eventData) =>
                {
                    method.Invoke(this, new UpEventData[] { eventData });
                });
            }

            if (name == "onClick" && node is IPointerEventFeature)
            {
                if (method.GetParameters()[0].ParameterType != typeof(ClickEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + node.GetType().ToString() + "类型上的" + name + "事件时方法参数不正确");
                    return;
                }

                ((IPointerEventFeature)node).onClick.AddListener((ClickEventData eventData) =>
                {
                    method.Invoke(this, new ClickEventData[] { eventData });
                });
            }

            if (name == "onDragged" && node is IPointerEventFeature)
            {
                if (method.GetParameters()[0].ParameterType != typeof(DraggedEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + node.GetType().ToString() + "类型上的" + name + "事件时方法参数不正确");
                    return;
                }

                ((IPointerEventFeature)node).onDragged.AddListener((DraggedEventData eventData) =>
                {
                    method.Invoke(this, new DraggedEventData[] { eventData });
                });
            }

            if (name == "onTouchStarted" && node is ITouchEventFeature)
            {
                if (method.GetParameters()[0].ParameterType != typeof(TouchStartedEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + node.GetType().ToString() + "类型上的" + name + "事件时方法参数不正确");
                    return;
                }

                ((ITouchEventFeature)node).onTouchStarted.AddListener((TouchStartedEventData eventData) =>
                {
                    method.Invoke(this, new TouchStartedEventData[] { eventData });
                });
            }

            if (name == "onTouchUpdate" && node is ITouchEventFeature)
            {
                if (method.GetParameters()[0].ParameterType != typeof(TouchUpdateEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + node.GetType().ToString() + "类型上的" + name + "事件时方法参数不正确");
                    return;
                }

                ((ITouchEventFeature)node).onTouchUpdate.AddListener((TouchUpdateEventData eventData) =>
                {
                    method.Invoke(this, new TouchUpdateEventData[] { eventData });
                });
            }

            if (name == "onTouchCompleted" && node is ITouchEventFeature)
            {
                if (method.GetParameters()[0].ParameterType != typeof(TouchCompletedEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + node.GetType().ToString() + "类型上的" + name + "事件时方法参数不正确");
                    return;
                }

                ((ITouchEventFeature)node).onTouchCompleted.AddListener((TouchCompletedEventData eventData) =>
                {
                    method.Invoke(this, new TouchCompletedEventData[] { eventData });
                });
            }

            if (name == "onFocusEnter" && node is IFocusEventFeature)
            {
                if (method.GetParameters()[0].ParameterType != typeof(FocusEnterEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + node.GetType().ToString() + "类型上的" + name + "事件时方法参数不正确");
                    return;
                }

                ((IFocusEventFeature)node).onFocusEnter.AddListener((FocusEnterEventData eventData) =>
                {
                    method.Invoke(this, new FocusEnterEventData[] { eventData });
                });
            }

            if (name == "onFocusExit" && node is IFocusEventFeature)
            {
                if (method.GetParameters()[0].ParameterType != typeof(FocusExitEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + node.GetType().ToString() + "类型上的" + name + "事件时方法参数不正确");
                    return;
                }

                ((IFocusEventFeature)node).onFocusExit.AddListener((FocusExitEventData eventData) =>
                {
                    method.Invoke(this, new FocusExitEventData[] { eventData });
                });
            }

            if (name == "onFocusChanged" && node is IFocusEventFeature)
            {
                if (method.GetParameters()[0].ParameterType != typeof(FocusChangedEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + node.GetType().ToString() + "类型上的" + name + "事件时方法参数不正确");
                    return;
                }

                ((IFocusEventFeature)node).onFocusChanged.AddListener((FocusChangedEventData eventData) =>
                {
                    method.Invoke(this, new FocusChangedEventData[] { eventData });
                });
            }


            if (name == "onScroll" && node is IScrollEventFeature)
            {
                if (method.GetParameters()[0].ParameterType != typeof(ScrollEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + node.GetType().ToString() + "类型上的" + name + "事件时方法参数不正确");
                    return;
                }

                ((IScrollEventFeature)node).onScroll.AddListener((ScrollEventData eventData) =>
                {
                    method.Invoke(this, new ScrollEventData[] { eventData });
                });
            }

            if (name == "onScrollReady" && node is IScrollEventFeature)
            {
                if (method.GetParameters()[0].ParameterType != typeof(ScrollReadyEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + node.GetType().ToString() + "类型上的" + name + "事件时方法参数不正确");
                    return;
                }

                ((IScrollEventFeature)node).onScrollReady.AddListener((ScrollReadyEventData eventData) =>
                {
                    method.Invoke(this, new ScrollReadyEventData[] { eventData });
                });
            }

            if (name == "onAppend" && node is IDocumentModelModify)
            {
                if (method.GetParameters()[0].ParameterType != typeof(AppendEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + node.GetType().ToString() + "类型上的" + name + "事件时方法参数不正确");
                    return;
                }

                ((IDocumentModelModify)node).onAppend.AddListener((AppendEventData eventData) =>
                {
                    method.Invoke(this, new AppendEventData[] { eventData });
                });
            }

            if (name == "onAppended" && node is IDocumentModelModify)
            {
                if (method.GetParameters()[0].ParameterType != typeof(AppendedEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + node.GetType().ToString() + "类型上的" + name + "事件时方法参数不正确");
                    return;
                }

                ((IDocumentModelModify)node).onAppended.AddListener((AppendedEventData eventData) =>
                {
                    method.Invoke(this, new AppendedEventData[] { eventData });
                });
            }

            if (name == "onInsert" && node is IDocumentModelModify)
            {
                if (method.GetParameters()[0].ParameterType != typeof(InsertEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + node.GetType().ToString() + "类型上的" + name + "事件时方法参数不正确");
                    return;
                }

                ((IDocumentModelModify)node).onInsert.AddListener((InsertEventData eventData) =>
                {
                    method.Invoke(this, new InsertEventData[] { eventData });
                });
            }

            if (name == "onInserted" && node is IDocumentModelModify)
            {
                if (method.GetParameters()[0].ParameterType != typeof(InsertedEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + node.GetType().ToString() + "类型上的" + name + "事件时方法参数不正确");
                    return;
                }

                ((IDocumentModelModify)node).onInserted.AddListener((InsertedEventData eventData) =>
                {
                    method.Invoke(this, new InsertedEventData[] { eventData });
                });
            }

            if (name == "onDestory" && node is IDocumentModelModify)
            {
                if (method.GetParameters()[0].ParameterType != typeof(DestoryEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + node.GetType().ToString() + "类型上的" + name + "事件时方法参数不正确");
                    return;
                }

                ((IDocumentModelModify)node).onDestory.AddListener((DestoryEventData eventData) =>
                {
                    method.Invoke(this, new DestoryEventData[] { eventData });
                });
            }

            if (name == "onDestoryed" && node is IDocumentModelModify)
            {
                if (method.GetParameters()[0].ParameterType != typeof(DestoryedEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + node.GetType().ToString() + "类型上的" + name + "事件时方法参数不正确");
                    return;
                }

                ((IDocumentModelModify)node).onDestoryed.AddListener((DestoryedEventData eventData) =>
                {
                    method.Invoke(this, new DestoryedEventData[] { eventData });
                });
            }

            if (name == "onManipulationStarted" && node is IManipulationEventFeature)
            {
                if (method.GetParameters()[0].ParameterType != typeof(ManipulationStartedEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + node.GetType().ToString() + "类型上的" + name + "事件时方法参数不正确");
                    return;
                }

                ((IManipulationEventFeature)node).onManipulationStarted.AddListener((ManipulationStartedEventData eventData) =>
                {
                    method.Invoke(this, new ManipulationStartedEventData[] { eventData });
                });
            }

            if (name == "onManipulationEnded" && node is IManipulationEventFeature)
            {
                if (method.GetParameters()[0].ParameterType != typeof(ManipulationEndedEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + node.GetType().ToString() + "类型上的" + name + "事件时方法参数不正确");
                    return;
                }

                ((IManipulationEventFeature)node).onManipulationEnded.AddListener((ManipulationEndedEventData eventData) =>
                {
                    method.Invoke(this, new ManipulationEndedEventData[] { eventData });
                });
            }

            if (name == "onBoundScaleStarted" && node is ISizeBoundsEventNode)
            {
                if (method.GetParameters()[0].ParameterType != typeof(BoundScaleStartedEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + node.GetType().ToString() + "类型上的" + name + "事件时方法参数不正确");
                    return;
                }

                ((ISizeBoundsEventNode)node).onBoundScaleStarted.AddListener((BoundScaleStartedEventData eventData) =>
                {
                    method.Invoke(this, new BoundScaleStartedEventData[] { eventData });
                });
            }

            if (name == "onBoundScaleEnded" && node is ISizeBoundsEventNode)
            {
                if (method.GetParameters()[0].ParameterType != typeof(BoundScaleEndedEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + node.GetType().ToString() + "类型上的" + name + "事件时方法参数不正确");
                    return;
                }

                ((ISizeBoundsEventNode)node).onBoundScaleEnded.AddListener((BoundScaleEndedEventData eventData) =>
                {
                    method.Invoke(this, new BoundScaleEndedEventData[] { eventData });
                });
            }

            if (name == "onCollisionEnter" && node is ICollisionEventFeature)
            {
                if (method.GetParameters()[0].ParameterType != typeof(CollisionEnterEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + node.GetType().ToString() + "类型上的" + name + "事件时方法参数不正确");
                    return;
                }

                ((ICollisionEventFeature)node).onCollisionEnter.AddListener((CollisionEnterEventData eventData) =>
                {
                    method.Invoke(this, new CollisionEnterEventData[] { eventData });
                });
            }

            if (name == "onCollisionStay" && node is ICollisionEventFeature)
            {
                if (method.GetParameters()[0].ParameterType != typeof(CollisionStayEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + node.GetType().ToString() + "类型上的" + name + "事件时方法参数不正确");
                    return;
                }

                ((ICollisionEventFeature)node).onCollisionStay.AddListener((CollisionStayEventData eventData) =>
                {
                    method.Invoke(this, new CollisionStayEventData[] { eventData });
                });
            }

            if (name == "onCollisionExit" && node is ICollisionEventFeature)
            {
                if (method.GetParameters()[0].ParameterType != typeof(CollisionExitEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + node.GetType().ToString() + "类型上的" + name + "事件时方法参数不正确");
                    return;
                }

                ((ICollisionEventFeature)node).onCollisionExit.AddListener((CollisionExitEventData eventData) =>
                {
                    method.Invoke(this, new CollisionExitEventData[] { eventData });
                });
            }
        }
        #endregion

        #region NPC事件
        /// <summary>
        /// 获取NPC事件的名-值集合
        /// </summary>
        /// <param name="current"></param>
        /// <param name="xmlNode"></param>
        /// <param name="events"></param>
        private void getNPCEventList(XmlNode xmlNode, ref Dictionary<string, MethodInfo> events)
        {
            // 计算当前节点的事件键值对
            for (var i = 0; i < xmlNode.Attributes.Count; i++)
            {
                if (xmlNode.Attributes[i].Name.StartsWith("onNpc"))
                {
                    string name = xmlNode.Attributes[i].Name.Substring(5, xmlNode.Attributes[i].Name.Length - 5);

                    var methodInfo = Utils.getMethod(xmlNode.Attributes[i].Value.Trim(), this);
                    if (methodInfo == null)
                    {
                        Debug.LogError("在组件" + this.GetType().ToString() + "中没有找到在类型" + xmlNode.Name + "上的" + name + "事件对应的方法" + xmlNode.Attributes[i].Value.Trim());
                        continue;
                    }
                    events.Add(name, methodInfo);
                }
            }
        }

        /// <summary>
        /// 设置NPC事件
        /// </summary>
        /// <param name="name"></param>
        /// <param name="method"></param>
        /// <param name="node"></param>
        private void setNPCEvent(string name, MethodInfo method, Node node)
        {
            if (method.GetParameters()[0].ParameterType != typeof(NPCEventData) && !typeof(NPCEventData).IsAssignableFrom(method.GetParameters()[0].ParameterType))
            {
                Debug.LogError("在" + this.GetType().ToString() + "中注册" + node.GetType().ToString() + "组件上的" + name + "事件时方法参数不正确");
                return;
            }

            node.npc?.AddListener(name, (NPCEventData eventData) =>
            {
                method.Invoke(this, new NPCEventData[] { eventData });
            });
        }
        #endregion


        /// <summary>
        /// 获取锚属性的名-值集合
        /// </summary>
        /// <param name="current"></param>
        /// <param name="xmlNode"></param>
        /// <param name="attributes"></param>
        private void getAnchorAttributeList(object current, XmlNode xmlNode, ref Dictionary<string, object> attributes)
        {
            // 计算当前节点的属性键值对
            for (var i = 0; i < xmlNode.Attributes.Count; i++)
            {
                if (!xmlNode.Attributes[i].Name.StartsWith("on"))
                {
                    // 是否为变量类型
                    bool isValueType = xmlNode.Attributes[i].Name.StartsWith("_");

                    // 获取属性名称
                    string name = isValueType ? xmlNode.Attributes[i].Name.Substring(1, xmlNode.Attributes[i].Name.Length - 1) : xmlNode.Attributes[i].Name;

                    // 计算属性值
                    object value;

                    if (!isValueType)
                    {
                        value = this.computeAttributeValue(current, name, xmlNode.Attributes[i].Value.Trim());
                    }
                    else
                    {
                        value = Utils.getProperty(xmlNode.Attributes[i].Value.Trim(), this);
                    }

                    // 将名-值对加入集合
                    attributes.Add(name, value);
                }
            }
        }

        #region 锚事件
        /// <summary>
        /// 获取锚事件的名-值集合
        /// </summary>
        /// <param name="current"></param>
        /// <param name="xmlNode"></param>
        /// <param name="events"></param>
        private void getAnchorEventList(object current, XmlNode xmlNode, ref Dictionary<string, MethodInfo> events)
        {
            // 计算当前节点的事件键值对
            for (var i = 0; i < xmlNode.Attributes.Count; i++)
            {
                if (xmlNode.Attributes[i].Name.StartsWith("on"))
                {
                    string name = xmlNode.Attributes[i].Name;

                    var methodInfo = Utils.getMethod(xmlNode.Attributes[i].Value.Trim(), this);

                    if (methodInfo == null)
                    {
                        Debug.LogError("在组件" + this.GetType().ToString() + "中没有找到在锚类型上的" + name + "事件对应的方法" + xmlNode.Attributes[i].Value.Trim());
                        continue;
                    }

                    events.Add(name, methodInfo);
                }
            }
        }

        /// <summary>
        /// 设置锚事件
        /// </summary>
        /// <param name="name"></param>
        /// <param name="method"></param>
        /// <param name="node"></param>
        private void setAnchorEvent(string name, MethodInfo method, Anchor anchor)
        {
            if (name == "onAnchorNodeHover")
            {
                if (method.GetParameters()[0].ParameterType != typeof(AnchorNodeHoverEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + anchor.node.GetType().ToString() + "类型上的锚" + name + "事件时方法参数不正确");
                    return;
                }

                anchor.onAnchorNodeHover.AddListener((AnchorNodeHoverEventData eventData) =>
                {
                    method.Invoke(this, new AnchorNodeHoverEventData[] { eventData });
                });
            }

            if (name == "onAnchorNodeOut")
            {
                if (method.GetParameters()[0].ParameterType != typeof(AnchorNodeOutEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + anchor.node.GetType().ToString() + "类型上的锚" + name + "事件时方法参数不正确");
                    return;
                }

                anchor.onAnchorNodeOut.AddListener((AnchorNodeOutEventData eventData) =>
                {
                    method.Invoke(this, new AnchorNodeOutEventData[] { eventData });
                });
            }

            if (name == "onAnchorJointHover")
            {
                if (method.GetParameters()[0].ParameterType != typeof(AnchorJointHoverEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + anchor.node.GetType().ToString() + "类型上的锚" + name + "事件时方法参数不正确");
                    return;
                }

                anchor.onAnchorJointHover.AddListener((AnchorJointHoverEventData eventData) =>
                {
                    method.Invoke(this, new AnchorJointHoverEventData[] { eventData });
                });
            }

            if (name == "onAnchorJointOut")
            {
                if (method.GetParameters()[0].ParameterType != typeof(AnchorJointOutEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + anchor.node.GetType().ToString() + "类型上的锚" + name + "事件时方法参数不正确");
                    return;
                }

                anchor.onAnchorJointOut.AddListener((AnchorJointOutEventData eventData) =>
                {
                    method.Invoke(this, new AnchorJointOutEventData[] { eventData });
                });
            }

            if (name == "onAnchorPointerHover")
            {
                if (method.GetParameters()[0].ParameterType != typeof(AnchorPointerHoverEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + anchor.node.GetType().ToString() + "类型上的锚" + name + "事件时方法参数不正确");
                    return;
                }

                anchor.onAnchorPointerHover.AddListener((AnchorPointerHoverEventData eventData) =>
                {
                    method.Invoke(this, new AnchorPointerHoverEventData[] { eventData });
                });
            }

            if (name == "onAnchorPointerOut")
            {
                if (method.GetParameters()[0].ParameterType != typeof(AnchorPointerOutEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + anchor.node.GetType().ToString() + "类型上的锚" + name + "事件时方法参数不正确");
                    return;
                }

                anchor.onAnchorPointerOut.AddListener((AnchorPointerOutEventData eventData) =>
                {
                    method.Invoke(this, new AnchorPointerOutEventData[] { eventData });
                });
            }
        }
        #endregion

        /// <summary>
        /// 获取组件属性的名-值集合
        /// </summary>
        /// <param name="current"></param>
        /// <param name="xmlNode"></param>
        /// <param name="attributes"></param>
        private void getComponentAttributeList(object current, XmlNode xmlNode, ref Dictionary<string, object> attributes)
        {
            // 计算当前节点的属性键值对
            for (var i = 0; i < xmlNode.Attributes.Count; i++)
            {
                // 过滤掉component的type类型
                if (xmlNode.Name == "Component" && xmlNode.Attributes[i].Name == "type")
                {
                    continue;
                }

                if (!xmlNode.Attributes[i].Name.StartsWith("on"))
                {
                    // 是否为变量类型
                    bool isValueType = xmlNode.Attributes[i].Name.StartsWith("_");

                    // 获取属性名称
                    string name = isValueType ? xmlNode.Attributes[i].Name.Substring(1, xmlNode.Attributes[i].Name.Length - 1) : xmlNode.Attributes[i].Name;

                    // 计算属性值
                    object value;

                    if (!isValueType)
                    {
                        value = this.computeAttributeValue(current, name, xmlNode.Attributes[i].Value.Trim());
                    }
                    else
                    {
                        value = Utils.getProperty(xmlNode.Attributes[i].Value.Trim(), this);
                    }

                    // 将名-值对加入集合
                    attributes.Add(name, value);
                }
            }
        }

        #region 组件事件
        /// <summary>
        /// 获取组件事件的名-值集合
        /// </summary>
        /// <param name="current"></param>
        /// <param name="xmlNode"></param>
        /// <param name="events"></param>
        private void getComponentEventList(XmlNode xmlNode, ref Dictionary<string, MethodInfo> events)
        {
            // 计算当前节点的事件键值对
            for (var i = 0; i < xmlNode.Attributes.Count; i++)
            {
                if (xmlNode.Attributes[i].Name.StartsWith("on"))
                {

                    string name = xmlNode.Attributes[i].Name;

                    var methodInfo = Utils.getMethod(xmlNode.Attributes[i].Value.Trim(), this);

                    if (methodInfo == null)
                    {
                        var componentTypeName = xmlNode.Name;
                        if (xmlNode.Name.IndexOf(".") == -1)
                        {
                            if (xmlNode.Name == "Component")
                            {
                                componentTypeName = "EMR." + xmlNode.Name;
                            }
                            else
                            {
                                componentTypeName = "EMR.Entity." + xmlNode.Name;
                            }
                        }

                        Debug.LogError("在组件" + this.GetType().ToString() + "中没有找到在类型" + componentTypeName + "上的" + name + "事件对应的方法" + xmlNode.Attributes[i].Value.Trim());
                        continue;
                    }


                    events.Add(name, methodInfo);
                }
            }
        }

        /// <summary>
        /// 设置组件事件
        /// </summary>
        /// <param name="name"></param>
        /// <param name="method"></param>
        /// <param name="component"></param>
        private void setComponentEvent(string name, MethodInfo method, Component component)
        {
            if (name == "onAppend" && component is IDocumentModelModify)
            {
                if (method.GetParameters()[0].ParameterType != typeof(AppendEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + component.GetType().ToString() + "类型上的" + name + "事件时方法参数不正确");
                    return;
                }

                ((IDocumentModelModify)component).onAppend.AddListener((AppendEventData eventData) =>
                {
                    method.Invoke(this, new AppendEventData[] { eventData });
                });
            }

            if (name == "onAppended" && component is IDocumentModelModify)
            {
                if (method.GetParameters()[0].ParameterType != typeof(AppendedEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + component.GetType().ToString() + "类型上的" + name + "事件时方法参数不正确");
                    return;
                }

                ((IDocumentModelModify)component).onAppended.AddListener((AppendedEventData eventData) =>
                {
                    method.Invoke(this, new AppendedEventData[] { eventData });
                });
            }

            if (name == "onInsert" && component is IDocumentModelModify)
            {
                if (method.GetParameters()[0].ParameterType != typeof(InsertEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + component.GetType().ToString() + "类型上的" + name + "事件时方法参数不正确");
                    return;
                }

               ((IDocumentModelModify)component).onInsert.AddListener((InsertEventData eventData) =>
               {
                   method.Invoke(this, new InsertEventData[] { eventData });
               });
            }

            if (name == "onInserted" && component is IDocumentModelModify)
            {
                if (method.GetParameters()[0].ParameterType != typeof(InsertedEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + component.GetType().ToString() + "类型上的" + name + "事件时方法参数不正确");
                    return;
                }

                ((IDocumentModelModify)component).onInserted.AddListener((InsertedEventData eventData) =>
                {
                    method.Invoke(this, new InsertedEventData[] { eventData });
                });
            }

            if (name == "onDestory" && component is IDocumentModelModify)
            {
                if (method.GetParameters()[0].ParameterType != typeof(DestoryEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + component.GetType().ToString() + "类型上的" + name + "事件时方法参数不正确");
                    return;
                }

                ((IDocumentModelModify)component).onDestory.AddListener((DestoryEventData eventData) =>
                {
                    method.Invoke(this, new DestoryEventData[] { eventData });
                });
            }

            if (name == "onDestoryed" && component is IDocumentModelModify)
            {
                if (method.GetParameters()[0].ParameterType != typeof(DestoryedEventData))
                {
                    Debug.LogError("在组件" + this.GetType().ToString() + "中注册" + component.GetType().ToString() + "类型上的" + name + "事件时方法参数不正确");
                    return;
                }

                ((IDocumentModelModify)component).onDestoryed.AddListener((DestoryedEventData eventData) =>
                {
                    method.Invoke(this, new DestoryedEventData[] { eventData });
                });
            }

            // 用户自定义事件
            if (name != "onAppend" && name != "onAppended" && name != "onInsert" && name != "onInserted" && name != "onDestory" && name != "onDestoryed")
            {
                if (method.GetParameters()[0].ParameterType != typeof(CustomEventData) && !typeof(CustomEventData).IsAssignableFrom(method.GetParameters()[0].ParameterType))
                {
                    Debug.LogError("在" + this.GetType().ToString() + "中注册" + component.GetType().ToString() + "组件上的" + name + "事件时方法参数不正确");
                    return;
                }

                component.AddListener(name.Substring(2, name.Length - 2), (CustomEventData eventData) =>
                {
                    method.Invoke(this, new CustomEventData[] { eventData });
                });
            }
        }
        #endregion


        /// <summary>
        /// 节点属性渲染
        /// </summary>
        /// <param name="node">当前处理的node</param>
        /// <param name="xmlNode">node在文档中对应的xml节点</param>
        private void renderNode(Node node, XmlNode xmlNode)
        {
            AttributeCatch attributeCatch = new AttributeCatch();

            // 属性字典
            Dictionary<string, object> attributes = new Dictionary<string, object>();

            // 事件字典
            Dictionary<string, MethodInfo> events = new Dictionary<string, MethodInfo>();

            // 属性样式缓冲列表
            List<AttributeCatchRecord> attributeCatchRecordList = new List<AttributeCatchRecord>();

            // 获取节点属性列表
            this.getNodeAttributeList(node, xmlNode, ref attributes);

            // SpaceMagic节点在视图上不能绑定除id和name以外的属性
            if (node is SpaceMagic)
            {
                if (attributes.Count > 2 || attributes.Count == 1 && !attributes.ContainsKey("name") && !attributes.ContainsKey("id") || attributes.Count == 2 && (!attributes.ContainsKey("name") || !attributes.ContainsKey("id")))
                {
                    Debug.LogError("在解析" + node.component + "组件的视图文档时发生错误，在视图文件中要求魔法节点上不能包含除name、id以外的任何属性");
                    return;
                }
            }


            foreach (var styleSheet in this.styleSheets)
            {
                // 设置节点的样式
                if (styleSheet != null)
                {
                    string tag = node.fullName;
                    string name = attributes.ContainsKey("name") ? attributes["name"].ToString() : null;
                    string id = attributes.ContainsKey("id") ? attributes["id"].ToString() : null;

                    string stylesheet = attributes.ContainsKey("stylesheet") ? attributes["stylesheet"].ToString() : null;

                    List<string> classNameList = new List<string>();
                    if (stylesheet != null)
                    {
                        var temp = stylesheet.Split(' ');
                        foreach (var item in temp)
                        {
                            if (item.Trim() != "")
                            {
                                classNameList.Add(item.Trim());
                            }
                        }
                    }

                    // 设置该节点样式
                    styleSheet.setStyle(node, tag, name, id, classNameList, ref attributeCatchRecordList);
                }
            }


            // 设置属性
            foreach (var key in attributes.Keys)
            {
                // 如果当节点的component属性为组件时并且节点中不存在给当前节点的该属性赋值同时该属性又为值类型时直接使用样式
                if (xmlNode.Attributes["_" + key] != null && !node.component.propertySetedMap.ContainsKey(xmlNode.Attributes["_" + key].Value))
                {
                    continue;
                }

                attributeCatchRecordList.Add(new AttributeCatchRecord
                {
                    name = key,
                    value = attributes[key],
                    from = 1
                });
            }

            // 将属性、样式加入缓冲
            foreach (var item in attributeCatchRecordList)
            {
                attributeCatch.add(item);
            }


            // 设置标识类属性
            foreach (var item in attributeCatch.markList)
            {
                if (item.name != "name" && item.name != "id")
                {
                    Utils.setProperty(item.name, item.value, node);
                }
            }

            // 设置参数类属性
            foreach (var item in attributeCatch.paramList)
            {
                Utils.setProperty(item.name, item.value, node);
            }

            // 设置尺寸类属性
            foreach (var item in attributeCatch.sizeList)
            {
                Utils.setProperty(item.name, item.value, node);
            }

            // 设置位置类属性
            foreach (var item in attributeCatch.positionList)
            {
                Utils.setProperty(item.name, item.value, node);
            }

            // 设置布局类属性
            foreach (var item in attributeCatch.layoutList)
            {
                Utils.setProperty(item.name, item.value, node);
            }

            // 设置其它属性
            foreach (var item in attributeCatch.otherList)
            {
                Utils.setProperty(item.name, item.value, node);
            }

            attributeCatch.clear();

            // 设置事件
            this.getNodeEventList(xmlNode, ref events);
            foreach (var name in events.Keys)
            {
                this.setNodeEvent(name, events[name], node);
            }


            // npc事件字典
            Dictionary<string, MethodInfo> npcEvents = new Dictionary<string, MethodInfo>();

            this.getNPCEventList(xmlNode, ref npcEvents);

            foreach (var name in npcEvents.Keys)
            {
                this.setNPCEvent(name, npcEvents[name], node);
            }
        }

        /// <summary>
        /// 渲染锚属性
        /// </summary>
        /// <param name="node">当前处理的节点</param>
        /// <param name="map">xmlNode 到 Anchor的映身map</param>
        public void renderAnchor(Node node, Dictionary<XmlNode, Anchor> map)
        {
            foreach (var item in map)
            {
                // 属性字典
                Dictionary<string, object> attributes = new Dictionary<string, object>();

                // 事件字典
                Dictionary<string, MethodInfo> events = new Dictionary<string, MethodInfo>();

                var xmlNode = item.Key;
                var anchor = item.Value;

                this.getAnchorAttributeList(anchor, xmlNode, ref attributes);

                if (attributes.ContainsKey("name"))
                {
                    var name = attributes["name"].ToString();
                    node.addAnchor(name, anchor);
                }
                else
                {
                    var name = Guid.NewGuid().ToString();
                    node.addAnchor(name, anchor);
                }

                foreach (var key in attributes.Keys)
                {
                    Utils.setProperty(key, attributes[key], anchor);
                }

                this.getAnchorEventList(anchor, xmlNode, ref events);
                foreach (var name in events.Keys)
                {
                    this.setAnchorEvent(name, events[name], anchor);
                }

                anchor.fresh();
            }
        }

        /// <summary>
        /// component属性渲染
        /// </summary>
        /// <param name="component">当前处理的component</param>
        /// <param name="xmlNode">component对应的xml节点</param>
        public void renderComponent(Component component, XmlNode xmlNode)
        {
            // 属性字典
            Dictionary<string, object> attributes = new Dictionary<string, object>();

            // 事件字典
            Dictionary<string, MethodInfo> events = new Dictionary<string, MethodInfo>();

            this.getComponentAttributeList(component, xmlNode, ref attributes);

            foreach (var styleSheet in this.styleSheets)
            {
                // 设置component的样式
                if (styleSheet != null)
                {
                    string tag = component.fullName;
                    string name = attributes.ContainsKey("name") ? attributes["name"].ToString() : null;
                    string Id = attributes.ContainsKey("id") ? attributes["id"].ToString() : null;

                    string stylesheet = attributes.ContainsKey("stylesheet") ? attributes["stylesheet"].ToString() : null;
                    List<string> classNameList = new List<string>();
                    if (stylesheet != null)
                    {
                        var temp = stylesheet.Split(' ');
                        foreach (var item in temp)
                        {
                            if (item.Trim() != "")
                            {
                                classNameList.Add(item);
                            }
                        }
                    }

                    // 设置该节点样式
                    styleSheet.setStyle(component, tag, name, Id, classNameList);
                }
            }

            foreach (var key in attributes.Keys)
            {
                // 如果当节点的component属性为组件时并且节点中不存在给当前节点的该属性赋值同时该属性又为值类型时直接使用样式
                if (xmlNode.Attributes["_" + key] != null && !component.parent.propertySetedMap.ContainsKey(xmlNode.Attributes["_" + key].Value))
                {
                    continue;
                }

                Utils.setProperty(key, attributes[key], component);
            }

            this.getComponentEventList(xmlNode, ref events);

            foreach (var name in events.Keys)
            {
                this.setComponentEvent(name, events[name], component);
            }
        }
    }
}
