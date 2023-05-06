class CountDown
{
	private object _lockObj = new object();
	private int _countDown;

	public CountDown(int countDown) {
		_countDown = countDown;
	}

	public void Signal()
	{
		lock (_lockObj)
		{
			_countDown -= 1;
			if (_countDown == 0)
			{
				Monitor.PulseAll(_lockObj);
			}
		}
	}

	public void Wait()
	{
		lock (_lockObj)
		{
			Monitor.Wait(_lockObj);
		}
	}
}
