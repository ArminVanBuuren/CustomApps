using System.Collections.Generic;

namespace SPAFilter.SPA.SC
{
	public sealed class CFS : SComponentBase
	{
		readonly Dictionary<string, List<HostOperation>> hostOperations = new Dictionary<string, List<HostOperation>>();
		public List<RFS> RFSList { get; } = new List<RFS>();

		internal CFS(string serviceCode, string description, [NotNull] HostOperation hostOp, LinkType linkType)
		{
			Name = serviceCode;
			Description = description;
			IsNewHost(hostOp, linkType);
		}

		internal bool IsNewHost([NotNull] HostOperation hostOp, LinkType linkType)
		{
			if (hostOperations.TryGetValue(hostOp.HostType, out var hostOpList))
			{
				foreach (var existHostOp in hostOpList)
				{
					if (existHostOp == hostOp)
						return false;
				}

				hostOpList.Add(hostOp);
				return false;
			}

			hostOp.AddChildRFS(this, linkType);
			hostOperations.Add(hostOp.HostType,
			                   new List<HostOperation>
			                   {
				                   hostOp
			                   });
			return true;
		}

		public string ToXml(CatalogComponents allCompontens)
		{
			var xmlStrStart = $"<CFS name=\"{Name}\" description=\"{Description}\">";
			var xmlStrMiddle = string.Empty;

			foreach (var rfs in RFSList)
			{
				xmlStrMiddle += rfs.ToXmlCFSChild(allCompontens.CollectionCFS, allCompontens.CollectionRFSGroup);
			}

			var servicesRestrictionInAllHosts = new HashSet<string>();
			foreach (var aaa in hostOperations.Values)
			{
				foreach (var bbb in aaa)
				{
					foreach (var srv in bbb.BindServices.RestrictedServices)
					{
						servicesRestrictionInAllHosts.Add(srv);
					}
				}
			}

			allCompontens.CollectionMutexCFSGroup.AddCFSGroup(Name, servicesRestrictionInAllHosts);

			return xmlStrStart + xmlStrMiddle + "</CFS>";
		}
	}
}