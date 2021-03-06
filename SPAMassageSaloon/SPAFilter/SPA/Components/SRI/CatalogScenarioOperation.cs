using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using SPAFilter.Properties;
using SPAFilter.SPA.Collection;
using Utils;
using Utils.CollectionHelper;

namespace SPAFilter.SPA.Components.SRI
{
	public sealed class CatalogScenarioOperation : CatalogOperation
	{
		private RFSBindings _bindings;

		public override string UniqueName
		{
			get => Name;
			protected set { }
		}

		internal XmlNode ScenarioNode { get; private set; }

		internal string ScenarioName { get; private set; }

		internal override RFSBindings Bindings
		{
			get
			{
				if (_bindings != null)
					return _bindings;

				_bindings = new RFSBindings(this);
				foreach (var rfsOperation in RFSList)
				{
					_bindings.CombineWith(rfsOperation.Bindings);
				}

				foreach (var rfsOperation in AppendRFSList)
				{
					_bindings.CombineWith(rfsOperation.Bindings);
				}

				return _bindings;
			}
		}

		private bool IsInner { get; } = false;

		DistinctList<CatalogRFSOperation> RFSList { get; } = new DistinctList<CatalogRFSOperation>();

		DistinctList<CatalogRFSOperation> AppendRFSList { get; } = new DistinctList<CatalogRFSOperation>();

		public override string Body
		{
			get
			{
				var builder = new StringBuilder();
				builder.Append("<Configuration>");

				AppendXmlNode(builder, "HostTypeList", Bindings.HostTypeList);
				AppendXmlNode(builder, "ResourceList", Bindings.ResourceList);
				AppendXmlNode(builder, "RFSParameterList", Bindings.RFSParameterList);
				AppendXmlNode(builder, "RFSList", Bindings.RFSList);
				AppendXmlNode(builder, "CFSList", Bindings.CFSList);
				AppendXmlNode(builder, "CFSGroupList", Bindings.CFSGroupList);
				AppendXmlNode(builder, "RFSGroupList", Bindings.RFSGroupList);
				AppendXmlNode(builder, "HandlerList", Bindings.HandlerList);
				AppendXmlNode(builder, "RFSDependencyList", Bindings.RFSDependencyList);
				AppendXmlNode(builder, "RestrictionList", Bindings.RestrictionList);

				if (!IsInner)
				{
					builder.Append("<ScenarioList>");
					builder.Append(ScenarioNode.OuterXml);
					builder.Append("</ScenarioList>");
				}

				builder.Append("</Configuration>");
				return XML.PrintXml(builder.ToString());
			}
		}

		public CatalogScenarioOperation(XmlNode scenarioNode, string scenarioName, string rfsName, ServiceCatalog catalog)
		{
			IsInner = true;
			Preload(scenarioNode, scenarioName, catalog);

			var type = scenarioNode.Attributes?["type"]?.Value;

			if (type.IsNullOrEmpty())
				throw new Exception(string.Format(Resources.ServiceCatalog_NotFoundTyoeAttribute, rfsName, ScenarioName));

			if (!catalog.CatalogRFSCollection.TryGetValue(rfsName, out var rfsOperationList))
				return;

			foreach (var rfs in rfsOperationList.Where(p => p.LinkType.Like(type)))
			{
				rfs.ChildCFSList.Clear();
				rfs.IncludedToScenarios.Add(this);
				RFSList.Add(rfs);
			}

			PostLoad();
		}

		class ScenarioRFSList
		{
			public List<CatalogRFSOperation> MandatoryList { get; } = new List<CatalogRFSOperation>();
			public List<CatalogRFSOperation> ChildList { get; } = new List<CatalogRFSOperation>();
		}

		public CatalogScenarioOperation(XmlNode scenarioNode, string scenarioName, XPathNavigator navigator, ServiceCatalog catalog)
		{
			Preload(scenarioNode, scenarioName, catalog);

			var isDropped = scenarioNode.Attributes?["sendType"]?.Value;
			if (isDropped != null && isDropped.Like("Drop"))
				IsDropped = true;

			var scenariosRFSs = XPATH.Select(navigator, $"/Configuration/ScenarioList/Scenario[@name='{ScenarioName}']/RFS");
			if (scenariosRFSs != null && scenariosRFSs.Count > 0)
			{
				var scenarioRFSs = LoadScenario(scenariosRFSs, navigator, catalog);

				if (scenarioRFSs.MandatoryList.Count == 0)
				{
					foreach (var rfs in scenarioRFSs.ChildList)
					{
						rfs.ChildCFSList.Clear();
						rfs.IncludedToScenarios.Add(this);
						RFSList.Add(rfs);
					}
				}
				else
				{
					var cfsOfMandatoryRFS = scenarioRFSs.MandatoryList.SelectMany(p => p.ChildCFSList).Distinct().ToList();

					foreach (var mandRFS in scenarioRFSs.MandatoryList)
					{
						mandRFS.ChildCFSList.Clear();
						mandRFS.IncludedToScenarios.Add(this);
						RFSList.Add(mandRFS);
					}

					foreach (var childRFS in scenarioRFSs.ChildList)
					{
						var commonCFS = cfsOfMandatoryRFS.Intersect(childRFS.ChildCFSList).ToList();
						foreach (var escapeCFS in commonCFS)
						{
							childRFS.ChildCFSList.Remove(escapeCFS);
						}

						childRFS.IncludedToScenarios.Add(this);
						RFSList.Add(childRFS);
					}
				}
			}

			var appendRFS = XPATH.Select(navigator, $"/Configuration/ScenarioList/Scenario[@name='{ScenarioName}']/Append");
			if (appendRFS != null && appendRFS.Count > 0)
			{
				foreach (var rfsNode in appendRFS)
				{
					if (rfsNode.Node.Attributes == null || rfsNode.Node.Attributes.Count == 0)
						continue;

					string rfsName = null;
					foreach (XmlAttribute rfsAttr in rfsNode.Node.Attributes)
					{
						if (rfsAttr.Name != "name")
							continue;

						rfsName = rfsAttr.Value;
						break;
					}

					if (string.IsNullOrEmpty(rfsName) || !catalog.CatalogRFSCollection.TryGetValue(rfsName, out var rfsOperationList) || rfsOperationList.Count == 0)
						continue;

					AppendRFSList.Add(rfsOperationList.First());
				}
			}

			PostLoad();
		}

		static ScenarioRFSList LoadScenario(IEnumerable<XPathResult> scenariosRFSs, XPathNavigator navigator, ServiceCatalog catalog)
		{
			var result = new ScenarioRFSList();
			foreach (var rfsNode in scenariosRFSs)
			{
				if (rfsNode.Node.Attributes == null || rfsNode.Node.Attributes.Count == 0)
					continue;

				string rfsName = null;
				string useType = null;
				List<string> scenarioTypeList = null;

				foreach (XmlAttribute rfsAttr in rfsNode.Node.Attributes)
				{
					switch (rfsAttr.Name)
					{
						case "name":
							rfsName = rfsAttr.Value;
							break;
						case "scenarioType":
							scenarioTypeList = rfsAttr.Value.Split(',').Select(p => p.Trim()).ToList();
							break;
						case "useType":
							useType = rfsAttr.Value;
							break;
					}
				}

				if (string.IsNullOrEmpty(rfsName)
				 || scenarioTypeList == null
				 || scenarioTypeList.Count == 0
				 || !catalog.CatalogRFSCollection.TryGetValue(rfsName, out var rfsOperationList)
				 || rfsOperationList.Count == 0)
					continue;


				var currentScenarioRFS = rfsOperationList.Where(x => scenarioTypeList.Any(p => x.LinkType.Equals(p.Trim()))).ToList();
				if (currentScenarioRFS.Count == 0)
				{
					var anyExistRFS = rfsOperationList.First();
					foreach (var linkType in scenarioTypeList)
					{
						// это костыль, т.к. хэндлеры не проверяются. В хэндлерах могут быть разные настройки где RFS может использоваться с типом modify и т.д., поэтому костыльно создаются недостающие
						var notExistRFS = new CatalogRFSOperation(anyExistRFS.Node, rfsName, linkType, anyExistRFS.HostTypeName, navigator, catalog);
						notExistRFS.ChildCFSList.AddRange(anyExistRFS.ChildCFSList);
						notExistRFS.ChildRFSList.AddRange(notExistRFS.ChildRFSList);
						rfsOperationList.Add(notExistRFS);

						if (useType != null && useType.Like("Mandatory"))
							result.MandatoryList.Add(notExistRFS);
						else
							result.ChildList.Add(notExistRFS);
					}
				}
				else
				{
					foreach (var rfs in currentScenarioRFS)
					{
						if (useType != null && useType.Like("Mandatory"))
							result.MandatoryList.Add(rfs);
						else
							result.ChildList.Add(rfs);
					}
				}
			}

			return result;
		}

		void Preload(XmlNode scenarioNode, string scenarioName, ServiceCatalog catalog)
		{
			if (scenarioNode.Attributes == null || scenarioNode.Attributes.Count == 0)
				throw new Exception(Resources.ServiceCatalog_InvalidScenario);

			ScenarioName = scenarioName;
			Name = $"{catalog.Prefix}{ScenarioName}";
			ScenarioNode = scenarioNode;
		}

		void PostLoad()
		{
			var scenarioHostType = RFSList.Select(x => x.HostTypeName).Distinct().ToList();

			if (scenarioHostType.Count == 0)
				throw new Exception(string.Format(Resources.ServiceCatalog_ScenarioNotFoundRFS, ScenarioName));

			// по идее хост должен быть один, но если каталог криво настроили то могут быть несколько хостов в одном сценарии
			if (scenarioHostType.Count > 1)
				throw new Exception(string.Format(Resources.ServiceCatalog_ScenarioWithDifferentHostType, ScenarioName, string.Join(",", scenarioHostType)));

			HostTypeName = scenarioHostType.First();
		}
	}
}