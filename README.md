# EasyTimer
Timer for unity.

```C#
        //1.计时器的创建与获取(计时器在结束后会自动删除，大部分情况不需要手动删除)
        //指定id创建计时器
        EasyTimerManager.Instance.Create("timer1");
        //在其他地方可以通过id获取该计时器
        var timer1 = EasyTimerManager.Instance.Get("timer1");
        
        //2.设置计时器的属性 可以通过链式方式调用
        EasyTimerManager.Instance.Create()
            .SetDuration(10)                         //设置周期为10秒，必须设置
            .SetPersistent(true)                     //设置为持续性计时器，默认为false
            .SetLoopTimes(3)                         //设置计时器循环次数，默认为1
            .SetAutoNext(true)                       //多次循环时，一次计时结束是否自动开始下一次计时，默认为false
            .SetUpdate(true)                         //是否使用真实时间，默认为false
            .SetInitAvailable(true)                  //创建计时器的第一次是否可用，默认为false
            .SetCountTimeOverflowInBackground(false);//后台计时的时间是否计入循环中，如一个3次10秒的计时器，重启时过去了60秒，
                                                     //如果不开启该选项，则算1次循环，开启算3次循环
        
        // 3.计时器一共有两种：
        //(1)一次性的计时器
        //创建一个一次性的，周期为10秒的计时器
        EasyTimerManager.Instance.Create()
            .SetDuration(10)
            .OnAllLoopFinish(t =>
            {
                Debug.Log("[Once Timer]Time over");
            });
        
        //(2)持续性的计时器(在应用关闭后，计时器也会正常工作)
        //创建一个持续性的计时器，注意不要重复创建相同id的计时器
        var createTimerFlag = PlayerPrefs.GetInt("CreateTimer", 1) == 1;
        var timerName = "PersistentTimer";
        if (createTimerFlag)
        {
            _persistentTimer = EasyTimerManager.Instance.Create(timerName)
                .SetDuration(24 * 60 * 60)
                .SetPersistent(true);
            PlayerPrefs.SetInt("CreateTimer", 0);
            PlayerPrefs.Save();
        }
        else
        {
            _persistentTimer = EasyTimerManager.Instance.Get(timerName);
        }
        _persistentTimer?.OnUpdate(t =>
        {
            Debug.Log($"[Persistent Timer]{t.RestTimeInFormat_HMS}");
        });

        //以上可简写为：
        EasyTimerManager.Instance.GetOrCreate(timerName, createTimerFlag)
            .OnCreate(t =>
            {
                t.SetDuration(24 * 60 * 60);
                t.SetPersistent(true);
                PlayerPrefs.SetInt("CreateTimer", 0);
                PlayerPrefs.Save();
            })?.OnUpdate(t => { Debug.Log(t.RestTimeInFormat_HMS); });
```
