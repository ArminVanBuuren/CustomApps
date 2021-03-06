using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using SPAFilter.Properties;
using Utils;
using Utils.CollectionHelper;

namespace SPAFilter.SPA.Components.SRI
{
	internal class RFSBindings
	{
		public CatalogOperation Base { get; }
		public DistinctList<XmlNode> HostTypeList { get; } = new DistinctList<XmlNode>();
		public DistinctList<XmlNode> ResourceList { get; } = new DistinctList<XmlNode>();
		public DistinctList<XmlNode> RFSParameterList { get; } = new DistinctList<XmlNode>();
		public DistinctList<XmlNode> RFSList { get; } = new DistinctList<XmlNode>();
		public DistinctList<XmlNode> CFSList { get; private set; } = new DistinctList<XmlNode>(new CatalogItemEqualityComparer());
		public DistinctList<XmlNode> CFSGroupList { get; } = new DistinctList<XmlNode>();
		public DistinctList<XmlNode> RestrictionList { get; } = new DistinctList<XmlNode>();
		public DistinctList<XmlNode> RFSGroupList { get; } = new DistinctList<XmlNode>();
		public DistinctList<XmlNode> HandlerList { get; } = new DistinctList<XmlNode>();
		public DistinctList<XmlNode> RFSDependencyList { get; } = new DistinctList<XmlNode>();
		public DistinctList<XmlNode> ScenarioList { get; } = new DistinctList<XmlNode>();

		internal RFSBindings(CatalogOperation baseOperation) => Base = baseOperation;

		internal RFSBindings(CatalogOperation baseOperation, string rfsName, XPathNavigator navigator)
		{
			Base = baseOperation;

			BuildRFSList(XPATH.Select(navigator, $"/Configuration/RFSList/RFS[@name='{rfsName}']"));
			BuildRFSList(XPATH.Select(navigator, $"/Configuration/RFSList/RFS[@base='{rfsName}']"));
			BuildRFSList(XPATH.Select(navigator, $"/Configuration/RFSList/RFS[@parent='{rfsName}']"));

			foreach (var rfs in RFSList)
			{
				var name = rfs.Attributes?["name"]?.Value;
				if (name == null)
					continue;

				Add(name, navigator);
			}
		}

		void BuildRFSList(IList<XPathResult> collection)
		{
			if (collection == null || collection.Count == 0)
				return;

			foreach (var rfs in collection)
			{
				RFSList.Add(rfs.Node);
			}
		}

		public void CombineWith(RFSBindings foreignBindings)
		{
			HostTypeList.AddRange(foreignBindings.HostTypeList);
			ResourceList.AddRange(foreignBindings.ResourceList);
			RFSParameterList.AddRange(foreignBindings.RFSParameterList);
			RFSList.AddRange(foreignBindings.RFSList);
			CFSList.AddRange(foreignBindings.CFSList);
			CFSGroupList.AddRange(foreignBindings.CFSGroupList);
			RestrictionList.AddRange(foreignBindings.RestrictionList);
			RFSGroupList.AddRange(foreignBindings.RFSGroupList);
			RFSDependencyList.AddRange(foreignBindings.RFSDependencyList);
			HandlerList.AddRange(foreignBindings.HandlerList);
			ScenarioList.AddRange(foreignBindings.ScenarioList);
		}

		void Add(string rfsName, XPathNavigator navigator)
		{
			try
			{
				if (!navigator.SelectFirst($"/Configuration/RFSList/RFS[@name='{rfsName}']", out var rfs))
					throw new Exception(string.Format(Resources.ServiceCatalog_NotFound, rfsName));

				var rfsHostType = rfs.Node.Attributes?["hostType"]?.Value;
				if (rfsHostType == null)
					throw new Exception(string.Format(Resources.ServiceCatalog_NoHostTypeAttr, rfsName));

				AddXmlNode(XPATH.Select(navigator, $"/Configuration/HostTypeList/HostType[@name='{rfsHostType}']"), HostTypeList);

				foreach (XmlNode rfsChild in rfs.Node.ChildNodes)
				{
					var itemName = rfsChild.Attributes?["name"]?.Value;
					switch (rfsChild.Name)
					{
						case "Resource" when itemName != null:
							AddXmlNode(XPATH.Select(navigator, $"/Configuration/ResourceList/Resource[@name='{itemName}']"), ResourceList);
							break;
						case "RFSParameter" when itemName != null:
							AddXmlNode(XPATH.Select(navigator, $"/Configuration/RFSParameterList/RFSParameter[@name='{itemName}']"), RFSParameterList);
							break;
					}
				}

				AddXmlNode(XPATH.Select(navigator, $"/Configuration/RestrictionList/Restriction[RFS[@name='{rfsName}']]"), RestrictionList);
				AddXmlNode(XPATH.Select(navigator, $"/Configuration/ScenarioList/Scenario[RFS[@name='{rfsName}'] or Append[@name='{rfsName}']]"), ScenarioList);

				var rfsGroupsCFSXPath = string.Empty;
				AddXmlNode(XPATH.Select(navigator, $"/Configuration/RFSGroupList/RFSGroup[RFS[@name='{rfsName}']]"), RFSGroupList);
				foreach (var rfsGroup in RFSGroupList)
				{
					var rfsGroupName = rfsGroup.Attributes?["name"]?.Value;
					if (!string.IsNullOrEmpty(rfsGroupName))
						rfsGroupsCFSXPath += $" | /Configuration/CFSList/CFS[RFSGroup[@name='{rfsGroupName}']]";
				}

				var isMarkesCFSList = new List<string>();
				if (navigator.Select($"/Configuration/CFSList/CFS[RFS[@name='{rfsName}']]{rfsGroupsCFSXPath}", out var cfsListConfig))
				{
					foreach (var cfs in cfsListConfig)
					{
						var cfsName = cfs.Node.Attributes?["name"]?.Value;
						var isMarker = cfs.Node.Attributes?["isMarker"]?.Value;

						if (cfsName == null)
							throw new Exception(string.Format(Resources.ServiceCatalog_NoNameAttribute, "CFS"));

						AddXmlNode(XPATH.Select(navigator, $"/Configuration/CFSGroupList/CFSGroup[CFS[@name='{cfsName}']]"), CFSGroupList);

						if (isMarker != null && isMarker.Like("true"))
						{
							isMarkesCFSList.Add(cfsName);
						}

						var cloneCFS = cfs.Node.Clone();
						for (var index = 0; index < cloneCFS.ChildNodes.Count; index++)
						{
							var childNode = cloneCFS.ChildNodes[index];
							var childNodeName = childNode.Attributes?["name"]?.Value;
							switch (childNode.Name)
							{
								case "RFS":
									if (navigator.SelectFirst($"/Configuration/RFSList/RFS[@name='{childNodeName}']/@hostType", out var hostType1)
									 && hostType1.Value.Equals(rfsHostType))
										continue;
									break;
								case "RFSGroup":
									if (navigator.SelectFirst($"/Configuration/RFSGroupList/RFSGroup[@name='{childNodeName}']", out var rfsGroupList))
									{
										var isExist = false;
										foreach (XmlNode rfsGroupChild in rfsGroupList.Node.ChildNodes)
										{
											var childRfsName = rfsGroupChild.Attributes?["name"]?.Value;
											if (!childRfsName.IsNullOrEmpty()
											 && navigator.SelectFirst($"/Configuration/RFSList/RFS[@name='{childRfsName}']/@hostType", out var hostType2)
											 && hostType2.Value.Equals(rfsHostType))
											{
												isExist = true;
												break;
											}
										}

										if (isExist)
										{
											RFSGroupList.Add(rfsGroupList.Node);
											continue;
										}
									}

									break;
							}

							cloneCFS.RemoveChild(childNode);
							index--;
						}

						CFSList.Add(cloneCFS);
					}
				}

				if (isMarkesCFSList.Count > 0 && navigator.Select($"/Configuration/CFSList/CFS[@copyOf!='']", out var copiedCFSList))
				{
					foreach (var copiedCFS in copiedCFSList)
					{
						var copyOfList = copiedCFS.Node.Attributes?["copyOf"]?.Value;
						if (copyOfList == null)
							continue;

						if (copyOfList.Split(',').Any(x => isMarkesCFSList.Any(p => p.Equals(x.Trim()))))
						{
							CFSList.Add(copiedCFS.Node);
							var cfsName = copiedCFS.Node.Attributes?["name"]?.Value;
							if (cfsName != null)
							{
								AddXmlNode(XPATH.Select(navigator, $"/Configuration/CFSGroupList/CFSGroup[CFS[@name='{cfsName}']]"), CFSGroupList);
							}
						}
					}
				}

				AddXmlNode(XPATH.Select(navigator, $"/Configuration/RFSDependencyList[RFSDependency/@parent='{rfsName}']/* | /Configuration/RFSDependencyList[RFSDependency/@child='{rfsName}']/*"), RFSDependencyList);

				foreach (var rfsGroup in RFSGroupList)
				{
					foreach (XmlNode rfsGroupChild in rfsGroup.ChildNodes)
					{
						var handlerName = rfsGroupChild.Attributes?["handler"]?.Value;
						if (handlerName != null && navigator.SelectFirst($"/Configuration/HandlerList/Handler[@name='{handlerName}']", out var handler))
						{
							HandlerList.Add(handler.Node);
						}
					}
				}

				AddXmlNode(XPATH.Select(navigator, $"/Configuration/HandlerList/Handler[Configuration/RFS[@name='{rfsName}']]"), HandlerList);

				//AddXmlNode(XPATH.Select(navigator, $"/Configuration/RFSGroupList/RFSGroup[RFS[@name='{rfsName}']]"), RFSGroupList);
				//CFSList = new DistinctList<XmlNode>(CFSList.OrderBy(p => p.Attributes?["name"]?.Value), new CatalogItemEqualityComparer());
			}
			catch (Exception ex)
			{
				// ignored
			}
		}

		static void AddXmlNode(IList<XPathResult> collection, ICollection<XmlNode> catalogComponent)
		{
			if (collection == null || collection.Count <= 0)
				return;

			foreach (var item in collection)
			{
				catalogComponent.Add(item.Node);
			}
		}
	}
}