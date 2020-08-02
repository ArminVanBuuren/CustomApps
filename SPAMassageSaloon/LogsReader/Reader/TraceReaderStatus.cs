namespace LogsReader.Reader
{
	public enum TraceReaderStatus
	{
		Waiting = 0,
		Processing = 1,
		Cancelled = 2,
		Failed = 4,
		Finished = 8
	}
}
