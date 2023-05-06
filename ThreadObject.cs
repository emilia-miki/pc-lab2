using System.Diagnostics;

class ThreadObject : IDisposable
{
    private int _id;

    public Stopwatch? SwSleep { get; }
    public Stopwatch? SwWait { get; }
    public Stopwatch? SwWork { get; }
    public Stopwatch? SwTermination { get; }
    public int TaskCounter { get => _taskCounter; }

    private TaskQueue _queue;
    private int _taskCounter = 0;
    private bool _isSleeping = false;
    private bool _isTerminated = false;
    private bool _isDisposed = false;
    private ThreadStart _emptyTask = () => { };

    private CountDown _countDown;
    private object _sleepLockObj;

    public ThreadObject(int id, CountDown countDown, object sleepLockObj)
    {
        _id = id;

        _countDown = countDown;
        _sleepLockObj = sleepLockObj;

        _queue = new TaskQueue(_id);

        SwSleep = new Stopwatch();
        SwWork = new Stopwatch();
        SwTermination = new Stopwatch();
        SwWait = new Stopwatch();
    }

    public void ThreadProc()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException("ThreadObject");
        }

        while (!_isTerminated)
        {
            Debug.WriteLine($"Worker {_id}: getting a task");
            SwWait?.Start();
            var task = _queue.GetTask();
            SwWait?.Stop();

            if (task != _emptyTask)
            {
                _taskCounter += 1;

                Debug.WriteLine($"Worker {_id}: executing a task");
                SwWork?.Start();
                task();
                SwWork?.Stop();
            }
            else
            {
                Debug.WriteLine($"Worker {_id}: received empty task");
            }

            SwSleep?.Start();
            if (_isSleeping)
            {
                Debug.WriteLine($"Worker {_id}: sleeping");
                lock (_sleepLockObj)
                {
                    Monitor.Wait(_sleepLockObj);
                }

                Debug.WriteLine($"Worker {_id}: waking up");
                _isSleeping = false;
            }
            SwSleep?.Stop();

            Debug.WriteLine($"Worker {_id}: accepting new tasks");
            _queue.AcceptTasks();
        }

        _countDown.Signal();
        SwTermination?.Stop();

        Debug.WriteLine($"Worker {_id}: thread terminated");
    }

    public bool AddTask(ThreadStart task)
    {
        return _queue.AddTask(task);
    }

    public void Sleep()
    {
        _isSleeping = true;
        _queue.AddTask(_emptyTask);
    }

    public void Terminate()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException("ThreadObject");
        }

        if (_isTerminated)
        {
            return;
        }

        _isTerminated = true;

        SwTermination?.Start();
        _queue.AddTask(_emptyTask);
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _queue.Dispose();
        _isDisposed = true;
    }
}
