using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TFSGeneration.Control.DataBase.Datas
{
	[Serializable, XmlRoot("DataCollection")]
	public class DataCollection
	{
		public TMData IsExist(MailItem info)
		{
			return Items.FirstOrDefault(field => field.ID.Equals(info.ID));
		}

		[XmlElement("Item")]
		public List<TMData> Items { get; set; } = new List<TMData>();

		public void Add(TMData item)
		{
			Items.Add(item);
		}

		public void Remove(TMData item)
		{
			Items.Remove(item);
		}

		public override string ToString()
		{
			return string.Format("Datas=[{0}]", Items.Count);
		}
	}
}
