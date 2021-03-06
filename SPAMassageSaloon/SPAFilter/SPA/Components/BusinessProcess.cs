using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Utils;
using Utils.WinForm.DataGridViewHelper;

namespace SPAFilter.SPA.Components
{
	public sealed class BusinessProcess : DriveTemplate
	{
		internal List<string> Operations { get; }

		[DGVColumn(ColumnPosition.After, "Process")]
		public override string Name { get; set; }

		/// <summary>
		/// Если ли ли вызов сервис каталога
		/// </summary>
		public bool HasCatalogCall { get; }

		/// <summary>
		/// Все операции которые прописаны в бизнесспроцессе существуют
		/// </summary>
		public bool AllOperationsExist { get; internal set; } = true;

		BusinessProcess(string filePath, List<string> operations, bool hasCatalogCall) : base(filePath)
		{
			if (GetNameWithId(Name, out var newName, out var newId))
			{
				Name = newName;
				ID = newId;
			}

			Operations = operations;
			HasCatalogCall = hasCatalogCall;
		}

		public static bool IsBusinessProcess(string filePath, out BusinessProcess bpResult)
		{
			bpResult = null;
			var document = XML.LoadXml(filePath);
			if (document == null || document.SelectNodes(@"/BusinessProcessData")?.Count == 0)
				return false;

			var operations = new List<string>();
			var getOperations = document.SelectNodes(@"//param[@name='operation']/@value");
			if (getOperations != null)
			{
				foreach (XmlNode xm in getOperations)
				{
					if (!operations.Any(p => p.Equals(xm.InnerText)))
						operations.Add(xm.InnerText);
				}
			}

			var navigator = document.CreateNavigator();
			var hasCatalogCall = false;
			var isExistscObject = XPATH.Select(navigator, @"/BusinessProcessData/businessprocess/scenario/objectlist/object[@class='FORIS.ServiceProvisioning.BPM.SCProcessingUnit']/@name");
			if (isExistscObject != null)
			{
				foreach (var obj in isExistscObject)
				{
					var res = XPATH.Select(navigator, $"/BusinessProcessData/businessprocess/scenario/automat/node[@object='{obj.Value}']");
					if (res == null || res.Count <= 0)
						continue;

					hasCatalogCall = true;
					break;
				}
			}

			if (operations.Count == 0 && !hasCatalogCall)
				return false;

			bpResult = new BusinessProcess(filePath, operations, hasCatalogCall);
			return true;
		}
	}
}