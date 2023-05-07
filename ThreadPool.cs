class ThreadPool
{
    private int _threadCount;

    private object _lockObj = new object();

    private bool _isTerminated = false;

    private ThreadObject[] _threadObjects;
    private Thread[] _threads;

    private int _discardedTasks = 0;
    private int _totalTasks = 0;

    public ThreadPool(int threadCount = 6)
    {
        _threadCount = threadCount;

        _threadObjects = new ThreadObject[_threadCount];
        _threads = new Thread[threadCount];
        
        for (var i = 0; i < _threadCount; i++)
        {
            _threadObjects[i] = new ThreadObject();
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

    public void Resume()
    {
        lock (_lockObj)
        {
            if (_isTerminated)
            {
                return;
            }

            foreach (var threadObject in _threadObjects)
            {
                threadObject.Resume();
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

            foreach (var thread in _threads)
            {
                thread.Join();
            }
        }

        long meanWaitTime = 0;
        long meanSleepTime = 0;
        long meanTerminationTime = 0;
        long meanWorkTime = 0;

        foreach (var threadObject in _threadObjects)
        {
            meanWaitTime += threadObject.swWait.ElapsedMilliseconds;
            meanSleepTime += threadObject.swSleep.ElapsedMilliseconds;
            meanWorkTime += threadObject.swWork.ElapsedMilliseconds / threadObject.TaskCounter;
            meanTerminationTime += threadObject.swTermination.ElapsedMilliseconds;
        }

        meanWaitTime /= _threadCount;
        meanSleepTime /= _threadCount;
        meanTerminationTime /= _threadCount;
        meanWorkTime /= _threadCount;

        Console.WriteLine("ThreadPool terminated.");
        Console.WriteLine($"Mean wait time: {meanWaitTime} ms");
        Console.WriteLine($"Mean sleep time: {meanSleepTime} ms");
        Console.WriteLine($"Mean termination time: {meanTerminationTime} ms");
        Console.WriteLine($"Mean work time: {meanWorkTime} ms");
        Console.WriteLine($"Count of discarded tasks: {_discardedTasks} out of {_totalTasks} total");
    }
}
