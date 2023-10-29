using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using EMR.Struct;
using EMR.Entity;
using EMR.Common;
using EMR.Event;

namespace EMR.Module
{
    public class Hand : IMixedRealityPointerHandler, IMixedRealityTouchHandler, IMixedRealitySourcePoseHandler
    {
        #region 基本字段
        /// <summary>
        /// 当前正在处理手势的节点
        /// </summary>
        private Node gestureNode;

        /// <summary>
        /// 当前手势
        /// </summary>
        private HandGesture currentGesture = HandGesture.none;

        /// <summary>
        /// 手势起始点
        /// </summary>
        private Vector3 gestureStartPoint;

        /// <summary>
        /// 原指针IsFocusLocked
        /// </summary>
        private bool orignIsFocusLocked = false;

        /// <summary>
        /// 原指针IsTargetPositionLockedOnFocusLock
        /// </summary>
        private bool orignIsTargetPositionLockedOnFocusLock = false;

        /// <summary>
        /// 是否手势处理节点
        /// </summary>
        private bool isGestureNode = false;

        /// <summary>
        /// 节点到手势handle的映射列表
        /// </summary>
        private Dictionary<Node, List<GestureHandle>> gestureNodeMap = new Dictionary<Node, List<GestureHandle>>();

        /// <summary>
        /// 惯用手
        /// </summary>
        public Handedness handedness = Handedness.Right;
        #endregion

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="handedness"></param>
        /// <param name="engine"></param>
        public Hand(Handedness handedness)
        {
            this.handedness = handedness;

            // 注册指针全局事件
            CoreServices.InputSystem?.RegisterHandler<IMixedRealityPointerHandler>(this);
            CoreServices.InputSystem?.RegisterHandler<IMixedRealityTouchHandler>(this);
            CoreServices.InputSystem?.RegisterHandler<IMixedRealitySourcePoseHandler>(this);
        }

        /// <summary>
        /// 获取手势handler count
        /// </summary>
        public int getHandlerCount(Node node)
        {
            List<GestureHandle> list = new List<GestureHandle>();

            if(gestureNodeMap.ContainsKey(node))
            {
                list = gestureNodeMap[node];
            }

            return list.Count;
        }

        protected readonly HandPoseChangeEvent _onHandPoseChange = new HandPoseChangeEvent();
        /// <summary>
        /// 插入节点完成事件
        /// </summary>
        public virtual HandPoseChangeEvent onHandPoseChange
        {
            get
            {
                return this._onHandPoseChange;
            }
        }

        void IMixedRealitySourcePoseHandler.OnSourcePoseChanged(SourcePoseEventData<MixedRealityPose> eventData)
        {
            // 只能对手输入源有效
            if (eventData.InputSource.SourceType != InputSourceType.Hand || eventData.Controller.ControllerHandedness != this.handedness)
            {
                return;
            }

            // 抛出组件插入完成事件
            HandPoseChangeEventData insertedEventData = new HandPoseChangeEventData
            {
                position = eventData.SourceData.Position,
                rotation = eventData.SourceData.Rotation,
                original = eventData
            };
            onHandPoseChange.Invoke(insertedEventData);
        }

        /// <summary>
        /// 指针
        /// </summary>
        public Pointer pointer
        {
            get
            {
                return EMR.Space.mainService.pointer != null && EMR.Space.mainService.pointer?.handedness == this.handedness ? EMR.Space.mainService.pointer : null;
            }
        }

        #region 手指位姿属性
        // 掌心
        public MixedRealityPose Palm
        {
            get
            {

                MixedRealityPose result;
                HandJointUtils.TryGetJointPose(TrackedHandJoint.Palm, this.handedness, out result);
                return result;
            }
        }

        // 腕关节
        public MixedRealityPose Wrist
        {
            get
            {
                MixedRealityPose result;
                HandJointUtils.TryGetJointPose(TrackedHandJoint.Wrist, this.handedness, out result);
                return result;
            }
        }

        // 拇指
        public MixedRealityPose ThumbTip
        {
            get
            {
                MixedRealityPose result;
                HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbTip, this.handedness, out result);
                return result;
            }
        }

        public MixedRealityPose ThumbDistalJoint
        {
            get
            {
                MixedRealityPose result;
                HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbDistalJoint, this.handedness, out result);
                return result;
            }
        }

        public MixedRealityPose ThumbProximalJoint
        {
            get
            {
                MixedRealityPose result;
                HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbProximalJoint, this.handedness, out result);
                return result;
            }
        }

        public MixedRealityPose ThumbMetacarpalJoint
        {
            get
            {
                MixedRealityPose result;
                HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbMetacarpalJoint, this.handedness, out result);
                return result;
            }
        }


        // 食指
        public MixedRealityPose IndexTip
        {
            get
            {
                MixedRealityPose result;
                HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, this.handedness, out result);
                return result;
            }
        }

        public MixedRealityPose IndexDistalJoint
        {
            get
            {
                MixedRealityPose result;
                HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexDistalJoint, this.handedness, out result);
                return result;
            }
        }

        public MixedRealityPose IndexMiddleJoint
        {
            get
            {
                MixedRealityPose result;
                HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexMiddleJoint, this.handedness, out result);
                return result;
            }
        }

        public MixedRealityPose IndexKnuckle
        {
            get
            {
                MixedRealityPose result;
                HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexKnuckle, this.handedness, out result);
                return result;
            }
        }

        public MixedRealityPose IndexMetacarpal
        {
            get
            {
                MixedRealityPose result;
                HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexMetacarpal, this.handedness, out result);
                return result;
            }
        }


        // 中指
        public MixedRealityPose MiddleTip
        {
            get
            {
                MixedRealityPose result;
                HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleTip, this.handedness, out result);
                return result;
            }
        }

        public MixedRealityPose MiddleDistalJoint
        {
            get
            {
                MixedRealityPose result;
                HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleDistalJoint, this.handedness, out result);
                return result;
            }
        }

        public MixedRealityPose MiddleMiddleJoint
        {
            get
            {
                MixedRealityPose result;
                HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleMiddleJoint, this.handedness, out result);
                return result;
            }
        }

        public MixedRealityPose MiddleKnuckle
        {
            get
            {
                MixedRealityPose result;
                HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleKnuckle, this.handedness, out result);
                return result;
            }
        }

        public MixedRealityPose MiddleMetacarpal
        {
            get
            {
                MixedRealityPose result;
                HandJointUtils.TryGetJointPose(TrackedHandJoint.MiddleMetacarpal, this.handedness, out result);
                return result;
            }
        }


        // 无名指
        public MixedRealityPose RingTip
        {
            get
            {
                MixedRealityPose result;
                HandJointUtils.TryGetJointPose(TrackedHandJoint.RingTip, this.handedness, out result);
                return result;
            }
        }

        public MixedRealityPose RingDistalJoint
        {
            get
            {
                MixedRealityPose result;
                HandJointUtils.TryGetJointPose(TrackedHandJoint.RingDistalJoint, this.handedness, out result);
                return result;
            }
        }

        public MixedRealityPose RingMiddleJoint
        {
            get
            {
                MixedRealityPose result;
                HandJointUtils.TryGetJointPose(TrackedHandJoint.RingMiddleJoint, this.handedness, out result);
                return result;
            }
        }

        public MixedRealityPose RingKnuckle
        {
            get
            {
                MixedRealityPose result;
                HandJointUtils.TryGetJointPose(TrackedHandJoint.RingKnuckle, this.handedness, out result);
                return result;
            }
        }

        public MixedRealityPose RingMetacarpal
        {
            get
            {
                MixedRealityPose result;
                HandJointUtils.TryGetJointPose(TrackedHandJoint.RingMetacarpal, this.handedness, out result);
                return result;
            }
        }


        // 小指
        public MixedRealityPose PinkyTip
        {
            get
            {
                MixedRealityPose result;
                HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyTip, this.handedness, out result);
                return result;
            }
        }

        public MixedRealityPose PinkyDistalJoint
        {
            get
            {
                MixedRealityPose result;
                HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyDistalJoint, this.handedness, out result);
                return result;
            }
        }

        public MixedRealityPose PinkyMiddleJoint
        {
            get
            {
                MixedRealityPose result;
                HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyMiddleJoint, this.handedness, out result);
                return result;
            }
        }

        public MixedRealityPose PinkyKnuckle
        {
            get
            {
                MixedRealityPose result;
                HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyKnuckle, this.handedness, out result);
                return result;
            }
        }

        public MixedRealityPose PinkyMetacarpal
        {
            get
            {
                MixedRealityPose result;
                HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyMetacarpal, this.handedness, out result);
                return result;
            }
        }
        #endregion

        #region 基本方法
        /// <summary>
        /// 添加手势handle
        /// </summary>
        /// <param name="node"></param>
        /// <param name="handle"></param>
        public void addGestureHandler(Node node, GestureHandle handle)
        {
            if(node == null)
            {
                Debug.LogError("添加手势处理函数时，node不能为空");
                return;
            }

            if (handle == null)
            {
                Debug.LogError("添加手势处理函数时，handle不能为空");
                return;
            }

            if (!gestureNodeMap.ContainsKey(node))
            {
                gestureNodeMap.Add(node, new List<GestureHandle>());
            }

            var handleList = gestureNodeMap[node];

            var isVaild = true;
            foreach (var item in handleList)
            {
                if (item == handle)
                {
                    isVaild = false;
                }
            }

            if (isVaild)
            {
                handleList.Add(handle);

                // 处理autoCollider
                if (!node.collider && node.autoCollider == null)
                {
                    node.addAutoCollider();
                }

                // 处理autoInteractionTouchable
                if (node.interactionTouchableAuto && !node.hasAutoInteractionTouchable)
                {
                    node.addAutoInteractionTouchable();
                }
            }
            else
            {
                Debug.LogError("重复添加gesture handle");
            }
        }

        /// <summary>
        /// 移除手势handle
        /// </summary>
        /// <param name="node"></param>
        /// <param name="handle"></param>
        public void removeGestureHandler(Node node, GestureHandle handle)
        {
            if (node == null)
            {
                Debug.LogError("移除手势处理函数时，node不能为空");
                return;
            }

            if (handle == null)
            {
                Debug.LogError("移除手势处理函数时，handle不能为空");
                return;
            }

            if (gestureNodeMap.ContainsKey(node))
            {
                var handleList = gestureNodeMap[node];

                for (var i = 0; i < handleList.Count; i++)
                {
                    if (handleList[i] == handle)
                    {
                        handleList.RemoveAt(i);
                        break;
                    }
                }

                if (handleList.Count == 0)
                {
                    gestureNodeMap.Remove(node);
                }
            }
        }

        /// <summary>
        /// 移除手势
        /// </summary>
        /// <param name="node"></param>
        public void removeGesture(Node node)
        {
            if (gestureNodeMap.ContainsKey(node))
            {
                var handleList = gestureNodeMap[node];

                handleList.Clear();

                if (handleList.Count == 0)
                {
                    gestureNodeMap.Remove(node);
                }
            }
        }
        #endregion

        #region 用户行为识别
        /*------------------------------------------------------定义用户行为识别支撑方法开始------------------------------------------------------*/
        /// <summary>
        /// 防抖动
        /// </summary>
        /// <param name="currentPoint"></param>
        /// <param name="xAllowRange"></param>
        /// <param name="yAllowRange"></param>
        private bool debounce(Vector3 currentPoint, float xAllowRange, float yAllowRange)
        {
            bool result = true;

            var currentLocalPoint = this.gestureNode.parasitifer.transform.InverseTransformPoint(currentPoint);
            var selectLocalPoint = this.gestureNode.parasitifer.transform.InverseTransformPoint(this.gestureStartPoint);
            var vector = currentLocalPoint - selectLocalPoint;

            var offestXDistance = Space.Unit.scaleToUnit(this.gestureNode, Mathf.Abs(vector.x), Axle.right);
            var offestYDistance = Space.Unit.scaleToUnit(this.gestureNode, Mathf.Abs(vector.y), Axle.up);

            if (!Utils.noExceed(offestXDistance, xAllowRange) || !Utils.noExceed(offestYDistance, yAllowRange))
            {
                result = false;
            }

            return result;
        }
        /// <summary>
        /// 清除手势
        /// </summary>
        private void cleanGesture()
        {
            this.currentGesture = HandGesture.none;
            this.isCanelHold = false;
        }

        /// <summary>
        /// 设置select手势
        /// </summary>
        /// <param name="nodeTarget"></param>
        /// <param name="selectPoint"></param>
        private void setSelect(Node nodeTarget, Vector3 selectPoint)
        {
            // 设置当前手势所在的目标节点
            this.gestureNode = nodeTarget;

            // 设置当前手势所在的空间位置
            this.gestureStartPoint = selectPoint;

            // 设置当前的手势
            this.currentGesture = HandGesture.select;

            // 势行用户注册的手势回调
            var handleList = gestureNodeMap[nodeTarget];
            foreach (var item in handleList)
            {
                item(this.gestureNode, this.currentGesture, GestureDirection.none);
            }
        }


        private bool isCanelHold = false;
        /// <summary>
        ///  设置hold手势
        /// </summary>
        private void delaySetHold()
        {
            Room.timeOut(2, () =>
            {
                if (this.gestureNode != null && !this.isCanelHold)
                {
                    this.currentGesture = HandGesture.hold;

                    var handleList = gestureNodeMap[this.gestureNode];
                    foreach (var item in handleList)
                    {
                        item(this.gestureNode, this.currentGesture, GestureDirection.none);
                    }
                }
            });
        }

        /// <summary>
        /// 设置导航手势
        /// </summary>
        /// <param name="currentPoint"></param>
        private void setNavigation(Vector3 currentPoint)
        {
            if(currentPoint == null)
            {
                return;
            }

            var currentLocalPoint = this.gestureNode.parasitifer.transform.InverseTransformPoint(currentPoint);
            var selectLocalPoint = this.gestureNode.parasitifer.transform.InverseTransformPoint(this.gestureStartPoint);
            var vector = currentLocalPoint - selectLocalPoint;

            var x = Space.Unit.scaleToUnit(this.gestureNode, Mathf.Abs(vector.x), Axle.right);
            var y = Space.Unit.scaleToUnit(this.gestureNode, Mathf.Abs(vector.y), Axle.up);

            if (Mathf.Sqrt(x * x + y * y) > 15)
            {
                if (Mathf.Abs(Vector3.Dot(vector.normalized, new Vector3(1f, 0f, 0f))) > Mathf.Abs(Vector3.Dot(vector.normalized, new Vector3(0f, 1f, 0f))))
                {
                    this.currentGesture = HandGesture.navigationX;
                    var handleList = gestureNodeMap[this.gestureNode];
                    foreach (var item in handleList)
                    {
                        item(this.gestureNode, this.currentGesture, vector.x > 0 ? GestureDirection.right : GestureDirection.left);
                    }
                }
                else
                {
                    this.currentGesture = HandGesture.navigationY;
                    var handleList = gestureNodeMap[this.gestureNode];
                    foreach (var item in handleList)
                    {
                        item(this.gestureNode, this.currentGesture, vector.y > 0 ? GestureDirection.up : GestureDirection.down);
                    }
                }

                this.isCanelHold = true;
            }
        }
        /*------------------------------------------------------定义用户行为识别支撑方法结束------------------------------------------------------*/


        /*
         *  以下逻辑通过事件识别用户行为
         */
        void IMixedRealityPointerHandler.OnPointerClicked(MixedRealityPointerEventData eventData){}

        void IMixedRealityPointerHandler.OnPointerDown(MixedRealityPointerEventData eventData)
        {
            this.orignIsFocusLocked = eventData.Pointer.IsFocusLocked;
            this.orignIsTargetPositionLockedOnFocusLock = eventData.Pointer.IsTargetPositionLockedOnFocusLock;

            // 手势只能对手输入源有效
            if(eventData.InputSource.SourceType != InputSourceType.Hand || eventData.Handedness != this.handedness || eventData.Pointer.Result?.Details.Object == null || EMR.Space.getNodeByParasitifer(eventData.Pointer.Result?.Details.Object) == null)
            {
                return;
            }

            this.isGestureNode = gestureNodeMap.ContainsKey(EMR.Space.getNodeByParasitifer(eventData.Pointer.Result?.Details.Object));

            if (!this.isGestureNode)
            {
                return;
            }

            eventData.Pointer.IsTargetPositionLockedOnFocusLock = false;
            eventData.Pointer.IsFocusLocked = false;

            this.cleanGesture();

            var currentTarget = eventData.Pointer.Result.CurrentPointerTarget;

            var nodeCurrentTarget = currentTarget != null ? Space.getNodeByParasitifer(currentTarget) : null;

            if (nodeCurrentTarget != null && gestureNodeMap.ContainsKey(nodeCurrentTarget))
            {
                var selectPoint = eventData.Pointer.Result.Details.Point;

                // 设置select手势
                this.setSelect(nodeCurrentTarget, selectPoint);

                // 延迟设置hold手势
                this.delaySetHold();
            }
        }

        void IMixedRealityPointerHandler.OnPointerDragged(MixedRealityPointerEventData eventData)
        {
            // 手势只能对手输入源有效
            if (!this.isGestureNode || eventData.InputSource.SourceType != InputSourceType.Hand || eventData.Handedness != this.handedness)
            {
                return;
            }

            // 当滑出节点边界、select下手指超出防抖范围时清空手势
            var currentTarget = eventData.Pointer.Result.CurrentPointerTarget;

            var nodeCurrentTarget = currentTarget != null ? Space.getNodeByParasitifer(currentTarget) : null;

            if ((nodeCurrentTarget == null || nodeCurrentTarget != null && nodeCurrentTarget != this.gestureNode || this.currentGesture == HandGesture.select && nodeCurrentTarget != null && !this.debounce(eventData.Pointer.Result.Details.Point, 50f, 50f)))
            {
                this.cleanGesture();
            }

            // 当滑出节点边界时清空gestureNode，并且阻止进入下一步手势的设置
            if ((nodeCurrentTarget != null && nodeCurrentTarget != this.gestureNode) || nodeCurrentTarget == null) {
                this.gestureNode = null;
                this.isGestureNode = false;
                return;
            }

            // 设置导航手势
            if(this.currentGesture != HandGesture.navigationX && this.currentGesture != HandGesture.navigationY)
            {
                this.setNavigation(eventData.Pointer.Result.Details.Point);
            }
        }

        void IMixedRealityPointerHandler.OnPointerUp(MixedRealityPointerEventData eventData)
        {
            // 手势只能对手输入源有效
            if (!this.isGestureNode || eventData.InputSource.SourceType != InputSourceType.Hand || eventData.Handedness != this.handedness)
            {
                return;
            }

            this.cleanGesture();
            this.gestureNode = null;
            this.isGestureNode = false;

            eventData.Pointer.IsFocusLocked = this.orignIsFocusLocked;
            eventData.Pointer.IsTargetPositionLockedOnFocusLock = this.orignIsTargetPositionLockedOnFocusLock;
        }


        void IMixedRealityTouchHandler.OnTouchStarted(HandTrackingInputEventData eventData)
        {
            // 手势只能对手输入源有效
            if (eventData.InputSource.SourceType != InputSourceType.Hand || eventData.Handedness != this.handedness)
            {
                return;
            }

            PokePointer pokePointer = PointerUtils.GetPointer<PokePointer>(eventData.Handedness);
            var currentTarget = pokePointer.Result?.CurrentPointerTarget;

            this.isGestureNode = gestureNodeMap.ContainsKey(EMR.Space.getNodeByParasitifer(currentTarget));

            if (!this.isGestureNode)
            {
                return;
            }

            this.cleanGesture();

            var nodeCurrentTarget = currentTarget != null ? Space.getNodeByParasitifer(currentTarget) : null;
            if (nodeCurrentTarget != null && gestureNodeMap.ContainsKey(nodeCurrentTarget))
            {
                var selectPoint = pokePointer.Result?.Details.Point;

                // 设置select手势
                if (selectPoint != null)
                {
                    this.setSelect(nodeCurrentTarget, (Vector3)selectPoint);

                    // 延迟设置hold手势
                    this.delaySetHold();
                }
            }
        }

        void IMixedRealityTouchHandler.OnTouchUpdated(HandTrackingInputEventData eventData)
        {
            // 手势只能对手输入源有效
            if (!this.isGestureNode || eventData.InputSource.SourceType != InputSourceType.Hand || eventData.Handedness != this.handedness)
            {
                return;
            }

            // 当滑出节点边界、select下手指超出防抖范围时清空手势
            PokePointer pokePointer = PointerUtils.GetPointer<PokePointer>(eventData.Handedness);
            var currentTarget = pokePointer.Result?.CurrentPointerTarget;

            var nodeCurrentTarget = currentTarget != null ? Space.getNodeByParasitifer(currentTarget) : null;
            if ((nodeCurrentTarget == null || nodeCurrentTarget != null && nodeCurrentTarget != this.gestureNode || this.currentGesture == HandGesture.select && nodeCurrentTarget != null && pokePointer.Result?.Details.Point != null && !this.debounce((Vector3)pokePointer.Result?.Details.Point, 50f, 50f)))
            {
                this.cleanGesture();
            }

            // 当滑出节点边界时清空gestureNode，并且阻止进入下一步手势的设置
            if ((nodeCurrentTarget != null && nodeCurrentTarget != this.gestureNode) || nodeCurrentTarget == null) {
                this.gestureNode = null;
                this.isGestureNode = false;
                return;
            }

            if (pokePointer.Result?.Details.Point != null && (this.currentGesture != HandGesture.navigationX && this.currentGesture != HandGesture.navigationY))
            {
                // 设置导航手势
                this.setNavigation((Vector3)pokePointer.Result?.Details.Point);
            }
        }

        void IMixedRealityTouchHandler.OnTouchCompleted(HandTrackingInputEventData eventData)
        {
            // 手势只能对手输入源有效
            if (!this.isGestureNode || eventData.InputSource.SourceType != InputSourceType.Hand || eventData.Handedness != this.handedness)
            {
                return;
            }

            this.cleanGesture();
            this.gestureNode = null;
            this.isGestureNode = false;
        }

        void IMixedRealitySourcePoseHandler.OnSourcePoseChanged(SourcePoseEventData<Quaternion> eventData) { }

        void IMixedRealitySourcePoseHandler.OnSourcePoseChanged(SourcePoseEventData<TrackingState> eventData) { }

        void IMixedRealitySourcePoseHandler.OnSourcePoseChanged(SourcePoseEventData<Vector2> eventData) { }

        void IMixedRealitySourcePoseHandler.OnSourcePoseChanged(SourcePoseEventData<Vector3> eventData) { }

        public void OnSourceDetected(SourceStateEventData eventData) { }

        public void OnSourceLost(SourceStateEventData eventData) { }
        #endregion
    }
}
