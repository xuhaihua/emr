using UnityEngine.Video;

namespace EMR.Event
{
    /// <summary>
    /// 视频资源准备好EventData类
    /// </summary>
    public class VideoReadyEventData : EventData
    {
        public VideoPlayer source;
    }

    /// <summary>
    /// 视频资源准备好事件类
    /// </summary>
    [System.Serializable]
    public class VideoReadyEvent : EMREvent<VideoReadyEventData>
    {
    }
}
