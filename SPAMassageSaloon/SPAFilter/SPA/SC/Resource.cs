using Utils;

namespace SPAFilter.SPA.SC
{
	public sealed class Resource : SComponentBase
	{
		public HostOperation HostOperation { get; }

		internal Resource(HostOperation hostOperation)
		{
			Name = $"RES_{hostOperation.HostType}_{hostOperation.OperationName}";
			Description = $"Общий ресурс для услуг - {string.Join(",", hostOperation.ChildCFS.Keys)}";
			HostOperation = hostOperation;
		}

		public string GetBaseCFSResource() => $"<Resource name=\"{Name}\" useType=\"Mandatory\" />";

		public string GetChildCFSResource(string serviceCode)
		{
			var resourceValue = serviceCode;
			if (resourceValue.Substring(0, 2).Like("CB") || resourceValue.Substring(0, 2).Like("FR"))
			{
				resourceValue = resourceValue.Substring(2, resourceValue.Length - 2);
			}

			return $"<Resource name=\"{Name}\" value=\"{resourceValue}\" />";
		}

		public override string ToXml() => $"<Resource name=\"{Name}\" description=\"{Description}\"/>";
	}
}