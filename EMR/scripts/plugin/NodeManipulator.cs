using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Input;
using EMR.Entity;


namespace EMR.Plugin
{
    public struct ManipulatorConfig
    {
        /// <summary>
        /// 碰撞体中心位置
        /// </summary>
        public Vector3 colliderCenter;

        /// <summary>
        /// 碰撞体尺寸
        /// </summary>
        public Vector3 colliderSize;
    }

    public class NodeManipulator : IPlugin
    {
        #region 基本字段
        /// <summary>
        /// 插件所作用的节点
        /// </summary>
        private Node _node;

        /// <summary>
        /// ObjectManipulator组件
        /// </summary>
        private ObjectManipulator _objectManipulator;

        /// <summary>
        /// 碰撞体
        /// </summary>
        private BoxCollider _collider;

        /// <summary>
        /// NearInteractionGrabbable组件
        /// </summary>
        private NearInteractionGrabbable _interactionGrabbable;

        /// <summary>
        /// 组件配制
        /// </summary>
        private ManipulatorConfig _config;
        #endregion


        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="node"></param>
        public NodeManipulator(Node node)
        {
            this._node = node;

            // 设置默认配制
            this.config = new ManipulatorConfig
            {
                colliderCenter = new Vector3(0, 0, 0),
                colliderSize = new Vector3(1, 1, 1),
            };
        }

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="node"></param>
        /// <param name="config"></param>
        public NodeManipulator(Node node, ManipulatorConfig config)
        {
            this._node = node;
            this._config = config;
        }

        #region 基本属性
        /// <summary>
        /// 使能
        /// </summary>
        public bool enable
        {
            get
            {
                return this._objectManipulator.enabled;
            }

            set
            {
                this._objectManipulator.enabled = value;
                this._collider.enabled = value;
            }
        }

        /// <summary>
        /// 配制
        /// </summary>
        public ManipulatorConfig config
        {
            get
            {
                return this._config;
            }

            set
            {
                this._config = value;
            }
        }
        #endregion

        #region 基本方法
        /// <summary>
        /// 刷新
        /// </summary>
        public void fresh()
        {
            if (this._objectManipulator == null)
            {
                
                this._collider = this._node.parasitifer.AddComponent<BoxCollider>();
                this._collider.enabled = true;
                this._interactionGrabbable = this._node.parasitifer.AddComponent<NearInteractionGrabbable>();
                this._objectManipulator = this._node.parasitifer.AddComponent<ObjectManipulator>();
                this._objectManipulator.OnManipulationStarted.AddListener((ManipulationEventData eventData) =>
                {
                    if(this._node is IManipulationEventFeature)
                    {
                        if (this._node is PanelNode)
                        {
                            PanelNode panelNode = (PanelNode)this._node;

                            var panelRoot = panelNode.panelRoot;
                            if (panelRoot != null)
                            {
                                this._objectManipulator.HostTransform = panelRoot.parasitifer.transform;
                            }
                            else
                            {
                                this._objectManipulator.HostTransform = null;
                            }
                        }

                        // 触发onManipulationStarted事件
                        ((IManipulationEventFeature)this._node).onManipulationStarted.Invoke(new Event.ManipulationStartedEventData
                        {
                            target = this._node,
                            original = eventData
                        });
                    }
                });


                this._objectManipulator.OnManipulationEnded.AddListener((ManipulationEventData eventData) =>
                {
                    if (this._node is IManipulationEventFeature)
                    {
                        // 触发onManipulationEnded事件
                        ((IManipulationEventFeature)this._node).onManipulationEnded.Invoke(new Event.ManipulationEndedEventData
                        {
                            target = this._node,
                            original = eventData
                        });
                    }
                });
            }

            this._objectManipulator.enabled = true;

            // 设置碰撞体
            this._collider.center = this.config.colliderCenter;
            this._collider.size = this.config.colliderSize;
        }


        /// <summary>
        /// 释放组件
        /// </summary>
        public void destory()
        {
            if (this._objectManipulator != null)
            {
                GameObject.Destroy(this._node.parasitifer.GetComponent<ObjectManipulator>());
                GameObject.Destroy(this._collider);
                GameObject.Destroy(this._interactionGrabbable);

                this._objectManipulator = null;
            }
        }
        #endregion
    }
}

