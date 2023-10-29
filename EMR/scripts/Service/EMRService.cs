
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections.Generic;
using EMR.Event;
using EMR.Entity;
using EMR.Common;
using System.Xml;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using System.Reflection;
using System.Collections;
using System.IO;

namespace EMR
{
    using SpatialAwarenessHandler = IMixedRealitySpatialAwarenessObservationHandler<SpatialAwarenessMeshObject>;

    [MixedRealityExtensionService(SupportedPlatforms.WindowsStandalone | SupportedPlatforms.MacStandalone | SupportedPlatforms.LinuxStandalone | SupportedPlatforms.WindowsUniversal)]
    public class EMRService : BaseExtensionService, IMixedRealityExtensionService, SpatialAwarenessHandler
    {
        public EMRService(IMixedRealityServiceRegistrar registrar, string name, uint priority, BaseMixedRealityProfile profile) : base(registrar, name, priority, profile)
        {
            
        }

        public EMRService(string name, uint priority, BaseMixedRealityProfile profile) : base(MixedRealityToolkit.Instance, name, priority, profile)
        {

        }

        #region 基本字段
        /// <summary>
        /// 下一周期开始时执行的任务列表
        /// </summary>
        private List<CirculateTaskHandler> nextTakeList = new List<CirculateTaskHandler>();

        /// <summary>
        /// 下一周期结尾时执行的任务列表
        /// </summary>
        private List<CirculateTaskHandler> lateNextTakeList = new List<CirculateTaskHandler>();

        /// <summary>
        /// 焦点提供者
        /// </summary>
        private IMixedRealityFocusProvider _focusProvider;

        /// <summary>
        /// 主指针提供者
        /// </summary>
        private IMixedRealityPointer _primaryPointer;

        /// <summary>
        /// EMR指针对象
        /// </summary>
        private EMR.Entity.Pointer _pointer;
        #endregion

        #region 相关属性
        /// <summary>
        /// EMR配制文件
        /// </summary>
        private XmlNode _config;
        private XmlNode config
        {
            get
            {
                // 加载配制文件
                if (this._config == null)
                {
                    _config = Utils.loadXml("EMRConfig");

                    if (_config == null)
                    {
                        Debug.LogError("加载EMR配制文件时错误请检查该文件是否存在或文件内容是否有效");
                    }
                }

                return _config;
            }
        }

        /// <summary>
        /// 类型查询列表
        /// </summary>
        private List<string> _typeQueryList = new List<string>();
        public List<string> typeQueryList
        {
            get
            {
                if (this._typeQueryList.Count == 0)
                {
                    foreach (XmlNode item in config.ChildNodes)
                    {
                        // 类型查询列表
                        if (item.Name == "typeQueryList")
                        {
                            foreach (XmlNode spaceQuery in item.ChildNodes)
                            {
                                this._typeQueryList.Add(spaceQuery.Name);
                            }

                            break;
                        }
                    }
                }

                return this._typeQueryList;
            }
        }

        private List<Assembly> _assemblyList = new List<Assembly>();
        /// <summary>
        /// 程序集列表
        /// </summary>
        public List<Assembly> assemblyList
        {
            get
            {
                if (this._assemblyList.Count == 0)
                {
                    this._assemblyList.Add(Assembly.GetExecutingAssembly());
                    foreach (XmlNode item in config.ChildNodes)
                    {
                        // 类型查询列表
                        if (item.Name == "assemblyList")
                        {
                            foreach (XmlNode assmblyNode in item.ChildNodes)
                            {
                                if (assmblyNode.Attributes["name"] != null && assmblyNode.Attributes["name"].Value != "")
                                {
                                    try
                                    {
                                        this._assemblyList.Add(Assembly.Load(assmblyNode.Attributes["name"].Value));
                                    }
                                    catch
                                    {
                                        Debug.LogWarning("加载程序集" + assmblyNode.Attributes["name"].Value + "失败");
                                    }
                                }
                            }

                            break;
                        }
                    }
                }

                return this._assemblyList;
            }
        }

        private Dictionary<string, object> _mainRoom = null;
        /// <summary>
        /// main room
        /// </summary>
        public Dictionary<string, object> mainRoom
        {
            get
            {
                if (this._mainRoom == null)
                {
                    this._mainRoom = new Dictionary<string, object>
                    {
                        { "name", "" },
                        { "type", null },
                        { "document", "" }
                    };

                    // 加载主Room
                    foreach (XmlNode item in this.config.ChildNodes)
                    {
                        if (item.Name == "default")
                        {
                            // room 名称
                            if (item.Attributes["name"] != null)
                            {
                                this._mainRoom["name"] = item.Attributes["name"].Value;
                            }

                            // 获取主Room的视图文档
                            if (item.Attributes["document"] != null)
                            {
                                this._mainRoom["document"] = item.Attributes["document"].Value.Trim();
                            }

                            // 获取room类型【查询方式：按程序集先在system中查找然后在类型列表中查找】
                            if (item.Attributes["type"] != null)
                            {
                                var typeName = item.Attributes["type"].Value.Trim();

                                // 当类型名称不存在级联时:
                                if (typeName.IndexOf(".") == -1)
                                {
                                    foreach (var assembly in this.assemblyList)
                                    {
                                        // 先尝试在System名称空间内查找
                                        var tempType = assembly.GetType(typeName);
                                        if (tempType != null && typeof(EMR.Room).IsAssignableFrom(tempType))
                                        {
                                            this._mainRoom["type"] = tempType;
                                        }

                                        // 如果没有找到则在指定的类型空间列表中查找
                                        if (this._mainRoom["type"] == null)
                                        {
                                            foreach (var spaceName in typeQueryList)
                                            {
                                                tempType = assembly.GetType(spaceName + "." + typeName);
                                                if (tempType != null && typeof(EMR.Room).IsAssignableFrom(tempType))
                                                {
                                                    this._mainRoom["type"] = tempType;
                                                    break;
                                                }
                                            }
                                        }

                                        if (this._mainRoom["type"] != null)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return this._mainRoom;
            }
        }

        /// <summary>
        /// 指针
        /// </summary>
        public EMR.Entity.Pointer pointer
        {
            get
            {
                return this._pointer;
            }
        }
        #endregion

        #region 空间感知
        private bool isRegistered = false;
        public void RegisterSpaceEventHandlers()
        {
            if (!isRegistered && (CoreServices.SpatialAwarenessSystem != null))
            {
                CoreServices.SpatialAwarenessSystem.RegisterHandler<SpatialAwarenessHandler>(this);
                isRegistered = true;
            }
        }

        public void UnregisterSpaceEventHandlers()
        {
            if (isRegistered && (CoreServices.SpatialAwarenessSystem != null))
            {
                CoreServices.SpatialAwarenessSystem.UnregisterHandler<SpatialAwarenessHandler>(this);
                isRegistered = false;
            }
        }

        /// <summary>
        /// 空间网格添加事件
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnObservationAdded(MixedRealitySpatialAwarenessEventData<SpatialAwarenessMeshObject> eventData)
        {

            // 拙出事件
            AddSpatialMeshEventData addSpatialMeshEventData = new AddSpatialMeshEventData
            {
                id = eventData.Id,
                mesh = eventData.SpatialObject.Filter.mesh,
                original = eventData
            };

            Space.onAddSpatialMesh.Invoke(addSpatialMeshEventData);
        }

        /// <summary>
        /// 空间网格更新事件
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnObservationUpdated(MixedRealitySpatialAwarenessEventData<SpatialAwarenessMeshObject> eventData)
        {
            // 拙出事件
            UpdatedSpatialMeshEventData updateSpatialMeshEventData = new UpdatedSpatialMeshEventData
            {
                id = eventData.Id,
                mesh = eventData.SpatialObject.Filter.mesh,
                original = eventData
            };

            Space.onUpdatedSpatialMesh.Invoke(updateSpatialMeshEventData);
        }

        /// <summary>
        /// 空间网格移除事件
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnObservationRemoved(MixedRealitySpatialAwarenessEventData<SpatialAwarenessMeshObject> eventData)
        {
            // 拙出事件
            RemovedSpatialMeshEventData removedSpatialMeshEventData = new RemovedSpatialMeshEventData
            {
                id = eventData.Id,
                mesh = eventData.SpatialObject?.Filter.mesh,
                original = eventData
            };

            Space.onRemovedSpatialMesh.Invoke(removedSpatialMeshEventData);
        }
        #endregion

        #region 相关方法
        /// <summary>
        /// 任务循环执行器
        /// </summary>
        /// <param name="taskList">任务列表</param>
        private void taskExecuteHandle(List<CirculateTaskHandler> taskList)
        {
            List<CirculateTaskHandler> delCallList = new List<CirculateTaskHandler>();

            for (var i = 0; i < taskList.Count; i++)
            {
                var callback = taskList[i];

                if (callback())
                {
                    delCallList.Add(callback);
                }
            }

            while (delCallList.Count > 0)
            {
                var call = delCallList[0];
                for (var i = 0; i < taskList.Count; i++)
                {
                    if (taskList[i] == call)
                    {
                        taskList.RemoveAt(i);
                        break;
                    }
                }
                delCallList.RemoveAt(0);
            }
        }

        /// <summary>
        /// 向循环周期添加任务
        /// </summary>
        /// <param name="task">任务</param>
        public void next(CirculateTaskHandler task)
        {
            this.nextTakeList.Add(task);
        }

        /// <summary>
        /// 向循环周期最后添加任务
        /// </summary>
        /// <param name="task"></param>
        public void lateNext(CirculateTaskHandler task)
        {
            this.lateNextTakeList.Add(task);
        }

        /// <summary>
        /// 加载主room
        /// </summary>
        private void loadMainRoom()
        {
            // 加载room
            EMR.Space.loadRoom((string)this.mainRoom["name"], (string)this.mainRoom["document"], this.mainRoom["type"] != null ? (Type)this.mainRoom["type"] : null);
        }

        /// <summary>
        /// 添加服务
        /// </summary>
        /// <param name="service"></param>
        public void addService(NodeService service)
        {
            MixedRealityServiceRegistry.AddService<NodeService>(service, MixedRealityToolkit.Instance);
        }

        /// <summary>
        /// 移除服务
        /// </summary>
        /// <param name="service"></param>
        public void removeService(NodeService service)
        {
            MixedRealityServiceRegistry.RemoveService<NodeService>(service, MixedRealityToolkit.Instance);
        }
        #endregion

        /// <summary>
        /// 服务初始化
        /// </summary>
        public override void Initialize()
        {
            
        }

        private bool isInit = true;

        /// <summary>
        /// 服务更新
        /// </summary>
        public override void Update()
        {
            if(isInit)
            {
                // 空间感知默认为关闭
                Space.spatialAwarenessEnable = false;

                //加载主场景开始
                loadMainRoom();

                // 获取焦点提供者
                this._focusProvider = CoreServices.InputSystem?.FocusProvider;

                // 注册空间感知事件
                RegisterSpaceEventHandlers();

                isInit = false;
            }

            this._primaryPointer = this._focusProvider?.PrimaryPointer;

            // EMR指针更新
            if (this._primaryPointer != null)
            {
                var focusProvider = CoreServices.InputSystem?.FocusProvider;
                var primaryPointer = focusProvider.PrimaryPointer;

                if (primaryPointer != null)
                {
                    this._pointer = new EMR.Entity.Pointer(primaryPointer, primaryPointer?.Result?.CurrentPointerTarget, primaryPointer?.Result?.Details.Point, primaryPointer?.Result?.Details.Normal, primaryPointer?.Controller?.ControllerHandedness);
                }
                else
                {
                    this._pointer = null;
                }
            }
            else
            {
                this._pointer = null;
            }

            // 执行循环开始任务
            this.taskExecuteHandle(this.nextTakeList);

            // 执行循环结尾任务
            this.taskExecuteHandle(this.lateNextTakeList);
        }

        /// <summary>
        /// 服务销毁
        /// </summary>
        public override void Destroy()
        {
            // 注销空间感知事件
            UnregisterSpaceEventHandlers();

            // 调用基类销毁方法
            base.Destroy();
        }
    }
}
