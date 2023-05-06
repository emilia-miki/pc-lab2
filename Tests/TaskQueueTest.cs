public class TaskQueueTest: ITest
{
    private const int minimumSleepTime = 100;

    private TaskQueue _taskQueue = null!;

    public void SetUp()
    {
        _taskQueue = new TaskQueue(0);
    }

    public void TearDown()
    {
        _taskQueue.Dispose();
    }

    public void AddGetTask()
    {
        var fn = new ThreadStart(() => { Thread.Sleep(minimumSleepTime); });
        _taskQueue.AddTask(fn);
        if (fn != _taskQueue.GetTask())
        {
            throw new Exception(
                "The returned task should be the one we added");
        }
    }

    public void WaitOnEmptyQueue()
    {
        var blockedThread = new Thread(() => _taskQueue.GetTask());

        blockedThread.Start();
        Thread.Sleep(minimumSleepTime);
        if (!blockedThread.IsAlive)
        {
            throw new Exception(
                "The thread should have waited for a task");
        }

        _taskQueue.AddTask(() => { });
        Thread.Sleep(minimumSleepTime);
        if (blockedThread.IsAlive)
        {
            throw new Exception(
                "The thread should have gotten a task and terminated");
        }
    }

    public void DoNotAcceptTasksWhenBusy()
    {
        // Add and get one task
        _taskQueue.AddTask(() => { });
        _taskQueue.GetTask();

        // Create a thread that will try to get a new task from the queue
        var blockedThread = new Thread(() => _taskQueue.GetTask());

        // The thread is busy; Try adding a new task (it must be discarded)
        _taskQueue.AddTask(() => { });
        blockedThread.Start();
        Thread.Sleep(minimumSleepTime);
        if (!blockedThread.IsAlive)
        {
            throw new Exception(
                "The thread getting a task should be blocked!");
        }

        _taskQueue.AcceptTasks();
        Thread.Sleep(minimumSleepTime);
        if (!blockedThread.IsAlive)
        {
            throw new Exception(
                "The thread should not have gotten any tasks " +
                "of the ones discarded");
        }

        _taskQueue.AddTask(() => { });
        Thread.Sleep(minimumSleepTime);
        if (blockedThread.IsAlive)
        {
            throw new Exception(
                "The thread should have gotten a task and terminated");
        }
    }
}
