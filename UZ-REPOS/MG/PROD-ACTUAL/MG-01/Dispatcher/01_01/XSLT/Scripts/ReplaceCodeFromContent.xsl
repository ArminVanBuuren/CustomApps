<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
  xmlns:user="urn:my-scripts">
  <msxsl:script language="C#" implements-prefix="user">
  <![CDATA[
        public static String get_codelist(string ContString, string StringCustomer)
        {
             return ContString.Replace("@@@@@@", StringCustomer);
        }
  ]]>
  </msxsl:script>

    <xsl:output method="xml"/>
  
    <xsl:template match="node()|@*">
        <xsl:copy>
            <xsl:apply-templates select="node()|@*" />
        </xsl:copy>
    </xsl:template>


    <xsl:template match="Message">
        <Message>            
            <xsl:variable name="temp">Dipatcher</xsl:variable>
            <xsl:attribute name="hostname">
                <xsl:value-of select="$temp"/>
            </xsl:attribute>
            
            <xsl:apply-templates select="node()|@*[name()!='hostname']" />
        </Message>
    </xsl:template>

	<xsl:template match="Item">
	    <xsl:element name="Item">
	        <xsl:apply-templates select="@*" />
			<xsl:copy-of select="./Attribute" />

			<xsl:variable name="ContString" select="/Message/Package/Item/Content/text()"/>
			<xsl:variable name="StringCustomer" select="/Message/Package/Item/@customer"/>
			<xsl:element  name="Attribute">
				<xsl:attribute name='name'>Content</xsl:attribute>				
				<xsl:attribute name='value'>
						<xsl:value-of select='user:get_codelist($ContString, $StringCustomer)' />
				</xsl:attribute>
			</xsl:element>


		</xsl:element>
	</xsl:template>

</xsl:stylesheet>
