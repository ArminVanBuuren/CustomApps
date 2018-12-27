<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
  xmlns:user="urn:my-scripts">
  <msxsl:script language="C#" implements-prefix="user">
  <![CDATA[
   public static String replace_val(string bonuscount)
  {
	//  accumulator - accumulator node

    String val = "-9999.99";
	try
	{
        	val = bonuscount.Replace(',', '.');
	}
	catch (Exception)
	{
		val = "Error";
	}
    return val;
  }

  ]]>
  </msxsl:script>

    <xsl:output method="xml"/>
    <xsl:template match="node()|@*">
        <xsl:copy>
            <xsl:apply-templates select="node()|@*" />
        </xsl:copy>
    </xsl:template>
<xsl:template match="Attribute">
		<xsl:element name="Attribute">
			<xsl:attribute name='name'>
					<xsl:value-of select="./@name"/>
			</xsl:attribute>
	       <xsl:choose>
	            <xsl:when test="./@name='bonuscount'">
					<xsl:attribute name='value'>
							<xsl:value-of select="user:replace_val(./@value)"/>
					</xsl:attribute>
	            </xsl:when>
				<xsl:otherwise>
					<xsl:attribute name='value'>
							<xsl:value-of select="user:replace_val(./@value)"/>
					</xsl:attribute>
				</xsl:otherwise>
	        </xsl:choose>
		</xsl:element>
	</xsl:template>
</xsl:stylesheet>
