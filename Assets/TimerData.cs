using System;

namespace EasyTimer
{

    [Serializable]
    public class TimerData
    {
        public string timerId;
        public float duration;
        public int loopTimes = 1;
        public bool initAvailable = false;
        public bool persistent = false;
        public bool autoNext = false;
        public bool realtimeUpdate = false;
        public bool countTimeOverflowInBackground = false;

        public long startDateTime;

        public bool isReady;

        public TimerData()
        {
            
        }
    }

}