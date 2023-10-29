using EMR.Entity;
using EMR.Struct;
using UnityEngine;

namespace EMR.Layout
{
    public class SpaceLayout
    {
        #region 布局更新支撑方法
        /// <summary>
        /// 节点新增(插入、添加)行为的布局更新
        /// </summary>
        /// <param name="actionInfo">行为信息</param>
        private static void updateForNodeIncrease(SpaceNodeActionInfo actionInfo)
        {
            // 在系统编译过程中阻止进入此处逻辑，同样作用的逻辑在assembling逻辑中(此处逻辑只会在用户在手动创建添加节点时才进入)
            if (actionInfo.currentParent.component.isFormCompileAppend)
            {
                return;
            }

            Debug.Log("bbbbb");

            /**
             * 以下逻辑用于处理当前节点
             */
            // 添加后当前节点的父节点的尺寸位置都有可能会发生变化，在此对其进行尺寸的重新计算
            actionInfo.current.ergodic((Node node) =>
            {
                // 刷新节点的尺寸、位置、边框
                if (node is ISpaceCharacteristic)
                {
                    node.sizeFresh();

                    node.positionFresh();

                    // 加上这个判断主要是因为SpaceMagic是一个代理节点，在这个遍历过程中子节点也会被执行到
                    if (!(node is SpaceMagic))
                    {
                        // 刷新该节点的边框
                        node.borderFresh();

                        // 刷新npc
                        node.npcFresh();
                    } 

                    if(node is SpaceMagic)
                    {
                        node.sizeFresh();

                        node.positionFresh();
                    }
                }

                // 刷新该节点的sizeBound
                if (node is ISpaceCharacteristic)
                {
                    ((ISpaceCharacteristic)node).sizeBoundFresh();
                }

                return true;
            });
            
            // 阻止因空间节点尺寸改变触发相关事件而引发不必要的更新（例如引发尺寸改变而带来不必要的第二轮更新)
            actionInfo.current.ergodic((Node node) =>
            {
                if(node is ISpaceCharacteristic)
                {
                    // 阻止触发节点的onSizeChange事件
                    node.preventSizeEvent = true;

                    // 阻止触发节点的onPositionChange事件
                    node.preventPositionEvent = true;
                }

                return true;
            });
        }

        /// <summary>
        /// 节点尺寸改变布局更新
        /// </summary>
        /// <param name="actionInfo">行为信息</param>
        private static void updateForNodeSizeChange(SpaceNodeActionInfo actionInfo)
        {
            actionInfo.current.ergodic((Node node) =>
            {
                // 处理spaceNode节点
                if (node is ISpaceCharacteristic)
                {
                    node.sizeFresh();

                    node.positionFresh();

                    // 加上这个判断主要是因为SpaceMagic是一个代理节点，在这个遍历过程中子节点也会被执行掉
                    if (!(node is SpaceMagic))
                    {
                        // 刷新该节点的边框
                        node.borderFresh();

                        // 刷新npc
                        node.npcFresh();
                    }
                }

                // 当该节点的sizeBound
                if (node is ISpaceCharacteristic)
                {
                    ((ISpaceCharacteristic)node).sizeBoundFresh();
                }

                return true;
            });

            // 阻止因空间节点尺寸改变触发相关事件而引发不必要的更新（例如引发尺寸改变而带来不必要的第二轮更新)
            actionInfo.current.ergodic((Node node) =>
            {
                if (node is ISpaceCharacteristic)
                {
                    // 阻止触发节点的onPositionChange事件
                    node.preventPositionEvent = true;

                    // 阻止触发节点的onSizeChange事件
                    node.preventSizeEvent = true;
                }

                return true;
            });
        }

        /// <summary>
        /// 节点位置改变布局更新
        /// </summary>
        /// <param name="actionInfo">行为信息</param>
        private static void updateForNodePositionChange(SpaceNodeActionInfo actionInfo)
        {
            // 位置刷新
            actionInfo.current.positionFresh();

            // 阻止触发节点的onPositionChange事件
            actionInfo.current.preventPositionEvent = true;

            // 将节点的当前位置与位置暂存单元同步，这是为了防止用户类似将某一节点移动一段距离后，将其添加或插入到其他节点之下，这时因为节点的当前位置与暂存单元不同步所以会造成位置与预期的不同
            //((SpaceNode)actionInfo.current).positionSynch();
        }

        /// <summary>
        /// 节点旋转量改变布局更新
        /// </summary>
        /// <param name="actionInfo">行为信息</param>
        private static void updateForNodeRotationChange(SpaceNodeActionInfo actionInfo)
        {
            var node = actionInfo.current;
            // 处理spaceNode节点
            if (node is ISpaceCharacteristic)
            {
                // 尺寸刷新
                ((ISpaceCharacteristic)node).rotationFresh();
            }


            // 阻止因空间节点旋转改变触发相关事件而引发不必要的更新
            if (node is SpaceNode)
            {
                ((SpaceNode)node).preventRotationEvent = true;
            }
            if (node is PanelRoot)
            {
                ((PanelRoot)node).preventRotationEvent = true;
            }
        }
        #endregion

        /// <summary>
        /// 布局更新
        /// </summary>
        /// <param name="actionInfo">行为信息</param>
        public static void update(SpaceNodeActionInfo actionInfo)
        {
            // 新增布局调整
            if (actionInfo.action == SpaceNodeAction.append || actionInfo.action == SpaceNodeAction.insert)
            {
                updateForNodeIncrease(actionInfo);
            }

            // 尺寸改变布局调整
            if (actionInfo.action == SpaceNodeAction.sizeChange)
            {
                updateForNodeSizeChange(actionInfo);
            }

            // 空间节点位置更改布局调整
            if (actionInfo.action == SpaceNodeAction.positionChange && (actionInfo.current is SpaceNode))
            {
                updateForNodePositionChange(actionInfo);
            }

            // 旋转量改变布局调整
            if(actionInfo.action == SpaceNodeAction.rotationChange)
            {
                updateForNodeRotationChange(actionInfo);
            }
        }
    }
}
