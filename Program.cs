// a pool of 6 threads
// if all threads are busy the task is discarded
// a task has a random length of 8-12s
// can realize 6 threads of queues of tasks

var test = new Test();
test.Register(new ThreadObjectTest());
test.Register(new ThreadPoolTest());
test.Run();
