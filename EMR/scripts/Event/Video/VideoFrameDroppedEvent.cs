using UnityEngine.Video;

namespace EMR.Event
{
    /// <summary>
    /// 丢帧EventData类
    /// </summary>
    public class VideoFrameDroppedEventData : EventData
    {
        public VideoPlayer source;
    }

    /// <summary>
    /// 丢帧事件类
    /// </summary>
    [System.Serializable]
    public class VideoFrameDroppedEvent : EMREvent<VideoFrameDroppedEventData>
    {
    }
}
