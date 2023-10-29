using UnityEngine.Video;

namespace EMR.Event
{
    /// <summary>
    /// 视频播放开始EventData类
    /// </summary>
    public class VideoPlayEventData : EventData
    {
        public VideoPlayer source;
    }

    /// <summary>
    /// 视频播放开始事件类
    /// </summary>
    [System.Serializable]
    public class VideoPlayEvent : EMREvent<VideoPlayEventData>
    {
    }
}
