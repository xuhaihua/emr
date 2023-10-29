using UnityEngine.Video;

namespace EMR.Event
{
    /// <summary>
    /// 发生错误EventData类
    /// </summary>
    public class VideoErrorEventData : EventData
    {
        public VideoPlayer source;
        public string message;
    }

    /// <summary>
    /// 发生错误事件类
    /// </summary>
    [System.Serializable]
    public class VideoErrorEvent : EMREvent<VideoErrorEventData>
    {
    }
}
