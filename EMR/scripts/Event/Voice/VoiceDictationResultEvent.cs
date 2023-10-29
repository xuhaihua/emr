using Microsoft.MixedReality.Toolkit.Input;

namespace EMR.Event
{
    /// <summary>
    /// VoiceDictationResult EventData类
    /// </summary>
    public class VoiceDictationResultEventData : EventData
    {
        /// <summary>
        /// 结果
        /// </summary>
        public string result;

        /// <summary>
        /// 事件原始数据
        /// </summary>
        public DictationEventData original;
    }

    /// <summary>
    /// VoiceDictationResult 事件类
    /// </summary>
    public class VoiceDictationResultEvent : EMREvent<VoiceDictationResultEventData>
    {
    }
}
