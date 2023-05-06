class CountDownTest : ITest
{
    private const int minimumSleepTime = 100;

    private CountDown _countDown = null!;
    private int _threadCount;

    public CountDownTest(int threadCount)
    {
        if (threadCount < 2)
        {
            throw new Exception(
                $"The minimum thread count is 2. Received {threadCount}");
        }

        _threadCount = threadCount;
    }

    public void SetUp()
    {
        _countDown = new CountDown(_threadCount);
    }

    public void TearDown() {}

    public void Main()
    {
        var waitingThread = new Thread(() => _countDown.Wait());

        waitingThread.Start();
        Thread.Sleep(minimumSleepTime);
        if (!waitingThread.IsAlive)
        {
            throw new Exception(
                "The waiting thread should be alive before countdown ends");
        }

        for (var i = 0; i < _threadCount - 1; i++)
        {
            _countDown.Signal();
        }

        Thread.Sleep(minimumSleepTime);
        if (!waitingThread.IsAlive)
        {
            throw new Exception(
                "The waiting thread should not be released " +
                "before the last countdown singal");
        }

        _countDown.Signal();
        Thread.Sleep(minimumSleepTime);
        if (waitingThread.IsAlive)
        {
            throw new Exception(
                "The waiting thread should be released " +
                "after the last countdown signal");
        }
    }
}
