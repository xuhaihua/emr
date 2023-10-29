using EMR.Entity;
using EMR.Struct;
using EMR.Plugin;
using UnityEngine;

namespace EMR.Layout
{
    public class PanelLayout
    {
        #region 布局更新支撑方法
        /// <summary>
        /// 滚动内容bound刷新
        /// </summary>
        /// <param name="node">需要更新的平面节点</param>
        private static void scrollContentBoundFresh(PanelNode node)
        {
            // 滚动内容尺寸刷新
            PanelNode scrollNode = node.getOverflow() != NodeOverflow.visible ? node : PanelNode.findParentScrollNode(node);

            if (scrollNode != null)
            {
                scrollNode.isContentBoundFresh = true;
            }
        }

        /// <summary>
        /// 节点浮动刷新
        /// </summary>
        /// <param name="node">需要更新的PanelLayer节点</param>
        private static void nodeFloatFresh(IPanelLayoutFeature node)
        {
            node.horizontalFresh();
            node.verticalFresh();
        }

        /// <summary>
        /// PanelLayer 节点z坐标刷新
        /// </summary>
        /// <param name="elementRoot">需要更新的PanelLayer节点</param>
        private static void coordZFresh(PanelLayer elementRoot)
        {
            // 从根PanelLayer节点开始刷新LayoutHeight
            var rootElement = elementRoot;
            while (rootElement.parent != null && !(rootElement.parent is PanelRoot))
            {
                rootElement = (PanelLayer)rootElement.parent;
            }
            rootElement.freshLayoutHeight();

            // 从根PanelLayer节点开始计算所有子节点包括自身的z坐标
            PanelNode computeCoordZParentNode = null;
            rootElement.ergodic((PanelLayer node) =>
            {
                if (node.parent != computeCoordZParentNode)
                {
                    var elementNode = (PanelLayer)node;
                    elementNode.compountCoordZ();

                    // 阻止同层内节点多次计算coordZ因为compountCoordZ已在同层内对所有兄弟节点做了z坐标的计算
                    computeCoordZParentNode = elementNode.parent;
                }

                return true;
            });
        }


        private static void updateForScroll(PanelNodeActionInfo actionInfo)
        {
            // 滚动时内容尺寸不需要刷新
            PanelScroll.viewFresh(actionInfo.current, false, false);

            // 滚动时需要刷新子节点的浮动，以使它在视窗中一直保持正确位置
            foreach (var item in actionInfo.current.children)
            {
                item.horizontalFresh();
                item.verticalFresh();
                item.preventPositionEvent = true;
            }
        }


        private static void updateForNodeOverflowChange(PanelNodeActionInfo actionInfo)
        {
            if (actionInfo.currentParent != null)
            {
                scrollContentBoundFresh(actionInfo.currentParent);
            }
        }

        /// <summary>
        /// 节点新增(插入、添加)行为的布局更新
        /// </summary>
        /// <param name="actionInfo"></param>
        private static void updateForNodeIncrease(PanelNodeActionInfo actionInfo)
        {
            // 由系统编译的不进入止处进行布局调整，同样作用的逻辑在assembling逻辑中(此处逻辑只会有用户在手动创建添加节点时才进入)
            if (actionInfo.currentParent.component.isFormCompileAppend)
            {
                return;
            }

            /**
             * 以下逻辑用于处理原父节点
             */
            if (actionInfo.originalParent != null)
            {
                // 原父节点下的子节点被抽离后对齐布局一定不正确所以此处需要重新调整该节点下的布局
                actionInfo.originalParent.contentAlignFresh();

                // 原父节点下的子节点被抽离后该节点的层叠高度及深度会发生改变故此处更新该节点的深度并重新计算该平面的z坐标
                PanelLayer compountCoordZNode = null;
                if (actionInfo.originalParent is PanelLayer)
                {
                    compountCoordZNode = (PanelLayer)actionInfo.originalParent;
                }
                else
                {
                    var childs = actionInfo.originalParent.children;
                    if (childs.Count > 0)
                    {
                        compountCoordZNode = actionInfo.originalParent.children[0];
                    }
                }

                if (compountCoordZNode != null)
                {
                    coordZFresh(compountCoordZNode);
                }

                // 原父节点下的子节点被抽离后该节点会对其上级的滚动内容有引响故在此查找并调整他的滚动内容（计算需要滚动的内容尺寸)
                scrollContentBoundFresh(actionInfo.originalParent);
            }

            /**
             * 以下逻辑用于处理当前节点
             */
            // 添加后当前节点的父节点的尺寸、位置都有可能会发生变化(例如节点node的left、top、right、bottom都为0这时他的尺寸取决于当前父节点)，在此对其进行尺寸的重新计算
            actionInfo.current.ergodic((PanelNode node) =>
            {
                // 刷新尺寸
                node.sizeFresh();

                // 位置刷新
                node.positionFresh();

                return true;
            });

            // 以下是基于在尺寸和位置都被刷新后需要对节点进行的逻辑处理（因为以下的逻辑依赖于节点的尺寸、位置）
            actionInfo.current.ergodic((PanelNode node) =>
            {
                // 边框刷新
                node.borderFresh();

                // 刷新PanelLayer节点的npc
                node.npcFresh();

                // 子节点对齐刷新
                node.contentAlignFresh();

                // 更新节点自身对齐（浮动）刷新
                if (node is PanelLayer)
                {
                    nodeFloatFresh((PanelLayer)node);
                }

                // 对文本进行刷新
                var text = node.textNodeList.Count > 0 ? node.textNodeList[0] : null;
                if (text != null)
                {
                    text.fresh();
                }

                // 添加后可能节点的可视区会发生变化所以需要刷新
                PanelScroll.viewFresh(node, true, true);

                return true;
            });

           
            /*
             * 以下逻辑处理添加后当前节点在新结构下的父节点
             */
            if (actionInfo.currentParent != null)
            {
                // 子节点对齐刷新
                actionInfo.currentParent.contentAlignFresh();

                // 更新节点自身对齐（浮动）刷新
                foreach (var item in actionInfo.currentParent.children)
                {
                    nodeFloatFresh(item);
                }

                // 自然添加后新节点的位置也有可能发生改变，在此重计算他的z坐标
                var compountCoordZNode = actionInfo.currentParent is PanelLayer ? (PanelLayer)actionInfo.currentParent : (PanelLayer)actionInfo.current;
                if (compountCoordZNode != null)
                {
                    coordZFresh(compountCoordZNode);
                }

                var text = actionInfo.currentParent.textNodeList.Count > 0 ? actionInfo.currentParent.textNodeList[0] : null;
                if (text != null)
                {
                    text.fresh();
                }

                // 滚动内容尺寸刷新
                scrollContentBoundFresh(actionInfo.currentParent);
            }

            // 阻止因节点尺寸改变触发相关事件而引发不必要的更新（例如引发尺寸改变而带来不必要的第二轮更新)
            actionInfo.current.ergodic((PanelNode node) =>
            {
                // 阻止触发节点的onSizeChange事件
                node.preventSizeEvent = true;

                // 阻止触发节点的onPositionChange事件
                node.preventPositionEvent = true;

                return true;
            });
        }

        /// <summary>
        /// 节点尺寸改变布局更新
        /// </summary>
        /// <param name="actionInfo"></param>
        private static void updateForNodeSizeChange(PanelNodeActionInfo actionInfo)
        {
            // 处理尺寸、位置、边框
            actionInfo.current.ergodic((PanelNode node) =>
            {
                // 尺寸刷新
                node.sizeFresh();

                // 位置刷新
                node.positionFresh();

                // 刷新边框
                node.borderFresh();

                if(node is PanelRoot)
                {
                    ((PanelRoot)node).sizeBoundFresh();
                }

                // 刷新节点的npc
                node.npcFresh();

                return true;
            });

            // 处理当前节点及子节点布局
            actionInfo.current.ergodic((PanelNode node) =>
            {
                // 刷新子节点对齐方式
                node.contentAlignFresh();

                // 节点浮动刷新
                if (node is PanelLayer)
                {
                    nodeFloatFresh((PanelLayer)node);
                }

                // 当尺寸发生改变时该节点的文本需要被刷新
                var text = node.textNodeList.Count > 0 ? node.textNodeList[0] : null;
                if (text != null)
                {
                    text.fresh();
                }

                // 滚动视口刷新
                PanelScroll.viewFresh(node, true, false);

                return true;
            });


            /*
             * 以下逻辑处理当前节点的父节点
             */
            if (actionInfo.currentParent != null)
            {
                // 子节点对齐刷新
                actionInfo.currentParent.contentAlignFresh();

                // 更新节点自身对齐（浮动）刷新
                foreach (var item in actionInfo.currentParent.children)
                {
                    nodeFloatFresh(item);
                }

                // 当尺寸发生改变时该节点的文本需要被刷新
                var text = actionInfo.currentParent.textNodeList.Count > 0 ? actionInfo.currentParent.textNodeList[0] : null;
                if (text != null)
                {
                    text.fresh();
                }
            }

            // 阻止因节点尺寸改变触发相关事件而引发不必要的更新（例如引发尺寸改变而带来不必要的第二轮更新)
            actionInfo.current.ergodic((PanelNode node) =>
            {
                // 阻止触发节点的onPositionChange事件
                node.preventPositionEvent = true;

                // 阻止触发节点的onSizeChange事件
                node.preventSizeEvent = true;

                return true;
            });
        }

        /// <summary>
        /// 节点位置改变布局更新
        /// </summary>
        /// <param name="actionInfo"></param>
        private static void updateForNodePositionChange(PanelNodeActionInfo actionInfo)
        {
            // 位置刷新
            actionInfo.current.positionFresh();

            // 如果PanelLayer节点位置发生改变，他有可能会引响父滚动节点的滚动尺寸，所以这里需要对父滚动节点进行滚动尺寸刷新
            if (actionInfo.current is PanelLayer)
            {
                scrollContentBoundFresh((PanelNode)actionInfo.current.parent);
            }

            // 因为节点位置发生改变，有可能该节点部分已被父节点切割所以这里需要重新计算该节点的可视区
            PanelScroll.viewFresh(actionInfo.current, true, false);
        }

        /// <summary>
        /// 节点滚动就绪布局更新
        /// </summary>
        /// <param name="actionInfo"></param>
        public static void updateForNodeScrollReady(PanelNodeActionInfo actionInfo)
        {
            PanelScroll.viewFresh(actionInfo.current, true, true);
        }

        /// <summary>
        /// 节点销毁布局更新
        /// </summary>
        /// <param name="actionInfo"></param>
        private static void updateForNodeDestory(PanelNodeActionInfo actionInfo)
        {
            // PanelRoot节点拥有空间特性它与空间节点在布局上的特征一样，所以他的销毁不需要进行布局的调整
            if (actionInfo.current is PanelRoot)
            {
                return;
            }

            // 刷新当前节点的parnet节点
            actionInfo.currentParent.contentAlignFresh();

            // 子节点浮动刷新
            foreach (var item in actionInfo.currentParent.children)
            {
                nodeFloatFresh(item);
            }

            // z坐标更新
            PanelLayer compountCoordZNode = actionInfo.currentParent is PanelLayer ? (PanelLayer)actionInfo.currentParent : null;
            if (compountCoordZNode != null)
            {
                coordZFresh(compountCoordZNode);
            }

            // 更新parent节点的滚动视口
            scrollContentBoundFresh(actionInfo.currentParent);
        }
        #endregion

        /// <summary>
        /// 布局更新
        /// </summary>
        /// <param name="actionInfo"></param>
        public static void update(PanelNodeActionInfo actionInfo)
        {
            // 新增
            if(actionInfo.current.panelRoot != null && (actionInfo.action == PanelNodeAction.append || actionInfo.action == PanelNodeAction.insert))
            {
                 updateForNodeIncrease(actionInfo);
            }

            // 尺寸改变
            if (actionInfo.action == PanelNodeAction.sizeChange)
            {
                updateForNodeSizeChange(actionInfo);
            }

            // 位置改变
            if (actionInfo.action == PanelNodeAction.positionChange)
            {
                updateForNodePositionChange(actionInfo);
            }

            // 滚动准备就绪
            if (actionInfo.action == PanelNodeAction.scrollReady)
            {
                updateForNodeScrollReady(actionInfo);
            }

            // 滚动
            if (actionInfo.action == PanelNodeAction.scroll)
            {
                updateForScroll(actionInfo);
            }

            // 节点溢出显示方式改变
            if (actionInfo.action == PanelNodeAction.overflowChange)
            {
                updateForNodeOverflowChange(actionInfo);
            }

            // 销毁节点
            if (actionInfo.action == PanelNodeAction.nodeDestory)
            {
                updateForNodeDestory(actionInfo);
            }
        }
    }
}

