using UnityEngine;
using EMR.Entity;
using EMR.Common;
using System.Collections.Generic;
using System.Reflection;

namespace EMR
{
    /// <summary>
    /// 元素允许作为样式的集合
    /// </summary>
    public class StyleCollect
    {
        private List<string> list = new List<string>();
        private object context = null;

        public StyleCollect(object context)
        {
            this.context = context;
        }

        /// <summary>
        /// 添加样式
        /// </summary>
        /// <param name="name">样式名</param>
        public void add(string name)
        {
            PropertyInfo propertyInfo = this.context.GetType().GetProperty(name);

            if(propertyInfo == null)
            {
                Debug.LogError("类型 " + this.context + " 中不存在名称为：" + name + "的属性");
            }

            if(propertyInfo.PropertyType == typeof(bool) || propertyInfo.PropertyType == typeof(string) || propertyInfo.PropertyType == typeof(int) || propertyInfo.PropertyType == typeof(double) || propertyInfo.PropertyType == typeof(decimal) || propertyInfo.PropertyType == typeof(float) || propertyInfo.PropertyType == typeof(Vector3) || propertyInfo.PropertyType == typeof(Color) || propertyInfo.PropertyType == typeof(float?))
            {
                this.list.Add(name);
            } else
            {
                Debug.LogError("只能类型为：数值、字符、布尔、Vector3的属性才能加入样式集");
            }
        }

        /// <summary>
        /// 是否包含样式
        /// </summary>
        /// <param name="name">样式名</param>
        /// <returns></returns>
        internal bool Contains(string name)
        {
            return this.list.Contains(name);
        }
    }

    /// <summary>
    /// 样式表
    /// </summary>
    public class Stylesheet : List<Style>
    {
        #region 基本属性
        /// <summary>
        /// id样式集合(以#开头)
        /// </summary>
        public Dictionary<string, Style> _idStyleCollect = new Dictionary<string, Style>();
        public Dictionary<string, Style> idStyleCollect
        {
            get
            {
                this._idStyleCollect = new Dictionary<string, Style>();

                foreach(var item in this)
                {
                    if (item.selector.StartsWith("#"))
                    {
                        this._idStyleCollect.Add(item.selector.Substring(1, item.selector.Length - 1), item);
                    }
                }

                return this._idStyleCollect;
            }
        }

        /// <summary>
        /// 类样式集合(以.开头)
        /// </summary>
        private Dictionary<string, Style> _classStyleCollect = null;
        public Dictionary<string, Style> classStyleCollect
        {
            get
            {
                this._classStyleCollect = new Dictionary<string, Style>();

                foreach (var item in this)
                {
                    if (item.selector.StartsWith("."))
                    {
                        this._classStyleCollect.Add(item.selector.Substring(1, item.selector.Length - 1), item);
                    }
                }

                return this._classStyleCollect;
            }
        }

        /// <summary>
        /// 名称样式集合(以$开头)
        /// </summary>
        private Dictionary<string, Style> _nameStyleCollect = null;
        public Dictionary<string, Style> nameStyleCollect
        {
            get
            {
                if(this._nameStyleCollect == null)
                {
                    this._nameStyleCollect = new Dictionary<string, Style>();

                    foreach (var item in this)
                    {
                        if (item.selector.StartsWith("$"))
                        {
                            this._nameStyleCollect.Add(item.selector.Substring(1, item.selector.Length - 1), item);
                        }
                    }
                }

                return this._nameStyleCollect;
            }
        }

        /// <summary>
        /// 元素样式集合(没有任何字符前缀)
        /// </summary>
        public Dictionary<string, Style> _elementStyleCollect = null;
        public Dictionary<string, Style> elementStyleCollect
        {
            get
            {
                if (this._elementStyleCollect == null)
                {
                    this._elementStyleCollect = new Dictionary<string, Style>();

                    foreach (var item in this)
                    {
                        if (!item.selector.StartsWith("$") && !item.selector.StartsWith(".") && !item.selector.StartsWith("#"))
                        {
                            this._elementStyleCollect.Add(item.selector, item);
                        }
                    }
                }

                return this._elementStyleCollect;
            }
        }

        /// <summary>
        /// 样式路径
        /// </summary>
        private string _path = "";
        public string path
        {
            get
            {
                return this._path;
            }

            set
            {
                this.load(value);
                this._path = value;
            }
        }
        #endregion

        #region 样式加载
        /*----------------------------------------定义样式加载支撑方法开始----------------------------------------*/
        /// <summary>
        /// 加载Json文档
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string loadJsonFile(string path)
        {
            string result = "";
            TextAsset xmlFile = Resources.Load<TextAsset>(path);
            if(xmlFile != null)
            {
                result = xmlFile.text;
            } else
            {
                Debug.LogError("在加载" + path + "样式文件时发生异常请检查该文件是否存在");
            }

            return result;
        }

        /// <summary>
        /// json字符串转字典
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        private Dictionary<string, object> jsonStringToDictionary(string jsonString)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            result = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            return result;
        }
        /*----------------------------------------定义样式加载支撑方法结束----------------------------------------*/

        /// <summary>
        /// 加载样式表
        /// </summary>
        /// <param name="path"></param>
        public void load(string path)
        {
            // 加载样式表文件
            var jsonString = this.loadJsonFile(path);
            var stylesheetDictionary = this.jsonStringToDictionary(jsonString);

            // 解析样式
            foreach (var item in stylesheetDictionary)
            {
                // 获取选择器
                var selector = item.Key;

                // 获取该选择器样式form
                var form = this.jsonStringToDictionary(item.Value.ToString());

                // 创建并向Stylesheet添加样式
                this.Add(new Style
                {
                    selector = selector,
                    form = form
                });
            }
        }
        #endregion

        #region 样式设置方法
        /// <summary>
        /// 设置节点样式
        /// </summary>
        /// <param name="element">设置的节点或componet</param>
        /// <param name="elementTag">标签名称</param>
        /// <param name="elementName">节点名称</param>
        /// <param name="elementId">节点id</param>
        /// <param name="classNameList">节点样式名列表</param>
        public void setStyle(object element, string elementTag, string elementName, string elementId, List<string> classNameList)
        {
            // tag属性器样式设置
            foreach (var item in this.elementStyleCollect)
            {
                if(item.Key == elementTag)
                {
                    item.Value.setStyle(element);
                }
            }

            // name属性器样式设置
            if(elementName != "" && elementName != null)
            {
                foreach (var item in this.nameStyleCollect)
                {
                    if(item.Key == elementName)
                    {
                        item.Value.setStyle(element);
                    }
                }
            }

            // class属性器样式设置
            if (classNameList != null && classNameList.Count != 0)
            {
                foreach (var item in this.classStyleCollect)
                {
                    foreach(var className in classNameList)
                    {
                        if (item.Key == className)
                        {
                            item.Value.setStyle(element);
                        }
                    }
                }
            }

            // id属性器样式设置
            if (elementId != "" && elementId != null)
            {
                foreach (var item in this.idStyleCollect)
                {
                    if (item.Key == elementId)
                    {
                        item.Value.setStyle(element);
                    }
                }
            }
        }

        /// <summary>
        /// 设置节点样式
        /// </summary>
        /// <param name="element">设置的节点或componet</param>
        /// <param name="elementTag">标签名称</param>
        /// <param name="elementName">节点名称</param>
        /// <param name="elementId">节点id</param>
        /// <param name="classNameList">节点样式名列表</param>
        /// <param name="AttributeCatchRecordList">属性样式名列表</param>
        internal void setStyle(object element, string elementTag, string elementName, string elementId, List<string> classNameList, ref List<AttributeCatchRecord> AttributeCatchRecordList)
        {
            // tag属性器样式设置
            foreach (var item in this.elementStyleCollect)
            {
                if (item.Key == elementTag)
                {
                    item.Value.setStyle(element, ref AttributeCatchRecordList);
                }
            }

            // name属性器样式设置
            if (elementName != "" && elementName != null)
            {
                foreach (var item in this.nameStyleCollect)
                {
                    if (item.Key == elementName)
                    {
                        item.Value.setStyle(element, ref AttributeCatchRecordList);
                    }
                }
            }

            // class属性器样式设置
            if (classNameList.Count != 0)
            {
                foreach (var item in this.classStyleCollect)
                {
                    foreach (var className in classNameList)
                    {
                        if (item.Key == className)
                        {
                            item.Value.setStyle(element, ref AttributeCatchRecordList);
                        }
                    }
                }
            }

            // id属性器样式设置
            if (elementId != "" && elementId != null)
            {
                foreach (var item in this.idStyleCollect)
                {
                    if (item.Key == elementId)
                    {
                        item.Value.setStyle(element, ref AttributeCatchRecordList);
                    }
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// 样式表
    /// </summary>
    public class Style
    {
        public string selector = "";
        public Dictionary<string, object> form = new Dictionary<string, object>();

        /// <summary>
        /// 设置样式
        /// </summary>
        public void setStyle(object element)
        {
            foreach (var item in this.form)
            {
                if (element is Node && ((Node)element).styleCollect.Contains(item.Key) || element is Component && ((Component)element).styleCollect.Contains(item.Key))
                {
                    CoreComponent component = null;

                    if (element is Node)
                    {
                        component = (CoreComponent)((Node)element).component;
                    }

                    if (element is Component)
                    {
                        component = (CoreComponent)element;
                    }

                    var value = component.computeAttributeValue(element, item.Key, item.Value.ToString(), true);

                    Utils.setProperty(item.Key, value, element, true);
                } else
                {
                    if(element is Node)
                    {
                        Debug.LogError("在类型为：" + element.GetType() + "的节点中不存在名称为：" + item.Key + " 的样式");
                    }

                    if (element is Component)
                    {
                        Debug.LogError("在类型为：" + element.GetType() + "的组件中不存在名称为：" + item.Key + " 的样式");
                    }
                }
            }
        }

        /// <summary>
        /// 设置样式
        /// </summary>
        internal void setStyle(object element, ref List<AttributeCatchRecord> AttributeCatchRecordList)
        {
            foreach (var item in this.form)
            {
                if (element is Node && ((Node)element).styleCollect.Contains(item.Key) || element is Component && ((Component)element).styleCollect.Contains(item.Key))
                {
                    CoreComponent component = null;

                    if (element is Node)
                    {
                        component = (CoreComponent)((Node)element).component;
                    }

                    if (element is Component)
                    {
                        component = (CoreComponent)element;
                    }

                    var value = component.computeAttributeValue(element, item.Key, item.Value.ToString(), true);

                    AttributeCatchRecordList.Add(new AttributeCatchRecord
                    {
                        name = item.Key,
                        value = value,
                        from = 0
                    });
                }
                else
                {
                    if (element is Node)
                    {
                        Debug.LogError("在类型为：" + element.GetType() + "的节点中不存在名称为：" + item.Key + " 的样式");
                    }

                    if (element is Component)
                    {
                        Debug.LogError("在类型为：" + element.GetType() + "的组件中不存在名称为：" + item.Key + " 的样式");
                    }
                }
            }
        }
    }
}
