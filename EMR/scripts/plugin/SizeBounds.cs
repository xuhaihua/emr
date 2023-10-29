using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.UI.BoundsControlTypes;
using EMR.Entity;
using EMR.Event;


namespace EMR.Plugin
{
    public class SizeBoundsConfig
    {
        /// <summary>
        /// 碰撞体中心位置
        /// </summary>
        public Vector3 colliderCenter;

        /// <summary>
        /// 约束轴
        /// </summary>
        public FlattenModeType flattenAxis;

        /// <summary>
        /// 控制柄尺寸
        /// </summary>
        public float handleSize;

        /// <summary>
        /// padding
        /// </summary>
        public Vector3 boxPadding;
    }

    public class SizeBounds : IPlugin
    {
        #region 基本字段
        /// <summary>
        /// 插件所作用的节点
        /// </summary>
        private Node _node;

        /// <summary>
        /// BoundsControl组件
        /// </summary>
        private BoundsControl _boundsControl;

        /// <summary>
        /// 原组件上的Collider列表
        /// </summary>
        private List<BoxCollider> oldBoxColliderList = new List<BoxCollider>();

        /// <summary>
        /// 组件的撞碰体
        /// </summary>
        private BoxCollider _collider;
        #endregion

        /// <summary>
        /// 组件配制
        /// </summary>
        private SizeBoundsConfig _config = new SizeBoundsConfig
        {
            colliderCenter = new Vector3(0, 0, 0),
            flattenAxis = FlattenModeType.DoNotFlatten,
            handleSize = 0.016f,
            boxPadding = new Vector3(0f, 0f, 0f)
        };

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="node"></param>
        public SizeBounds(Node node)
        {
            this._node = node;
        }

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="node">插件所作用的节点</param>
        /// <param name="config">插件的基本配制</param>
        public SizeBounds(Node node, SizeBoundsConfig config)
        {
            this._node = node;
            this.config = config;
        }

        #region 基本属性
        /// <summary>
        /// 配制
        /// </summary>
        public SizeBoundsConfig config
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

        public BoxCollider collider
        {
            get
            {
                return this._collider;
            }
        }

        /// <summary>
        /// 使能
        /// </summary>
        public bool enable
        {
            get
            {
                return this._boundsControl.enabled;
            }

            set
            {
                this._boundsControl.enabled = value;
                this._collider.enabled = value;
            }
        }
        #endregion

        #region 基本方法
        /// <summary>
        /// 刷新
        /// </summary>
        public void fresh()
        {
            if (this._boundsControl == null)
            {
                // 原节点上的collider集合
                var boxColliders = this._node.parasitifer.GetComponents<BoxCollider>();
                foreach(var item in boxColliders)
                {
                    oldBoxColliderList.Add(item);
                }

                this._boundsControl = this._node.parasitifer.AddComponent<BoundsControl>();

                boxColliders = this._node.parasitifer.GetComponents<BoxCollider>();
                foreach(var item in boxColliders)
                {
                    if(!oldBoxColliderList.Contains(item))
                    {
                        this._collider = item;
                        break;
                    }
                }

                oldBoxColliderList.Clear();
            }

            // 设置碰撞体
            this._collider.center = this.config.colliderCenter;

            if (this._node is PanelNode)
            {
                this._collider.size = new Vector3(1f, 1f, Space.zero);
            } else
            {
                this._collider.size = new Vector3(1f, 1f, 1f);
            }
            
            this._boundsControl.Target = this._node.parasitifer;

            this._boundsControl.BoundsOverride = this._collider;
            this._boundsControl.FlattenAxis = this.config.flattenAxis;

            this._boundsControl.ScaleHandlesConfig.HandleSize = this.config.handleSize;
            this._boundsControl.ScaleHandlesConfig.ColliderPadding = new Vector3(0.016f, 0.016f, 0.016f);
            this._boundsControl.ScaleHandlesConfig.HandleMaterial = Resources.Load<Material>("Material/boundControl/BoundingBoxHandleWhite");
            this._boundsControl.ScaleHandlesConfig.HandleGrabbedMaterial = Resources.Load<Material>("Material/boundControl/BoundingBoxHandleBlueGrabbed");
            

            if(this._node is PanelNode)
            {
                this._boundsControl.ScaleHandlesConfig.HandleSlatePrefab = Resources.Load<GameObject>("Prefab/boundControl/MRTK_BoundingBox_ScaleHandle_Slate");
                this._boundsControl.ScaleHandlesConfig.HandlePrefab = Resources.Load<GameObject>("Prefab/boundControl/MRTK_BoundingBox_ScaleHandle"); ;

            } else
            {
                this._boundsControl.ScaleHandlesConfig.HandlePrefab = Resources.Load<GameObject>("Prefab/boundControl/MRTK_BoundingBox_ScaleHandle");
                this._boundsControl.ScaleHandlesConfig.HandleSlatePrefab = Resources.Load<GameObject>("Prefab/boundControl/MRTK_BoundingBox_ScaleHandle_Slate");
            }
            

            this._boundsControl.HandleProximityEffectConfig.ProximityEffectActive = true;
            this._boundsControl.HandleProximityEffectConfig.ObjectMediumProximity = 0.1f;
            this._boundsControl.HandleProximityEffectConfig.ObjectCloseProximity = 0.016f;
            this._boundsControl.HandleProximityEffectConfig.FarScale = 1f;
            this._boundsControl.HandleProximityEffectConfig.MediumScale = 1f;
            this._boundsControl.HandleProximityEffectConfig.CloseScale = 1.5f;
            this._boundsControl.HandleProximityEffectConfig.FarGrowRate = 0.3f;
            this._boundsControl.HandleProximityEffectConfig.MediumGrowRate = 0.2f;
            this._boundsControl.HandleProximityEffectConfig.CloseGrowRate = 0.3f;

            this._boundsControl.RotationHandlesConfig.ShowHandleForX = false;
            this._boundsControl.RotationHandlesConfig.ShowHandleForY = false;
            this._boundsControl.RotationHandlesConfig.ShowHandleForZ = false;


            this._boundsControl.ScaleStarted.AddListener(() =>
            {
                // 触发onBoundScaleStarted事件
                if(this._node is ISizeBoundsEventNode)
                {
                    ((ISizeBoundsEventNode)this._node).onBoundScaleStarted.Invoke(new BoundScaleStartedEventData
                    {
                        target = this._node
                    });
                }
            });

            this._boundsControl.ScaleStopped.AddListener(() =>
            {
                // 触发onBoundScaleEnded事件
                if (this._node is ISizeBoundsEventNode)
                {
                    ((ISizeBoundsEventNode)this._node).onBoundScaleEnded.Invoke(new BoundScaleEndedEventData
                    {
                        target = this._node
                    });
                }
            });
        }

        /// <summary>
        /// 释放组件
        /// </summary>
        public void destory()
        {
            if(this._boundsControl != null)
            {
                GameObject.Destroy(this._node.parasitifer.GetComponent<BoundsControl>());
                GameObject.Destroy(this._collider);
                
                this._boundsControl = null;
            }
        }
        #endregion
    }
}


