using UnityEngine;
using EMR.Event;

namespace EMR
{
    /// <summary>
    /// Room
    /// </summary>
    public delegate void TimerCallbackHandle();
    public class Room : EMR.CoreComponent
    {
        public Room()
        {
            ;
        }

        public static void timeOut(int delayTime, TimerCallbackHandle callback)
        {
            var currentTime = Time.deltaTime;
            Space.mainService.next(() =>
            {
                bool result = false;
                currentTime += Time.deltaTime;
                if(currentTime > delayTime)
                {
                    callback();
                    result = true;
                }
                return result;
            });
        }

        /// <summary>
        /// 销毁room
        /// </summary>
        public override void destory()
        {
            base.destory();

            foreach (var item in EMR.Space.roomDictionary)
            {
                if (this == item.Value)
                {
                    EMR.Space.roomDictionary.Remove(item.Key);
                }
            }
        }
    }
}