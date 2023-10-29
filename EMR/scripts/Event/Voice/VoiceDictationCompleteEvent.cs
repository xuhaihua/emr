using Microsoft.MixedReality.Toolkit.Input;

namespace EMR.Event
{
    /// <summary>
    ///  VoiceDictationComplete EventData类
    /// </summary>
    public class VoiceDictationCompleteEventData : EventData
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
    ///  VoiceDictationComplete 事件类
    /// </summary>
    public class VoiceDictationCompleteEvent : EMREvent<VoiceDictationCompleteEventData>
    {
    }
}
