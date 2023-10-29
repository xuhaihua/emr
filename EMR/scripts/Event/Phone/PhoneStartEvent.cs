using UnityEngine.Windows.WebCam;

namespace EMR.Event
{
    /// <summary>
    /// 拍照开始EventData类
    /// </summary>
    public class PhoneStartEventData : EventData
    {
        public PhotoCapture photoCapture;
    }

    /// <summary>
    /// 拍照开始事件类
    /// </summary>
    [System.Serializable]
    public class PhoneStartEvent : EMREvent<PhoneStartEventData>
    {
    }
}
