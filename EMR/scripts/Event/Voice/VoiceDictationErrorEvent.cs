using Microsoft.MixedReality.Toolkit.Input;

namespace EMR.Event
{
    /// <summary>
    /// VoiceDictationError EventData类
    /// </summary>
    public class VoiceDictationErrorEventData : EventData
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
    /// VoiceDictationError 事件类
    /// </summary>
    public class VoiceDictationErrorEvent : EMREvent<VoiceDictationErrorEventData>
    {
    }
}
