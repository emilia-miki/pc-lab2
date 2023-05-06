// a pool of 6 threads
// if all threads are busy the task is discarded
// a task has a random length of 8-12s
// can realize 6 threads of queues of tasks

var test = new Test();
test.Register(new CountDownTest(6));
test.Register(new TaskQueueTest());
test.Register(new ThreadObjectTest());
test.Register(new ThreadPoolTest());
test.Run();

static void Task()
{
    var rand = new Random();
    var duration = 8000 + rand.Next(4001);
    Console.WriteLine($"task began ({duration} ms)");
    Thread.Sleep(duration);
    Console.WriteLine("task ended");
}

