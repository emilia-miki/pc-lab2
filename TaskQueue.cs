class TaskQueue : IDisposable
{
    private int _id;

    private ThreadStart? _task = null;
    private bool _busy = false;
    private bool _isDisposed = false;

    private readonly object _lockObj = new object();

    public TaskQueue(int id)
    {
        _id = id;
    }

    public bool AddTask(ThreadStart task)
    {
        lock (_lockObj)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("TaskQueue");
            }

            if (_busy)
            {
                Debug.WriteLine($"Worker {_id}: queue: task discarded");

                return false;
            }

            _task = task;

            Debug.WriteLine($"Worker {_id}: queue: task added");

            Monitor.Pulse(_lockObj);
            return true;
        }
    }

    public ThreadStart GetTask()
    {
        lock (_lockObj)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("TaskQueue");
            }

            if (_task == null)
            {
                Monitor.Wait(_lockObj);
            }

            Debug.WriteLine($"Worker {_id}: queue: busy now");
            _busy = true;

            var task = _task;
            _task = null;
            return task!;
        }
    }

    public void AcceptTasks()
    {
        lock (_lockObj)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("TaskQueue");
            }

            Debug.WriteLine($"Worker {_id}: queue: not busy anymore");
            _busy = false;
        }
    }

    public void Dispose()
    {
        lock (_lockObj)
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
        }
    }
}
