using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SPAFilter.SPA.SC
{
	public sealed class RFSGroup : SComponentBase
	{
		public string Type { get; }
		public IEnumerable<string> DependenceRFSList { get; }

		public RFSGroup(string type, IEnumerable<string> listOfRFS)
		{
			Name = "RFS_GROUP";
			Description = "Группа RFS";
			Type = type;
			DependenceRFSList = listOfRFS;
			foreach (var rfsName in DependenceRFSList)
			{
				if (rfsName.StartsWith("RFS_", StringComparison.InvariantCultureIgnoreCase))
					Name += "_" + rfsName.Substring(4, rfsName.Length - 4);
				else
					Name += "_" + rfsName;
			}
		}

		public override string ToXml()
		{
			var header = $"<RFSGroup name=\"{Name}\" type=\"{Type}\" description=\"{Description}\">";
			var rfsList = new StringBuilder();
			foreach (var rfsName in DependenceRFSList)
			{
				rfsList.Append($"<RFS name=\"{rfsName}\" linkType=\"Add\" />");
			}

			return header + rfsList + "</RFSGroup>";
		}

		public override string ToString()
			=> $"Type = {Type} DependenceRFS = \"{string.Join(",", DependenceRFSList.OrderBy(p => p))}\"";
	}
}