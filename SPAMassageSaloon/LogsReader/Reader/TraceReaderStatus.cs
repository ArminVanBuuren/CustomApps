namespace LogsReader.Reader
{
	public enum TraceReaderStatus
	{
		Waiting = 0,
		Processing = 1,
		OnPause = 2,
		Aborted = 4,
		Failed = 8,
		Finished = 16
	}
}