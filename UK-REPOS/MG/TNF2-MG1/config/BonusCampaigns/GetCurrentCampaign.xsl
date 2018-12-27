<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
  xmlns:user="urn:my-scripts"
  xmlns:ext="urn:MessagingGateway">
  <msxsl:script language="C#" implements-prefix="user">
  <![CDATA[
        /// <summary>
        /// Gets index of actual campaign
        /// </summary>
        /// <param name="xniMessage">Message</param>
        /// <returns>Campaign</returns>
	public static int GetCurrentCampaign(System.Xml.XPath.XPathNodeIterator xniMessage)
		{
			if (xniMessage == null) return 0;
			if (!xniMessage.MoveNext()) return 0;
			string format = "dd.MM.yyyy";
			System.Globalization.CultureInfo provider = System.Globalization.CultureInfo.InvariantCulture;
			try
			{
				int index = 1;
				XPathNodeIterator campaigns = xniMessage.Current.Select("Message/Item/Campaign");
				while (campaigns.MoveNext())
				{
                    XPathNavigator campaign = campaigns.Current;
                    DateTime startDate = DateTime.ParseExact(campaign.GetAttribute("startdate", ""), format, provider);
                    string endDateString = campaign.GetAttribute("enddate", "");
                    DateTime endDate = String.IsNullOrEmpty(endDateString) ? DateTime.MinValue : DateTime.ParseExact(endDateString, format, provider);

                    if (DateTime.Now >= startDate && (DateTime.Now <= endDate || endDate == DateTime.MinValue))
                    {
                        return index;
                    }
                    index++;
				}
			}
			catch (Exception)
			{
			}
			return 0;
		}
  ]]>
  </msxsl:script>

    <xsl:output method="xml"/>
    <xsl:template match="/">
        <xsl:variable name="index" select="user:GetCurrentCampaign(.)"/>
        <Current>
        	<xsl:copy-of select="/Message/Item/Campaign[$index]" />
        </Current>
    </xsl:template>
</xsl:stylesheet>
