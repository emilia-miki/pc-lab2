class ThreadObjectTest : ITest
{
    private const int minimumSleepTime = 100;

    private ThreadObject _threadObject = null!;
    private Thread _thread = null!;

    public void SetUp()
    {

        _threadObject = new ThreadObject();
        _thread = new Thread(_threadObject.ThreadProc);
        _thread.Start();
    }

    public void TearDown()
    {
        _threadObject.Terminate();
        _thread.Join();
    }

    // TODO: do everything with cancellation tokens and callbacks
    private bool CompleteTaskCheck()
    {
        var completed = false;
        _threadObject.AddTask(() => completed = true);
        Thread.Sleep(minimumSleepTime);
        return completed;
    }

    public void CompletesTasks()
    {
        if (!CompleteTaskCheck())
        {
            throw new Exception(
                "ThreadObject should complete added tasks");
        }
    }

    public void DiscardsTasksWhenBusy()
    {
        var cts = new CancellationTokenSource();
        _threadObject.AddTask(() => cts.Token.WaitHandle.WaitOne());
        Thread.Sleep(minimumSleepTime / 2);

        for (var i = 0; i < 4; i++)
        {
            if (CompleteTaskCheck())
            {
                throw new Exception(
                    "ThreadObject should discard new tasks when busy");
            }
        }
        
        cts.Cancel();
        Thread.Sleep(minimumSleepTime);
        cts.Dispose();
        
        if (!CompleteTaskCheck())
        {
            throw new Exception(
                "ThreadObject should accept and complete new tasks " +
                "after finishing the previous one");
        }
    }

    public void SleepWhenBusy()
    {
        _threadObject.AddTask(() => Thread.Sleep(minimumSleepTime * 2));
        Thread.Sleep(minimumSleepTime);

        _threadObject.Sleep();

        for (var i = 0; i < 3; i++)
        {
            if (CompleteTaskCheck())
            {
                throw new Exception(
                    "ThreadObject should not execute new tasks while sleeping");
            }
        }

        _threadObject.Resume();

        Thread.Sleep(minimumSleepTime);
        if (!CompleteTaskCheck())
        {
            throw new Exception(
                "ThreadObject should execute new tasks after waking");
        }
    }

    public void SleepWhenIdle()
    {
        _threadObject.Sleep();
        Thread.Sleep(minimumSleepTime);
        
        if (CompleteTaskCheck())
        {
            throw new Exception("ThreadObject should not execute new tasks while sleeping");
        }

        _threadObject.Resume();
        Thread.Sleep(minimumSleepTime);

        if (!CompleteTaskCheck())
        {
            throw new Exception("ThreadObject should accept new tasks after waking");
        }
    }

    public void TerminateWhenBusy()
    {
        _threadObject.AddTask(() => Thread.Sleep(minimumSleepTime * 3));
        Thread.Sleep(minimumSleepTime);

        _threadObject.Terminate();
        Thread.Sleep(minimumSleepTime);

        if (!_thread.IsAlive)
        {
            throw new Exception(
                "ThreadObject should have completed the task before terminating");
        }

        Thread.Sleep(minimumSleepTime * 2);
        if (_thread.IsAlive)
        {
            throw new Exception(
                "ThreadObject should have terminated");
        }
    }

    public void TerminateWhenIdle()
    {
        _threadObject.Terminate();
        Thread.Sleep(minimumSleepTime);
        if (_thread.IsAlive)
        {
            throw new Exception("ThreadObject should have terminated");
        }
    }
}