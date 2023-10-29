using System;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using EMR.Event;
using EMR.Module;

namespace EMR.Entity
{
    public class Anchor 
    {
        #region 基本字段
        // 锚坐标
        private float? _x = 0f;
        private float? _y = 0f;
        private float? _z = 0f;

        // 锚所在节点
        private float _radius = 0f;

        // 锚关联的节点
        private Node _node;

        private Room _room;

        // 锚点目标
        private Node _target;

        // emr engine
        private EMR.EMRService _mainService;

        // 感兴趣id
        private string _intersetId;

        // 感兴趣name
        private string _intersetName;

        // 感兴趣节点
        private List<Node> _intersetNodeList = new List<Node>();

        // 感兴趣手
        private Handedness _intersetHand = Handedness.None;

        // 感兴趣手指针
        private Handedness _intersetHandPointer = Handedness.None;

        // 捕获的节点列表
        private List<Node> captureNodeList = new List<Node>();

        // 捕获的左手关节列表
        private List<TrackedHandJoint> captureLeftHandJointList = new List<TrackedHandJoint>();

        // 捕获的右手关节列表
        private List<TrackedHandJoint> captureRightHandJointList = new List<TrackedHandJoint>();

        // 捕获的左手指针
        private Pointer captureLeftHandPointer;

        // 捕获的右手指针
        private Pointer captureRightHandPointer;

        /// <summary>
        /// 名称
        /// </summary>
        private string _name;

        
        /// <summary>
        /// 节点进入事件
        /// </summary>
        private AnchorNodeHoverEvent _onAnchorNodeHover;

        /// <summary>
        /// 节点离开事件
        /// </summary>
        private AnchorNodeOutEvent _onAnchorNodeOut;

        /// <summary>
        /// 手关节进入事件
        /// </summary>
        private AnchorJointHoverEvent _onAnchorJointHover;

        /// <summary>
        /// 手关节离开事件
        /// </summary>
        private AnchorJointOutEvent _onAnchorJointOut;

        /// <summary>
        /// 手指针进入事件
        /// </summary>
        private AnchorPointerHoverEvent _onAnchorPointerHover;

        /// <summary>
        /// 手指针离开事件
        /// </summary>
        private AnchorPointerOutEvent _onAnchorPointerOut;
        #endregion

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="node">锚所在的节点</param>
        /// <param name="x">坐标x</param>
        /// <param name="y">坐标y</param>
        /// <param name="z">坐标z</param>
        /// <param name="radius">锚半径</param>
        public Anchor(Node node, float? x = null, float? y = null, float? z = null, float radius = 0f, Room room = null)
        {
            this._x = x;
            this._y = y;
            this._z = z;
            this._radius = radius;
            this._node = node;
            this._mainService = EMR.Space.mainService;

            this._room = room;

            // 创建相关事件
            this._onAnchorNodeHover = new AnchorNodeHoverEvent();
            this._onAnchorNodeOut = new AnchorNodeOutEvent();
            this._onAnchorJointHover = new AnchorJointHoverEvent();
            this._onAnchorJointOut = new AnchorJointOutEvent();
            this._onAnchorPointerHover = new AnchorPointerHoverEvent();
            this._onAnchorPointerOut = new AnchorPointerOutEvent();

            this.init();
        }

        /// <summary>
        /// 初始化方法
        /// </summary>
        private void init()
        {
            this._mainService.next(() =>
            {
                if (this.x != null && this.y != null && this.z != null)
                {
                    #region 节点

                    // 感兴趣节点列表
                    List<Node> interestNodeList = new List<Node>();

                    if (this.interestId != null)
                    {
                        Node node = null;
                        if(this._node != null)
                        {
                            node = this._node.component.getNodeById(this.interestId);
                        } else if (this._room != null)
                        {
                            node = this._room.getNodeById(this.interestId);
                        }

                        if (node != null)
                        {
                            if (this.intersetNodeList.IndexOf(node) == -1)
                            {
                                interestNodeList.Add(node);
                            }
                        }
                    }

                    if (this.intersetName != null)
                    {
                        List<Node> list = new List<Node>();
                        if(this._node != null)
                        {
                            list = this._node.component.getNodesByName(this.intersetName);
                        } else if (this._room != null)
                        {
                            list = this._room.getNodesByName(this.intersetName);
                        }
                        
                        foreach (var item in list)
                        {
                            if (this.intersetNodeList.IndexOf(item) == -1)
                            {
                                interestNodeList.Add(item);
                            }
                        }
                    }

                    foreach (var item in this.intersetNodeList)
                    {
                        interestNodeList.Add(item);
                    }


                    var worldSphereCenter = new Vector3(0f, 0f, 0f);

                    if (this._node != null)
                    {
                        var node = this._node;

                        float x = (float)this.x;

                        float y = (float)this.y;

                        float z = (float)this.z;

                        worldSphereCenter = this._node.parasitifer.transform.TransformPoint(new Vector3(x, y, z)) * 1000f;
                    }
                    else
                    {
                        worldSphereCenter = new Vector3((float)this.x, (float)this.y, (float)this.z) * 1000f;
                    }

                    foreach (var item in interestNodeList)
                    {
                        var index = captureNodeList.IndexOf(item);

                        var isCross = EMR.Space.Mathematics.isNodeCrossSphereForApproximate(item.parasitifer, worldSphereCenter, this.radius);
                        if (isCross && index == -1)
                        {
                            captureNodeList.Add(item);
                            this.onAnchorNodeHover.Invoke(new AnchorNodeHoverEventData
                            {
                                anchor = this,
                                interestNode = item,
                            });
                        }
                        else if (!isCross && index != -1)
                        {
                            this.captureNodeList.RemoveAt(index);

                            this.onAnchorNodeOut.Invoke(new AnchorNodeOutEventData
                            {
                                anchor = this,
                                interestNode = item,
                            });

                        }
                    }
                    #endregion

                    #region 手部关节
                    List<Hand> handList = new List<Hand>();
                    if (this._intersetHand == Handedness.Left || this._intersetHand == Handedness.Both)
                    {
                        handList.Add(EMR.Space.leftHand);
                    }
                    if (this._intersetHand == Handedness.Right || this._intersetHand == Handedness.Both)
                    {
                        handList.Add(EMR.Space.rightHand);
                    }

                    foreach (var hand in handList)
                    {
                        foreach (TrackedHandJoint joint in Enum.GetValues(typeof(TrackedHandJoint)))
                        {
                            // 关节位姿
                            MixedRealityPose jointPose;

                            HandJointUtils.TryGetJointPose(joint, hand.handedness, out jointPose);

                            bool contains = false;
                            if (hand.handedness == Handedness.Left)
                            {
                                contains = captureLeftHandJointList.Contains(joint);
                            }
                            if (hand.handedness == Handedness.Right)
                            {
                                contains = captureRightHandJointList.Contains(joint);
                            }

                            var isCross = EMR.Space.Mathematics.isSphereIncludePoint(((Vector3)jointPose.Position) * 1000f, worldSphereCenter, this.radius);

                            if (isCross && !contains)
                            {
                                if (hand.handedness == Handedness.Left)
                                {
                                    captureLeftHandJointList.Add(joint);
                                }
                                if (hand.handedness == Handedness.Right)
                                {
                                    captureRightHandJointList.Add(joint);
                                }

                                this.onAnchorJointHover.Invoke(new AnchorJointHoverEventData
                                {
                                    anchor = this,
                                    interestJoint = joint,
                                    handedness = hand.handedness
                                });
                            }
                            else if (!isCross && contains)
                            {
                                if (hand.handedness == Handedness.Left)
                                {
                                    this.captureLeftHandJointList.Remove(joint);
                                }
                                if (hand.handedness == Handedness.Right)
                                {
                                    this.captureRightHandJointList.Remove(joint);
                                }

                                this.onAnchorJointOut.Invoke(new AnchorJointOutEventData
                                {
                                    anchor = this,
                                    interestJoint = joint,
                                    handedness = hand.handedness
                                });
                            }
                        }
                    }
                    #endregion

                    #region 手部指针
                    // 左手指针
                    if ((this._intersetHandPointer == Handedness.Left || this._intersetHandPointer == Handedness.Both))
                    {
                        Vector3? point = EMR.Space.leftHand?.pointer?.point;
                        var oldCaptureLeftHandPointer = this.captureLeftHandPointer;
                        bool isCross = point != null ? EMR.Space.Mathematics.isSphereIncludePoint(((Vector3)point) * 1000f, worldSphereCenter, this.radius) : false;

                        if (isCross && oldCaptureLeftHandPointer == null)
                        {
                            var pointer = EMR.Space.leftHand?.pointer;
                            this.onAnchorPointerHover.Invoke(new AnchorPointerHoverEventData
                            {
                                anchor = this,
                                interestPointer = pointer
                            });

                            this.captureLeftHandPointer = (Pointer)EMR.Space.leftHand?.pointer;
                        }
                        else if (!isCross && oldCaptureLeftHandPointer != null)
                        {
                            var pointer = EMR.Space.leftHand?.pointer;
                            this.onAnchorPointerOut.Invoke(new AnchorPointerOutEventData
                            {
                                anchor = this,
                                interestPointer = pointer
                            });
                            this.captureLeftHandPointer = null;
                        }
                    }

                    // 右手指针
                    if ((this._intersetHandPointer == Handedness.Right || this._intersetHandPointer == Handedness.Both))
                    {
                        Vector3? point = EMR.Space.rightHand?.pointer?.point;
                        var oldCaptureRightHandPointer = this.captureRightHandPointer;
                        bool isCross = point != null ? EMR.Space.Mathematics.isSphereIncludePoint(((Vector3)point) * 1000f, worldSphereCenter, this.radius) : false;

                        if (isCross && oldCaptureRightHandPointer == null)
                        {
                            var pointer = EMR.Space.rightHand?.pointer;
                            this.onAnchorPointerHover.Invoke(new AnchorPointerHoverEventData
                            {
                                anchor = this,
                                interestPointer = pointer
                            });

                            this.captureRightHandPointer = (Pointer)EMR.Space.rightHand?.pointer;
                        }
                        else if (!isCross && oldCaptureRightHandPointer != null)
                        {
                            var pointer = EMR.Space.rightHand?.pointer;
                            this.onAnchorPointerOut.Invoke(new AnchorPointerOutEventData
                            {
                                anchor = this,
                                interestPointer = pointer
                            });
                            this.captureRightHandPointer = null;
                        }
                    }
                    #endregion
                }

                return false;
            });
        }

        #region 基本属性
        public Node node
        {
            get
            {
                return this._node;
            }
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string name
        {
            get
            {
                return this._name;
            }

            set
            {
                this._name = value;
            }
        }

        /// <summary>
        /// 锚x坐标 scale
        /// </summary>
        public float? x
        {
            get
            {
                float? result;

                GameObject container = this._node?.parasitifer;

                GameObject scrollContainer = null;

                if (this._node != null && this._node is PanelNode && ((PanelNode)this._node).scrollComponent != null)
                {
                    scrollContainer = ((PanelNode)this._node).scrollComponent.scrollContainer;
                }

                if (this._node != null && this._node is SpaceNode && ((SpaceNode)this._node).clipComponent != null)
                {
                    scrollContainer = ((SpaceNode)this._node).clipComponent.scrollContainer;
                }

                if (scrollContainer != null)
                {
                    container = scrollContainer;
                }

                if(this._node != null)
                {
                    result = this._x == null && target != null ? container.transform.InverseTransformPoint(target.parasitifer.transform.position).x : this._x;
                } else
                {
                    result = this._x == null && target != null ? target.parasitifer.transform.position.x : this._x;
                }

                return result;
            }

            set
            {
                this._x = value;
            }
        }

        /// <summary>
        /// 锚y坐标 scale
        /// </summary>
        public float? y
        {
            get
            {
                float? result;

                GameObject container = this._node?.parasitifer;

                GameObject scrollContainer = null;

                if (this._node != null && this._node is PanelNode && ((PanelNode)this._node).scrollComponent != null)
                {
                    scrollContainer = ((PanelNode)this._node).scrollComponent.scrollContainer;
                }

                if (this._node != null && this._node is SpaceNode && ((SpaceNode)this._node).clipComponent != null)
                {
                    scrollContainer = ((SpaceNode)this._node).clipComponent.scrollContainer;
                }

                if (scrollContainer != null)
                {
                    container = scrollContainer;
                }

                if (this._node != null)
                {
                    result = this._y == null && target != null && container != null ? container.transform.InverseTransformPoint(target.parasitifer.transform.position).y : this._y;
                }
                else
                {
                    result = this._y == null && target != null && container != null ? target.parasitifer.transform.position.y : this._y;
                }

                return result;
            }

            set
            {
                
                this._y = value;
            }
        }

        /// <summary>
        /// 锚z坐标 scale
        /// </summary>
        public float? z
        {
            get
            {
                float? result;

                GameObject container = this._node.parasitifer;

                GameObject scrollContainer = null;
                if (this._node != null && this._node is PanelNode && ((PanelNode)this._node).scrollComponent != null)
                {
                    scrollContainer = ((PanelNode)this._node).scrollComponent.scrollContainer;
                }

                if (this._node != null && this._node is SpaceNode && ((SpaceNode)this._node).clipComponent != null)
                {
                    scrollContainer = ((SpaceNode)this._node).clipComponent.scrollContainer;
                }

                if(scrollContainer != null)
                {
                    container = scrollContainer;
                }

                if (this._node != null)
                {
                    result = this._z == null && target != null && container != null ? container.transform.InverseTransformPoint(target.parasitifer.transform.position).z : this._z;
                }
                else
                {
                    result = this._z == null && target != null && container != null ? target.parasitifer.transform.position.z : this._z;
                }

                return result;
            }

            set
            {
                this._z = value;
            }
        }

        /// <summary>
        /// 半径 单位米
        /// </summary>
        public float radius
        {
            get
            {
                return this._radius;
            }

            set
            {
                this._radius = value / 1000f;
            }
        }

        /// <summary>
        /// 目标
        /// </summary>
        public Node target
        {
            get
            {
                return this._target;
            }

            set
            {
                this._target = value;
                this._targetId = value.id;
            }
        }

        private string _targetId = "";
        /// <summary>
        /// 目标Id
        /// </summary>
        public string targetId
        {
            get
            {
                return this._targetId;
            }

            set
            {
                this._targetId = value;
                this._target = this._node.component.getNodeById(value);
            }
        }

        /// <summary>
        /// 感兴趣Id
        /// </summary>
        public string interestId
        {
            get
            {
                return this._intersetId;
            }

            set
            {
                this._intersetId = value;
            }
        }

        /// <summary>
        /// 感兴趣名称
        /// </summary>
        public string intersetName
        {
            get
            {
                return this._intersetName;
            }

            set
            {
                this._intersetName = value;
            }
        }

        /// <summary>
        /// 感兴趣节点
        /// </summary>
        public List<Node> intersetNodeList
        {
            get
            {
                return this._intersetNodeList;
            }
        }

        /// <summary>
        /// 感兴趣手
        /// </summary>
        public string intersetHand
        {
            get
            {
                return this._intersetHand.ToString();
            }

            set
            {
                if (value == "none")
                {
                    this._intersetHand = Handedness.None;
                }
                if (value == "left")
                {
                    this._intersetHand = Handedness.Left;
                }
                if (value == "right")
                {
                    this._intersetHand = Handedness.Right;
                }
                if (value == "both")
                {
                    this._intersetHand = Handedness.Both;
                }
            }
        }

        /// <summary>
        /// 感兴趣手指针
        /// </summary>
        public string intersetHandPointer
        {
            get
            {
                return this._intersetHandPointer.ToString();
            }

            set
            {
                if (value == "none")
                {
                    this._intersetHandPointer = Handedness.None;
                }
                if (value == "left")
                {
                    this._intersetHandPointer = Handedness.Left;
                }
                if (value == "right")
                {
                    this._intersetHandPointer = Handedness.Right;
                }
                if (value == "both")
                {
                    this._intersetHandPointer = Handedness.Both;
                }
            }
        }
        #endregion

        #region 事件
        /// <summary>
        /// 节点悬浮事件
        /// </summary>
        public AnchorNodeHoverEvent onAnchorNodeHover
        {
            get
            {
                return this._onAnchorNodeHover;
            }
        }

        /// <summary>
        /// 节点退出事件
        /// </summary>
        public AnchorNodeOutEvent onAnchorNodeOut
        {
            get
            {
                return this._onAnchorNodeOut;
            }
        }

        /// <summary>
        /// 手指悬浮事件
        /// </summary>
        public AnchorJointHoverEvent onAnchorJointHover
        {
            get
            {
                return this._onAnchorJointHover;
            }
        }

        /// <summary>
        /// 手指退出事件
        /// </summary>
        public AnchorJointOutEvent onAnchorJointOut
        {
            get
            {
                return this._onAnchorJointOut;
            }
        }

        /// <summary>
        /// 指针悬浮事件
        /// </summary>
        public AnchorPointerHoverEvent onAnchorPointerHover
        {
            get
            {
                return this._onAnchorPointerHover;
            }
        }

        /// <summary>
        /// 指针退出事件
        /// </summary>
        public AnchorPointerOutEvent onAnchorPointerOut
        {
            get
            {
                return this._onAnchorPointerOut;
            }
        }
        #endregion

        #region 基本方法
        /// <summary>
        /// 刷新
        /// </summary>
        public void fresh()
        {
            if (this.target != null && this.x != null && this.y != null && this.z != null)
            {
                var x = (float)this.x;
                var y = (float)this.y;
                var z = (float)this.z;

                if (this._node != null)
                {
                    GameObject container = this._node.parasitifer;

                    GameObject scrollContainer = null;
                    if (this._node is PanelNode && ((PanelNode)this._node).scrollComponent != null)
                    {
                        scrollContainer = ((PanelNode)this._node).scrollComponent.scrollContainer;
                    }

                    if (this._node is SpaceNode && ((SpaceNode)this._node).clipComponent != null)
                    {
                        scrollContainer = ((SpaceNode)this._node).clipComponent.scrollContainer;
                    }

                    if (scrollContainer != null)
                    {
                        container = scrollContainer;
                    }

                    var point = container.transform.TransformPoint(new Vector3(x, y, z));
                    target.parasitifer.transform.position = point;
                }
                else
                {
                    target.parasitifer.transform.position = new Vector3(x, y, z);
                }

                if (target is PanelLayer)
                {
                    ((PanelLayer)target).positionSync();
                }

                if (target is PanelRoot)
                {
                    ((PanelRoot)target).positionSync();
                }

                if (target is SpaceNode)
                {
                    ((SpaceNode)target).positionSync();
                }
            }
        }

        public void alignment()
        {
            if (this.target != null && this.x != null && this.y != null && this.z != null)
            {
                var x = (float)this.x;
                var y = (float)this.y;
                var z = (float)this.z;

                if (this._node != null)
                {
                    GameObject container = this._node.parasitifer;

                    GameObject scrollContainer = null;
                    if (this._node is PanelNode && ((PanelNode)this._node).scrollComponent != null)
                    {
                        scrollContainer = ((PanelNode)this._node).scrollComponent.scrollContainer;
                    }

                    if (this._node is SpaceNode && ((SpaceNode)this._node).clipComponent != null)
                    {
                        scrollContainer = ((SpaceNode)this._node).clipComponent.scrollContainer;
                    }

                    if (scrollContainer != null)
                    {
                        container = scrollContainer;
                    }

                    var point = container.transform.TransformPoint(new Vector3(x, y, z));

                    var offsetX = Space.Unit.unitToScaleForGameObject(target.parasitifer, target.offset.x, Struct.Axle.right);
                    var offsetY = Space.Unit.unitToScaleForGameObject(target.parasitifer, target.offset.y, Struct.Axle.up);
                    var offsetZ = Space.Unit.unitToScaleForGameObject(target.parasitifer, target.offset.z, Struct.Axle.forward);

                    target.parasitifer.transform.position = point + new Vector3(offsetX, offsetY, offsetZ);
                }
                else
                {
                    var offsetX = Space.Unit.unitToScaleForGameObject(target.parasitifer, target.offset.x, Struct.Axle.right);
                    var offsetY = Space.Unit.unitToScaleForGameObject(target.parasitifer, target.offset.y, Struct.Axle.up);
                    var offsetZ = Space.Unit.unitToScaleForGameObject(target.parasitifer, target.offset.z, Struct.Axle.forward);

                    target.parasitifer.transform.position = new Vector3(x, y, z) + new Vector3(offsetX, offsetY, offsetZ);
                }

                if (target is PanelLayer)
                {
                    ((PanelLayer)target).positionSync();
                }

                if (target is PanelRoot)
                {
                    ((PanelRoot)target).positionSync();
                }

                if (target is SpaceNode)
                {
                    ((SpaceNode)target).positionSync();
                }
            }
        }
        #endregion
    }
}
