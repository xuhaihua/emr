using EMR.Event;
using EMR.Entity;

namespace EMR
{
    /// <summary>
    /// 该节点具有触发文档内容修改事件能力
    /// </summary>
    public interface IDocumentModelModify { 

        /// <summary>
        /// 添加节点
        /// </summary>
        /// <param name="node">需要添加的节点</param>
        /// <returns></returns>
        Node appendNode(Node node);

        /// <summary>
        /// 添加组件
        /// </summary>
        /// <param name="component">需要添加的组件</param>
        /// <returns></returns>
        Component appendComponent(Component component);

        /// <summary>
        /// 插入节点
        /// </summary>
        /// <param name="current">要插入的节点</param>
        /// <param name="refNode">参照节点</param>
        /// <returns></returns>
        Node insertBefore(Node current, Node refNode);

        /// <summary>
        /// 插入节点
        /// </summary>
        /// <param name="current">要插入的节点</param>
        /// <param name="refComponent">参照组件</param>
        /// <returns></returns>
        Node insertBefore(Node current, Component refComponent);

        /// <summary>
        /// 插入组件
        /// </summary>
        /// <param name="component">要插入的组件</param>
        /// <param name="refNode">参照节点</param>
        /// <returns></returns>
        Component insertComponent(Component component, Node refNode);

        /// <summary>
        /// 插入组件
        /// </summary>
        /// <param name="component">要插入的组件</param>
        /// <param name="refComponent">参照组件</param>
        /// <returns></returns>
        Component insertComponent(Component component, Component refComponent);


        /// <summary>
        /// // 添加节点前事件
        /// </summary>
        AppendEvent onAppend { get; }

        /// <summary>
        /// 添加节点完成事件
        /// </summary>
        AppendedEvent onAppended { get; }

        /// <summary>
        /// 插入节点前事件
        /// </summary>
        InsertEvent onInsert { get; }

        /// <summary>
        /// 插入节点完成事件
        /// </summary>
        InsertedEvent onInserted { get; }

        DestoryEvent onDestory { get; }

        DestoryedEvent onDestoryed { get; }

        /// <summary>
        /// 销毁
        /// </summary>
        void destory();
    }
}
