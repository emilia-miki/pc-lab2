using System.Diagnostics;

class ThreadObject
{
    private static int _currentId = 0;

    private int _id;

    private object _taskLockObj = new object();
    private ThreadStart? _task = null;

    private object _sleepLockObj = new object();
    private bool _isSleeping = false;

    private object _terminationLockObj = new object();
    private bool _isTerminated = false;

    public readonly Stopwatch swSleep = new Stopwatch();
    public readonly Stopwatch swWait = new Stopwatch();
    public readonly Stopwatch swWork = new Stopwatch();
    public readonly Stopwatch swTermination = new Stopwatch();
    public int TaskCounter { get => _taskCounter; }
    private int _taskCounter = 0;

    public ThreadObject()
    {
        _id = _currentId;
        _currentId += 1;
    }

    public void ThreadProc()
    {
        while (!_isTerminated)
        {
            Debug.WriteLine($"Worker {_id}: getting a task");
            swWait.Start();
            lock (_taskLockObj)
            {
                while (!_isTerminated && _task == null)
                {
                    Monitor.Wait(_taskLockObj);
                }
            }
            swWait.Stop();

            lock (_sleepLockObj)
            {
                if (!_isTerminated && _isSleeping)
                {
                    Debug.WriteLine($"Worker {_id}: sleeping");
                    swSleep.Start();
                    Monitor.Wait(_sleepLockObj);
                    swSleep.Stop();
                    Debug.WriteLine($"Worker {_id}: waking up");
                }
            }

            if (_isTerminated)
            {
                break;
            }

            Debug.WriteLine($"Worker {_id}: executing a task");
            swWork.Start();
            _task!();
            _task = null;
            swWork.Stop();
            _taskCounter += 1;
        }

        swTermination.Stop();
        Debug.WriteLine($"Worker {_id}: thread terminated");
    }

    public bool AddTask(ThreadStart task)
    {
        if (_isTerminated)
        {
            throw new Exception("The thread has been terminated");
        }

        lock (_taskLockObj)
        {
            if (_task != null)
            {
                return false;
            }

            _task = task;

            Monitor.Pulse(_taskLockObj);
        }

        return true;
    }

    public void Sleep()
    {
        if (_isTerminated)
        {
            throw new Exception("The thread has been terminated");
        }

        _isSleeping = true;
    }

    public void Resume()
    {
        if (_isTerminated)
        {
            throw new Exception("The thread has been terminated");
        }

        lock (_sleepLockObj)
        {
            _isSleeping = false;
            Monitor.Pulse(_sleepLockObj);
        }
    }

    public void Terminate()
    {
        if (_isTerminated)
        {
            return;
        }

        swTermination.Start();
        _isTerminated = true;

        lock (_taskLockObj)
        {
            Monitor.Pulse(_taskLockObj);
        }

        lock (_sleepLockObj)
        {
            _isSleeping = false;
            Monitor.Pulse(_sleepLockObj);
        }
    }
}
