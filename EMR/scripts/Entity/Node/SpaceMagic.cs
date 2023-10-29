using UnityEngine;
using EMR.Common;
using EMR.Event;
using EMR.Layout;
using EMR.Struct;

namespace EMR.Entity
{
    public class SpaceMagic : MagicProxy, ISpaceCharacteristic, IDocumentModelModify, Element
    {
        /// <summary>
        /// 构造器
        /// </summary>
        public SpaceMagic(CoreComponent component) : base(0f, 0f, 0f, 0f, 0f, 0f, 1f, 1f, 1f)
        {
            this._component = component;
        }

        #region 基本属性
        public ISpaceCharacteristic node
        {
            get
            {
                return this._node;
            }

            set
            {
                this._node = value;
            }
        }

        public override Anchor alignAnchor
        {
            get
            {
                return this.parent?.alignAnchor;
            }
        }

        /// <summary>
        /// x
        /// </summary>
        public float x
        {
            get
            {
                return this._node.x;
            }

            set
            {
                this._node.x = value;
            }
        }

        /// <summary>
        /// y
        /// </summary>
        public float y
        {
            get
            {
                return this._node.y;
            }

            set
            {
                this._node.y = value;
            }
        }

        /// <summary>
        /// z
        /// </summary>
        public float z
        {
            get
            {
                return this._node.z;
            }

            set
            {
                this._node.z = value;
            }
        }

        /// <summary>
        /// x轴旋转量
        /// </summary>
        public float xAngle
        {
            get
            {
                return this._node.xAngle;
            }

            set
            {
                this._node.xAngle = value;
            }
        }

        /// <summary>
        /// y轴旋转量
        /// </summary>
        public float yAngle
        {
            get
            {
                return this._node.yAngle;
            }

            set
            {
                this._node.yAngle = value;
            }
        }

        /// <summary>
        /// z轴旋转量
        /// </summary>
        public float zAngle
        {
            get
            {
                return this._node.zAngle;
            }

            set
            {
                this._node.zAngle = value;
            }
        }

        public string forwardFloat
        {
            get
            {
                var result = "none";
                if(this._node is SpaceNode)
                {
                    result = ((SpaceNode)this._node).forwardFloat;
                }

                if (this._node is PanelRoot)
                {
                    result = ((PanelRoot)this._node).forwardFloat;
                }

                return result;
            }

            set
            {
                if (this._node is SpaceNode)
                {
                    ((SpaceNode)this._node).forwardFloat = value;
                }

                if (this._node is PanelRoot)
                {
                    ((PanelRoot)this._node).forwardFloat = value;
                }
            }
        }
        #endregion

        #region 基本方法
        /*
         *  以下方法主要用于节点尺寸、位置刷新
         */
        /// <summary>
        /// 是否有零节点
        /// </summary>
        /// <returns></returns>
        private bool hasZeroNode()
        {
            Node node = this.parent;
            bool result = false;
            while (node != null)
            {
                if (Utils.equals(node.size.width, 0f) || Utils.equals(node.size.height, 0f) || Utils.equals(node.size.depth, 0f))
                {
                    result = true;
                    break;
                }
                node = node.parent;
            }
            return result;
        }

        /// <summary>
        /// 解锁节点尺寸有效设值过逻辑标识
        /// </summary>
        public override void unlockSizeSetedTag()
        {
            this._hasSizeSeted = false;
            this._node?.unlockSizeSetedTag();
        }

        /// <summary>
        /// 解锁节点位置有效设值过逻辑标识
        /// </summary>
        public void unlockPositionSetedTag()
        {
            this._hasPositionSeted = false;
            this._node?.unlockPositionSetedTag();
        }

        private bool _hasSizeSeted = false;
        /// <summary>
        /// 尺寸刷新
        /// </summary>
        public override void sizeFresh()
        {
            if (!this._hasSizeSeted && !this.hasZeroNode())
            {

                
                this.size.width = 1f;
                this.size.height = 1f;
                this.size.depth = 1f;

                this._hasSizeSeted = true;
            }
        }

        /// <summary>
        /// sizeBounds组件刷新
        /// </summary>
        public void sizeBoundFresh()
        {
            this._node.sizeBoundFresh();
        }

        private bool _hasPositionSeted = false;
        /// <summary>
        /// 位置刷新
        /// </summary>
        public override void positionFresh()
        {
            if (!this._hasPositionSeted && !this.hasZeroNode())
            {
                this.position.x = 0f;
                this.position.y = 0f;
                this.position.z = 0f;

                this._hasPositionSeted = true;
            }
        }

        public void rotationFresh()
        {
            ;
        }

        /// <summary>
        /// 位置同步
        /// </summary>
        public void positionSync()
        {
            if(this.node is SpaceNode)
            {
                ((SpaceNode)this.node).positionSync();
            }

            if (this.node is PanelRoot)
            {
                ((PanelRoot)this.node).positionSync();
            }
        }

        /// <summary>
        /// 尺寸同步
        /// </summary>
        public void sizeSync()
        {
            if (this.node is SpaceNode)
            {
                ((SpaceNode)this.node).sizeSync();
            }

            if (this.node is PanelRoot)
            {
                ((PanelRoot)this.node).sizeSync();
            }
        }

        /// <summary>
        /// 旋转量同步
        /// </summary>
        public void roationSync()
        {
            if (this.node is SpaceNode)
            {
                ((SpaceNode)this.node).roationSync();
            }

            if (this.node is PanelRoot)
            {
                ((PanelRoot)this.node).roationSync();
            }
        }

        public void fresh()
        {
            this.sizeFresh();
            this.positionFresh();
        }
        #endregion

        #region 节点数据结构基本操作
        /// <summary>
        /// 添加PanelRoot节点
        /// </summary>
        /// <param name="node">需要添加的节点</param>
        /// <returns></returns>
        public ISpaceCharacteristic appendNode(ISpaceCharacteristic node)
        {
            ISpaceCharacteristic result = null;

            // 抛出节点添加前事件
            AppendEventData appendEventData = new AppendEventData
            {
                target = (Element)node
            };
            onAppend.Invoke(appendEventData);

            // 使用默认行为添加节点
            if (!appendEventData.isPreventDefault)
            {
                var originalParent = ((Node)node).parent;

                node.unlockSizeSetedTag();
                node.unlockPositionSetedTag();

                var temp = base.appendNode((Node)node);
                result = temp != null ? (ISpaceCharacteristic)temp : null;
                if (result != null)
                {
                    this.addCentent((Node)node);

                    this._node = node;

                    SpaceLayout.update(new SpaceNodeActionInfo
                    {
                        originalParent = originalParent,
                        current = (Node)node,
                        currentParent = this,
                        action = SpaceNodeAction.append
                    });
                }
            }

            // 拙出节点添加完成事件
            AppendedEventData appendedEventData = new AppendedEventData
            {
                target = (Element)node,
                isSuccess = result != null ? true : false,
            };
            onAppended.Invoke(appendedEventData);

            // 返回添加结果
            return result != null ? (ISpaceCharacteristic)result : null;
        }

        /// <summary>
        /// 添加节点
        /// </summary>
        /// <param name="node">需要添加的节点</param>
        /// <returns></returns>
        public override Node appendNode(Node node)
        {
            Node result = null;

            if (!(node is ISpaceCharacteristic))
            {
                Debug.LogError("SpaceMagic 只能添加ISpaceCharacteristic类型的节点");
                return null;
            }

            if(this.children.Count > 0)
            {
                Debug.LogError("SpaceMagic 下只能包含一个节点");
                return null;
            }

            if (node is ISpaceCharacteristic)
            {
                result = (Node)this.appendNode((ISpaceCharacteristic)node);
            }

            return result;
        }
        #endregion
    }
}