using System;

namespace TFSAssist.Control.DataBase.Datas
{
	public struct MailItem
	{
		public string ID;
		public DateTime ReceivedDate;
		public string From;
		public string[] Recipients;
		public string Subject;
		public string Body;
	}
}
