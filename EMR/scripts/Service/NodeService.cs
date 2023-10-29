
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections.Generic;
using EMR.Entity;
namespace EMR
{
    // 任务循环委托
    public delegate bool CirculateTaskHandler();

    // emit循环委托
    public delegate void CirculateEmitHandler();

    [MixedRealityExtensionService(SupportedPlatforms.WindowsStandalone | SupportedPlatforms.MacStandalone | SupportedPlatforms.LinuxStandalone | SupportedPlatforms.WindowsUniversal)]
    public class NodeService : BaseExtensionService, IMixedRealityExtensionService
    {
        #region 基本字段
        /// <summary>
        /// 关联的空间节点
        /// </summary>
        public Node node;

        /// <summary>
        /// 下一周期开始时执行的任务列表
        /// </summary>
        private List<CirculateTaskHandler> nextTakeList = new List<CirculateTaskHandler>();


        private List<CirculateEmitHandler> emiiterList = new List<CirculateEmitHandler>();


        private List<CirculateEmitHandler> lateEmiiterList = new List<CirculateEmitHandler>();

        /// <summary>
        /// 下一周期结尾时执行的任务列表
        /// </summary>
        private List<CirculateTaskHandler> lateNextList = new List<CirculateTaskHandler>();

        public List<CirculateTaskHandler> annexTaskList = new List<CirculateTaskHandler>();
        #endregion

        public NodeService(IMixedRealityServiceRegistrar registrar, string name, uint priority, BaseMixedRealityProfile profile) : base(registrar, name, priority, profile)
        {
        }

        #region 基本方法
        /// <summary>
        /// 任务循环执行器
        /// </summary>
        /// <param name="taskList"></param>
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
        /// emit循环执行器
        /// </summary>
        /// <param name="taskList"></param>
        private void emitExecuteHandle(List<CirculateEmitHandler> taskList)
        {
            for (var i = 0; i < taskList.Count; i++)
            {
                taskList[i]();
            }
        }

        /// <summary>
        /// 创建EventEngine emit
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="callback"></param>
        public void createEmit(CirculateEmitHandler trigger, CirculateEmitHandler callback = null)
        {
            this.emiiterList.Add(trigger);
            if (callback != null)
            {
                this.lateEmiiterList.Add(callback);
            }
        }

        /// <summary>
        /// 向循环周期添加任务
        /// </summary>
        /// <param name="task"></param>
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
            this.lateNextList.Add(task);
        }

        /// <summary>
        /// 向循环的附属部分(最后)添加任务
        /// </summary>
        /// <param name="task"></param>
        public void annexUpdate(CirculateTaskHandler task)
        {
            this.annexTaskList.Add(task);
        }
        #endregion

        public override void Initialize()
        {
            
        }

        public override void Update()
        {
            // 循环开始
            this.taskExecuteHandle(this.nextTakeList);

            // emit部分 主要用来触发内部驱动事件如：_onSizeChange、_onPositionChange、_onRotationChange
            this.emitExecuteHandle(this.emiiterList);
            this.emitExecuteHandle(this.lateEmiiterList);

            // 循环结尾
            this.taskExecuteHandle(this.lateNextList);

            // 附属部分
            this.taskExecuteHandle(this.annexTaskList);
        }


    }
}
