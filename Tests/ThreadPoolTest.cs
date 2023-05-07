class ThreadPoolTest : ITest
{
	private const int threadCount = 6;
	private const int maxTaskDuration = 1200;

	private ThreadPool _threadPool = null!;

	public void SetUp()
	{
		_threadPool = new ThreadPool(threadCount);
	}

	public void TearDown()
	{
		_threadPool.Terminate();
	}

	private class TaskResult
	{
		public bool Running { get; set; } = false;
		public bool Completed { get; set; } = false;
	}

	private static void Task(TaskResult result)
	{
		result.Running = true;
	    var rand = new Random();

		var id = rand.Next();
	    var duration = 
			2 * maxTaskDuration / 3 +
			rand.Next(maxTaskDuration / 3 + 1);

	    Console.WriteLine($"Task {id} began ({duration} ms)");
	    Thread.Sleep(duration);
	    Console.WriteLine($"Task {id} completed");

		result.Completed = true;
		result.Running = false;
	}

	public void AddTasksToFull()
	{
		var taskResults = new TaskResult[threadCount];
		for (var i = 0; i < threadCount; i++)
		{
			taskResults[i] = new TaskResult();
		}

		foreach (var result in taskResults)
		{
			_threadPool.AddTask(() => Task(result));
		}

		Thread.Sleep(threadCount * 100);
		
		foreach (var result in taskResults)
		{
			if (!result.Running)
			{
				throw new Exception(
					"The tasks should all be running concurrently");
			}
		}

		Thread.Sleep(maxTaskDuration + threadCount * 10);

		foreach (var result in taskResults)
		{
			if (result.Running || !result.Completed)
			{
				throw new Exception(
					"The tasks should have all been completed by now");
			}
		}
	}
}
