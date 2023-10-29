using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;
using EMR.Entity;

namespace EMR.Event
{
    public class ClockResyncOccurredEventData : EventData
    {
        public VideoPlayer source;
        public double seconds;
    }

    [System.Serializable]
    public class ClockResyncOccurredEvent : EMREvent<ClockResyncOccurredEventData>
    {
    }
}
