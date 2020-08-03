namespace LogsReader.Reader
{
	public enum TraceReaderStatus
	{
		Waiting = 0,
		Processing = 1,
		Aborted = 2,
		Failed = 4,
		Finished = 8
	}
}
