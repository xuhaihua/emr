
using UnityEngine.Video;

namespace EMR.Event
{
    /// <summary>
    /// 视频播放结束EventData类
    /// </summary>
    public class VideoEndEventData : EventData
    {
        public VideoPlayer source;
    }

    /// <summary>
    /// 视频播放结束事件类
    /// </summary>
    [System.Serializable]
    public class VideoEndEvent : EMREvent<VideoEndEventData>
    {
    }
}
