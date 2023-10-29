using UnityEngine.Windows.WebCam;


namespace EMR.Event
{
    /// <summary>
    /// 摄像开始EventData类
    /// </summary>
    public class VideoStartRecordingEventData : EventData
    {
        public VideoCapture videoCapture;
    }

    /// <summary>
    /// 摄像开始事件类
    /// </summary>
    [System.Serializable]
    public class VideoStartRecordingEvent : EMREvent<VideoStartRecordingEventData>
    {
    }
}
