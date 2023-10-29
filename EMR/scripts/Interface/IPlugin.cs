using System.Collections.Generic;

namespace EMR
{
    public interface IPlugin
    {
        /// <summary>
        /// 刷新
        /// </summary>
        void fresh();

        /// <summary>
        /// 销毁
        /// </summary>
        void destory();
    }
}

