using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using EMR.Event;
using EMR.Entity;
using EMR.Common;

namespace EMR.Module
{
    public class Eye
    {
        #region 基本字段
        /// <summary>
        /// 旧目标游戏对象
        /// </summary>
        private GameObject oldEnity = null;

        /// <summary>
        /// 当前目标游戏对象
        /// </summary>
        private GameObject currentEnity = null;

        /// <summary>
        /// 旧目标节点
        /// </summary>
        private Node oldNode = null;

        /// <summary>
        /// 当前目标节点
        /// </summary>
        private Node currentNode = null;

        /// <summary>
        /// 旧击中点
        /// </summary>
        private Vector3 oldPoint = new Vector3(0f, 0f, 0f);

        /// <summary>
        /// 当前击中点
        /// </summary>
        private Vector3 currentPoint = new Vector3(0f, 0f, 0f);

        /// <summary>
        /// 眼睛凝视改变事件
        /// </summary>
        public EyeGazeChangeEvent onEyeGazeChange = new EyeGazeChangeEvent();

        /// <summary>
        /// 眼睛扫视事件
        /// </summary>
        public EyeSaccadeEvent onEyeSaccade = new EyeSaccadeEvent();
        #endregion

        /// <summary>
        /// 构造器
        /// </summary>
        public Eye()
        {
            /*
             * 以下逻辑用于实时监视凝视对象的改变，并触发onEyeGazeChange事件
             */
            Space.mainService.next(() =>
            {
                var gazeProvider = CoreServices.InputSystem.EyeGazeProvider;

                if (gazeProvider.IsEyeTrackingEnabledAndValid)
                {
                    this.currentEnity = gazeProvider.GazeTarget;
                    this.currentNode = this.currentEnity != null ? Space.getNodeByParasitifer(this.currentEnity) : null;

                    if (this.currentEnity != this.oldEnity)
                    {
                        var hitInfo = gazeProvider.HitInfo;

                        var eventData = new EyeGazeChangeEventData
                        {
                            // 设置凝视对象
                            target = this.currentNode,
                            oldNode = this.oldNode,

                            // 设置凝视位置
                            point = hitInfo.collider != null ? hitInfo.point : gazeProvider.GazeOrigin + gazeProvider.GazeDirection.normalized * 10f,
                            normal = hitInfo.collider != null ? hitInfo.normal : gazeProvider.GazeOrigin,
                            distance = hitInfo.collider != null ? hitInfo.distance : 10f,
                        };

                        // 触发eyeGazeChange事件
                        this.onEyeGazeChange.Invoke(eventData);
                    }
                }

                return false;
            });

            Space.mainService.lateNext(() =>
            {
                this.oldEnity = this.currentEnity;
                this.oldNode = this.currentNode;
                return false;
            });


            /*
             * 以下逻辑用于实时监视眼球的转动，并触发onEyeSaccade事件
             */
            Space.mainService.next(() =>
            {
                var gazeProvider = CoreServices.InputSystem.EyeGazeProvider;

                if (gazeProvider.IsEyeTrackingEnabledAndValid && this.onEyeSaccade.listenerCount > 0)
                {
                    var currentEnity = gazeProvider.GazeTarget;
                    var currentNode = this.currentEnity != null ? Space.getNodeByParasitifer(currentEnity) : null;

                    // 设置凝视位置
                    var hitInfo = gazeProvider.HitInfo;
                    this.currentPoint = hitInfo.collider != null ? hitInfo.point : gazeProvider.GazeOrigin + gazeProvider.GazeDirection.normalized * 10f;

                    if (!Utils.equals(this.currentPoint.x, this.oldPoint.x) || !Utils.equals(this.currentPoint.y, this.oldPoint.y) || !Utils.equals(this.currentPoint.z, this.oldPoint.z))
                    {
                        var eventData = new EyeSaccadeEventData
                        {
                            // 设置凝视对象
                            target = currentNode,
                            point = this.currentPoint,
                            normal = hitInfo.collider != null ? hitInfo.normal : gazeProvider.GazeOrigin,
                            distance = hitInfo.collider != null ? hitInfo.distance : 10f,
                        };

                        // 触发eyeSaccade事件
                        this.onEyeSaccade.Invoke(eventData);
                    }
                }

                return false;
            });

            Space.mainService.lateNext(() =>
            {
                this.oldPoint = this.currentPoint;
                return false;
            });
        }
    }
}
