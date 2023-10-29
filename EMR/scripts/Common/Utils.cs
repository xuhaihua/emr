using System;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using EMR.Event;
using EMR.Module;
using EMR.Struct;
using System.Xml;
using System.Text.RegularExpressions;
using System.Reflection;
using System.IO;

namespace EMR.Common
{
    // emit循环委托
    public delegate void XmlNodeHandler(XmlNode node);

    public delegate void ClickHandler(ClickEventData eventData);

    public class Utils
    {
        #region 常用方法
        public static Vector3 stringToVector3(string data)
        {
            var weightList = data.Split(',');
            var x = System.Convert.ToInt32(weightList[0]);
            var y = System.Convert.ToInt32(weightList[1]);
            var z = System.Convert.ToInt32(weightList[2]);

            return new Vector3(x, y, z);
        }

        public static string vector3ToString(Vector3 data)
        {
            return data.x + "," + data.y + "," + data.z;
        }

        public static Color stringToColor(string data)
        {
            var weightList = data.Split(',');
            var x = (float)System.Convert.ToDouble(weightList[0]) / 255f;
            var y = (float)System.Convert.ToDouble(weightList[1]) / 255f;
            var z = (float)System.Convert.ToDouble(weightList[2]) / 255f;
            var a = 1f;
            if(weightList.Length == 4)
            {
                a = (float)System.Convert.ToDouble(weightList[3]);
            }

            return new Color(x, y, z, a);
        }

        public static string colorToString(Color color)
        {
            return color.r + "," + color.g + "," + color.b + "," + color.a;
        }
        #endregion

        #region 数值比较相关
        /// <summary>
        /// 等于
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool equals(float a, float b)
        {
            bool result = false;
            if (Mathf.Abs(a - b) < 0.0005)
            {
                result = true;
            }
            return result;
        }

        /// <summary>
        /// 小于等于
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool noExceed(float a, float b)
        {
            return a < b || equals(a, b);
        }

        /// <summary>
        /// 大于等于
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool noUnder(float a, float b)
        {
            return a > b || equals(a, b);
        }
        #endregion

        #region 基础数据类型验证
        /// <summary>
        /// 是否为数字
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool isNumber(string value)
        {
            bool result = true;
            try
            {
                double a = Convert.ToInt32(value);//如果成功就是小数
            }
            catch
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// 是否为布尔true
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool isTrue(string value)
        {
            bool result = false;
            if (value == "true")
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// 是否为布尔false
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool isFalse(string value)
        {
            bool result = false;
            if (value == "false")
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// 是否为整数
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool isInt(string value)
        {
            var data = value;
            if (value.StartsWith("-"))
            {
                data = value.Substring(1, value.Length);
            }
            Regex regex = new Regex(@"^[0-9]\d*$");

            return regex.IsMatch(data);
        }
        #endregion

        #region 类型反射相关
        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <param name="name"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object getProperty(string name, object obj)
        {
            var type = obj.GetType();
            PropertyInfo pi = type.GetProperty(name);
            
            if(pi == null)
            {
                Debug.LogError("在解析" + obj + "组件时发生异常，因为该组件中不存在名为：" + name + "的属性器");
                return null;
            }

            return pi.GetValue(obj);
        }

        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool setProperty(string name, object value, object obj, bool isFromStyle = false)
        {
            bool result = true;

            if (name != "stylesheet")
            {
                try
                {
                    var type = obj.GetType();
                    PropertyInfo pi = type.GetProperty(name);
                    pi.SetValue(obj, value);
                }
                catch (Exception ex)
                {
                    var errorMessage = "类型或组件" + obj + "在设置属性" + name + "时发生异常请检查该属性是否存在和数据类型\n" + ex.Message;
                    if (isFromStyle)
                    {
                        errorMessage = "从样式表设置类型或组件" + obj + "的" + name + "属性时发生异常请检查该属性是否存在和数据类型\n" + ex.Message;
                    }
                    result = false;
                }
            }
            return result;
        }

        public static MethodInfo getMethod(string name, object obj)
        {
            var type = obj.GetType();
            return type.GetMethod(name);
        }
        #endregion

        #region Xml处理

        /// <summary>
        /// 加载xml文档
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static XmlNode loadXml(string path)
        {
            XmlNode result = null;
            TextAsset xmlFile = Resources.Load<TextAsset>(path);

            XmlDocument xmlDoc = new XmlDocument();

            try
            {
                xmlDoc.LoadXml(xmlFile.text);

                if (xmlDoc.ChildNodes.Count > 0)
                {
                    result = xmlDoc.ChildNodes[1];
                }
            } catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
            return result;
        }

        /// <summary>
        /// 遍历xml
        /// </summary>
        /// <param name="node"></param>
        public static void ergodicXml(XmlNode node, XmlNodeHandler nodeHandler)
        {
            nodeHandler(node);
            foreach (XmlNode item in node.ChildNodes)
            {
                ergodicXml(item, nodeHandler);
            }
        }
        #endregion
    }

    public static class GameObjectExtensions
    {
        public static Bounds CalculateBounds(this GameObject gameObject, bool localSpace = false)
        {
            Vector3 position = gameObject.transform.position;
            Quaternion rotation = gameObject.transform.rotation;
            Vector3 localScale = gameObject.transform.localScale;

            if (localSpace)
            {
                gameObject.transform.position = Vector3.zero;
                gameObject.transform.rotation = Quaternion.identity;
                gameObject.transform.localScale = Vector3.one;
            }

            Bounds bounds1 = new Bounds();
            Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();

            if (componentsInChildren.Length != 0)
            {
                Bounds bounds2 = componentsInChildren[0].bounds;

                bounds1.center = bounds2.center;
                bounds1.extents = bounds2.extents;
                for (int index = 1; index < componentsInChildren.Length; ++index)
                {
                    Bounds bounds3 = componentsInChildren[index].bounds;
                    bounds1.Encapsulate(bounds3);
                }
            }

            if (localSpace)
            {
                gameObject.transform.position = position;
                gameObject.transform.rotation = rotation;
                gameObject.transform.localScale = localScale;
            }

            return bounds1;
        }

        /// <summary>
        /// 获取模型的Bounds
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static Bounds CalculateModelBounds(GameObject gameObject)
        {
            Bounds bounds = gameObject.CalculateBounds(false);
            return bounds;
        }
    }
}
