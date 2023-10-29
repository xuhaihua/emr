using UnityEngine.Windows.WebCam;

namespace EMR.Event
{
    /// <summary>
    /// 拍照完成EventData类
    /// </summary>
    public class PhoneCompleteEventData : EventData
    {
        public PhotoCapture.PhotoCaptureResult photoCaptureResult;

        public PhotoCaptureFrame photoCaptureFrame;
    }

    /// <summary>
    /// 拍照完成事件类
    /// </summary>
    [System.Serializable]
    public class PhoneCompleteEvent : EMREvent<PhoneCompleteEventData>
    {
    }
}
