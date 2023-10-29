using UnityEngine.Video;

namespace EMR.Event
{
    /// <summary>
    /// 新帧准备好 EventData类
    /// </summary>
    public class VideoFrameReadyEventData : EventData
    {
        public VideoPlayer source;
        public long frameIdx;
    }

    /// <summary>
    /// 新帧准备好事件类
    /// </summary>
    [System.Serializable]
    public class VideoFrameReadyEvent : EMREvent<VideoFrameReadyEventData>
    {
    }
}
