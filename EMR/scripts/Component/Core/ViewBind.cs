/*
 * 本文件内的方法主要用于视图绑定
 */

using System.Collections.Generic;
using System.Xml;
using EMR.Common;

namespace EMR
{
    /// <summary>
    /// 视图关联信息描述结构
    /// </summary>
    public struct CorrelatorInfo
    {
        public string name;
        public object correlatorObject;
    }

    /// <summary>
    /// 属性获取完成回调
    /// </summary>
    /// <param name="resultValue"></param>
    /// <param name="isSuccess"></param>
    /// <returns></returns>
    public delegate object GetPropertyCallback(object resultValue, bool isSuccess);

    /// <summary>
    /// 属性设值完成回调
    /// </summary>
    /// <param name="resultValue"></param>
    public delegate void SetPropertyCallback(object resultValue);

    public partial class CoreComponent
    {
        /// <summary>
        /// 属性关联map
        /// </summary>
        public Dictionary<string, List<CorrelatorInfo>> correlatorMap = new Dictionary<string, List<CorrelatorInfo>>();

        /// <summary>
        /// 删除属性关联
        /// </summary>
        /// <param name="correlatorObject"></param>
        public void delCorrelator(object correlatorObject)
        {
            foreach (var item in this.correlatorMap)
            {
                var list = item.Value;
                for (var i = 0; i < list.Count; i++)
                {
                    var correlatorInfo = list[i];
                    if (correlatorInfo.correlatorObject == correlatorObject)
                    {
                        list.Remove(correlatorInfo);
                    }
                }
            }
        }

        /// <summary>
        /// 属性关联绑定
        /// </summary>
        /// <param name="current"></param>
        /// <param name="xmlNode"></param>
        private void correlatorBind(object current, XmlNode xmlNode)
        {
            // 计算当前节点的属性键值对
            for (var i = 0; i < xmlNode.Attributes.Count; i++)
            {
                // 是否为变量类型
                if (xmlNode.Attributes[i].Name.StartsWith("_"))
                {
                    // 获取属性名称
                    string name = xmlNode.Attributes[i].Name.Substring(1, xmlNode.Attributes[i].Name.Length - 1);

                    // 关联信息对象
                    var correlatorInfo = new CorrelatorInfo
                    {
                        name = name,
                        correlatorObject = current
                    };

                    List<CorrelatorInfo> correlatorInfoList = new List<CorrelatorInfo>();
                    if (!correlatorMap.ContainsKey(name))
                    {
                        correlatorMap.Add(name, correlatorInfoList);
                    }
                    else
                    {
                        correlatorInfoList = correlatorMap[name];
                    }

                    correlatorInfoList.Add(correlatorInfo);
                }
            }
        }

        /// <summary>
        /// 获取关联对象属性值
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public T getProperty<T>(string name, GetPropertyCallback propertyHandler)
        {
            object result = null;

            if (!this.isAssembling && correlatorMap.ContainsKey(name))
            {
                var correlatorInfoList = correlatorMap[name];

                if (correlatorInfoList.Count > 0)
                {
                    var attributeCorrelatorInfo = correlatorInfoList[0];
                    result = Utils.getProperty(attributeCorrelatorInfo.name, attributeCorrelatorInfo.correlatorObject);
                }
            }

            if (propertyHandler != null)
            {
                bool isSuccess = !this.isAssembling && correlatorMap.ContainsKey(name);

                result = propertyHandler(result, isSuccess);
            }

            return (T)result;
        }

        private Dictionary<string, bool> propertySetedMap = new Dictionary<string, bool>();

        /// <summary>
        /// 设置关联对象属性值
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="correlatorObject"></param>
        public bool setProperty(string name, object value, SetPropertyCallback propertyHandler)
        {
            bool result = false;

            if(!propertySetedMap.ContainsKey(name))
            {
                propertySetedMap.Add(name, true);
            }

            if (!this.isAssembling && correlatorMap.ContainsKey(name))
            {
                var correlatorInfoList = correlatorMap[name];
                foreach (var item in correlatorInfoList)
                {
                    result = Utils.setProperty(item.name, value, item.correlatorObject);
                }
            }

            if (propertyHandler != null)
            {
                propertyHandler(value);
                result = true;
            }

            return result;
        }
    }
}
