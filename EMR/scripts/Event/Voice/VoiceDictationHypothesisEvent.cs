using Microsoft.MixedReality.Toolkit.Input;

namespace EMR.Event
{
    /// <summary>
    /// VoiceDictationHypothesis EventData类
    /// </summary>
    public class VoiceDictationHypothesisEventData : EventData
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
    /// VoiceDictationHypothesis 事件类
    /// </summary>
    public class VoiceDictationHypothesisEvent : EMREvent<VoiceDictationHypothesisEventData>
    {
    }
}
