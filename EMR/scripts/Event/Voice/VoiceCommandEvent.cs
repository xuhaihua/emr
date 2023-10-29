using Microsoft.MixedReality.Toolkit.Input;

namespace EMR.Event
{
    /// <summary>
    /// Voice Command EventData类
    /// </summary>
    public class VoiceCommandEventData : EventData
    {
        /// <summary>
        /// 识别的命令
        /// </summary>
        public string command;

        /// <summary>
        /// 事件原始数据
        /// </summary>
        public SpeechEventData original;
    }

    /// <summary>
    /// VoiceCommand 事件类
    /// </summary>
    public class VoiceCommandEvent : EMREvent<VoiceCommandEventData>
    {
    }
}
