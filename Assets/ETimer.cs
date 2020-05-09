using System;
using UnityEngine;

namespace EasyTimer
{
    public class ETimer
    {
        internal TimerData _timerData;
        private double _restTime;

        private Action<ETimer> _onCreate;
        private Action<ETimer> _onUpdate;
        private Action<ETimer> _onOnceLoop;
        private Action<ETimer> _onAllLoopFinish;

        #region public

        public string TimerId => _timerData.timerId;
        public bool IsReady => _timerData.isReady;
        public double RestTime => _restTime;

        public string RestTimeInFormat_HMS
        {
            get
            {
                var ts = TimeSpan.FromSeconds(_restTime);
                return $"{ts.TotalHours:00}:{ts.Minutes:d2}:{ts.Seconds:d2}";
            }
        }

        public string RestTimeInFormat_MS
        {
            get
            {
                var ts = TimeSpan.FromSeconds(_restTime);
                return $"{ts.TotalMinutes:00}:{ts.Seconds:d2}";
            }
        }
        
        public string RestTimeInFormat_HM
        {
            get
            {
                var ts = TimeSpan.FromSeconds(_restTime);
                return $"{ts.TotalHours:00}:{ts.Minutes:d2}";
            }
        }

        
        #endregion

        internal ETimer()
        {
            if (_timerData == null)
            {
                _timerData = new TimerData();
            }
        }

        internal ETimer(TimerData timerData)
        {
            _timerData = timerData;
        }

        internal ETimer(string timerId)
        {

            _timerData = new TimerData {timerId = timerId};
        }

        internal void OnCreate()
        {
            _onCreate?.Invoke(this);
        }

        internal void Start()
        {
            if (_timerData.duration <= 0)
            {
                Debug.LogError("Duration should be positive.");
            }
            if (_timerData.initAvailable)
            {
                _timerData.isReady = true;
                _restTime = 0;
                _timerData.initAvailable = false;
            }
            else
            {
                _timerData.isReady = false;
                _restTime = _timerData.duration;
            }
            OnStart();
        }
        
        internal void Update()
        {
            if (IsReady)
            {
                if (_timerData.autoNext)
                {
                    Start();
                }
                else
                {
                    return;
                }
            }
            
            OnUpdate();
            _restTime -= _timerData.realtimeUpdate ? Time.unscaledDeltaTime : Time.deltaTime;
            if (_restTime <= 0)
            {
                OnOnceLoop();
            }
        }

        internal void SetData(TimerData timerData)
        {
            _timerData = timerData;
        }

        internal bool NeedToSave()
        {
            return _timerData.persistent;
        }

        internal void OnLoad()
        {
            var now = DateTime.Now;
            var startDateTime = DateTime.FromBinary(_timerData.startDateTime);
            var ts = now - startDateTime;
            var eclipseSeconds = ts.TotalSeconds;
            if (eclipseSeconds < 0) eclipseSeconds = 0;

            if (_timerData.autoNext && _timerData.countTimeOverflowInBackground)
            {
                while (eclipseSeconds >= _timerData.duration)
                {
                    eclipseSeconds -= _timerData.duration;
                    OnOnceLoop(true, false);
                }

                _restTime = _timerData.duration - eclipseSeconds;
            }
            else
            {
                if (!_timerData.isReady)
                {
                    _restTime = _timerData.duration;
                    _restTime -= eclipseSeconds;
                    if (_restTime <= 0)
                    {
                        _restTime = 0;
                        OnOnceLoop(true);
                    }
                }
            }
        }

        private void CheckNextLoop(bool delayInvokeEvent, bool setStart)
        {
            if (_timerData.loopTimes == 0)
            {
                OnAllLoopFinish(delayInvokeEvent);
                return;
            }

            if (_timerData.autoNext)
            {
                if (setStart)
                {
                    Start();
                }
            }
        }
        #region SetData

        public ETimer SetDuration(float duration)
        {
            _timerData.duration = duration;
            return this;
        }

        public ETimer SetLoopTimes(int loopTimes)
        {
            _timerData.loopTimes = loopTimes;
            return this;
        }

        public ETimer SetUpdate(bool realTimeUpdate)
        {
            _timerData.realtimeUpdate = realTimeUpdate;
            return this;
        }

        public ETimer SetPersistent(bool persistent)
        {
            _timerData.persistent = persistent;
            return this;
        }

        public ETimer SetInitAvailable(bool initAvailable)
        {
            _timerData.initAvailable = initAvailable;
            return this;
        }

        public ETimer SetAutoNext(bool autoNext)
        {
            _timerData.autoNext = autoNext;
            return this;
        }

        public ETimer SetCountTimeOverflowInBackground(bool countTimeOverflowInBackground)
        {
            _timerData.countTimeOverflowInBackground = countTimeOverflowInBackground;
            return this;
        }

        public ETimer OnCreate(Action<ETimer> cb)
        {
            _onCreate += cb;
            return this;
        }

        public ETimer OnUpdate(Action<ETimer> cb)
        {
            _onUpdate += cb;
            return this;
        }
        
        public ETimer OnOnceLoop(Action<ETimer> cb)
        {
            _onOnceLoop += cb;
            return this;
        }

        public ETimer OnAllLoopFinish(Action<ETimer> cb)
        {
            _onAllLoopFinish += cb;
            return this;
        }

        #endregion

        #region Event

        private void OnStart()
        {
            _timerData.startDateTime = DateTime.Now.ToBinary();
        }
        
        private void OnUpdate()
        {
            _onUpdate?.Invoke(this);
        }
        
        private void OnOnceLoop(bool delayInvokeEvent = false, bool setStart = true)
        {
            _timerData.isReady = true;
            if (_timerData.loopTimes != -1)
            {
                _timerData.loopTimes--;
            }

            if (delayInvokeEvent)
            {
                EasyTimerManager.Instance.DelayFrame(1, () =>
                {
                    _onOnceLoop?.Invoke(this);
                });
            }
            else
            {
                _onOnceLoop?.Invoke(this);
            }
            

            CheckNextLoop(delayInvokeEvent, setStart);
        }

        private void OnAllLoopFinish(bool delayInvokeEvent = false)
        {
            if (delayInvokeEvent)
            {
                EasyTimerManager.Instance.DelayFrame(1, () =>
                {
                    _onAllLoopFinish?.Invoke(this);
                });
            }
            else
            {
                _onAllLoopFinish?.Invoke(this);
            }
            
            EasyTimerManager.Instance.Delete(TimerId);
        }
        #endregion
        
    }
}

