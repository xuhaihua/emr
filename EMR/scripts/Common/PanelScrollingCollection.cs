// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

/*
    本组件在ScrollingCollection基础上作了一些调整以下为做的一些修改的地方：
    1、初始化时ScrollingTouchable、ScrollCollider、ClipBox位姿由外界传入的config决定
    2、在该组件内定义了一些新的对象引用_emptyScroll、 _scrollContainer、_scrollBoxCollider、_clippingBounds分别对应用本组件中的_emptyScroll.transform.gameobject、ScrollCollider、ClipBox、ClipBox.transform.gameobject，引入这些引用原因是与PanelScroll创建的结构名称上一致，只是为了后续阅读方便
    3、提供了方法computeContentBounds、修改了一下UpdateContentBounds以为PanelScroll提供能力上的支持
    4、提供了overflowBack方法主要功能是当内容超出边界时将其回滚
    5、提供了fresh方法主要是用于在尺寸等发生改变时调整本组件内的基本参数，例如contentBounds
    6、1895行、1905行做了语句修改和注释
    7、加入了边界溢出处理回调，当滚动内容超出边界时自动执行由外界赋于的处理逻辑
 */

using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Microsoft.MixedReality.Toolkit.UI
{

    public delegate void BoundOverflowHandler();

    /// <summary>
    /// A scrollable frame where content scroll is triggered by manual controller click and drag or according to pagination settings.
    //// </summary>
    ///<remarks>Executing also in edit mode to properly catch and mask any new content added to scroll container.</remarks>
    [ExecuteAlways]
    [AddComponentMenu("Scripts/MRTK/SDK/PanelScrollingCollection")]
    public class PanelScrollingCollection : MonoBehaviour,
            IMixedRealityPointerHandler,
            IMixedRealitySourceStateHandler,
            IMixedRealityTouchHandler
    {
        /// <summary>
        /// How velocity is applied to a <see cref="PanelScrollingCollection"/> when a scroll is released.
        /// </summary>
        public enum VelocityType
        {
            FalloffPerFrame = 0,
            FalloffPerItem,
            NoVelocitySnapToItem,
            None
        }


        /// <summary>
        /// The direction in which a <see cref="PanelScrollingCollection"/> can scroll.
        /// </summary>
        public enum ScrollDirectionType
        {
            UpAndDown = 0,
            LeftAndRight,
        }

        [SerializeField]
        [Tooltip("Enables/disables scrolling with near/far interaction.")]
        private bool canScroll = true;

        /// <summary>
        /// Enables/disables scrolling with near/far interaction.
        /// </summary>
        /// <remarks>Helpful for controls where you may want pagination or list movement without freeform scrolling.</remarks>
        public bool CanScroll
        {
            get { return canScroll; }
            set { canScroll = value; }
        }

        /// <summary>
        /// Edit modes for defining scroll viewable area and scroll interaction boundaries.
        /// </summary>
        public enum EditMode
        {
            Auto = 0, // Use pagination values
            Manual, // Use direct manipulation of the object
        }

        [SerializeField]
        [Tooltip("Edit modes for defining the clipping box masking boundaries. Choose 'Auto' to automatically use pagination values. Choose 'Manual' for enabling direct manipulation of the clipping box object.")]
        private EditMode maskEditMode;

        /// <summary>
        /// Edit modes for defining the clipping box masking boundaries. Choose 'Auto' to automatically use pagination values. Choose 'Manual' for enabling direct manipulation of the clipping box object.
        /// </summary>
        public EditMode MaskEditMode
        {
            get { return maskEditMode; }
            set { maskEditMode = value; }
        }

        [SerializeField]
        [Tooltip("Edit modes for defining the scroll interaction collider boundaries. Choose 'Auto' to automatically use pagination values. Choose 'Manual' for enabling direct manipulation of the collider.")]
        private EditMode colliderEditMode;

        /// <summary>
        /// Edit modes for defining the scroll interaction collider boundaries. Choose 'Auto' to automatically use pagination values. Choose 'Manual' for enabling direct manipulation of the collider.
        /// </summary>
        public EditMode ColliderEditMode
        {
            get { return colliderEditMode; }
            set { colliderEditMode = value; }
        }

        [SerializeField]
        private bool maskEnabled = true;

        /// <summary>
        /// Visibility mode of scroll content. Default value will mask all objects outside of the scroll viewable area.
        /// </summary>
        public bool MaskEnabled
        {
            get { return maskEnabled; }
            set
            {
                if (!value && value != wasMaskEnabled)
                {
                    RestoreContentVisibility();
                }
                wasMaskEnabled = value;
                maskEnabled = value;
            }
        }

        // Helps catching any changes on the mask enabled value made from the inspector.
        // With the custom editor, the mask enabled field is changed before mask enabled setter is called.
        private bool wasMaskEnabled = true;

        [SerializeField]
        [Tooltip("The distance, in meters, the current pointer can travel along the scroll direction before triggering a scroll drag.")]
        [Range(0.0f, 0.2f)]
        private float handDeltaScrollThreshold = 0.02f;

        /// <summary>
        /// The distance, in meters, the current pointer can travel along the scroll direction before triggering a scroll drag.
        /// </summary>
        public float HandDeltaScrollThreshold
        {
            get { return handDeltaScrollThreshold; }
            set { handDeltaScrollThreshold = value; }
        }

        [SerializeField]
        [Tooltip("Withdraw amount, in meters, from the front of the scroll boundary needed to transition from touch engaged to released.")]
        private float releaseThresholdFront = 0.03f;
        /// <summary>
        /// Withdraw amount, in meters, from the front of the scroll boundary needed to transition from touch engaged to released.
        /// </summary>
        public float ReleaseThresholdFront
        {
            get { return releaseThresholdFront; }
            set { releaseThresholdFront = value; }
        }

        [SerializeField]
        [Tooltip("Withdraw amount, in meters, from the back of the scroll boundary needed to transition from touch engaged to released.")]
        private float releaseThresholdBack = 0.20f;
        /// <summary>
        /// Withdraw amount, in meters, from the back of the scroll boundary needed to transition from touch engaged to released.
        /// </summary>
        public float ReleaseThresholdBack
        {
            get { return releaseThresholdBack; }
            set { releaseThresholdBack = value; }
        }

        [SerializeField]
        [Tooltip("Withdraw amount, in meters, from the right or left of the scroll boundary needed to transition from touch engaged to released.")]
        private float releaseThresholdLeftRight = 0.20f;
        /// <summary>
        /// Withdraw amount, in meters, from the right or left of the scroll boundary needed to transition from touch engaged to released.
        /// </summary>
        public float ReleaseThresholdLeftRight
        {
            get { return releaseThresholdLeftRight; }
            set { releaseThresholdLeftRight = value; }
        }

        [SerializeField]
        [Tooltip("Withdraw amount, in meters, from the top or bottom of the scroll boundary needed to transition from touch engaged to released.")]
        private float releaseThresholdTopBottom = 0.20f;
        /// <summary>
        /// Withdraw amount, in meters, from the top or bottom of the scroll boundary needed to transition from touch engaged to released.
        /// </summary>
        public float ReleaseThresholdTopBottom
        {
            get { return releaseThresholdTopBottom; }
            set { releaseThresholdTopBottom = value; }
        }

        [SerializeField]
        [Tooltip("Distance, in meters, to position a local xy plane used to verify if a touch interaction started in the front of the scroll view.")]
        [Range(0.0f, 0.05f)]
        private float frontTouchDistance = 0.00f;
        /// <summary>
        /// Distance, in meters, to position a local xy plane used to verify if a touch interaction started in the front of the scroll view.
        /// </summary>
        public float FrontTouchDistance
        {
            get { return frontTouchDistance; }
            set { frontTouchDistance = value; }
        }

        [SerializeField]
        [Tooltip("The direction in which content should scroll.")]
        private ScrollDirectionType scrollDirection;

        /// <summary>
        /// The direction in which content should scroll.
        /// </summary>
        public ScrollDirectionType ScrollDirection
        {
            get { return scrollDirection; }
            set { scrollDirection = value; }
        }

        [SerializeField]
        [Tooltip("Toggles whether the scrollingObjectCollection will use the Camera OnPreRender event to manage content visibility.")]
        private bool useOnPreRender;

        /// <summary>
        /// Toggles whether Camera OnPreRender callback will be used to manage content visibility.
        /// The fallback is MonoBehaviour.LateUpdate().
        /// </summary>
        /// <remarks>
        /// This is especially helpful if you're trying to scroll dynamically created objects that may be added to the list after LateUpdate,
        /// </remarks>
        public bool UseOnPreRender
        {
            get { return useOnPreRender; }
            set
            {
                if (useOnPreRender == value) { return; }

                if (cameraMethods == null)
                {
                    cameraMethods = CameraCache.Main.gameObject.EnsureComponent<CameraEventRouter>();
                }

                ClipBox.UseOnPreRender = true;

                if (value)
                {
                    cameraMethods.OnCameraPreRender += OnCameraPreRender;
                }
                else
                {
                    cameraMethods.OnCameraPreRender -= OnCameraPreRender;
                }

                useOnPreRender = value;

                ClipBox.UseOnPreRender = useOnPreRender;
            }
        }

        [SerializeField]
        [Tooltip("Amount of (extra) velocity to be applied to scroller")]
        [Range(0.0f, 0.02f)]
        private float velocityMultiplier = 0.008f;

        /// <summary>
        /// Amount of (extra) velocity to be applied to scroller.
        /// </summary>
        /// <remarks>Helpful if you want a small movement to fling the list.</remarks>
        public float VelocityMultiplier
        {
            get { return velocityMultiplier; }
            set { velocityMultiplier = value; }
        }

        [SerializeField]
        [Tooltip("Amount of falloff applied to velocity")]
        [Range(0.0001f, 0.9999f)]
        private float velocityDampen = 0.90f;

        /// <summary>
        /// Amount of drag applied to velocity.
        /// </summary>
        /// <remarks>This can't be 0.0f since that won't allow ANY velocity - set <see cref="TypeOfVelocity"/> to <see cref="VelocityType.None"/>. It can't be 1.0f since that won't allow ANY drag.</remarks>
        public float VelocityDampen
        {
            get { return velocityDampen; }
            set { velocityDampen = value; }
        }

        [SerializeField]
        [Tooltip("The desired type of velocity for the scroller.")]
        private VelocityType typeOfVelocity;

        /// <summary>
        /// The desired type of velocity for the scroller.
        /// </summary>
        public VelocityType TypeOfVelocity
        {
            get { return typeOfVelocity; }
            set { typeOfVelocity = value; }
        }

        [SerializeField]
        [Tooltip("Animation curve for pagination.")]
        private AnimationCurve paginationCurve = new AnimationCurve(
                                                                    new Keyframe(0, 0),
                                                                    new Keyframe(1, 1));
        /// <summary>
        /// Animation curve used to interpolate the pagination and movement methods.
        /// </summary>
        public AnimationCurve PaginationCurve
        {
            get { return paginationCurve; }
            set { paginationCurve = value; }
        }

        [SerializeField]
        [Tooltip("The amount of time (in seconds) the PaginationCurve will take to evaluate.")]
        private float animationLength = 0.25f;

        /// <summary>
        /// The amount of time (in seconds) the <see cref="PaginationCurve"/> will take to evaluate.
        /// </summary>
        public float AnimationLength
        {
            get { return (animationLength < 0) ? 0 : animationLength; }
            set { animationLength = value; }
        }

        [Tooltip("Number of cells in a row on up-down scroll view or number of cells in a column on left-right scroll view.")]
        [SerializeField]
        [FormerlySerializedAs("tiers")]
        [Min(1)]
        private int cellsPerTier = 1;

        /// <summary>
        /// Number of cells in a row on up-down scroll or number of cells in a column on left-right scroll.
        /// </summary>
        public int CellsPerTier
        {
            get
            {
                return (int)(this._node.parasitifer.transform.lossyScale.x / this.config.cellWidth);
                // return cellsPerTier;
            }
            set
            {
                Debug.Assert(value > 0, "Cells per tier should have a positive non zero value");
                cellsPerTier = Mathf.Max(1, value);
            }
        }

        [SerializeField]
        [Tooltip("Number of visible tiers in the scrolling area.")]
        [FormerlySerializedAs("viewableArea")]
        [Min(1)]
        private int tiersPerPage = 2;

        /// <summary>
        /// Number of visible tiers in the scrolling area.
        /// </summary>
        public int TiersPerPage
        {
            get
            {
                return (int)(this._node.parasitifer.transform.lossyScale.y / this.config.cellHeight);
                // return tiersPerPage;
            }
            set
            {
                Debug.Assert(value > 0, "Tiers per page should have a positive non zero value");
                tiersPerPage = Mathf.Max(1, value);
            }
        }

        [Tooltip("Width of the pagination cell.")]
        [SerializeField]
        [Min(0.001f)]
        private float cellWidth = 0.25f;

        /// <summary>
        /// Width of the pagination cell.
        /// </summary>
        public float CellWidth
        {
            get
            {
                return cellWidth;
            }
            set
            {
                Debug.Assert(value > 0, "Cell width should have a positive non zero value");
                cellWidth = Mathf.Max(0.001f, value);
            }
        }

        [Tooltip("Height of the pagination cell.")]
        [SerializeField]
        [Min(0.001f)]
        private float cellHeight = 0.25f;

        /// <summary>
        /// Height of the pagination cell.Hhide
        /// </summary>
        public float CellHeight
        {
            get
            {
                return cellHeight;
            }
            set
            {
                Debug.Assert(cellHeight > 0, "Cell height should have a positive non zero value");
                cellHeight = Mathf.Max(0.001f, value);
            }
        }

        [Tooltip("Depth of cell used for masking out content renderers that are out of bounds.")]
        [SerializeField]
        [Min(0.001f)]
        private float cellDepth = 0.25f;

        /// <summary>
        /// Depth of cell used for masking out content renderers that are out of bounds.
        /// </summary>
        public float CellDepth
        {
            get
            {
                return cellDepth;
            }
            set
            {
                Debug.Assert(value > 0, "Cell depth should have a positive non zero value");
                cellDepth = Mathf.Max(0.001f, value);
            }
        }

        [SerializeField]
        [Tooltip("Multiplier to add more bounce to the overscroll of a list when using VelocityType.FalloffPerFrame or VelocityType.FalloffPerItem.")]
        private float bounceMultiplier = 0.1f;

        /// <summary>
        /// Multiplier to add more bounce to the overscroll of a list when using <see cref="VelocityType.FalloffPerFrame"/> or <see cref="VelocityType.FalloffPerItem"/>.
        /// </summary>
        public float BounceMultiplier
        {
            get { return bounceMultiplier; }
            set { bounceMultiplier = value; }
        }

        // Lerping time interval used for smoothing between positions during scroll drag. Number was empirically defined.
        private const float DragLerpInterval = 0.5f;

        // Lerping time interval used for smoothing between positions during scroll drag passed max and min scroll positions. Number was empirically defined.
        private const float OverDampLerpInterval = 0.9f;

        // Lerping time interval used for smoothing between positions during bouncing. Number was empirically defined.
        private const float BounceLerpInterval = 0.2f;

        /// <summary>
        /// The UnityEvent type the PanelScrollingCollection sends.
        /// GameObject is the object the fired the scroll.
        /// </summary>
        [System.Serializable]
        public class ScrollEvent : UnityEvent<GameObject> { }

        /// <summary>
        /// Event that is fired on the target object when the PanelScrollingCollection deems event as a Click.
        /// </summary>
        [Tooltip("Event that is fired on the target object when the PanelScrollingCollection deems event as a Click.")]
        public ScrollEvent OnClick = new ScrollEvent();

        /// <summary>
        /// Event that is fired on the target object when the PanelScrollingCollection is touched.
        /// </summary>
        [Tooltip("Event that is fired on the target object when the PanelScrollingCollection is touched.")]
        public ScrollEvent OnTouchStarted = new ScrollEvent();

        /// <summary>
        /// Event that is fired on the target object when the PanelScrollingCollection is no longer touched.
        /// </summary>
        [Tooltip("Event that is fired on the target object when the PanelScrollingCollection is no longer touched.")]
        public ScrollEvent OnTouchEnded = new ScrollEvent();

        /// <summary>
        /// Event that is fired on the target object when the PanelScrollingCollection is no longer in motion from velocity
        /// </summary>
        [Tooltip("Event that is fired on the target object when the PanelScrollingCollection is no longer in motion from velocity.")]
        public UnityEvent OnMomentumEnded = new UnityEvent();

        /// <summary>
        /// Event that is fired on the target object when the PanelScrollingCollection is starting motion with velocity.
        /// </summary>
        [Tooltip("Event that is fired on the target object when the PanelScrollingCollection is starting motion with velocity.")]
        public UnityEvent OnMomentumStarted = new UnityEvent();

        [SerializeField]
        [HideInInspector]
        private CameraEventRouter cameraMethods;

        Dictionary<EMR.Entity.PanelNode, float> ContainerYDictionary = new Dictionary<EMR.Entity.PanelNode, float>();
        // Maximum amount the scroller can travel (vertically)
        private float MaxY
        {
            get
            {
                var panelRoot = this._node.panelRoot;
                if (panelRoot == null)
                {
                    return 0f;
                }

                // 将坐标系调整至正交状态
                var originalRotation = panelRoot.parasitifer.transform.eulerAngles;
                panelRoot.parasitifer.transform.eulerAngles = new Vector3(0f, 0f, 0f);

                // 每次获取MaxY之前先记录下所有父滚动节点(包括自身)的Container的Y轴坐标，然后将其设定为初值0，以便计算结束后将其恢复
                EMR.Entity.PanelNode temp = this._node;
                var scrollNodeList = new List<EMR.Entity.PanelNode>();
                while (temp != null)
                {
                    if (temp.scrollComponent != null)
                    {
                        ContainerYDictionary.Add(temp, temp.scrollComponent.scrollCollection.scrollContainer.transform.localPosition.y);
                        scrollNodeList.Add(temp);

                        var localPosition = temp.scrollComponent.scrollCollection.scrollContainer.transform.localPosition;
                        temp.scrollComponent.scrollCollection.scrollContainer.transform.localPosition = new Vector3(localPosition.x, 0f, localPosition.z);
                    }

                    temp = temp.parent != null && temp.parent is EMR.Entity.PanelNode ? (EMR.Entity.PanelNode)temp.parent : null;
                }

                // 计算MaxY
                var max = (contentBounds == null || contentBounds.size.y <= 0) ? 0 : Mathf.Max(0, contentBounds.size.y - 1);
                if (maskEditMode == EditMode.Auto)
                {
                    // Making it a multiple of cell height
                    max = Mathf.Round(SafeDivisionFloat(max, CellHeight)) * CellHeight;
                }

                // 计算补尝值 (轴的溢出部分)
                var boundsCenter = this._node.parasitifer.transform.InverseTransformPoint(this.contentBounds.center);
                var compensate = (boundsCenter.y + this.contentBounds.size.y / 2) - 0.5f;

                // 给MaxY加上补尝值
                if (EMR.Common.Utils.noUnder(compensate, 0f))
                {
                    max = max - compensate;
                }

                // 恢复Container的Y轴坐标
                for (int i = scrollNodeList.Count - 1; i > -1; i--)
                {
                    var containerY = ContainerYDictionary[scrollNodeList[i]];
                    var localPosition = scrollNodeList[i].scrollComponent.scrollCollection.scrollContainer.transform.localPosition;
                    scrollNodeList[i].scrollComponent.scrollCollection.scrollContainer.transform.localPosition = new Vector3(localPosition.x, containerY, localPosition.z);
                }

                // 清空ContainerY暂存
                ContainerYDictionary.Clear();

                panelRoot.parasitifer.transform.eulerAngles = originalRotation;

                return max;
            }
        }

        // Minimum amount the scroller can travel (vertically) - this will always be zero. Here for readability
        private float _minY = 0.0f;

        private float minY
        {
            get
            {
                var panelRoot = this._node.panelRoot;
                if (panelRoot == null)
                {
                    return 0f;
                }

                // 将坐标系调整至正交状态
                var originalRotation = panelRoot.parasitifer.transform.eulerAngles;
                panelRoot.parasitifer.transform.eulerAngles = new Vector3(0f, 0f, 0f);

                // 每次获取MaxY之前先记录下所有父滚动节点(包括自身)的Container的Y轴坐标，然后将其设定为初值0，以便计算结束后将其恢复
                EMR.Entity.PanelNode temp = this._node;
                var scrollNodeList = new List<EMR.Entity.PanelNode>();
                while (temp != null)
                {
                    if (temp.scrollComponent != null)
                    {
                        ContainerYDictionary.Add(temp, temp.scrollComponent.scrollCollection.scrollContainer.transform.localPosition.y);
                        scrollNodeList.Add(temp);

                        var localPosition = temp.scrollComponent.scrollCollection.scrollContainer.transform.localPosition;
                        temp.scrollComponent.scrollCollection.scrollContainer.transform.localPosition = new Vector3(localPosition.x, 0f, localPosition.z);
                    }

                    temp = temp.parent != null && temp.parent is EMR.Entity.PanelNode ? (EMR.Entity.PanelNode)temp.parent : null;
                }

                // 计算补尝值 (轴的溢出部分)
                var boundsCenter = this._node.parasitifer.transform.InverseTransformPoint(this.contentBounds.center);
                var compensate = (boundsCenter.y + this.contentBounds.size.y / 2) - 0.5f;

                // 给minY加上补尝值
                if (EMR.Common.Utils.noUnder(compensate, 0f))
                {
                    this._minY = -compensate;
                }

                // 恢复Container的Y轴坐标
                for (int i = scrollNodeList.Count - 1; i > -1; i--)
                {
                    var containerY = ContainerYDictionary[scrollNodeList[i]];
                    var localPosition = scrollNodeList[i].scrollComponent.scrollCollection.scrollContainer.transform.localPosition;
                    scrollNodeList[i].scrollComponent.scrollCollection.scrollContainer.transform.localPosition = new Vector3(localPosition.x, containerY, localPosition.z);
                }

                // 清空ContainerY暂存
                ContainerYDictionary.Clear();

                panelRoot.parasitifer.transform.eulerAngles = originalRotation;

                return _minY;
            }
        }

        Dictionary<EMR.Entity.PanelNode, float> ContainerXDictionary = new Dictionary<EMR.Entity.PanelNode, float>();
        // Maximum amount the scroller can travel (horizontally) - this will always be zero. Here for readability
        private float _maxX = 0.0f;
        private float maxX
        {
            get
            {
                var panelRoot = this._node.panelRoot;
                if (panelRoot == null)
                {
                    return 0f;
                }

                // 将坐标系调整至正交状态
                var originalRotation = panelRoot.parasitifer.transform.eulerAngles;
                panelRoot.parasitifer.transform.eulerAngles = new Vector3(0f, 0f, 0f);

                // 每次获取MaxX之前先记录下所有父滚动节点(包括自身)的Container的Y轴坐标，然后将其设定为初值0，以便计算结束后将其恢复
                EMR.Entity.PanelNode temp = this._node;
                var scrollNodeList = new List<EMR.Entity.PanelNode>();
                while (temp != null)
                {
                    if (temp.scrollComponent != null)
                    {
                        ContainerXDictionary.Add(temp, temp.scrollComponent.scrollCollection.scrollContainer.transform.localPosition.x);
                        scrollNodeList.Add(temp);

                        var localPosition = temp.scrollComponent.scrollCollection.scrollContainer.transform.localPosition;
                        temp.scrollComponent.scrollCollection.scrollContainer.transform.localPosition = new Vector3(0f, localPosition.y, localPosition.z);
                    }

                    temp = temp.parent != null && temp.parent is EMR.Entity.PanelNode ? (EMR.Entity.PanelNode)temp.parent : null;
                }

                // 计算补尝值 (轴的溢出部分)
                var boundsCenter = this._node.parasitifer.transform.InverseTransformPoint(this.contentBounds.center);
                var compensate = (boundsCenter.x - this.contentBounds.size.x / 2) + 0.5f;

                // 给MaxX加上补尝值
                if (EMR.Common.Utils.noExceed(compensate, 0f))
                {
                    this._maxX = -compensate;
                }

                // 恢复Container的Y轴坐标
                panelRoot.parasitifer.transform.eulerAngles = originalRotation;
                for (int i = scrollNodeList.Count - 1; i > -1; i--)
                {
                    var containerX = ContainerXDictionary[scrollNodeList[i]];
                    var localPosition = scrollNodeList[i].scrollComponent.scrollCollection.scrollContainer.transform.localPosition;
                    scrollNodeList[i].scrollComponent.scrollCollection.scrollContainer.transform.localPosition = new Vector3(containerX, localPosition.y, localPosition.z);
                }

                // 清空ContainerY暂存
                ContainerXDictionary.Clear();

                panelRoot.parasitifer.transform.eulerAngles = originalRotation;

                return _maxX;
            }
        }

        public EMR.Plugin.ScrollConfig config;

        public bool isBoth = false;
        public bool isBothScrollReady = false;

        public EMR.Entity.PanelNode _node = null;

        // Minimum amount the scroller can travel (horizontally)
        private float MinX
        {
            get
            {
                var panelRoot = this._node.panelRoot;
                if (panelRoot == null)
                {
                    return 0f;
                }

                // 将坐标系调整至正交状态
                var originalRotation = panelRoot.parasitifer.transform.eulerAngles;
                panelRoot.parasitifer.transform.eulerAngles = new Vector3(0f, 0f, 0f);

                // 每次获取MaxX之前先记录下所有父滚动节点(包括自身)的Container的Y轴坐标，然后将其设定为初值0，以便计算结束后将其恢复
                EMR.Entity.PanelNode temp = this._node;
                var scrollNodeList = new List<EMR.Entity.PanelNode>();
                while (temp != null)
                {
                    if (temp.scrollComponent != null)
                    {
                        ContainerXDictionary.Add(temp, temp.scrollComponent.scrollCollection.scrollContainer.transform.localPosition.x);
                        scrollNodeList.Add(temp);

                        var localPosition = temp.scrollComponent.scrollCollection.scrollContainer.transform.localPosition;
                        temp.scrollComponent.scrollCollection.scrollContainer.transform.localPosition = new Vector3(0f, localPosition.y, localPosition.z);
                    }

                    temp = temp.parent != null && temp.parent is EMR.Entity.PanelNode ? (EMR.Entity.PanelNode)temp.parent : null;
                }

                // 计算MinX
                var max = (contentBounds == null || contentBounds.size.x <= 0) ? 0 : Mathf.Max(0, contentBounds.size.x - 1);
                if (maskEditMode == EditMode.Auto)
                {
                    // Making it a multiple of cell width
                    max = Mathf.Round(SafeDivisionFloat(max, CellWidth)) * CellWidth;
                }

                // 计算补尝值 (轴的溢出部分)
                var boundsCenter = this._node.parasitifer.transform.InverseTransformPoint(this.contentBounds.center);
                var compensate = (boundsCenter.x - this.contentBounds.size.x / 2) + 0.5f;

                // 给MinX加上补尝值
                if (EMR.Common.Utils.noExceed(compensate, 0f))
                {
                    max = max + compensate;
                }

                // 恢复Container的Y轴坐标
                panelRoot.parasitifer.transform.eulerAngles = originalRotation;
                for (int i = scrollNodeList.Count - 1; i > -1; i--)
                {
                    var containerX = ContainerXDictionary[scrollNodeList[i]];
                    var localPosition = scrollNodeList[i].scrollComponent.scrollCollection.scrollContainer.transform.localPosition;
                    scrollNodeList[i].scrollComponent.scrollCollection.scrollContainer.transform.localPosition = new Vector3(containerX, localPosition.y, localPosition.z);
                }

                // 清空ContainerY暂存
                ContainerXDictionary.Clear();

                panelRoot.parasitifer.transform.eulerAngles = originalRotation;

                return max * -1.0f;
            }
        }

        // Bounds that wrap all scroll container content. Used for calculating MinX and MaxY.
        private Bounds contentBounds;

        /// <summary>
        /// Index of the first visible cell.
        /// </summary>
        public int FirstVisibleCellIndex
        {
            get
            {
                if (scrollDirection == ScrollDirectionType.UpAndDown)
                {
                    return (int)Mathf.Ceil(ScrollContainer.transform.localPosition.y / CellHeight) * CellsPerTier;
                }
                else
                {
                    // Scroll container most to the right local position has x component equals to zero. This value goes negative as scroll container moves to the left. 
                    return ((int)Mathf.Ceil(Mathf.Abs(ScrollContainer.transform.localPosition.x / CellWidth)) * CellsPerTier);
                }
            }
        }

        /// <summary>
        /// Index of the first hidden cell.
        /// </summary>
        public int FirstHiddenCellIndex
        {
            get
            {
                if (scrollDirection == ScrollDirectionType.UpAndDown)
                {
                    return ((int)Mathf.Floor(ScrollContainer.transform.localPosition.y / CellHeight) * CellsPerTier) + (TiersPerPage * CellsPerTier);
                }
                else
                {
                    return ((int)Mathf.Floor(-ScrollContainer.transform.localPosition.x / CellWidth) * CellsPerTier) + (TiersPerPage * CellsPerTier);
                }
            }
        }

        private BoxCollider scrollingCollider;
        /// <summary>
        /// Scrolling interaction collider used to catch pointer and touch events on empty spaces.
        /// </summary>
        public BoxCollider ScrollingCollider
        {
            get
            {
                if (scrollingCollider == null)
                {
                    scrollingCollider = gameObject.EnsureComponent<BoxCollider>();
                }

                return scrollingCollider;
            }
        }

        // Depth of the scrolling interaction collider. Used for defining a plane depth if 'Auto' collider edit mode is selected.
        private const float ScrollingColliderDepth = 0.001f;

        private NearInteractionTouchable scrollingTouchable;
        /// <summary>
        /// Scrolling interaction touchable used to catch touch events on empty spaces.
        /// </summary>
        public NearInteractionTouchable ScrollingTouchable
        {
            get
            {
                if (scrollingTouchable == null)
                {
                    scrollingTouchable = gameObject.EnsureComponent<NearInteractionTouchable>();
                }

                return scrollingTouchable;
            }
        }

        /// <summary>
        /// The local position of the moving scroll container. Can be used to represent the container drag displacement.
        /// </summary>
        public Vector3 ScrollContainerPosition => ScrollContainer.transform.localPosition;

        // The empty game object that contains our nodes and be scrolled
        [SerializeField]
        [HideInInspector]
        private GameObject scrollContainer;

        public GameObject ScrollContainer
        {
            get
            {
                if (scrollContainer == null)
                {
                    Transform oldContainer = transform.Find("Container");

                    if (oldContainer != null)
                    {
                        scrollContainer = oldContainer.gameObject;
                        Debug.LogWarning(name + " PanelScrollingCollection found an existing Container object, using it for the list");
                    }
                    else
                    {
                        scrollContainer = new GameObject();
                        scrollContainer.name = "Container";
                        scrollContainer.transform.parent = transform;
                        scrollContainer.transform.localPosition = Vector3.zero;
                        scrollContainer.transform.localRotation = Quaternion.identity;
                        scrollContainer.transform.localScale = new Vector3(1f, 1f, 1f);
                    }
                }

                return scrollContainer;
            }
        }

        // The empty game object that contains the ClipppingBox
        [SerializeField]
        [HideInInspector]
        private GameObject clippingObject;

        /// <summary>
        /// The empty GameObject containing the PanelScrollingCollection's <see cref="Microsoft.MixedReality.Toolkit.Utilities.ClippingBox"/>.
        /// </summary>
        public GameObject ClippingObject
        {
            get
            {
                if (clippingObject == null)
                {
                    Transform oldClippingObj = transform.Find("Clipping Bounds");

                    if (oldClippingObj != null)
                    {
                        clippingObject = oldClippingObj.gameObject;
                        Debug.LogWarning(name + " PanelScrollingCollection found an existing Clipping object, using it for the list");
                    }
                    else
                    {
                        clippingObject = new GameObject();
                    }

                    clippingObject.name = "Clipping Bounds";
                    clippingObject.transform.parent = transform;
                    clippingObject.transform.localRotation = Quaternion.identity;
                    clippingObject.transform.localPosition = Vector3.zero;
                }

                return clippingObject;
            }
        }

        [SerializeField]
        [HideInInspector]
        private ClippingBox clipBox;

        /// <summary>
        /// The PanelScrollingCollection's <see cref="Microsoft.MixedReality.Toolkit.Utilities.ClippingBox"/> 
        /// that is used for clipping items in and out of the list.
        /// </summary>
        public ClippingBox ClipBox
        {
            get
            {
                if (clipBox == null)
                {
                    clipBox = ClippingObject.EnsureComponent<ClippingBox>();
                    clipBox.ClippingSide = ClippingPrimitive.Side.Outside;
                }

                return clipBox;
            }
        }

        // This collider will be used for checking intersection of the scroll visible area with any content collider or renderer bounds.
        private Collider clippingBoundsCollider;
        private Collider ClippingBoundsCollider
        {
            get
            {
                if (clippingBoundsCollider == null)
                {
                    clippingBoundsCollider = ClippingObject.EnsureComponent<BoxCollider>();
                    clippingBoundsCollider.enabled = false;
                }

                return clippingBoundsCollider;
            }
        }

        // Ratio that defines the outer clipping bounds size relative to the actual clipping bounds.
        // The outer clipping bounds is used for ensuring that content collider that are mostly visible can still stay interactable.
        private readonly float contentVisibilityThresholdRatio = 1.025f;

        private bool oldIsTargetPositionLockedOnFocusLock;

        #region scroll state variables

        /// <summary>
        /// Tracks whether content or scroll background is being interacted with.
        /// </summary>
        public bool IsEngaged { get; private set; } = false;

        /// <summary>
        /// Tracks whether the scroll is being dragged due to a controller movement. 
        /// </summary>
        public bool IsDragging { get; private set; } = false;

        /// <summary>
        /// Tracks whether the scroll content or background is touched by a near pointer.
        /// Remains true while the same near pointer does not cross the scrolling release boundaries.
        /// </summary>
        public bool IsTouched { get; private set; } = false;

        /// <summary>
        /// Tracks whether the scroll has any kind of momentum.
        /// True if scroll is being dragged by a controller, the velocity is falling off after a drag release or during pagination movement.
        /// </summary>
        public bool HasMomentum { get; private set; } = false;

        // The position of the scollContainer before we do any updating to it
        private Vector3 initialScrollerPos;

        // The new of the scollContainer before we've set the position / finished the updateloop
        private Vector3 workingScrollerPos;

        // A list of content renderers that need to be added to the clippingBox
        private List<Renderer> renderersToClip = new List<Renderer>();

        // A list of content renderers that need to be removed from the clippingBox
        private List<Renderer> renderersToUnclip = new List<Renderer>();

        private IMixedRealityPointer currentPointer;

        // The initial focused object from scroll content. This may not always be currentPointer.Result.CurrentPointerTarget
        private GameObject initialFocusedObject;

        #endregion scroll state variables

        #region drag position calculation variables

        // Hand position when starting a motion
        private Vector3 initialPointerPos;

        // Hand position previous frame
        private Vector3 lastPointerPos;

        #endregion drag position calculation variables

        #region velocity calculation variables

        // Simple velocity of the scroller: current - last / timeDelta
        private float scrollVelocity = 0.0f;

        // Filtered weight of scroll velocity
        private float avgVelocity = 0.0f;

        // How much we should filter the velocity - yes this is a magic number. Its been tuned so lets leave it.
        private readonly float velocityFilterWeight = 0.97f;

        // Simple state enum to handle velocity falloff logic
        private enum VelocityState
        {
            None = 0,
            Resolving,
            Calculating,
            Bouncing,
            Dragging,
            Animating,
        }

        // Internal enum for tracking the velocity state of the list
        private VelocityState currentVelocityState;

        private VelocityState CurrentVelocityState
        {
            get => currentVelocityState;

            set
            {
                if (value != currentVelocityState)
                {
                    if (value == VelocityState.None)
                    {
                        OnMomentumEnded.Invoke();
                    }
                    else if (currentVelocityState == VelocityState.None)
                    {
                        OnMomentumStarted.Invoke();
                    }
                    previousVelocityState = currentVelocityState;
                    currentVelocityState = value;
                }
            }
        }

        private VelocityState previousVelocityState;

        // Pre calculated destination with velocity and falloff when using per item snapping
        private Vector3 velocityDestinationPos;

        // Velocity container for storing previous filtered velocity
        private float velocitySnapshot;

        #endregion velocity calculation variables

        // The Animation CoRoutine
        private IEnumerator animateScroller;

        /// <summary>
        /// Scroll pagination modes.
        /// </summary>
        public enum PaginationMode
        {
            ByTier = 0, // By number of tiers
            ByPage, // By number of pages
            ToCellIndex // To selected cell
        }

        #region performance variables
        [SerializeField]
        [Tooltip("Disables Gameobjects with Renderer components which are clipped by the clipping box.")]
        private bool disableClippedGameObjects = true;

        /// <summary>
        /// Disables GameObjects with Renderer components which are clipped by the clipping box.
        /// Improves performance significantly by reducing the number of GameObjects that need to be managed in engine.
        /// </summary>
        public bool DisableClippedGameObjects
        {
            get { return disableClippedGameObjects; }
            set { disableClippedGameObjects = value; }
        }

        [SerializeField]
        [Tooltip("Disables the Renderer components of Gameobjects which are clipped by the clipping box.")]
        private bool disableClippedRenderers = false;

        /// <summary>
        /// Disables the Renderer components of Gameobjects which are clipped by the clipping box.
        /// Improves performance by reducing the number of renderers that need to be tracked, while still allowing the
        /// GameObjects associated with those renders to continue updating. Less performant compared to using DisableClippedGameObjects
        /// </summary>
        public bool DisableClippedRenderers
        {
            get { return disableClippedRenderers; }
            set { disableClippedRenderers = value; }
        }

        #endregion performance variables


        /// <summary>
        /// calculate content bound
        /// </summary>
        /// <param name="current"></param>
        /// <param name="bounds"></param>
        private void computeContentBounds(GameObject current, ref Bounds bounds)
        {
            for (var i = 0; i < current.transform.childCount; i++)
            {
                var child = current.transform.GetChild(i).gameObject;

                
                var childrenRenderers = child.GetComponents<Renderer>();
                if (childrenRenderers.Length > 0)
                {
                    foreach (var renderer in childrenRenderers)
                    {
                        bounds.Encapsulate(renderer.bounds);
                    }
                }
               

                EMR.Entity.PanelNode testNode = null;
                if (child.transform.name == "EmptyScroll")
                {
                    var node = EMR.Space.getNodeByParasitifer(child.transform.parent.gameObject);
                    if(node != null)
                    {
                        testNode = (EMR.Entity.PanelNode)node;
                    }
                }

                if (child.transform.name == "EmptyScroll" && testNode.overflow == "visible" || child.transform.name != "EmptyScroll")
                {
                    computeContentBounds(child, ref bounds);
                }
            }
        }


        Dictionary<EMR.Entity.PanelNode, Vector3> containerCoordDictionary = new Dictionary<EMR.Entity.PanelNode, Vector3>();
        /// <summary>
        /// update content bounds
        /// </summary>
        private void UpdateContentBounds()
        {
            var panelRoot = this._node.panelRoot;
            if (panelRoot == null)
            {
                return;
            }

            // 将坐标系调整为正交状态
            var originalRotation = panelRoot.parasitifer.transform.eulerAngles;
            panelRoot.parasitifer.transform.eulerAngles = new Vector3(0f, 0f, 0f);

            // 每次计算bounds之前先记录下所有父滚动节点(包括自身)的Container的Y轴坐标，然后将其设定为初值0，以便计算结束后将其恢复
            EMR.Entity.PanelNode temp = this._node;
            var scrollNodeList = new List<EMR.Entity.PanelNode>();
            while (temp != null)
            {
                if (temp.scrollComponent != null)
                {
                    containerCoordDictionary.Add(temp, new Vector3(temp.scrollComponent.scrollCollection.scrollContainer.transform.localPosition.x, temp.scrollComponent.scrollCollection.scrollContainer.transform.localPosition.y, temp.scrollComponent.scrollCollection.scrollContainer.transform.localPosition.z));
                    scrollNodeList.Add(temp);

                    var localPosition = temp.scrollComponent.scrollCollection.scrollContainer.transform.localPosition;
                    temp.scrollComponent.scrollCollection.scrollContainer.transform.localPosition = new Vector3(0f, 0f, localPosition.z);
                }

                temp = temp.parent != null && temp.parent is EMR.Entity.PanelNode ? (EMR.Entity.PanelNode)temp.parent : null;
            }

            ScrollContainer.transform.localPosition = new Vector3(0f, 0f, 0f);
            contentBounds = new Bounds
            {
                size = transform.lossyScale,
                center = transform.position
            };

            computeContentBounds(transform.gameObject, ref contentBounds);

            Vector3 localSize;

            localSize.y = SafeDivisionFloat(contentBounds.size.y, transform.parent.lossyScale.y);
            localSize.x = SafeDivisionFloat(contentBounds.size.x, transform.parent.lossyScale.x);
            localSize.z = SafeDivisionFloat(contentBounds.size.z, transform.parent.lossyScale.z);

            contentBounds.size = localSize;

            // 恢复Container的坐标
            for (int i = scrollNodeList.Count - 1; i > -1; i--)
            {
                var containerCoord = containerCoordDictionary[scrollNodeList[i]];
                scrollNodeList[i].scrollComponent.scrollCollection.scrollContainer.transform.localPosition = containerCoord;
            }

            // 清空ContainerY暂存
            containerCoordDictionary.Clear();

            panelRoot.parasitifer.transform.eulerAngles = originalRotation;


            var xScale = workingScrollerPos.x / MinX;
            var yScale = workingScrollerPos.y / MaxY;

            if (!float.IsNaN(xScale))
            {
                workingScrollerPos.x = MinX * xScale;
            }
            if (!float.IsNaN(yScale))
            {
                workingScrollerPos.y = MaxY * yScale;
            }
            ApplyPosition(workingScrollerPos);
        }

        

        /// <summary>
        /// /// reset relevant parameters without initialization
        /// for example, when the size changes
        /// </summary>
        /// <param name="isScrollPositionReset"> is scroll position reset </param>
        public void fresh(bool isScrollPositionReset)
        {
            
            this.UpdateContentBounds();
            this.overflowBack();

            // 这个地方加上一个容器抖动，以解决添加子节点后，子滚动组件内容显示不正确
            var origian = new Vector3(this.scrollContainer.transform.localPosition.x, this.scrollContainer.transform.localPosition.y, this.scrollContainer.transform.localPosition.z);
            this.scrollContainer.transform.localPosition = new Vector3(this.scrollContainer.transform.localPosition.x + 0.001f , this.scrollContainer.transform.localPosition.y , this.scrollContainer.transform.localPosition.z);
            this.scrollContainer.transform.localPosition = origian;

            // reset position
            if (isScrollPositionReset)
            {
                workingScrollerPos.x = 0f;
                workingScrollerPos.y = 0f;
                ApplyPosition(workingScrollerPos);
            }
        }


        public BoundOverflowHandler boundOverflowHandler = null;
        /// <summary>
        /// position overflow back
        /// </summary>
        public void overflowBack()
        {
            workingScrollerPos = ScrollContainer.transform.localPosition;

            // 是否边界溢出
            var isBoundOverflow = false;
            if (workingScrollerPos.y > MaxY)
            {
                workingScrollerPos.y = MaxY;
                isBoundOverflow = true;
            }

            if (workingScrollerPos.y < minY )
            {
                workingScrollerPos.y = minY;
                isBoundOverflow = true;
            }

            if (workingScrollerPos.x > maxX )
            {
                workingScrollerPos.x = maxX;
                isBoundOverflow = true;
            }

            if (workingScrollerPos.x < MinX)
            {
             
                workingScrollerPos.x = MinX;
                isBoundOverflow = true;
            }

            ApplyPosition(workingScrollerPos);

            // 当内容超出可滚动范围时，需要对该节点下的所有子节点进行浮动刷新
            if (isBoundOverflow)
            {
                foreach (var item in this._node.children)
                {
                    item.horizontalFresh();
                }
            }
        }


        #region Setup methods

        /// <summary>
        /// Sets up the scroll clipping object and the interactable components according to the scroll content and chosen settings.
        /// </summary>
        public void UpdateContent()
        {
            UpdateContentBounds();
            SetupScrollingInteractionCollider();
            SetupClippingObject();
            ManageVisibility();
        }



        // Setting up the initial transform values for the scrolling interaction collider and near touchable.
        private void SetupScrollingInteractionCollider()
        {
            // Boundaries will be defined by direct manipulation of the scroll interaction components
            if (colliderEditMode == EditMode.Manual)
            {
                return;
            }

            ScrollingCollider.size = this.config.colliderSize;
            ScrollingCollider.center = this.config.colliderCenter;

            ScrollingTouchable.SetBounds(new Vector2(Math.Abs(Vector3.Dot(this.config.colliderSize, ScrollingTouchable.LocalRight)), Math.Abs(Vector3.Dot(this.config.colliderSize, ScrollingTouchable.LocalUp))));
            ScrollingTouchable.SetLocalCenter(new Vector3(this.config.colliderCenter.x, this.config.colliderCenter.y, -EMR.Space.Unit.unitToScaleForGameObject(ScrollContainer, this.config.thickness, EMR.Struct.Axle.forward) / 2));
        }

        /// <summary>
        /// Setting up the initial transform values for the clippingBox.
        /// </summary>
        private void SetupClippingObject()
        {
            // Boundaries will be defined by direct manipulation of the clipping object
            if (maskEditMode == EditMode.Manual)
            {
                return;
            }

            // The bounds of the clipping object, this is to make helper math easier later, it doesn't matter that its AABB since we're really not using it for bounds operations
            Bounds clippingBounds = new Bounds();
            clippingBounds.size = Vector3.one;

            Vector3 viewableCenter = new Vector3();

            // Adjust scale and position of clipping box
            switch (scrollDirection)
            {
                case ScrollDirectionType.UpAndDown:
                default:

                    // Apply the viewable area and column/row multiplier
                    // Use a dummy bounds of one to get the local scale to match;
                    clippingBounds.size = this.config.clippingSize;
                    ClipBox.transform.localScale = new Bounds(Vector3.zero, Vector3.one).GetScaleToMatchBounds(clippingBounds);

                    break;

                case ScrollDirectionType.LeftAndRight:

                    // Same as above for L <-> R
                    clippingBounds.size = this.config.clippingSize;
                    ClipBox.transform.localScale = new Bounds(Vector3.zero, Vector3.one).GetScaleToMatchBounds(clippingBounds);

                    break;
            }

            // Adjust where the center of the clipping box is
            viewableCenter.x = this.config.clippingCenter.x;
            viewableCenter.y = this.config.clippingCenter.y;
            viewableCenter.z = this.config.clippingCenter.z;

            // Apply new values
            ClipBox.transform.localPosition = viewableCenter;
        }

        #endregion Setup methods

        #region MonoBehaviour Implementation

        private void OnEnable()
        {
            // Register for global input events
            //CoreServices.InputSystem?.RegisterHandler<IMixedRealitySourceStateHandler>(this);
            //CoreServices.InputSystem?.RegisterHandler<IMixedRealityTouchHandler>(this);
            //CoreServices.InputSystem?.RegisterHandler<IMixedRealityPointerHandler>(this);

            if (useOnPreRender)
            {
                ClipBox.UseOnPreRender = true;

                // Subscribe to the preRender callback on the main camera so we can intercept it and make sure we catch
                // any dynamically added content
                cameraMethods = CameraCache.Main.gameObject.EnsureComponent<CameraEventRouter>();
                cameraMethods.OnCameraPreRender += OnCameraPreRender;
            }
        }

        private void Start()
        {
            UpdateContent();
        }

        private void Update()
        {
            if (this.isBoth && !this.isBothScrollReady)
            {
                return;
            }

            if (!Application.isPlaying)
            {
                return;
            }

            // Force the scroll container position if no content
            if (ScrollContainer.GetComponentInChildren<Renderer>(true) == null)
            {
                workingScrollerPos = Vector3.zero;
                ApplyPosition(workingScrollerPos);

                return;
            }

            // The scroller has detected input and has a valid pointer
            if (IsEngaged && TryGetPointerPositionOnPlane(out Vector3 currentPointerPos))
            {
                Vector3 handDelta = initialPointerPos - currentPointerPos;
                handDelta = transform.InverseTransformDirection(handDelta);

                if (IsDragging && currentPointer != null) // Changing lock after drag started frame to allow for focus provider to move pointer focus to scroll background before locking
                {
                    currentPointer.IsFocusLocked = true;
                }

                // Lets see if this is gonna be a click or a drag
                // Check the scroller's length state to prevent resetting calculation
                if (!IsDragging)
                {
                    // Grab the delta value we care about
                    float absAxisHandDelta = (scrollDirection == ScrollDirectionType.UpAndDown) ? Mathf.Abs(handDelta.y) : Mathf.Abs(handDelta.x);

                    // Catch an intentional finger in scroller to stop momentum, this isn't a drag its definitely a stop
                    if (absAxisHandDelta > handDeltaScrollThreshold)
                    {
                        scrollVelocity = 0.0f;
                        avgVelocity = 0.0f;

                        IsDragging = true;
                        handDelta = Vector3.zero;

                        CurrentVelocityState = VelocityState.Dragging;

                        // Reset initialHandPos to prevent the scroller from jumping
                        initialScrollerPos = workingScrollerPos = ScrollContainer.transform.localPosition;
                        initialPointerPos = currentPointerPos;
                    }
                }

                if (IsTouched && DetectScrollRelease(currentPointerPos))
                {
                    // We're on the other side of the original touch position. This is a release.
                    if (IsDragging)
                    {
                        // Its a drag release
                        initialScrollerPos = workingScrollerPos;
                        CurrentVelocityState = VelocityState.Calculating;
                    }
                    else
                    {
                        // Its a click release
                        OnClick?.Invoke(initialFocusedObject);
                    }

                    ResetInteraction();
                }
                else if (IsDragging && canScroll)
                {
                    if (scrollDirection == ScrollDirectionType.UpAndDown)
                    {
                        // Lock X, clamp Y
                        float handLocalDelta = SafeDivisionFloat(handDelta.y, transform.lossyScale.y);

                        // Over damp if scroll position out of bounds
                        if (workingScrollerPos.y > MaxY || workingScrollerPos.y < minY)
                        {
                            workingScrollerPos.y = MathUtilities.CLampLerp(initialScrollerPos.y - handLocalDelta, minY, MaxY, OverDampLerpInterval);
                        }
                        else
                        {
                            workingScrollerPos.y = MathUtilities.CLampLerp(initialScrollerPos.y - handLocalDelta, minY, MaxY, DragLerpInterval);
                        }
                        // workingScrollerPos.x = 0.0f;
                    }
                    else
                    {

                        // Lock Y, clamp X
                        float handLocalDelta = SafeDivisionFloat(handDelta.x, transform.lossyScale.x);

                        // Over damp if scroll position out of bounds
                        if (workingScrollerPos.x > maxX || workingScrollerPos.x < MinX)
                        {
                            workingScrollerPos.x = MathUtilities.CLampLerp(initialScrollerPos.x - handLocalDelta, MinX, maxX, OverDampLerpInterval);
                        }
                        else
                        {
                            workingScrollerPos.x = MathUtilities.CLampLerp(initialScrollerPos.x - handLocalDelta, MinX, maxX, DragLerpInterval);
                        }
                        // workingScrollerPos.y = 0.0f;
                    }

                    // Update the scrollContainer Position
                    ApplyPosition(workingScrollerPos);

                    CalculateVelocity();

                    // Update the prev val for velocity
                    lastPointerPos = currentPointerPos;
                }
            }
            else if ((CurrentVelocityState != VelocityState.None
                      || previousVelocityState != VelocityState.None)
                      && CurrentVelocityState != VelocityState.Animating) // Prevent the Animation coroutine from being overridden
            {
                // We're not engaged, so handle any not touching behavior
                HandleVelocityFalloff();
                // Apply our position
                ApplyPosition(workingScrollerPos);
            }

            // Setting HasMomentum to true if scroll velocity state has changed or any movement happened during this update
            if (CurrentVelocityState != VelocityState.None || previousVelocityState != VelocityState.None)
            {
                HasMomentum = true;
            }

            else
            {
                HasMomentum = false;
            }

            previousVelocityState = CurrentVelocityState;
        }

        private void LateUpdate()
        {
            if (!UseOnPreRender)
            {
                ManageVisibility();
            }
        }

        private void OnDisable()
        {
            // Unregister global input events
            CoreServices.InputSystem?.UnregisterHandler<IMixedRealitySourceStateHandler>(this);
            CoreServices.InputSystem?.UnregisterHandler<IMixedRealityTouchHandler>(this);
            CoreServices.InputSystem?.UnregisterHandler<IMixedRealityPointerHandler>(this);

            RestoreContentVisibility();

            if (useOnPreRender && cameraMethods != null)
            {
                CameraEventRouter cameraMethods = CameraCache.Main.gameObject.EnsureComponent<CameraEventRouter>();
                cameraMethods.OnCameraPreRender -= OnCameraPreRender;
            }
        }

        #endregion MonoBehaviour Implementation

        #region private methods

        /// <summary>
        /// When <see cref="UseOnPreRender"/>, the <see cref="PanelScrollingCollection"/> subscribes to the <see cref="CameraEventRouter"/> call back for OnCameraPreRender
        /// </summary>
        /// <param name="router">The active <see cref="CameraEventRouter"/> on the camera.</param>
        private void OnCameraPreRender(CameraEventRouter router)
        {
            ManageVisibility();
        }

        // Add or remove renderers from clipping primitive
        private void ReconcileClippingContent()
        {
            if (renderersToClip.Count > 0)
            {
                AddRenderersToClippingObject(renderersToClip);

                renderersToClip.Clear();
            }

            if (renderersToUnclip.Count > 0)
            {
                RemoveRenderersFromClippingObject(renderersToUnclip);

                renderersToUnclip.Clear();
            }
        }

        /// <summary>
        /// Gets the cursor position (pointer end point) on the scrollable plane,
        /// projected onto the direction being scrolled if far pointer.
        /// Returns false if the pointer is null.
        /// </summary>
        private bool TryGetPointerPositionOnPlane(out Vector3 result)
        {
            result = Vector3.zero;

            if (((MonoBehaviour)currentPointer) == null)
            {
                return false;
            }
            if (currentPointer.GetType() == typeof(PokePointer))
            {
                result = currentPointer.Position;
                return true;
            }

            var scrollVector = (scrollDirection == ScrollDirectionType.UpAndDown) ? transform.up : transform.right;

            result = transform.position + Vector3.Project(currentPointer.Position - transform.position, scrollVector);
            return true;
        }

        /// <summary>
        /// Calculates our <see cref="VelocityType"/> falloff
        /// </summary>
        private void HandleVelocityFalloff()
        {
            switch (typeOfVelocity)
            {
                case VelocityType.FalloffPerFrame:

                    HandleFalloffPerFrame();
                    break;

                case VelocityType.FalloffPerItem:
                default:
                    HandleFalloffPerItem();
                    break;

                case VelocityType.NoVelocitySnapToItem:
                    CurrentVelocityState = VelocityState.None;

                    avgVelocity = 0.0f;

                    // Round to the nearest cell
                    if (scrollDirection == ScrollDirectionType.UpAndDown)
                    {
                        workingScrollerPos.y = Mathf.Round(ScrollContainer.transform.localPosition.y / CellHeight) * CellHeight;
                    }
                    else
                    {
                        workingScrollerPos.x = Mathf.Round(ScrollContainer.transform.localPosition.x / CellWidth) * CellWidth;
                    }

                    initialScrollerPos = workingScrollerPos;
                    break;

                case VelocityType.None:
                    CurrentVelocityState = VelocityState.None;

                    avgVelocity = 0.0f;
                    break;
            }

            if (CurrentVelocityState == VelocityState.None)
            {
                workingScrollerPos.y = Mathf.Clamp(workingScrollerPos.y, minY, MaxY);
                workingScrollerPos.x = Mathf.Clamp(workingScrollerPos.x, MinX, maxX);
            }
        }

        /// <summary>
        /// Handles <see cref="PanelScrollingCollection"/> drag release behavior when <see cref="TypeOfVelocity"/> is set to <see cref="VelocityType.FalloffPerItem"/>
        /// </summary>
        private void HandleFalloffPerItem()
        {
            switch (CurrentVelocityState)
            {
                case VelocityState.Calculating:

                    int numSteps;
                    float newPosAfterVelocity;
                    if (scrollDirection == ScrollDirectionType.UpAndDown)
                    {
                        if (avgVelocity == 0.0f)
                        {
                            // Velocity was cleared out so we should just snap
                            newPosAfterVelocity = ScrollContainer.transform.localPosition.y;
                        }
                        else
                        {
                            // Precalculate where the velocity falloff would land our scrollContainer, then round it to the nearest cell so it feels natural
                            velocitySnapshot = IterateFalloff(avgVelocity, out numSteps);
                            newPosAfterVelocity = initialScrollerPos.y - velocitySnapshot;
                        }

                        velocityDestinationPos.y = (Mathf.Round(newPosAfterVelocity / CellHeight)) * CellHeight;

                        CurrentVelocityState = VelocityState.Resolving;
                    }
                    else
                    {
                        if (avgVelocity == 0.0f)
                        {
                            // Velocity was cleared out so we should just snap
                            newPosAfterVelocity = ScrollContainer.transform.localPosition.x;
                        }
                        else
                        {
                            // Precalculate where the velocity falloff would land our scrollContainer, then round it to the nearest cell so it feels natural
                            velocitySnapshot = IterateFalloff(avgVelocity, out numSteps);
                            newPosAfterVelocity = initialScrollerPos.x + velocitySnapshot;
                        }

                        velocityDestinationPos.x = (Mathf.Round(newPosAfterVelocity / CellWidth)) * CellWidth;

                        CurrentVelocityState = VelocityState.Resolving;
                    }

                    workingScrollerPos = Solver.SmoothTo(scrollContainer.transform.localPosition, velocityDestinationPos, Time.deltaTime, BounceLerpInterval);

                    // Clear the velocity now that we've applied a new position
                    avgVelocity = 0.0f;
                    break;

                case VelocityState.Resolving:

                    if (scrollDirection == ScrollDirectionType.UpAndDown)
                    {
                        if (ScrollContainer.transform.localPosition.y > MaxY
                            || ScrollContainer.transform.localPosition.y < minY)
                        {
                            CurrentVelocityState = VelocityState.Bouncing;
                            velocitySnapshot = 0.0f;
                            break;
                        }
                        else
                        {
                            workingScrollerPos = Solver.SmoothTo(ScrollContainer.transform.localPosition, velocityDestinationPos, Time.deltaTime, BounceLerpInterval);

                            SnapVelocityFinish();
                        }
                    }
                    else
                    {
                        if (ScrollContainer.transform.localPosition.x > maxX + (FrontTouchDistance * bounceMultiplier)
                            || ScrollContainer.transform.localPosition.x < MinX - (FrontTouchDistance * bounceMultiplier))
                        {
                            CurrentVelocityState = VelocityState.Bouncing;
                            velocitySnapshot = 0.0f;
                            break;
                        }
                        else
                        {
                            workingScrollerPos = Solver.SmoothTo(ScrollContainer.transform.localPosition, velocityDestinationPos, Time.deltaTime, BounceLerpInterval);

                            SnapVelocityFinish();
                        }
                    }
                    break;

                case VelocityState.Bouncing:

                    HandleBounceState();
                    break;

                case VelocityState.None:
                default:
                    // clean up our position for next frame
                    initialScrollerPos = workingScrollerPos;
                    break;

            }
        }

        /// <summary>
        /// Handles <see cref="PanelScrollingCollection"/> drag release behavior when <see cref="TypeOfVelocity"/> is set to <see cref="VelocityType.FalloffPerFrame"/>
        /// </summary>
        private void HandleFalloffPerFrame()
        {
            switch (CurrentVelocityState)
            {
                case VelocityState.Calculating:

                    if (scrollDirection == ScrollDirectionType.UpAndDown)
                    {
                        workingScrollerPos.y = initialScrollerPos.y + avgVelocity;
                    }
                    else
                    {
                        workingScrollerPos.x = initialScrollerPos.x + avgVelocity;
                    }

                    CurrentVelocityState = VelocityState.Resolving;

                    // clean up our position for next frame
                    initialScrollerPos = workingScrollerPos;
                    break;

                case VelocityState.Resolving:

                    if (scrollDirection == ScrollDirectionType.UpAndDown)
                    {
                        if (ScrollContainer.transform.localPosition.y > MaxY + (FrontTouchDistance * bounceMultiplier)
                            || ScrollContainer.transform.localPosition.y < minY - (FrontTouchDistance * bounceMultiplier))
                        {
                            CurrentVelocityState = VelocityState.Bouncing;
                            avgVelocity = 0.0f;
                            break;
                        }
                        else
                        {
                            avgVelocity *= velocityDampen;
                            workingScrollerPos.y = initialScrollerPos.y + avgVelocity;

                            SnapVelocityFinish();

                        }
                    }
                    else
                    {
                        if (ScrollContainer.transform.localPosition.x > maxX + (FrontTouchDistance * bounceMultiplier)
                            || ScrollContainer.transform.localPosition.x < MinX - (FrontTouchDistance * bounceMultiplier))
                        {
                            CurrentVelocityState = VelocityState.Bouncing;
                            avgVelocity = 0.0f;
                            break;
                        }
                        else
                        {
                            avgVelocity *= velocityDampen;
                            workingScrollerPos.x = initialScrollerPos.x + avgVelocity;

                            SnapVelocityFinish();
                        }
                    }

                    // clean up our position for next frame
                    initialScrollerPos = workingScrollerPos;

                    break;

                case VelocityState.Bouncing:

                    HandleBounceState();

                    break;
            }
        }

        /// <summary>
        /// Smooths <see cref="ScrollContainer"/>'s position to the proper clamped edge 
        /// while <see cref="CurrentVelocityState"/> is <see cref="VelocityState.Bouncing"/>.
        /// </summary>
        private void HandleBounceState()
        {
            Vector3 clampedDest = new Vector3(Mathf.Clamp(ScrollContainer.transform.localPosition.x, MinX, maxX), Mathf.Clamp(ScrollContainer.transform.localPosition.y, minY, MaxY), 0.0f);
            if ((scrollDirection == ScrollDirectionType.UpAndDown && Mathf.Approximately(ScrollContainer.transform.localPosition.y, clampedDest.y))
                || (scrollDirection == ScrollDirectionType.LeftAndRight && Mathf.Approximately(ScrollContainer.transform.localPosition.x, clampedDest.x)))
            {
                CurrentVelocityState = VelocityState.None;

                // clean up our position for next frame
                initialScrollerPos = workingScrollerPos = clampedDest;
                return;
            }
            workingScrollerPos.y = Solver.SmoothTo(ScrollContainer.transform.localPosition, clampedDest, Time.deltaTime, BounceLerpInterval).y;
            workingScrollerPos.x = Solver.SmoothTo(ScrollContainer.transform.localPosition, clampedDest, Time.deltaTime, BounceLerpInterval).x;
        }

        /// <summary>
        /// Snaps to the final position of the <see cref="ScrollContainer"/> once velocity as resolved.
        /// </summary>
        private void SnapVelocityFinish()
        {
            if (Vector3.Distance(ScrollContainer.transform.localPosition, workingScrollerPos) > Mathf.Epsilon)
            {
                return;
            }

            if (typeOfVelocity == VelocityType.FalloffPerItem)
            {
                if (scrollDirection == ScrollDirectionType.UpAndDown)
                {
                    // Ensure we've actually snapped the position to prevent an extreme in-between state
                    workingScrollerPos.y = (Mathf.Round(ScrollContainer.transform.localPosition.y / CellHeight)) * CellHeight;
                }
                else
                {
                    workingScrollerPos.x = (Mathf.Round(ScrollContainer.transform.localPosition.x / CellWidth)) * CellWidth;
                }
            }

            CurrentVelocityState = VelocityState.None;
            avgVelocity = 0.0f;

            // clean up our position for next frame
            initialScrollerPos = workingScrollerPos;
        }

        /// <summary>
        /// Wrapper for per frame velocity calculation and filtering.
        /// </summary>
        private void CalculateVelocity()
        {
            // Update simple velocity
            TryGetPointerPositionOnPlane(out Vector3 newPos);

            scrollVelocity = (scrollDirection == ScrollDirectionType.UpAndDown)
                             ? (newPos.y - lastPointerPos.y) / Time.deltaTime * velocityMultiplier
                             : (newPos.x - lastPointerPos.x) / Time.deltaTime * velocityMultiplier;

            // And filter it...
            avgVelocity = (avgVelocity * (1.0f - velocityFilterWeight)) + (scrollVelocity * velocityFilterWeight);
        }

        /// <summary>
        /// The Animation Override to position our scroller based on manual movement <see cref="PageBy(int, bool)"/>, <see cref="MoveTo(int, bool)"/>,
        /// </summary>
        /// <param name="initialPos">The start position of the scrollContainer</param>
        /// <param name="finalPos">Where we want the scrollContainer to end up, typically this should be <see cref="workingScrollerPos"/></param>
        /// <param name="curve"><see cref="AnimationCurve"/> representing the easing desired</param>
        /// <param name="time">Time for animation, in seconds</param>
        /// <param name="callback">Optional callback action to be invoked after animation coroutine has finished</param>
        private IEnumerator AnimateTo(Vector3 initialPos, Vector3 finalPos, AnimationCurve curve = null, float? time = null, System.Action callback = null)
        {
            if (curve == null)
            {
                curve = paginationCurve;
            }

            if (time == null)
            {
                time = animationLength;
            }

            float counter = 0.0f;
            while (counter <= time)
            {
                workingScrollerPos = Vector3.Lerp(initialPos, finalPos, curve.Evaluate(counter / (float)time));
                ScrollContainer.transform.localPosition = workingScrollerPos;

                counter += Time.deltaTime;
                yield return null;
            }

            // Update our values so they stick
            if (scrollDirection == ScrollDirectionType.UpAndDown)
            {
                workingScrollerPos.y = initialScrollerPos.y = finalPos.y;
            }
            else
            {
                workingScrollerPos.x = initialScrollerPos.x = finalPos.x;
            }

            if (callback != null)
            {
                callback?.Invoke();
            }

            CurrentVelocityState = VelocityState.None;
            animateScroller = null;
        }

        /// <summary>
        /// Checks if the engaged joint has released the scrollable list
        /// </summary>
        private bool DetectScrollRelease(Vector3 pointerPos)
        {
            Vector3 scrollToPointerVector = pointerPos - ClipBox.transform.position;

            var ScrollingColliderDepth = this._node.isPanel ? 0.01f : ScrollingCollider.transform.lossyScale.z;

            bool isScrollRelease = Vector3.Magnitude(Vector3.Project(scrollToPointerVector, ClipBox.transform.up)) > ClipBox.transform.lossyScale.y / 2 + releaseThresholdTopBottom
                               || Vector3.Magnitude(Vector3.Project(scrollToPointerVector, ClipBox.transform.right)) > ClipBox.transform.lossyScale.x / 2 + releaseThresholdLeftRight

                               || (Vector3.Dot(scrollToPointerVector, transform.forward) > 0 ?
                                       Vector3.Magnitude(Vector3.Project(scrollToPointerVector, ClipBox.transform.forward)) > ScrollingColliderDepth / 2 + releaseThresholdBack :
                                       Vector3.Magnitude(Vector3.Project(scrollToPointerVector, ClipBox.transform.forward)) > ScrollingColliderDepth / 2 + releaseThresholdFront);

            return isScrollRelease;
        }

        private bool HasPassedThroughFrontPlane(PokePointer pokePointer)
        {
            var p = transform.InverseTransformPoint(pokePointer.PreviousPosition);

            return p.z <= -FrontTouchDistance;
        }

        /// <summary>
        /// Adds list of renderers to the ClippingBox
        /// </summary>
        private void AddRenderersToClippingObject(List<Renderer> renderers)
        {
            foreach (var renderer in renderers)
            {
                ClipBox.AddRenderer(renderer);
            }
        }

        /// <summary>
        /// Removes list of renderers from the ClippingBox
        /// </summary>
        private void RemoveRenderersFromClippingObject(List<Renderer> renderers)
        {
            foreach (var renderer in renderers)
            {
                ClipBox.RemoveRenderer(renderer);
            }
        }

        /// <summary>
        /// Removes all renderers currently being clipped by the clipping box
        /// </summary>
        private void ClearClippingBox()
        {
            ClipBox.ClearRenderers();
        }

        /// <summary>
        /// Helper to perform division operations and prevent division by 0.
        /// </summary>
        private static int SafeDivisionInt(int numerator, int denominator)
        {
            return (denominator != 0) ? numerator / denominator : 0;
        }

        private float SafeDivisionFloat(float numerator, float denominator)
        {
            return (denominator != 0) ? numerator / denominator : 0;
        }

        /// <summary>
        /// Checks visibility of scroll content by iterating through all content renderers and colliders.
        /// All inactive content objects and colliders are reactivated during visibility restoration. 
        /// </summary>
        private void ManageVisibility(bool isRestoringVisibility = false)
        {
            if (!MaskEnabled && !isRestoringVisibility)
            {
                return;
            }

            ClippingBoundsCollider.enabled = true;
            Bounds clippingThresholdBounds = ClippingBoundsCollider.bounds;

            Renderer[] contentRenderers = ScrollContainer.GetComponentsInChildren<Renderer>(true);

            List<Renderer> clippedRenderers = ClipBox.GetRenderersCopy().ToList();

            // Remove all renderers from clipping primitive that are not part of scroll content
            foreach (var clippedRenderer in clippedRenderers)
            {
                if (clippedRenderer != null && !clippedRenderer.transform.IsChildOf(ScrollContainer.transform))
                {
                    if (disableClippedGameObjects)
                    {
                        if (!clippedRenderer.gameObject.activeSelf)
                        {
                            clippedRenderer.gameObject.SetActive(true);
                        }
                    }
                    if (disableClippedRenderers)
                    {
                        if (!clippedRenderer.enabled)
                        {
                            clippedRenderer.enabled = true;
                        }
                    }

                    renderersToUnclip.Add(clippedRenderer);
                }
            }

            // Check render visibility
            foreach (var renderer in contentRenderers)
            {
                // All content renderers should be added to clipping primitive
                if (!isRestoringVisibility && MaskEnabled && !clippedRenderers.Contains(renderer))
                {
                    renderersToClip.Add(renderer);
                }

                // Complete or partially visible renders should be clipped and its game object should be active
                if (isRestoringVisibility
                    || clippingThresholdBounds.ContainsBounds(renderer.bounds)
                    || clippingThresholdBounds.Intersects(renderer.bounds))
                {
                    if (disableClippedGameObjects)
                    {
                        if (!renderer.gameObject.activeSelf)
                        {
                            renderer.gameObject.SetActive(true);
                        }
                    }
                    if (disableClippedRenderers)
                    {
                        if (!renderer.enabled)
                        {
                            renderer.enabled = true;
                        }
                    }
                }

                // Hidden renderer game objects should be inactive
                else
                {
                    if (disableClippedGameObjects)
                    {
                        if (renderer.gameObject.activeSelf)
                        {
                            renderer.gameObject.SetActive(false);
                        }
                    }
                    if (disableClippedRenderers)
                    {
                        if (renderer.enabled)
                        {
                            renderer.enabled = false;
                        }
                    }
                }
            }

            // Check collider visibility
            if (Application.isPlaying)
            {
                // Outer clipping bounds is used to ensure collider has minimum visibility to stay enabled
                Bounds outerClippingThresholdBounds = ClippingBoundsCollider.bounds;
                outerClippingThresholdBounds.size *= contentVisibilityThresholdRatio;

                var colliders = ScrollContainer.GetComponentsInChildren<Collider>(true);
                foreach (var collider in colliders)
                {
                    if (collider.transform.name != "EmptyScroll")
                    {
                        // Disabling content colliders during drag to stop interaction even if game object is inactive
                        if (!isRestoringVisibility && IsDragging)
                        {
                            if (collider.enabled)
                            {
                                collider.enabled = false;
                            }

                            continue;
                        }

                        // No need to manage collider visibility in case game object is inactive and no pointer is dragging the scroll
                        if (!isRestoringVisibility && !collider.gameObject.activeSelf)
                        {
                            continue;
                        }

                        // Temporary activating for getting bounds
                        var wasColliderEnabled = collider.enabled;

                        if (!wasColliderEnabled)
                        {
                            // collider.enabled = false;
                            collider.enabled = true;
                        }

                        // Completely or partially visible colliders should be enabled if scroll is not drag engaged
                        if (isRestoringVisibility || outerClippingThresholdBounds.ContainsBounds(collider.bounds))
                        {
                            if (!wasColliderEnabled)
                            {
                                // wasColliderEnabled = false;
                                wasColliderEnabled = true;
                            }
                        }
                        // Hidden colliders should be disabled
                        else
                        {
                            if (wasColliderEnabled)
                            {
                                wasColliderEnabled = false;
                            }
                        }

                        // Update collider state or revert to previous state
                        collider.enabled = wasColliderEnabled;
                    }
                }
            }

            ClippingBoundsCollider.enabled = false;

            if (!isRestoringVisibility)
            {
                ReconcileClippingContent();
            }
        }

        /// <summary>
        /// Precalculates the total amount of travel given the scroller's current average velocity and drag.
        /// </summary>
        /// <param name="steps"><see cref="out"/> Number of steps to get our <see cref="avgVelocity"/> to effectively "zero" (0.00001).</param>
        /// <returns>The total distance the <see cref="avgVelocity"/> with <see cref="velocityDampen"/> as drag would travel.</returns>
        private float IterateFalloff(float vel, out int steps)
        {
            // Some day this should be a falloff formula, below is the number of steps. Just can't figure out how to get the right velocity.
            // float numSteps = (Mathf.Log(0.00001f)  - Mathf.Log(Mathf.Abs(avgVelocity))) / Mathf.Log(velocityFalloff);

            float newVal = 0.0f;
            float v = vel;
            steps = 0;

            while (Mathf.Abs(v) > 0.00001)
            {
                v *= velocityDampen;
                newVal += v;
                steps++;
            }

            return newVal;
        }

        /// <summary>
        /// Applies <paramref name="workingPos"/> to the <see cref="Transform.localPosition"/> of our <see cref="scrollContainer"/>
        /// </summary>
        /// <param name="workingPos">The new desired position for <see cref="scrollContainer"/> in local space</param>
        private void ApplyPosition(Vector3 workingPos)
        {
            Vector3 newScrollPos = new Vector3(workingPos.x, workingPos.y, 0.0f);

            if(!float.IsNaN( newScrollPos.x) && !float.IsNaN(newScrollPos.y) && !float.IsNaN(newScrollPos.z))
            {
                ScrollContainer.transform.localPosition = newScrollPos;
            }
        }

        /// <summary>
        /// Resets the interaction state of the PanelScrollingCollection for the next scroll.
        /// </summary>
        private void ResetInteraction()
        {
            OnTouchEnded?.Invoke(initialFocusedObject);

            // Release the pointer
            if (currentPointer != null) currentPointer.IsFocusLocked = false;
            currentPointer = null;
            initialFocusedObject = null;

            // Clear our states
            IsTouched = false;
            IsEngaged = false;
            IsDragging = false;

            this.isBothScrollReady = false;
            this.startPoint = new Vector3(float.NaN, float.NaN, float.NaN);
            if (this.isBoth)
            {
                this.overflowBack();
            }
        }

        /// <summary>
        /// Resets the scroll offset state of the PanelScrollingCollection.
        /// </summary>
        private void ResetScrollOffset()
        {
            MoveToIndex(0, false);
            workingScrollerPos = Vector3.zero;
            ApplyPosition(workingScrollerPos);
        }

        /// <summary>
        /// All inactive content objects and colliders are reactivated and renderers are unclipped.
        /// </summary>
        private void RestoreContentVisibility()
        {
            ClearClippingBox();
            ManageVisibility(true);
        }

        /// <summary>
        /// Moves the scroll container to the position that makes the tier with the tierIndex the first in the viewable area
        /// </summary>
        public void MoveToTier(int tierIndex, bool animateToPosition = true, System.Action callback = null)
        {
            if (animateScroller != null)
            {
                CurrentVelocityState = VelocityState.None;
                StopAllCoroutines();
            }

            if (scrollDirection == ScrollDirectionType.UpAndDown)
            {
                workingScrollerPos.y = tierIndex * CellHeight;

                // Clamp the working pos since we already have calculated it
                workingScrollerPos.y = Mathf.Clamp(workingScrollerPos.y, minY, MaxY);

                // Zero out the other axes
                workingScrollerPos = workingScrollerPos.Mul(Vector3.up);
            }
            else
            {
                workingScrollerPos.x = tierIndex * CellWidth * -1.0f;


                // Clamp the working pos since we already have calculated it
                workingScrollerPos.x = Mathf.Clamp(workingScrollerPos.x, MinX, maxX);

                // Zero out the other axes
                workingScrollerPos = workingScrollerPos.Mul(Vector3.right);
            }

            if (initialScrollerPos != workingScrollerPos)
            {
                CurrentVelocityState = VelocityState.Animating;

                if (animateToPosition)
                {
                    animateScroller = AnimateTo(ScrollContainer.transform.localPosition, workingScrollerPos, paginationCurve, animationLength, callback);
                    StartCoroutine(animateScroller);
                }
                else
                {
                    CurrentVelocityState = VelocityState.None; // Flagging the instant position change to trigger momentum events
                    initialScrollerPos = workingScrollerPos;
                }

                if (callback != null)
                {
                    callback?.Invoke();
                }
            }
        }

        #endregion private methods

        #region public methods

        /// <summary>
        /// Resets the PanelScrollingCollection
        /// </summary>
        public void Reset()
        {
            ResetInteraction();
            UpdateContent();
            ResetScrollOffset();
        }

        /// <summary>
        /// Safely adds a child game object to scroll collection.
        /// </summary>
        public void AddContent(GameObject content)
        {
            var x = content.transform.localPosition.x;
            var y = content.transform.localPosition.y;
            var z = content.transform.localPosition.z;
            content.transform.SetParent(ScrollContainer.transform);
            content.transform.localPosition = new Vector3(x, y, z);
            this.fresh(false);
            // Reset();
        }

        /// <summary>
        /// Safely removes a child game object from scroll content and clipping box.
        /// </summary>
        public void RemoveItem(GameObject item)
        {
            if (item == null)
            {
                return;
            }

            var itemRenderers = item.GetComponentsInChildren<Renderer>();
            if (itemRenderers != null)
            {
                foreach (var renderer in item.GetComponentsInChildren<Renderer>())
                {
                    renderersToUnclip.Add(renderer);
                }
            }

            item.transform.parent = null;
            Reset();
        }

        /// <summary>
        /// Checks whether the given cell is visible relative to viewable area or page.
        /// </summary>
        /// <param name="cellIndex">the index of the pagination cell</param>
        /// <returns>true when cell is visible</returns>
        public bool IsCellVisible(int cellIndex)
        {
            bool isCellVisible = true;

            if (cellIndex < FirstVisibleCellIndex)
            {
                // It's above the visible area
                isCellVisible = false;
            }
            else if (cellIndex >= FirstHiddenCellIndex)
            {
                // It's below the visible area
                isCellVisible = false;
            }
            return isCellVisible;
        }

        /// <summary>
        /// Moves scroller container by a multiplier of the number of tiers in the viewable area.
        /// </summary>
        /// <param name="numberOfPages">Amount of pages to move by</param>
        /// <param name="animate"> If true, scroller will animate to new position</param>
        /// <param name="callback"> An optional action to pass in to get notified that the <see cref="PanelScrollingCollection"/> is finished moving</param>
        public void MoveByPages(int numberOfPages, bool animate = true, System.Action callback = null)
        {
            int tierIndex = SafeDivisionInt(FirstVisibleCellIndex, CellsPerTier) + (numberOfPages * TiersPerPage);

            MoveToTier(tierIndex, animate, callback);
        }

        /// <summary>
        /// Moves scroller container a relative number of tiers of cells.
        /// </summary>
        /// <param name="numberOfTiers">Amount of tiers to move by</param>
        /// <param name="animate">if true, scroller will animate to new position</param>
        /// <param name="callback"> An optional action to pass in to get notified that the <see cref="ScrollingCollection"/> is finished moving</param>
        public void MoveByTiers(int numberOfTiers, bool animate = true, System.Action callback = null)
        {
            int tierIndex = SafeDivisionInt(FirstVisibleCellIndex, CellsPerTier) + numberOfTiers;

            MoveToTier(tierIndex, animate, callback);
        }

        /// <summary>
        /// Moves scroller container to a position where the selected cell is in the first tier of the viewable area.
        /// </summary>
        /// <param name="cellIndex">Index of the cell to move to</param>
        /// <param name="animate">if true, scroller will animate to new position</param>
        /// <param name="callback"> An optional action to pass in to get notified that the <see cref="ScrollingCollection"/> is finished moving</param>
        public void MoveToIndex(int cellIndex, bool animateToPosition = true, System.Action callback = null)
        {
            cellIndex = (cellIndex < 0) ? 0 : cellIndex;
            int tierIndex = SafeDivisionInt(cellIndex, CellsPerTier);

            MoveToTier(tierIndex, animateToPosition, callback);
        }

        #endregion public methods

        #region IMixedRealityPointerHandler implementation

        /// <inheritdoc/>
        void IMixedRealityPointerHandler.OnPointerUp(MixedRealityPointerEventData eventData)
        {
            if (this._node is EMR.IPointerEventFeature)
            {
                // EmptyScroll是其父节点自身的一部分只不过用其结构化了滚动，所以其父节点的onUp事件由EmptyScroll接管
                ((EMR.IPointerEventFeature)this._node).onUp.Invoke(new EMR.Event.UpEventData
                {
                    target = this._node,
                    normal = eventData.Pointer.Result.Details.LastRaycastHit.normal,
                    point = eventData.Pointer.Result.Details.LastRaycastHit.point,
                    pointer = eventData.Pointer,
                    original = eventData
                });
            }

            if (currentPointer == null || eventData.Pointer.PointerId != currentPointer.PointerId)
            {
                return;
            }

            // Release the pointer
            currentPointer.IsTargetPositionLockedOnFocusLock = oldIsTargetPositionLockedOnFocusLock;

            if (!IsTouched && IsEngaged && animateScroller == null)
            {
                if (IsDragging)
                {
                    // Its a drag release
                    initialScrollerPos = workingScrollerPos;
                    CurrentVelocityState = VelocityState.Calculating;
                }

                ResetInteraction();
            }
        }

        private Vector3 startPoint = new Vector3(float.NaN, float.NaN, float.NaN);

        /// <inheritdoc/>
        void IMixedRealityPointerHandler.OnPointerDown(MixedRealityPointerEventData eventData)
        {
            if (this._node is EMR.IPointerEventFeature)
            {
                // EmptyScroll是其父节点自身的一部分只不过用其结构化了滚动，所以其父节点的onDowm事件由EmptyScroll接管
                ((EMR.IPointerEventFeature)this._node).onDown.Invoke(new EMR.Event.DownEventData
                {
                    target = this._node,
                    normal = eventData.Pointer.Result.Details.LastRaycastHit.normal,
                    point = eventData.Pointer.Result.Details.LastRaycastHit.point,
                    pointer = eventData.Pointer,
                    original = eventData
                });
            }

            // Current pointer owns scroll interaction until scroll release happens. Ignoring any interaction with other pointers.
            if (currentPointer != null)
            {
                return;
            }

            var selectedObject = eventData.Pointer.Result?.CurrentPointerTarget;

            if (selectedObject == null || !selectedObject.transform.IsChildOf(transform))
            {
                return;
            }

            currentPointer = eventData.Pointer;
            
            oldIsTargetPositionLockedOnFocusLock = currentPointer.IsTargetPositionLockedOnFocusLock;

            if (!(currentPointer is IMixedRealityNearPointer) && currentPointer.Controller.IsRotationAvailable)
            {
                currentPointer.IsTargetPositionLockedOnFocusLock = false;
            }

            initialFocusedObject = selectedObject;
            currentPointer.IsFocusLocked = true; // Unwanted focus locked on children items

            // Reset the scroll state
            scrollVelocity = 0.0f;

            if (TryGetPointerPositionOnPlane(out initialPointerPos))
            {
                initialScrollerPos = ScrollContainer.transform.localPosition;
                CurrentVelocityState = VelocityState.None;

                IsTouched = false;
                IsEngaged = true;
                IsDragging = false;

                if (this.isBoth)
                {
                    startPoint = transform.InverseTransformPoint(eventData.Pointer.Result.Details.Point);
                }

                OnTouchStarted?.Invoke(initialFocusedObject);
            }
        }

        /// <inheritdoc/>
        /// Pointer Click handled during Update.
        void IMixedRealityPointerHandler.OnPointerClicked(MixedRealityPointerEventData eventData)
        {
            if (this._node is EMR.IPointerEventFeature)
            {
                // EmptyScroll是其父节点自身的一部分只不过用其结构化了滚动，所以其父节点的onClick事件由EmptyScroll接管
                ((EMR.IPointerEventFeature)this._node).onClick.Invoke(new EMR.Event.ClickEventData
                {
                    target = this._node,
                    normal = eventData.Pointer.Result.Details.LastRaycastHit.normal,
                    point = eventData.Pointer.Result.Details.LastRaycastHit.point,
                    pointer = eventData.Pointer,
                    original = eventData
                });
            }
        }

        /// <inheritdoc/>
        void IMixedRealityPointerHandler.OnPointerDragged(MixedRealityPointerEventData eventData)
        {

            if (this._node is EMR.IPointerEventFeature)
            {
                // EmptyScroll是其父节点自身的一部分只不过用其结构化了滚动，所以其父节点的onDragged事件由EmptyScroll接管
                ((EMR.IPointerEventFeature)this._node).onDragged.Invoke(new EMR.Event.DraggedEventData
                {
                    target = this._node,
                    normal = eventData.Pointer.Result.Details.LastRaycastHit.normal,
                    point = eventData.Pointer.Result.Details.LastRaycastHit.point,
                    pointer = eventData.Pointer,
                    original = eventData
                });
            }

            // set scroll direction
            if (!float.IsNaN(startPoint.x) && !float.IsNaN(startPoint.y) && !float.IsNaN(startPoint.z) && !this.isBothScrollReady)
            {
                var targetPoint = transform.InverseTransformPoint(eventData.Pointer.Result.Details.Point);
                var vector = targetPoint - startPoint;

                var x = EMR.Space.Unit.scaleToUnitForGameObject(transform.gameObject, vector.x, EMR.Struct.Axle.right);
                var y = EMR.Space.Unit.scaleToUnitForGameObject(transform.gameObject, vector.y, EMR.Struct.Axle.up);

                var distance = Mathf.Sqrt(x * x + y * y);
                if (distance > 15)
                {
                    var data_1 = Mathf.Abs(Vector3.Dot(vector.normalized, new Vector3(1f, 0f, 0f)));
                    var data_2 = Mathf.Abs(Vector3.Dot(vector.normalized, new Vector3(0f, 1f, 0f)));

                    if (data_1 > data_2)
                    {
                        this.scrollDirection = ScrollDirectionType.LeftAndRight;
                    }
                    else
                    {
                        this.scrollDirection = ScrollDirectionType.UpAndDown;
                    }

                    this.isBothScrollReady = true;
                }
            }
        }

        #endregion IMixedRealityPointerHandler implementation

        #region IMixedRealityTouchHandler implementation

        /// <inheritdoc/>
        void IMixedRealityTouchHandler.OnTouchStarted(HandTrackingInputEventData eventData)
        {
            // Current pointer owns scroll interaction until scroll release happens. Ignoring any interaction with other pointers.
            if (currentPointer != null)
            {
                return;
            }



            PokePointer pokePointer = PointerUtils.GetPointer<PokePointer>(eventData.Handedness);

            if (this._node is EMR.ITouchEventFeature)
            {
                // EmptyScroll是其父节点自身的一部分只不过用其结构化了滚动，所以其父节点的onTouchStarted事件由EmptyScroll接管
                ((EMR.ITouchEventFeature)this._node).onTouchStarted.Invoke(new EMR.Event.TouchStartedEventData
                {
                    target = this._node,
                    normal = pokePointer.Result.Details.LastRaycastHit.normal,
                    point = pokePointer.Result.Details.LastRaycastHit.point,
                    pointer = currentPointer,
                    original = eventData
                });
            }


            var selectedObject = pokePointer.Result?.CurrentPointerTarget;
            if (selectedObject == null || !selectedObject.transform.IsChildOf(transform))
            {
                return;
            }

            if (!HasPassedThroughFrontPlane(pokePointer))
            {
                return;
            }

            currentPointer = pokePointer;

            if (this.isBoth)
            {
                if(currentPointer != null)
                {
                    startPoint = transform.InverseTransformPoint(currentPointer.Result.Details.Point);
                } else
                {
                    startPoint = new Vector3(float.NaN, float.NaN, float.NaN);
                }
            }

            StopAllCoroutines();
            CurrentVelocityState = VelocityState.None;
            animateScroller = null;

            if (!IsTouched && !IsEngaged)
            {
                initialPointerPos = currentPointer.Position;
                initialFocusedObject = selectedObject;
                initialScrollerPos = ScrollContainer.transform.localPosition;

                IsTouched = true;
                IsEngaged = true;
                IsDragging = false;

                OnTouchStarted?.Invoke(initialFocusedObject);
            }
        }

        /// <inheritdoc/>
        /// Touch release handled during Update.
        void IMixedRealityTouchHandler.OnTouchCompleted(HandTrackingInputEventData eventData)
        {
            if (this._node is EMR.ITouchEventFeature)
            {
                // EmptyScroll是其父节点自身的一部分只不过用其结构化了滚动，所以其父节点的onTouchCompleted事件由EmptyScroll接管
                ((EMR.ITouchEventFeature)this._node).onTouchCompleted.Invoke(new EMR.Event.TouchCompletedEventData
                {
                    target = this._node,
                    normal = currentPointer !=null ? currentPointer.Result.Details.LastRaycastHit.normal : new Vector3(float.NaN, float.NaN, float.NaN),
                    point = currentPointer != null ? currentPointer.Result.Details.LastRaycastHit.point : new Vector3(float.NaN, float.NaN, float.NaN),
                    pointer = currentPointer,
                    original = eventData
                });
            }
        }

        /// <inheritdoc/>
        void IMixedRealityTouchHandler.OnTouchUpdated(HandTrackingInputEventData eventData)
        {
            if (currentPointer == null || eventData.SourceId != currentPointer.InputSourceParent.SourceId)
            {
                return;
            }

            // set scroll direction
            if (currentPointer != null && !float.IsNaN(startPoint.x) && !float.IsNaN(startPoint.y) && !float.IsNaN(startPoint.z) && !this.isBothScrollReady)
            {
                var targetPoint = currentPointer.Result.Details.LastRaycastHit.point;
                var vector = targetPoint - startPoint;

                var x = EMR.Space.Unit.scaleToUnitForGameObject(transform.gameObject, vector.x, EMR.Struct.Axle.right);
                var y = EMR.Space.Unit.scaleToUnitForGameObject(transform.gameObject, vector.y, EMR.Struct.Axle.up);

                var distance = Mathf.Sqrt(x * x + y * y);
                if (distance > 15)
                {
                    var data_1 = Mathf.Abs(Vector3.Dot(vector.normalized, new Vector3(1f, 0f, 0f)));
                    var data_2 = Mathf.Abs(Vector3.Dot(vector.normalized, new Vector3(0f, 1f, 0f)));

                    if (data_1 > data_2)
                    {
                        this.scrollDirection = ScrollDirectionType.LeftAndRight;
                    }
                    else
                    {
                        this.scrollDirection = ScrollDirectionType.UpAndDown;
                    }

                    this.isBothScrollReady = true;
                }
            }

            if (this._node is EMR.ITouchEventFeature)
            {
                // EmptyScroll是其父节点自身的一部分只不过用其结构化了滚动，所以其父节点的onTouchUpdate事件由EmptyScroll接管
                ((EMR.ITouchEventFeature)this._node).onTouchUpdate.Invoke(new EMR.Event.TouchUpdateEventData
                {
                    target = this._node,
                    normal = currentPointer != null ? currentPointer.Result.Details.LastRaycastHit.normal : new Vector3(float.NaN, float.NaN, float.NaN),
                    point = currentPointer != null ? currentPointer.Result.Details.LastRaycastHit.point : new Vector3(float.NaN, float.NaN, float.NaN),
                    pointer = currentPointer,
                    original = eventData
                });
            }

            if (IsDragging)
            {
                eventData.Use();
            }
        }

        #endregion IMixedRealityTouchHandler implementation

        #region IMixedRealitySourceStateHandler implementation

        void IMixedRealitySourceStateHandler.OnSourceDetected(SourceStateEventData eventData) { }

        void IMixedRealitySourceStateHandler.OnSourceLost(SourceStateEventData eventData)
        {
            if (currentPointer == null || eventData.SourceId != currentPointer.InputSourceParent.SourceId)
            {
                return;
            }

            // We'll consider this a drag release
            if (IsEngaged && animateScroller == null)
            {
                if (IsTouched || IsDragging)
                {
                    // Its a drag release
                    initialScrollerPos = workingScrollerPos;
                }

                ResetInteraction();

                CurrentVelocityState = VelocityState.Calculating;
            }
        }

        #endregion IMixedRealitySourceStateHandler implementation
    }
}
