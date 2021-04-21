namespace SPAMassageSaloon.Common
{

	public delegate void ChangeChildLoadingHandler(ISaloonForm child, int percent, bool isCompleted);

	public interface ISaloonForm : IUserForm
	{
		int ActiveProcessesCount { get; }

		int ActiveTotalProgress { get; }
	}

	public interface IUserForm
	{
		void ApplySettings();

		void SaveData();
	}
}