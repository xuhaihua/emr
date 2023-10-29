
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using EMR.Entity;

namespace EMR.Event
{
    /// <summary>
    /// 本类主要用于注册及管理节点的基本行为
    /// </summary>
    public class EventEngine : MonoBehaviour, IMixedRealityPointerHandler, IMixedRealityTouchHandler, IMixedRealityFocusChangedHandler, IMixedRealityFocusHandler
    {
        // 关联的空间节点
        public Node node;


        // 触发指针按下事件
        void IMixedRealityPointerHandler.OnPointerDown(MixedRealityPointerEventData eventData)
        {
            // 当该节点带有指针事件时，则引发该指针的相关事件
            if(this.node is IPointerEventFeature)
            {
                ((IPointerEventFeature)this.node).onDown.Invoke(new DownEventData
                {
                    target = this.node,
                    normal = eventData.Pointer.Result.Details.LastRaycastHit.normal,
                    point = eventData.Pointer.Result.Details.LastRaycastHit.point,
                    pointer = eventData.Pointer,
                    original = eventData
                });
            }
        }

        // 触发指针释放事件
        void IMixedRealityPointerHandler.OnPointerUp(MixedRealityPointerEventData eventData)
        {
            // 当该节点带有指针事件时，则引发该指针的相关事件
            if (this.node is IPointerEventFeature)
            {
                ((IPointerEventFeature)this.node).onUp.Invoke(new UpEventData
                {
                    target = this.node,
                    normal = eventData.Pointer.Result.Details.LastRaycastHit.normal,
                    point = eventData.Pointer.Result.Details.LastRaycastHit.point,
                    pointer = eventData.Pointer,
                    original = eventData
                });
            }
        }

        // 触发指针拖动事件
        void IMixedRealityPointerHandler.OnPointerDragged(MixedRealityPointerEventData eventData)
        {
            // 当该节点带有指针事件时，则引发该指针的相关事件
            if (this.node is IPointerEventFeature)
            {
                ((IPointerEventFeature)this.node).onDragged.Invoke(new DraggedEventData
                {
                    target = this.node,
                    normal = eventData.Pointer.Result.Details.LastRaycastHit.normal,
                    point = eventData.Pointer.Result.Details.LastRaycastHit.point,
                    pointer = eventData.Pointer,
                    original = eventData
                });
            }
        }

        // 触发单击事件
        void IMixedRealityPointerHandler.OnPointerClicked(MixedRealityPointerEventData eventData)
        {
            // 当该节点带有指针事件时，则引发该指针的相关事件
            if (this.node is IPointerEventFeature)
            {
                ((IPointerEventFeature)this.node).onClick.Invoke(new ClickEventData
                {
                    // 指针目标节点
                    target = this.node,

                    // 击中点法线
                    normal = eventData.Pointer.Result.Details.LastRaycastHit.normal,

                    // 击中点
                    point = eventData.Pointer.Result.Details.LastRaycastHit.point,

                    // 指针
                    pointer = eventData.Pointer,

                    // 原始事件数据
                    original = eventData
                });
            }
        }

        void IMixedRealityTouchHandler.OnTouchStarted(HandTrackingInputEventData eventData)
        {
            // 当该节点带有指针事件时，则引发该指针的相关事件
            if (this.node is ITouchEventFeature)
            {
                var pointer = PointerUtils.GetPointer<PokePointer>(eventData.Handedness);
                ((ITouchEventFeature)this.node).onTouchStarted.Invoke(new TouchStartedEventData
                {
                    // 触摸目标节点
                    target = this.node,

                    // 击中点法线
                    
                    normal = pointer.Result.Details.LastRaycastHit.normal,

                    // 击中点
                    point = pointer.Result.Details.LastRaycastHit.point,

                    // 指针
                    pointer = pointer,

                    // 原始事件数据
                    original = eventData
                });
            }
        }

        void IMixedRealityTouchHandler.OnTouchUpdated(HandTrackingInputEventData eventData)
        {
            // 当该节点带有指针事件时，则引发该指针的相关事件
            if (this.node is ITouchEventFeature)
            {
                var pointer = PointerUtils.GetPointer<PokePointer>(eventData.Handedness);
                ((ITouchEventFeature)this.node).onTouchUpdate.Invoke(new TouchUpdateEventData
                {
                    // 触摸目标节点
                    target = this.node,

                    // 击中点法线

                    normal = pointer.Result.Details.LastRaycastHit.normal,

                    // 击中点
                    point = pointer.Result.Details.LastRaycastHit.point,

                    // 指针
                    pointer = pointer,

                    // 原始事件数据
                    original = eventData
                });
            }
        }

        void IMixedRealityTouchHandler.OnTouchCompleted(HandTrackingInputEventData eventData)
        {
            // 当该节点带有指针事件时，则引发该指针的相关事件
            if (this.node is ITouchEventFeature)
            {
                var pointer = PointerUtils.GetPointer<PokePointer>(eventData.Handedness);
                ((ITouchEventFeature)this.node).onTouchCompleted.Invoke(new TouchCompletedEventData
                {
                    // 触摸目标节点
                    target = this.node,

                    // 击中点法线

                    normal = pointer.Result.Details.LastRaycastHit.normal,

                    // 击中点
                    point = pointer.Result.Details.LastRaycastHit.point,

                    // 指针
                    pointer = pointer,

                    // 原始事件数据
                    original = eventData
                });
            }
        }

        void IMixedRealityFocusChangedHandler.OnBeforeFocusChange(FocusEventData eventData)
        {

        }

        void IMixedRealityFocusChangedHandler.OnFocusChanged(FocusEventData eventData)
        {
            if(this.node is IFocusEventFeature)
            {
                ((IFocusEventFeature)this.node).onFocusChanged.Invoke(new FocusChangedEventData
                {
                    // 指针目标节点
                    target = eventData.NewFocusedObject != null ? EMR.Space.getNodeByParasitifer(eventData.NewFocusedObject) : null,

                    oldNode = eventData.OldFocusedObject != null ? EMR.Space.getNodeByParasitifer(eventData.OldFocusedObject) : null,

                    // 原始事件数据
                    original = eventData
                });
            }
        }

        void IMixedRealityFocusHandler.OnFocusEnter(FocusEventData eventData)
        {
            if (this.node is IFocusEventFeature)
            {
                ((IFocusEventFeature)this.node).onFocusEnter.Invoke(new FocusEnterEventData
                {
                    // 指针目标节点
                    target = this.node,

                    oldNode = eventData.OldFocusedObject != null ? EMR.Space.getNodeByParasitifer(eventData.OldFocusedObject) : null,

                    // 原始事件数据
                    original = eventData
                });
            }
        }

        void IMixedRealityFocusHandler.OnFocusExit(FocusEventData eventData)
        {
            if (this.node is IFocusEventFeature)
            {
                ((IFocusEventFeature)this.node).onFocusExit.Invoke(new FocusExitEventData
                {
                    // 指针目标节点
                    target = eventData.NewFocusedObject != null ? EMR.Space.getNodeByParasitifer(eventData.NewFocusedObject) : null, 
                    
                    oldNode = this.node,

                    // 原始事件数据
                    original = eventData
                });
            }
        }

        //碰撞开始
        public void OnCollisionEnter(Collision collision)
        {

            if (this.node is ICollisionEventFeature)
            {
                ((ICollisionEventFeature)this.node).onCollisionEnter.Invoke(new CollisionEnterEventData
                {
                    // 指针目标节点
                    target = this.node,


                    // 原始事件数据
                    original = collision
                });
            }
        }

        //碰撞中
        public void OnCollisionStay(Collision collision)
        {
            if (this.node is ICollisionEventFeature)
            {
                ((ICollisionEventFeature)this.node).onCollisionStay.Invoke(new CollisionStayEventData
                {
                    // 指针目标节点
                    target = this.node,

                    // 原始事件数据
                    original = collision
                });
            }
        }

        //碰撞结束
        public void OnCollisionExit(Collision collision)
        {
            if (this.node is ICollisionEventFeature)
            {
                ((ICollisionEventFeature)this.node).onCollisionExit.Invoke(new CollisionExitEventData
                {
                    // 指针目标节点
                    target = this.node,

                    // 原始事件数据
                    original = collision
                });
            }
        }
    }
}
