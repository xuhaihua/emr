using System;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit;
using EMR.Struct;
using EMR.Module;
using EMR.Event;
using EMR.Entity;
using EMR.Common;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using System.Reflection;


namespace EMR
{
    public delegate void GestureHandle(Node node, HandGesture gesture, GestureDirection direction);

    public class Space
    {
        public static Dictionary<string, Room> roomDictionary = new Dictionary<string, Room>();

        /// <summary>
        /// 名称映射
        /// </summary>
        public static Dictionary<string, List<Node>> nodeNameMap = new Dictionary<string, List<Node>>();

        /// <summary>
        /// id映射
        /// </summary>
        public static Dictionary<string, List<Node>> nodeIdMap = new Dictionary<string, List<Node>>();

        /// <summary>
        /// 表演者(源)映射
        /// </summary>
        public static Dictionary<GameObject, Node> parasitiferMap = new Dictionary<GameObject, Node>();

        /// <summary>
        /// 名称映射
        /// </summary>
        public static Dictionary<string, List<Component>> componentNameMap = new Dictionary<string, List<Component>>();

        /// <summary>
        /// id映射
        /// </summary>
        public static Dictionary<string, List<Component>> componentIdMap = new Dictionary<string, List<Component>>();

        /// <summary>
        /// component列表
        /// </summary>
        public static List<Component> componentMap = new List<Component>();

        /// <summary>
        /// 头部
        /// </summary>
        public static Head head = new Head();

        /// <summary>
        /// 左手
        /// </summary>
        public static Hand leftHand = new Hand(Handedness.Left);

        /// <summary>
        /// 右手
        /// </summary>
        public static Hand rightHand = new Hand(Handedness.Right);

        /// <summary>
        /// 眼睛
        /// </summary>
        public static Eye eye = new Eye();

        /// <summary>
        /// 语音
        /// </summary>
        public static Voice voice = new Voice();

        /// <summary>
        /// 相机
        /// </summary>
        public static EMR.Module.Camera camera = new EMR.Module.Camera();

        /// <summary>
        /// 空间指针
        /// </summary>
        public static EMR.Entity.Pointer pointer
        {
            get
            {
                return mainService.pointer;
            }
        }

        /// <summary>
        /// 零
        /// </summary>
        public static float zero = 0.00001f;

        private static EMRService _mainService;
        /// <summary>
        /// EMRService
        /// </summary>
        public static EMRService mainService
        {
            get
            {
                if(_mainService == null)
                {
                    MixedRealityServiceRegistry.TryGetService<EMRService>(out EMRService service);
                    _mainService = service;
                }
                
                return _mainService;
            }
        }

        public static AddSpatialMeshEvent onAddSpatialMesh = new AddSpatialMeshEvent();

        public static UpdatedSpatialMeshEvent onUpdatedSpatialMesh = new UpdatedSpatialMeshEvent();

        public static RemovedSpatialMeshEvent onRemovedSpatialMesh = new RemovedSpatialMeshEvent();

        #region 空间感知
        /// <summary>
        /// 获取空间网格观察者
        /// </summary>
        /// <returns></returns>
        private static IMixedRealitySpatialAwarenessMeshObserver getSpatialObjectMeshObserver()
        {
            return CoreServices.GetSpatialAwarenessSystemDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();
        }

        /// <summary>
        /// 空间网格观察者
        /// </summary>
        public static IMixedRealitySpatialAwarenessMeshObserver spatialObjectMeshObserver
        {
            get
            {
                return getSpatialObjectMeshObserver();
            }
        }


        /// <summary>
        /// 空间感知开启/关闭
        /// </summary>
        public static bool spatialAwarenessEnable
        {
            set
            {
                if(value)
                {
                    CoreServices.SpatialAwarenessSystem.Enable();
                    Space.mainService.RegisterSpaceEventHandlers();
                }
                else {
                    CoreServices.SpatialAwarenessSystem.Disable();
                    Space.mainService.UnregisterSpaceEventHandlers();
                }
            }
        }

        /// <summary>
        /// 空间网格显示方式
        /// </summary>
        public static SpatialMeshVisibleMode spatialMeshVisibleMode
        {
            set
            {
                if (value == SpatialMeshVisibleMode.none)
                {
                    //需要引入Microsoft.MixedReality.Toolkit.SpatialAwareness命名空间
                    IMixedRealityDataProviderAccess dataProviderAccess = CoreServices.SpatialAwarenessSystem as IMixedRealityDataProviderAccess;
                    if (dataProviderAccess != null)
                    {
                        IReadOnlyList<IMixedRealitySpatialAwarenessMeshObserver> observers = dataProviderAccess.GetDataProviders<IMixedRealitySpatialAwarenessMeshObserver>();
                        foreach (IMixedRealitySpatialAwarenessMeshObserver observer in observers)
                        {
                            observer.DisplayOption = SpatialAwarenessMeshDisplayOptions.None;
                        }
                    }
                }

                if (value == SpatialMeshVisibleMode.visible)
                {
                    //需要引入Microsoft.MixedReality.Toolkit.SpatialAwareness命名空间
                    IMixedRealityDataProviderAccess dataProviderAccess = CoreServices.SpatialAwarenessSystem as IMixedRealityDataProviderAccess;
                    if (dataProviderAccess != null)
                    {
                        IReadOnlyList<IMixedRealitySpatialAwarenessMeshObserver> observers = dataProviderAccess.GetDataProviders<IMixedRealitySpatialAwarenessMeshObserver>();
                        foreach (IMixedRealitySpatialAwarenessMeshObserver observer in observers)
                        {
                            observer.DisplayOption = SpatialAwarenessMeshDisplayOptions.Visible;
                        }
                    }
                }

                if (value == SpatialMeshVisibleMode.occlusion)
                {
                    //需要引入Microsoft.MixedReality.Toolkit.SpatialAwareness命名空间
                    IMixedRealityDataProviderAccess dataProviderAccess = CoreServices.SpatialAwarenessSystem as IMixedRealityDataProviderAccess;
                    if (dataProviderAccess != null)
                    {
                        IReadOnlyList<IMixedRealitySpatialAwarenessMeshObserver> observers = dataProviderAccess.GetDataProviders<IMixedRealitySpatialAwarenessMeshObserver>();
                        foreach (IMixedRealitySpatialAwarenessMeshObserver observer in observers)
                        {
                            //显示
                            observer.DisplayOption = SpatialAwarenessMeshDisplayOptions.Occlusion;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 空间感知Mesh集合
        /// </summary>
        public static List<Mesh> spatialMeshs
        {
            get
            {
                List<Mesh> result = new List<Mesh>();
                if(spatialObjectMeshObserver.Meshes != null)
                {
                    foreach (SpatialAwarenessMeshObject meshObject in spatialObjectMeshObserver?.Meshes.Values)
                    {
                        Mesh mesh = meshObject.Filter.mesh;

                        result.Add(mesh);
                    }
                }

                return result;
            }
        }
        #endregion

        #region 单位换算
        /// <summary>
        /// 本类主要处理单位转换
        /// </summary>
        public class Unit
        {
            /// <summary>
            /// 单位制转比例 Node
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public static float unitToScale(Node node, float value, Axle type)
            {
                return unitToScaleForGameObject(node.parasitifer, value, type);
            }

            /// <summary>
            /// 比例值转单位制(mm) Node
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public static float scaleToUnit(Node node, float value, Axle type)
            {
                return scaleToUnitForGameObject(node.parasitifer, value, type);
            }

            /// <summary>
            /// 比例制转单位 GameObject
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public static float scaleToUnitForGameObject(GameObject obj, float value, Axle type)
            {
                float result = 0;
                if (obj.transform.parent == null)
                {
                    result = value * 1000f;
                }
                else if (type == Axle.right)
                {
                    result = value * obj.transform.parent.transform.localScale.x;
                }
                else if (type == Axle.up)
                {
                    result = value * obj.transform.parent.transform.localScale.y;
                }
                else if (type == Axle.forward)
                {
                    result = value * obj.transform.parent.transform.localScale.z;
                }

                if(float.IsNaN(result) || result.ToString() == "0")
                {
                    return 0;
                }

                var parent = obj.transform.parent;
                if (parent != null)
                {
                    result = scaleToUnitForGameObject(parent.gameObject, result, type);
                }
                return result;
            }

            /// <summary>
            /// 单位制转比例 GameObject
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public static float unitToScaleForGameObject(GameObject obj, float value, Axle type)
            {
                float result = 0;
                if (obj.transform.parent == null)
                {
                    result = value / 1000f;
                }
                else if (type == Axle.right)
                {
                    result = value / obj.transform.parent.transform.localScale.x;
                }
                else if (type == Axle.up)
                {
                    result = value / obj.transform.parent.transform.localScale.y;
                }
                else if (type == Axle.forward)
                {
                    result = value / obj.transform.parent.transform.localScale.z;
                }

                if (float.IsNaN(result) || result.ToString() == "0" || float.IsInfinity(result))
                {
                    return 0;
                }

                var parent = obj.transform.parent;
                if (parent != null)
                {
                    result = unitToScaleForGameObject(parent.gameObject, result, type);
                }

                return result;
            }
        }
        #endregion

        #region 坐标转换
        /// <summary>
        /// 本类主要处理坐标变换
        /// </summary>
        public class Coordinate
        {
            /// <summary>
            /// 将世界坐标转本地坐标 Node
            /// </summary>
            /// <param name="obj"></param>
            /// <param name="coord"></param>
            /// <returns></returns>
            public static Vector3 worldCoordToLocalCoord(Node node, Vector3 coord)
            {
                return node.parasitifer.transform.InverseTransformPoint(coord);
            }

            /// <summary>
            /// 本地坐标转世界坐标 Node
            /// </summary>
            /// <param name="node"></param>
            /// <param name="coord"></param>
            /// <returns></returns>
            public static Vector3 localCoordToWorldCoord(Node node, Vector3 coord)
            {
                return node.parasitifer.transform.TransformPoint(coord);
            }

            /// <summary>
            /// 将世界坐标转本地坐标 GameObject
            /// </summary>
            /// <param name="obj"></param>
            /// <param name="coord"></param>
            /// <returns></returns>
            public static Vector3 worldCoordToLocalCoordForGameObject(GameObject obj, Vector3 coord)
            {
                return obj.transform.InverseTransformPoint(coord);
            }

            /// <summary>
            /// 本地坐标转世界坐标 GameObject
            /// </summary>
            /// <param name="obj"></param>
            /// <param name="coord"></param>
            /// <returns></returns>
            public static Vector3 localCoordToWorldCoordForGameObject(GameObject obj, Vector3 coord)
            {
                return obj.transform.TransformPoint(coord);
            }

            /// <summary>
            /// 计算新坐标在原坐标系中的坐标
            /// </summary>
            /// <param name="origin">坐标原点</param>
            /// <param name="position">新坐标系中的点</param>
            /// <returns></returns>
            public static Vector3 computeOriginalForCoordinateTranslation(Vector3 origin, Vector3 position)
            {
                return position + origin;
            }

            /// <summary>
            /// 计算原坐标在新坐标系中的坐标
            /// </summary>
            /// <param name="origin">坐标原点</param>
            /// <param name="position">原坐标系中的点</param>
            /// <returns></returns>
            public static Vector3 computeNewForCoordinateTranslation(Vector3 origin, Vector3 position)
            {
                return position - origin;
            }

            /// <summary>
            /// 坐标旋转
            /// </summary>
            /// <param name="angle">旋转角度</param>
            /// <param name="point">点</param>
            /// <returns></returns>
            public static Vector3 rotate(Vector3 angle, Vector3 point)
            {
                var qAngle = Quaternion.Euler(angle.x, angle.y, angle.z);
                var q0 = qAngle.w;
                var q1 = qAngle.x;
                var q2 = qAngle.y;
                var q3 = qAngle.z;

                var m11 = (q0 * q0 + q1 * q1 - q2 * q2 - q3 * q3);
                var m12 = (2 * q1 * q2 - 2 * q0 * q3);
                var m13 = (2 * q0 * q2 + 2 * q1 * q3);

                var m21 = (2 * q0 * q3 + 2 * q1 * q2);
                var m22 = (q0 * q0 - q1 * q1 + q2 * q2 - q3 * q3);
                var m23 = (2 * q2 * q3 - 2 * q0 * q1);

                var m31 = (2 * q1 * q3 - 2 * q0 * q2);
                var m32 = (2 * q0 * q1 + 2 * q2 * q3);
                var m33 = (q0 * q0 - q1 * q1 - q2 * q2 + q3 * q3);

                var x0 = point.x;
                var y0 = point.y;
                var z0 = point.z;

                var x1 = m11 * x0 + m12 * y0 + m13 * z0;
                var y1 = m21 * x0 + m22 * y0 + m23 * z0;
                var z1 = m31 * x0 + m32 * y0 + m33 * z0;

                return new Vector3(x1, y1, z1);
            }
        }
        #endregion

        #region 游戏对象处理相关
        /// <summary>
        /// 本类主要用于处理游戏实体对象
        /// </summary>
        public class GameObjectEntity
        {
            public static bool isEmpty(GameObject gameEntity)
            {
                bool result = false;

                var components = gameEntity.GetComponents(typeof(UnityEngine.Component));

                if(components.Length == 0 || components.Length == 1 && gameEntity.GetComponent<Transform>() != null)
                {
                    result = true;
                }

                return result;
            }

            public static GameObject createSource(string path)
            {
                return GameObject.Instantiate(Resources.Load<GameObject>(path)) as GameObject;
            }


            /// <summary>
            /// 添加子游戏对象
            /// </summary>
            /// <param name="parent"></param>
            /// <param name="obj"></param>
            /// <returns></returns>
            public static GameObject appendNode(GameObject obj, GameObject parent)
            {
                var x = obj.transform.localPosition.x;
                var y = obj.transform.localPosition.y;
                var z = obj.transform.localPosition.z;

                var xAngle = obj.transform.localRotation.x;
                var yAngle = obj.transform.localRotation.y;
                var zAngle = obj.transform.localRotation.z;

                obj.transform.SetParent(parent.transform);
                obj.transform.localPosition = new Vector3(x, y, z);
                obj.transform.localEulerAngles = new Vector3(xAngle, yAngle, zAngle);
                return obj;
            }

            /// <summary>
            /// 是否有尺寸比例为零的父节点
            /// </summary>
            /// <param name="start"></param>
            /// <param name="axle"></param>
            /// <returns></returns>
            public static bool hasParentScaleZero(GameObject start, Axle axle)
            {
                bool result = false;
                Transform temp = start.transform.parent;
                while (temp != null)
                {
                    if (axle == Axle.right && Utils.equals(temp.localScale.x, 0f))
                    {
                        result = true;
                        break;
                    }

                    if (axle == Axle.up && Utils.equals(temp.localScale.y, 0f))
                    {
                        result = true;
                        break;
                    }

                    if (axle == Axle.forward && Utils.equals(temp.localScale.z, 0f))
                    {
                        result = true;
                        break;
                    }

                    temp = temp.parent;
                }

                return result;
            }

            /// <summary>
            /// 获取子节点集合
            /// </summary>
            /// <param name="node"></param>
            /// <param name="nodeList"></param>
            public static void getChildrenList(Node node, ref List<Node> nodeList, bool isIncludeSelf = false)
            {
                if(isIncludeSelf)
                {
                    nodeList.Add(node);
                }
                var childrens = node.children;
                foreach (var item in childrens)
                {
                    if(!isIncludeSelf)
                    {
                        nodeList.Add(item);
                    }
                    getChildrenList(item, ref nodeList);
                }
            }
        }
        #endregion

        #region 空间计算相关
        public class Mathematics
        {
            /// <summary>
            /// 节点与球体相交运算(近似运算，使用球体外接正方体)
            /// </summary>
            /// <param name="parasitifer"></param>
            /// <param name="worldSphereCenter"></param>
            /// <param name="worldSphereRadius"></param>
            /// <returns></returns>
            public static bool isNodeCrossSphereForApproximate(GameObject parasitifer, Vector3 worldSphereCenter, float worldSphereRadius)
            {
                bool result = false;

                // 计算球心坐标
                var sphereCenter = new Vector3(worldSphereCenter.x / 1000f, worldSphereCenter.y / 1000f, worldSphereCenter.z / 1000f);

                // 获取球的半径
                var sphereRadius = worldSphereRadius / 1000f;

                // sphereBound顶点集合
                List<Vector3> sphereBoundPointList = new List<Vector3>();
                sphereBoundPointList.Add(parasitifer.transform.InverseTransformPoint(Space.Coordinate.rotate(parasitifer.transform.eulerAngles, new Vector3(-sphereRadius, +sphereRadius, +sphereRadius)) + sphereCenter));
                sphereBoundPointList.Add(parasitifer.transform.InverseTransformPoint(Space.Coordinate.rotate(parasitifer.transform.eulerAngles, new Vector3(+sphereRadius, +sphereRadius, +sphereRadius)) + sphereCenter));
                sphereBoundPointList.Add(parasitifer.transform.InverseTransformPoint(Space.Coordinate.rotate(parasitifer.transform.eulerAngles, new Vector3(+sphereRadius, +sphereRadius, -sphereRadius)) + sphereCenter));
                sphereBoundPointList.Add(parasitifer.transform.InverseTransformPoint(Space.Coordinate.rotate(parasitifer.transform.eulerAngles, new Vector3(-sphereRadius, +sphereRadius, -sphereRadius)) + sphereCenter));

                sphereBoundPointList.Add(parasitifer.transform.InverseTransformPoint(Space.Coordinate.rotate(parasitifer.transform.eulerAngles, new Vector3(-sphereRadius, -sphereRadius, +sphereRadius)) + sphereCenter));
                sphereBoundPointList.Add(parasitifer.transform.InverseTransformPoint(Space.Coordinate.rotate(parasitifer.transform.eulerAngles, new Vector3(+sphereRadius, -sphereRadius, +sphereRadius)) + sphereCenter));
                sphereBoundPointList.Add(parasitifer.transform.InverseTransformPoint(Space.Coordinate.rotate(parasitifer.transform.eulerAngles, new Vector3(+sphereRadius, -sphereRadius, -sphereRadius)) + sphereCenter));
                sphereBoundPointList.Add(parasitifer.transform.InverseTransformPoint(Space.Coordinate.rotate(parasitifer.transform.eulerAngles, new Vector3(-sphereRadius, -sphereRadius, -sphereRadius)) + sphereCenter));

                // 将中心点转换到parasitifer坐标系中
                sphereCenter = parasitifer.transform.InverseTransformPoint(sphereCenter);

                // parasitifer顶点集合
                List<Vector3> parasitiferPointList = new List<Vector3>();
                parasitiferPointList.Add(new Vector3(-1f / 2, +1f / 2, +1f / 2));
                parasitiferPointList.Add(new Vector3(+1f / 2, +1f / 2, +1f / 2));
                parasitiferPointList.Add(new Vector3(+1f / 2, +1f / 2, -1f / 2));
                parasitiferPointList.Add(new Vector3(-1f / 2, +1f / 2, -1f / 2));

                parasitiferPointList.Add(new Vector3(-1f / 2, -1f / 2, +1f / 2));
                parasitiferPointList.Add(new Vector3(+1f / 2, -1f / 2, +1f / 2));
                parasitiferPointList.Add(new Vector3(+1f / 2, -1f / 2, -1f / 2));
                parasitiferPointList.Add(new Vector3(-1f / 2, -1f / 2, -1f / 2));

                // 相交判断
                if (Mathf.Abs(sphereCenter.x) < (Mathf.Abs(sphereBoundPointList[0].x - sphereBoundPointList[1].x) + Mathf.Abs(parasitiferPointList[0].x - parasitiferPointList[1].x)) / 2 && Mathf.Abs(sphereCenter.y) < (Mathf.Abs(sphereBoundPointList[0].y - sphereBoundPointList[4].y) + Mathf.Abs(parasitiferPointList[0].y - parasitiferPointList[4].y)) / 2 && Mathf.Abs(sphereCenter.z) < (Mathf.Abs(sphereBoundPointList[0].z - sphereBoundPointList[3].z) + Mathf.Abs(parasitiferPointList[0].z - parasitiferPointList[3].z)) / 2)
                {
                    result = true;
                }

                return result;
            }

            /// <summary>
            /// 点是否包含在球体内
            /// </summary>
            /// <param name="worldPoint"></param>
            /// <param name="worldSphereCenter"></param>
            /// <param name="worldSphereRadius"></param>
            /// <returns></returns>
            public static bool isSphereIncludePoint(Vector3 worldPoint, Vector3 worldSphereCenter, float worldSphereRadius)
            {
                bool result = false;

                var point = worldPoint / 1000f;

                // 计算球心在Node坐标系中的坐标
                var sphereCenter = new Vector3(worldSphereCenter.x / 1000f, worldSphereCenter.y / 1000f, worldSphereCenter.z / 1000f);

                // 获取球的半径
                var sphereRadius = worldSphereRadius / 1000f;

                if (Utils.noExceed( Vector3.Magnitude(point - sphereCenter), sphereRadius))
                {
                    result = true;
                }

                return result;
            }
        }
        #endregion

        #region 节点索引相关
        /// <summary>
        /// 添加Id映射
        /// </summary>
        /// <param name="node"></param>
        public static void addNodeIdMap(Node node)
        {
            /*
             以下逻辑用于校验id是否重复
             */
            // 在节点内查找重复id
            if (!nodeIdMap.ContainsKey(node.id))
            {
                nodeIdMap.Add(node.id, new List<Node>());
            }
            var nodeList = nodeIdMap[node.id];

            foreach(var item in nodeList)
            {
                if(node.component != null && item.component == node.component)
                {
                    Debug.LogError("在组件" + node.component + "中出现重复节点, id：" + node.id);
                    return;
                }
            }

            // 在组件内查找重复id
            if (!componentIdMap.ContainsKey(node.id))
            {
                componentIdMap.Add(node.id, new List<Component>());
            }
            var componentlist = componentIdMap[node.id];

            foreach (var item in componentlist)
            {
                if (node.component != null && node.component == item.parent)
                {
                    Debug.LogError("componentId不能重复");

                    return;
                }
            }

            nodeList.Add(node);
        }

        /// <summary>
        /// 添加名字映射
        /// </summary>
        /// <param name="node"></param>
        public static void addNodeNameMap(Node node)
        {
            if (node.name != "")
            {
                if (!nodeNameMap.ContainsKey(node.name))
                {
                    nodeNameMap.Add(node.name, new List<Node>());
                }

                var list = nodeNameMap[node.name];

                list.Add(node);
            }
        }

        /// <summary>
        /// 添加Parasitifer映射
        /// </summary>
        /// <param name="node"></param>
        public static void addParasitiferMap(Node node)
        {
            parasitiferMap.Add(node.parasitifer, node);
        }


        /// <summary>
        /// 删除映射
        /// </summary>
        /// <param name="node"></param>
        public static bool delNodeMap(Node node)
        {
            if (node.name != "" && nodeNameMap.ContainsKey(node.name))
            {
                var list = nodeNameMap[node.name];
                for (var i = 0; i < list.Count; i++)
                {
                    if (list[i] == node)
                    {
                        list.RemoveAt(i);
                        break;
                    }
                }
                if (list.Count == 0)
                {
                    nodeNameMap.Remove(node.name);
                }
            }

            if (node.id != "" && nodeIdMap.ContainsKey(node.id))
            {
                var list = nodeIdMap[node.id];
                for (var i = 0; i < list.Count; i++)
                {
                    if (list[i] == node)
                    {
                        list.RemoveAt(i);
                        break;
                    }
                }
                if (list.Count == 0)
                {
                    nodeIdMap.Remove(node.id);
                }
            }

            return parasitiferMap.Remove(node.parasitifer);
        }

        /// <summary>
        /// 获取所有Node
        /// </summary>
        /// <returns></returns>
        public static List<Node> getNodes()
        {
            List<Node> result = new List<Node>();

            foreach(var item in parasitiferMap)
            {
                result.Add(item.Value);
            }
            
            return result;
        }

        /// <summary>
        /// 获取Node集合
        /// </summary>
        /// <returns></returns>
        public static List<T> getNodes<T>() where T : Node
        {
            List<T> result = new List<T>();

            foreach (var item in parasitiferMap)
            {
                // 匹配类型为T的节点
                if(typeof(T) == item.Value.GetType())
                {
                    result.Add((T)item.Value);
                }
            }
            return result;
        }

        /// <summary>
        /// 根据name获取Node
        /// </summary>
        /// <param name="name">要查询的节点名称</param>
        /// <returns></returns>
        public static List<Node> getNodesByName(string name)
        {
            List<Node> result = new List<Node>();

            if (name != "" && name != null && nodeNameMap.ContainsKey(name))
            {
                result = nodeNameMap[name];
            }
            return result;
        }

        /// <summary>
        /// 根据name获取Node
        /// </summary>
        /// <typeparam name="T">要查询的节点类型</typeparam>
        /// <param name="name">要查询的节点名称</param>
        /// <returns></returns>
        public static List<T> getNodesByName<T>(string name) where T : Node
        {
            List<T> result = new List<T>();
            List<Node> list = getNodesByName(name);

            foreach (var item in list)
            {
                // 匹配类型为T的节点
                if(typeof(T) == item.GetType())
                {
                    result.Add((T)item);
                }
            }

            return result;
        }

        /// <summary>
        /// 根据表演者(源)获取Node
        /// </summary>
        /// <param name="parasitifer">节点表演者(节点的GameObject)</param>
        /// <returns></returns>
        public static Node getNodeByParasitifer(GameObject parasitifer)
        {
            Node result = null;
            var node = parasitifer;

            if (parasitifer.name == "EmptyScroll")
            {
                node = parasitifer.transform.parent.gameObject;
            }

            if (parasitiferMap.ContainsKey(node))
            {
                result = parasitiferMap[node];
            }

            return result;
        }
        #endregion

        #region 组件索引相关
        /// <summary>
        /// 添加Id映射
        /// </summary>
        /// <param name="component"></param>
        public static void addComponentIdMap(Component component)
        {
            if (component.id != "")
            {
                /*
                 以下逻辑用于校验id是否重复
                 */
                // 在节点内查找重复id
                if (!nodeIdMap.ContainsKey(component.id))
                {
                    nodeIdMap.Add(component.id, new List<Node>());
                }
                var nodeList = nodeIdMap[component.id];

                foreach (var item in nodeList)
                {
                    if (item.component != null && item.component == component.parent)
                    {
                        Debug.LogError("在组件" + item.component + "中出现重复节点, id：" + item.id);
                        return;
                    }
                }

                // 在组件内查找重复id
                if (!componentIdMap.ContainsKey(component.id))
                {
                    componentIdMap.Add(component.id, new List<Component>());
                }

                var componentlist = componentIdMap[component.id];

                foreach(var item in componentlist)
                {
                    if (component.parent != null && item.parent == component.parent)
                    {
                        Debug.LogError("componentId不能重复");
                        return;
                    }
                }

                componentlist.Add(component);
            }
        }

        /// <summary>
        /// 添加名字映射
        /// </summary>
        /// <param name="node"></param>
        public static void addComponentNameMap(Component component)
        {
            if (component.name != "")
            {
                if (!componentNameMap.ContainsKey(component.name))
                {
                    componentNameMap.Add(component.name, new List<Component>());
                }

                var list = componentNameMap[component.name];

                list.Add(component);
            }
        }

        /// <summary>
        /// 添加component到列表
        /// </summary>
        /// <param name="node"></param>
        public static void addComponentMap(Component component)
        {
            componentMap.Add(component);
        }

        /// <summary>
        /// 删除映射
        /// </summary>
        /// <param name="node"></param>
        public static bool delComponentMap(Component component)
        {
            if (component.name != "" && componentNameMap.ContainsKey(component.name))
            {
                var list = componentNameMap[component.name];
                for (var i = 0; i < list.Count; i++)
                {
                    if (list[i] == component)
                    {
                        list.RemoveAt(i);
                        break;
                    }
                }
                if (list.Count == 0)
                {
                    componentNameMap.Remove(component.name);
                }
            }

            if (component.id != "" && componentIdMap.ContainsKey(component.id))
            {
                componentIdMap.Remove(component.id);
            }

            return componentMap.Remove(component);
        }

        /// <summary>
        /// 查询所有组件
        /// </summary>
        /// <returns></returns>
        public static List<Component> getComponents()
        {
            List<Component> result = new List<Component>();

            foreach (var item in componentMap)
            {
                result.Add(item);
            }

            return result;
        }

        /// <summary>
        /// 查询所有组件
        /// </summary>
        /// <typeparam name="T">要查询的组件类型</typeparam>
        /// <returns></returns>
        public static List<T> getComponents<T>() where T : Component
        {
            List<T> result = new List<T>();

            foreach (var item in componentMap)
            {
                // 匹配类型为T的节点
                if (typeof(T) == item.GetType())
                {
                    result.Add((T)item);
                }
            }
            return result;
        }

        /// <summary>
        /// 按名称查询组件
        /// </summary>
        /// <param name="name">要查询的组件名称</param>
        /// <returns></returns>
        public static List<Component> getComponentsByName(string name)
        {
            List<Component> result = new List<Component>();

            if (name != null && componentNameMap.ContainsKey(name))
            {
                result = componentNameMap[name];
            }
            return result;
        }

        /// <summary>
        /// 按名称查询组件
        /// </summary>
        /// <typeparam name="T">要查询的组件类型</typeparam>
        /// <param name="name">要查询的组件名称</param>
        /// <returns></returns>
        public static List<T> getComponentsByName<T>(string name) where T : Component
        {
            List<T> result = new List<T>();
            List<Component> list = getComponentsByName(name);

            foreach (var item in list)
            {
                // 匹配类型为T的节点
                if (typeof(T) == item.GetType())
                {
                    result.Add((T)item);
                }
            }

            return result;
        }
        #endregion

        #region room相关操作
        /// <summary>
        /// 按名称获取room
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns></returns>
        public static Room getRoomByName(string name)
        {
            Room result = null;
            if(name != "" && EMR.Space.roomDictionary.ContainsKey(name))
            {
                result = EMR.Space.roomDictionary[name];
            }
            return result;
        }

        private static Room currentRoom = null;
        /// <summary>
        /// 加载room
        /// </summary>
        /// <param name="name">room 名称</param>
        /// <param name="document">room 视图文档</param>
        /// <param name="typeName">room 类型</param>
        /// <returns></returns>
        public static Room loadRoom(string name = "", string document = "", Type type = null)
        {
            // 销毁当前room
            if(currentRoom != null)
            {
                currentRoom.destory();

                currentRoom = null;
            }

            Room result = null;

            if (type != null)
            {
                currentRoom = (EMR.Room)type.Assembly.CreateInstance(type.FullName);

                if(document != "")
                {
                    currentRoom.document = document;
                }
            }

            if(currentRoom == null && document != "")
            {
                currentRoom = new Room();
                currentRoom.document = document;
            }

            if(currentRoom != null)
            {
                var roomName = name == "" ? Guid.NewGuid().ToString() : name;

                EMR.Space.roomDictionary.Add(roomName, currentRoom);

                currentRoom.assemble();

                result = currentRoom;
            }

            return result;
        }



        /// <summary>
        /// 查找组件类型
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        private static Type findRoomType(string typeName)
        {
            Type result = null;

            foreach (var assembly in EMR.Space.mainService.assemblyList)
            {
                // typeName中不含级联运算符
                if (typeName.IndexOf(".") == -1)
                {
                    List<string> queryList = new List<string>
                    {
                        typeName
                    };

                    foreach (var spaceName in EMR.Space.mainService.typeQueryList)
                    {
                        queryList.Add(spaceName + "." + typeName);
                    }

                    foreach (var name in queryList)
                    {
                        var tempResult = assembly.GetType(name);
                        if (tempResult != null && typeof(EMR.Room).IsAssignableFrom(tempResult))
                        {
                            result = tempResult;
                            break;
                        }
                    }
                }

                if (typeName.IndexOf(".") != -1)
                {
                    var tempResult = assembly.GetType(typeName);
                    if (tempResult != null && typeof(EMR.Room).IsAssignableFrom(tempResult))
                    {
                        result = tempResult;
                        break;
                    }
                }

                if (result != null)
                {
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// 加载Room
        /// </summary>
        /// <param name="name">room自定义名称</param>
        /// <param name="document">room的视图文档</param>
        /// <param name="typeName">room类型名称</param>
        /// <returns></returns>
        public static Room loadRoom(string name = "", string document = "", string typeName = "")
        {
            Type roomType = null;
            if(typeName != "")
            {
                roomType = findRoomType(typeName);
            }

            return loadRoom(name, document, roomType);
        }
        #endregion
    }
}
