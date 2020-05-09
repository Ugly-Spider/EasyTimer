using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EasyTimer
{
    public class EasyTimerDataForSave
    {
        public List<TimerData> list;

        public EasyTimerDataForSave()
        {
            
        }
    }
    
    public class EasyTimerManager : MonoBehaviour
    {

        private static EasyTimerManager _Instance;

        public static EasyTimerManager Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = FindObjectOfType<EasyTimerManager>();
                    if (_Instance == null)
                    {
                        var go = new GameObject($"[{nameof(EasyTimerManager)}]");
                        _Instance = go.AddComponent<EasyTimerManager>();
                        DontDestroyOnLoad(go);
                    }
                }

                return _Instance;
            }
        }

        private Dictionary<string, ETimer> _timers = new Dictionary<string, ETimer>();
        private Queue<(ETimer, bool)> _toDoList = new Queue<(ETimer, bool)>();

        void Awake()
        {
            Load();
        }

        public ETimer Create(string timerId = "")
        {
            if (string.IsNullOrEmpty(timerId))
            {
                timerId = Guid.NewGuid().ToString();
            }
            
            var timer = new ETimer(timerId);
            OnTimerCreate(timer);
            return timer;
        }

        public ETimer Create(TimerData timerData, string timerId = "")
        {
            var timer = Create(timerId);
            timer.SetData(timerData);
            return timer;
        }

        public void Delete(ETimer timer)
        {
            if(timer == null) return;
            
            Delete(timer.TimerId);
        }

        public void Delete(string timerId)
        {
            OnTimerDelete(Get(timerId));
        }

        public ETimer Get(string timerId)
        {
            if (!_timers.TryGetValue(timerId, out var timer))
            {
                foreach (var v in _toDoList)
                {
                    if (v.Item1.TimerId == timerId)
                    {
                        timer = v.Item1;
                        break;
                    }
                }
            }
            return timer;
        }

        public ETimer GetOrCreate(string timerId)
        {
            if (!_timers.TryGetValue(timerId, out var timer))
            {
                timer = Create(timerId);
            }

            return timer;
        }
        
        public ETimer GetOrCreate(string timerId, bool createCondition)
        {
            if (!_timers.TryGetValue(timerId, out var timer))
            {
                if (!createCondition) return null;
                timer = Create(timerId);
            }

            return timer;
        }

        public void Restart(string timerId, bool checkReady = false)
        {
            var timer = Get(timerId);
            Restart(timer, checkReady);
        }

        public void Restart(ETimer timer, bool checkReady = false)
        {
            if(timer == null) return;
            if(checkReady && !timer.IsReady) return;
            
            timer.Start();
        }
        
        void Update()
        {
            while (_toDoList.Count > 0)
            {
                var v = _toDoList.Dequeue();
                if (v.Item2)
                {
                    if (!_timers.ContainsKey(v.Item1.TimerId))
                    {
                        _timers.Add(v.Item1.TimerId, v.Item1);
                        v.Item1.OnCreate();
                        v.Item1.Start(); 
                    }
                    else
                    {
                        Debug.LogError($"Add timer failed! Because repeat timer id:{v.Item1.TimerId}");
                    }
                }
                else
                {
                    if (v.Item1 != null)
                    {
                        if (_timers.ContainsKey(v.Item1.TimerId))
                        {
                            _timers.Remove(v.Item1.TimerId);
                        }
                        else
                        {
                            Debug.LogError($"Delete timer failed! There is no timer with id:{v.Item1.TimerId}");
                        }
                    }
                }
            }
            
            foreach (var timer in _timers)
            {
                timer.Value.Update();
            }
        }

        private void OnTimerCreate(ETimer timer)
        {
            _toDoList.Enqueue((timer, true));
        }

        private void OnTimerDelete(ETimer timer)
        {
            _toDoList.Enqueue((timer, false));
        }

        private void Save()
        {
            var list = _timers.Values.Where(x => x.NeedToSave()).Select(x=>x._timerData).ToList();
            var data = new EasyTimerDataForSave();
            data.list = list;
            var json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(nameof(EasyTimer), json);
            PlayerPrefs.Save();
        }

        private void Load()
        {
            var json = PlayerPrefs.GetString(nameof(EasyTimer), "");
            var data = JsonUtility.FromJson<EasyTimerDataForSave>(json);
            var list = data?.list;
            if (list == null) return;
            foreach (var v in list)
            {
                if (_timers.TryGetValue(v.timerId, out var timer))
                {
                    timer.OnLoad();
                }
                else
                {
                    timer = new ETimer(v);
                    _timers.Add(v.timerId, timer);
                    timer.OnLoad();
                }
            }
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                Save();
            }
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                Load();
            }
        }

        #if UNITY_EDITOR
        void OnDestroy()
        {
            Save();
        }
        #endif

        public void DelayFrame(int frameCount, Action cb)
        {
            StartCoroutine(_DelayFrame(frameCount, cb));
        }

        private IEnumerator _DelayFrame(int frameCount, Action cb)
        {
            for (int i = 0; i < frameCount; i++)
            {
                yield return new WaitForEndOfFrame();
            }

            cb?.Invoke();
        }
    }
}