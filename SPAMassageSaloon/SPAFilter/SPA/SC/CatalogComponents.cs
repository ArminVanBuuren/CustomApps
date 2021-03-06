using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;
using Utils;

namespace SPAFilter.SPA.SC
{
	public class CatalogComponents
	{
		private readonly List<HostOperation> _hostOperations = new List<HostOperation>();
		public Dictionary<string, CFS> CollectionCFS { get; } = new Dictionary<string, CFS>();
		public Dictionary<string, RFS> CollectionRFS { get; } = new Dictionary<string, RFS>();
		public Dictionary<string, Resource> CollectionResource { get; } = new Dictionary<string, Resource>();
		public Dictionary<string, RFSGroup> CollectionRFSGroup { get; } = new Dictionary<string, RFSGroup>();
		public CFSGroups CollectionMutexCFSGroup { get; } = new CFSGroups();

		private Func<string, string> GetDescriptionFunc { get; }

		private DataTable ServiceTable { get; }

		public CatalogComponents(DataTable serviceTable)
		{
			ServiceTable = serviceTable;
			if (ServiceTable != null)
				GetDescriptionFunc = GetDescription;
		}

		public void Add(string fileName, XmlDocument document, string hostType)
		{
			var navigator = document.CreateNavigator();

			var operationName = fileName;
			foreach (var prefix in new[]
			{
				"Add",
				"Assign",
				"Delete",
				"Del",
				"Remove",
				"FR",
				"CB"
			})
			{
				RemovePrefix(ref operationName, prefix);
			}

			operationName = operationName.Replace(" ", "");

			var bindSrv = new BindingServices(navigator);
			var hostOp = new HostOperation(operationName, hostType, bindSrv);
			var getServices = new Dictionary<string, XPathResult>();

			if (navigator.Select("//ProvisionList/*", out var provisionList) && GetServices(getServices, provisionList))
			{
				if (!IsExistSameHostOperation(hostOp, getServices, LinkType.Add))
					LoadNewService(hostOp, getServices, LinkType.Add); //bindSrv
			}
			else if (navigator.Select("//WithdrawalList/*", out var withdrawalList) && GetServices(getServices, withdrawalList))
			{
				if (!IsExistSameHostOperation(hostOp, getServices, LinkType.Remove))
					LoadNewService(hostOp, getServices, LinkType.Remove); //bindSrv
			}
		}

		static void RemovePrefix(ref string operationName, string prefix)
		{
			if (operationName.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
			{
				operationName = operationName.Substring(prefix.Length, operationName.Length - prefix.Length);
			}
		}

		static bool GetServices(IDictionary<string, XPathResult> services, ICollection<XPathResult> result)
		{
			if (result == null || result.Count == 0)
				return false;

			foreach (var res in result)
			{
				if (res.NodeName.Like("Include"))
					continue;

				if (BindingServices.isAttributeContains(res, "Type", "Restricted"))
					continue;

				if (!services.ContainsKey(res.NodeName))
					services.Add(res.NodeName, res);
			}

			return result.Count > 0;
		}

		bool IsExistSameHostOperation(HostOperation newHostOp, Dictionary<string, XPathResult> newServices, LinkType type)
		{
			foreach (var existhostOp in _hostOperations)
			{
				//string xmlTEMP = string.Join("\r\n\r\n", newHostOp.XML_BODY) + "\r\n\r\n" + string.Join("\r\n\r\n", existhostOp.XML_BODY);
				if (existhostOp.HostOperationName.Like(newHostOp.HostOperationName))
				{
					foreach (var srvCode in newServices.Values)
					{
						if (existhostOp.ChildCFS.TryGetValue(srvCode.NodeName, out var cfs_rfs))
						{
							cfs_rfs.ChangeLinkType(type);
						}
						else
						{
							AddCFS(srvCode, existhostOp, type);
						}
					}

					newServices.Clear();
					existhostOp.CombineSameHostOperation(newHostOp);
					return true;
				}

				if (!existhostOp.HostType.Like(newHostOp.HostType))
					continue;

				var serviceForRemove = new List<string>();
				foreach (var srvCode in newServices)
				{
					if (existhostOp.ChildCFS.TryGetValue(srvCode.Key, out var cfs_rfs))
					{
						cfs_rfs.ChangeLinkType(type);
						serviceForRemove.Add(srvCode.Key);
					}
				}

				if (serviceForRemove.Count > 0)
				{
					foreach (var serviceCode in serviceForRemove)
					{
						newServices.Remove(serviceCode);
					}

					existhostOp.CombineSameHostOperation(newHostOp);
				}
			}

			return newServices.Count == 0;
		}

		void LoadNewService(HostOperation hostOp, Dictionary<string, XPathResult> srvCodeList, LinkType link) //BindingServices bindSrv
		{
			//var resHostOp = new Dictionary<HostOperation, List<RFS>>();
			foreach (var srvCode in srvCodeList.Values)
			{
				AddCFS(srvCode, hostOp, link);
			}

			if (hostOp.ChildCFS.Count > 0)
				_hostOperations.Add(hostOp);
		}

		void AddCFS(XPathResult srvCode, HostOperation hostOp, LinkType link)
		{
			if (!CollectionCFS.TryGetValue(srvCode.NodeName, out var getExistCFS))
			{
				var description = GetDescriptionFunc?.Invoke(srvCode.NodeName);
				description = string.IsNullOrEmpty(description) ? "-" : description;

				var createNewCFS = new CFS(srvCode.NodeName, description, hostOp, link);
				CollectionCFS.Add(srvCode.NodeName, createNewCFS);
			}
			else
			{
				getExistCFS.IsNewHost(hostOp, link);
			}
		}

		string GetDescription(string serviceCode)
		{
			var results = from myRow in ServiceTable.AsEnumerable()
			              where myRow.Field<string>("SERVICE_CODE") == serviceCode
			              select myRow["SERVICE_NAME"];

			return results.Any() ? NormalizeString(results.First().ToString()) : null;
		}

		private string NormalizeString(string input)
		{
			var source = XML.RemoveUnallowable(input);
			var builder = new StringBuilder(source.Length + 10);

			foreach (var ch in source)
			{
				if (GetNameByCharLight(ch, out var res))
				{
					builder.Append(res);
					continue;
				}

				builder.Append(ch);
			}

			return builder.ToString();
		}

		public static bool GetNameByCharLight(char symbol, out string result)
		{
			result = null;
			switch (symbol)
			{
				case '&': result = "&amp;"; break;
				case '\"': result = "&quot;"; break;
				case '<': result = "&lt;"; break;
				case '>': result = "&gt;"; break;
				case 'ˆ': result = ""; break;
				case '˜': result = ""; break;
				case '–': result = "-"; break;
				case '—': result = "-"; break;
				case '‘': result = "'"; break;
				case '’': result = "'"; break;
				case '‹': result = "'"; break;
				case '›': result = "'"; break;
				case '\r': result = ""; break;
				case '\n': result = " "; break;
				default: return false;
			}
			return true;
		}

		public void GenerateRFS()
		{
			foreach (var hostOp in _hostOperations)
			{
				hostOp.GenerateRFS();
			}
		}

		public string ToXml()
		{
			var cfsXmlList = new StringBuilder();
			var rfsXmlList = new StringBuilder();
			var resourceXmlList = new StringBuilder();
			var rfsGroupXmlList = new StringBuilder();

			foreach (var cfs in CollectionCFS.Values)
			{
				cfsXmlList.Append(cfs.ToXml(this));

				foreach (var rfs in cfs.RFSList)
				{
					if (!CollectionRFS.ContainsKey(rfs.Name))
						CollectionRFS.Add(rfs.Name, rfs);
				}
			}

			foreach (var rfs in CollectionRFS.Values.OrderBy(p => p.HostType).ThenBy(x => x.Name))
			{
				rfsXmlList.Append(rfs.ToXml());
				if (rfs.SC_Resource != null && !CollectionResource.ContainsKey(rfs.SC_Resource.Name))
				{
					CollectionResource.Add(rfs.SC_Resource.Name, rfs.SC_Resource);
					resourceXmlList.Append(rfs.SC_Resource.ToXml());
				}
			}

			// коллекция RFSGroup должна быть в конце т.к. она формируется только после того когда все RFS созданы
			foreach (var rfsGroup in CollectionRFSGroup.Values)
			{
				rfsGroupXmlList.Append(rfsGroup.ToXml());
			}

			var result = "<ResourceList>" + resourceXmlList + "</ResourceList>"
			           + "<RFSList>" + rfsXmlList + "</RFSList>"
			           + "<CFSList>" + cfsXmlList + "</CFSList>"
			           + "<CFSGroupList>" + CollectionMutexCFSGroup.ToXml() + "</CFSGroupList>"
			           + "<RFSGroup>" + rfsGroupXmlList + "</RFSGroup>";

			return result;
		}
	}
}