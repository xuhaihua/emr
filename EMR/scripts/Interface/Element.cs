using EMR.Event;
using System.Collections.Generic;
using UnityEngine;

namespace EMR
{
    public interface Element
    {
        /// <summary>
        /// id
        /// </summary>
        string id { get; }

        /// <summary>
        /// 名称
        /// </summary>
        string name { get; }

        /// <summary>
        /// 偏移量
        /// </summary>
        Vector3 offset { get; set; }

        /// <summary>
        /// 宽度
        /// </summary>
        float width { get; set; }

        /// <summary>
        /// 高度
        /// </summary>
        float height { get; set; }

        /// <summary>
        /// 深度
        /// </summary>
        float depth { get; set; }

        /// <summary>
        /// 背景色
        /// </summary>
        string backgroundColor { get; set; }

        /// <summary>
        /// 背景图
        /// </summary>
        string backgroundImage { get; set; }

        /// <summary>
        /// 边框宽度
        /// </summary>
        float borderWidth { get; set; }

        /// <summary>
        /// 圆角半径
        /// </summary>
        float borderRadius { get; set; }

        /// <summary>
        /// 样式类
        /// </summary>
        string stylesheet { get; set; }

        /// <summary>
        /// 元素在X轴上的浮动
        /// </summary>
        string horizontalFloat { get; set; }

        /// <summary>
        /// 元素在Y轴上的浮动
        /// </summary>
        string verticalFloat { get; set; }

        /// <summary>
        /// 子元素在X轴上的对齐方式
        /// </summary>
        string contentHorizontal { get; set; }

        /// <summary>
        /// X轴上各子节点之间的对齐间距
        /// </summary>
        float horizontalInterval { get; set; }

        /// <summary>
        /// 开始元素左侧是否存在空隙间隔
        /// </summary>
        bool horizontalLeftInterval { get; set; }

        /// <summary>
        /// 结束元素右侧是否存在空隙间隔
        /// </summary>
        bool horizontalRightInterval { get; set; }

        /// <summary>
        /// 子元素在Y轴上的对齐方式
        /// </summary>
        string contentVertical { get; set; }

        /// <summary>
        /// Y轴上各子节点之间的对齐间距
        /// </summary>
        float verticalInterval { get; set; }

        /// <summary>
        /// 开始元素顶部是否存在空隙间隔
        /// </summary>
        bool verticalTopInterval { get; set; }

        /// <summary>
        /// 结束节点底部是否存在空隙间隔
        /// </summary>
        bool verticalBottomInterval { get; set; }

        /// <summary>
        /// 元素灯光强度
        /// </summary>
        float lightIntensity { get; set; }

        /// <summary>
        /// 悬浮灯光颜色
        /// </summary>
        string hoverColor { get; set; }

        /// <summary>
        /// 宽度是否固定
        /// </summary>
        bool widthFixed { get; set; }

        /// <summary>
        /// 高度是否固定
        /// </summary>
        bool heightFixed { get; set; }

        /// <summary>
        /// 深度或厚度是否固定
        /// </summary>
        bool depthFixed { get; set; }

        /// <summary>
        /// 元素是否可操纵
        /// </summary>
        bool enableManipulator { get; set; }


        /// <summary>
        /// NPC Resources路径
        /// </summary>
        string npcPath { get; set; }


        /// <summary>
        /// NPC 偏移量
        /// </summary>
        Vector3 npcOffset { get; set; }

        /// <summary>
        /// 父元素
        /// </summary>
        Element parentElement { get; }

        /// <summary>
        /// 前一个元素
        /// </summary>
        Element previousElement { get; }

        /// <summary>
        /// 后一个元素
        /// </summary>
        Element nextElement { get; }

        /// <summary>
        /// 第一个子元素
        /// </summary>
        Element firstChildElement { get; }

        /// <summary>
        /// 最后一个子元素
        /// </summary>
        Element lastChildElement { get; }

        /// <summary>
        /// 子元素列表
        /// </summary>
        List<Element> childElements { get; }
        

        /// <summary>
        /// 元素添加事件
        /// </summary>
        AppendEvent onAppend { get; }

        /// <summary>
        /// 元素添加完成事件
        /// </summary>
        AppendedEvent onAppended { get; }

        /// <summary>
        /// 元素插入事件
        /// </summary>
        InsertEvent onInsert { get; }

        /// <summary>
        /// 元素插入完成事件
        /// </summary>
        InsertedEvent onInserted { get; }

        /// <summary>
        /// 元素销毁事件
        /// </summary>
        DestoryEvent onDestory { get; }

        /// <summary>
        /// 元素销毁完成事件
        /// </summary>
        DestoryedEvent onDestoryed { get; }


        /// <summary>
        /// 查找子元素
        /// </summary>
        /// <returns></returns>
        List<Element> getChildElements();

        /// <summary>
        /// 按id查找子元素
        /// </summary>
        /// <param name="id">元素id</param>
        /// <returns></returns>
        Element getChildElementById(string id);

        /// <summary>
        /// 按名称查找子元素
        /// </summary>
        /// <param name="name">元素name</param>
        /// <returns></returns>
        List<Element> getChildElementsByName(string name);

        /// <summary>
        /// 添加元素
        /// </summary>
        /// <param name="element">要添加的元素</param>
        /// <returns></returns>
        Element appendElement(Element element);

        /// <summary>
        /// 插入子元素
        /// </summary>
        /// <param name="current">要插入的元素</param>
        /// <param name="refElement">参照元素，必须为直接子元素</param>
        /// <returns></returns>
        Element insertElement(Element current, Element refElement);

        /// <summary>
        /// 销毁
        /// </summary>
        void destory();
    }
}

