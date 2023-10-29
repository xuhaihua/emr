using UnityEngine.Video;

namespace EMR.Event
{
    /// <summary>
    /// 搜寻操作完成EventData类
    /// </summary>
    public class VideoPlaySeekCompletedEventData : EventData
    {
        public VideoPlayer source;
    }

    /// <summary>
    /// 搜寻操作完成事件类
    /// </summary>
    [System.Serializable]
    public class VideoPlaySeekCompletedEvent : EMREvent<VideoPlaySeekCompletedEventData>
    {
    }
}
