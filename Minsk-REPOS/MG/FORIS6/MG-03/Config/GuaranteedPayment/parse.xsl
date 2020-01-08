<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
  xmlns:user="urn:my-scripts">
  <msxsl:script language="C#" implements-prefix="user">
  <![CDATA[
    public static String build_list(System.Xml.XPath.XPathNodeIterator rows, string language)
	  //  rows - nodeset of Attribute node
    {
      System.Text.StringBuilder result = new System.Text.StringBuilder();
      try 
      {  
            while (rows.MoveNext())
            {
				string datetime = rows.Current.SelectSingleNode("./Attribute[@name='expirationdate']/@value").Value.ToString();
				string sum = rows.Current.SelectSingleNode("./Attribute[@name='intpartfracpart']/@value").Value.ToString();
				if(datetime.LastIndexOf(":")!=-1)
					datetime = datetime.Substring(0,datetime.LastIndexOf(":"));

                result.Append("Сумма обещанного платежа ");
                result.Append(Convert.ToInt32(sum)/100);
                result.Append(" руб., cрок действия до ");
                result.Append(datetime);
                result.Append("; " );
            }
            if (result.Length>0) result.Remove(result.Length-1,1);
      } 
      catch (Exception ex) 
      {
        return ex.Message;
      }
      return result.ToString();
    }
  ]]>
  </msxsl:script>

  <xsl:output method="xml"/>
  
  <xsl:template match="node()|@*">
      <xsl:copy>
          <xsl:apply-templates select="node()|@*" />
      </xsl:copy>
  </xsl:template>

	<xsl:template match="Item">
		<xsl:element name="Item">
				<xsl:variable name="rownodes" select="./row"/>
				<xsl:variable name="language_id" select="/Message/@language_id"/>
				<xsl:attribute name="paymentslist">
						<xsl:value-of select="user:build_list($rownodes, $language_id)"/>
				</xsl:attribute>

        <xsl:apply-templates select="@*" />
				<xsl:copy-of select="./Attribute" />
			
		</xsl:element>
	</xsl:template>

</xsl:stylesheet>

