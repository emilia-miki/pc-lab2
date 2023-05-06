class ThreadPool : IDisposable
{
    private int _threadCount;

    private ThreadObject[] _threadObjects;
    private Thread[] _threads;
    private bool _isTerminated = false;

    private object _lockObj = new object();
    private object _sleepLockObj = new object();
    private CountDown _countDown;

    private int _discardedTasks = 0;
    private int _totalTasks = 0;

    public ThreadPool(int threadCount = 6)
    {
        _threadCount = threadCount;

        _threadObjects = new ThreadObject[_threadCount];
        _countDown = new CountDown(_threadCount);
        _threads = new Thread[threadCount];
        
        for (var i = 0; i < _threadCount; i++)
        {
            _threadObjects[i] = new ThreadObject(i, _countDown, _sleepLockObj);
            _threads[i] = new Thread(_threadObjects[i].ThreadProc);
            _threads[i].Start();
        }
    }

    public void AddTask(ThreadStart task)
    {
        lock (_lockObj)
        {
            if (_isTerminated)
            {
                return;
            }

            var discarded = true;
            for (var i = 0; i < _threadCount; i++)
            {
                if (_threadObjects[i].AddTask(task))
                {
                    discarded = false;
                    break;
                }
            }

            if (discarded)
            {
                _discardedTasks += 1;
            }

            _totalTasks += 1;
        }
    }

    public void Resume()
    {
        lock (_lockObj)
        {
            if (_isTerminated)
            {
                return;
            }

            lock (_sleepLockObj)
            {
                Monitor.PulseAll(_sleepLockObj);
            }
        }
    }

    public void Sleep()
    {
        lock (_lockObj)
        {
            if (_isTerminated)
            {
                return;
            }

            foreach (var threadObject in _threadObjects)
            {
                threadObject.Sleep();
            }
        }
    }

    public void Terminate()
    {
        lock (_lockObj)
        {
            if (_isTerminated)
            {
                return;
            }

            _isTerminated = true;

            foreach (var threadObject in _threadObjects)
            {
                threadObject.Terminate();
            }

            _countDown.Wait();
        }

        long meanWaitTime = 0;
        long meanSleepTime = 0;
        long meanTerminationTime = 0;
        long meanWorkTime = 0;

        foreach (var threadObject in _threadObjects)
        {
            meanWaitTime += threadObject.SwWait!.ElapsedMilliseconds;
            meanSleepTime += threadObject.SwSleep!.ElapsedMilliseconds;
            meanTerminationTime += threadObject.SwTermination!.ElapsedMilliseconds;
            meanWorkTime += threadObject.SwWork!.ElapsedMilliseconds / threadObject.TaskCounter;
        }

        meanWaitTime /= _threadCount;
        meanSleepTime /= _threadCount;
        meanTerminationTime /= _threadCount;
        meanWorkTime /= _threadCount;

        Console.WriteLine("ThreadPool terminated.");
        Console.WriteLine($"Mean wait time: {meanWaitTime}");
        Console.WriteLine($"Mean sleep time: {meanSleepTime}");
        Console.WriteLine($"Mean termination time: {meanTerminationTime}");
        Console.WriteLine($"Mean work time: {meanWorkTime}");
        Console.WriteLine($"Count of discarded tasks: {_discardedTasks}/{_totalTasks}");
    }

    public void Dispose()
    {
        lock (_lockObj)
        {
            if (!_isTerminated)
            {
                Terminate();
            }

            foreach (var threadObject in _threadObjects)
            {
                threadObject.Dispose();
            }
        }
    }
}
