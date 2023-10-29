using UnityEngine.Windows.WebCam;

namespace EMR.Event
{
    /// <summary>
    /// 摄像停止EventData类
    /// </summary>
    public class VideoStopRecordingEventData : EventData
    {
        public VideoCapture.VideoCaptureResult videoCaptureResult;
    }

    /// <summary>
    /// 摄像停止事件类
    /// </summary>
    [System.Serializable]
    public class VideoStopRecordingEvent : EMREvent<VideoStopRecordingEventData>
    {
    }
}

