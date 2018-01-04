using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFSGeneration.Control.DataBase.Datas
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
