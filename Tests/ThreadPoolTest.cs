class ThreadPoolTest : ITest
{
	private const int threadCount = 6;
	private const int maxTaskDuration = 400;

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

		Thread.Sleep(maxTaskDuration / 5);
		
		foreach (var result in taskResults)
		{
			if (!result.Running)
			{
				throw new Exception(
					"The tasks should all be running concurrently");
			}
		}

		Thread.Sleep(6 * maxTaskDuration / 5);

		foreach (var result in taskResults)
		{
			if (result.Running || !result.Completed)
			{
				throw new Exception(
					"The tasks should have all been completed by now");
			}
		}
	}

	public void DiscardTasksWhenFull()
	{
		for (var i = 0; i < threadCount; i++)
		{
			_threadPool.AddTask(() => Thread.Sleep(200));
		}

		var completed = false;
		_threadPool.AddTask(() => completed = true);

		Thread.Sleep(300);

		if (completed)
		{
			throw new Exception(
				"New tasks should be discarded when the thread pool is full");
		}
	}

	public void StateManagement()
	{
		_threadPool.Sleep();

		var taskResults = new TaskResult[threadCount];
		for (var i = 0; i < threadCount; i++)
		{
			taskResults[i] = new TaskResult();
		}
		
		foreach (var result in taskResults)
		{
			_threadPool.AddTask(() => Task(result));
		}
		Thread.Sleep(50);

		foreach (var result in taskResults)
		{
			if (result.Running || result.Completed)
			{
				throw new Exception(
					"Tasks shouldn't start running when the thread pool is sleeping");
			}
		}

		_threadPool.Resume();
		Thread.Sleep(50);

		var discardedTaskResults = new List<TaskResult>();
		for (var i = 0; i < threadCount * 2; i++)
		{
			var result = new TaskResult();
			_threadPool.AddTask(() => Task(result));
			discardedTaskResults.Add(result);
		}

		Thread.Sleep(50);

		foreach (var result in discardedTaskResults)
		{
			if (result.Completed || result.Running)
			{
				throw new Exception(
					"Tasks should be discarded when all threads are busy");
			}
		}

		Thread.Sleep(maxTaskDuration);

		foreach (var result in taskResults)
		{
			if (result.Running || !result.Completed)
			{
				throw new Exception(
					"Tasks should execute after resuming the thread pool");
			}
		}

		var taskResult = new TaskResult();
		_threadPool.AddTask(() => Task(taskResult));
		Thread.Sleep(50);

		_threadPool.Terminate();
		Thread.Sleep(maxTaskDuration);

		if (!taskResult.Completed)
		{
			throw new Exception(
				"Tasks should execute when termination is requested");
		}

		var completed = false;
		_threadPool.AddTask(() => completed = true);
		Thread.Sleep(50);
		if (completed)
		{
			throw new Exception(
				"Thread pool should not execute new tasks after termination");
		}
	}
}
