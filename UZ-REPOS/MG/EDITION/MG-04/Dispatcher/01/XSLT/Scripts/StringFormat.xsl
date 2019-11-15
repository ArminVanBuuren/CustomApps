<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" 
  xmlns:ext="urn:CRMFilter"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
  xmlns:user="urn:my-scripts"
  >
  <msxsl:script language="C#" implements-prefix="user">
  <![CDATA[
        public static String get_codelist(string customer, string format, string @params, string splitSign)
        {
            try
            {
                return string.Format(CharReplacer(format.Replace("[", "{").Replace("]", "}")), CharReplacer(@params).Split(new char[] { splitSign.ToCharArray()[0] }));
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public static String CharReplacer(string input)
        {
            return input
                    .Replace("&amp;", "&")
                    .Replace("&apos;", "'")
                    .Replace(@"^", "'")
                    .Replace("&quot;", "\"")
                    .Replace("&lt;", "<")
                    .Replace("&gt;", ">")
                ;
        }
  ]]>
  </msxsl:script>
     <xsl:output method="xml" indent="no" encoding="utf-16" cdata-section-elements="Content" />
     <xsl:template match="/">
	          <xsl:variable name="Customer"     select="/Message/Item/@customer"/>
	          <xsl:variable name="StringFormat" select="/Message/Item/Attribute[@name='stringformat']/text()"/>
            <xsl:variable name="Params"       select="/Message/Item/Attribute[@name='params']/text()"/>
            <xsl:variable name="Split"        select="/Message/Item/Attribute[@name='split']/@value"/>
            <xsl:variable name="Result"       select="user:get_codelist($Customer, $StringFormat, $Params, $Split)"/>
		  <Message type="{/Message/@type}result" hostname="dispatcher" id="{ext:newGuid()}" request_id="{/Message/@id}" session_id="{/Message/@session_id}" >
			<Item customer="{/Message/Item/@customer}">
            	        	<xsl:attribute name="dbresult">
            	        	  <xsl:choose>
            	        	    <xsl:when test="$Result = ''">
            	        	      <xsl:value-of select="-1"/>
            	        	    </xsl:when>
            	        	    <xsl:otherwise>
            	        	      <xsl:value-of select="0"/>
            	        	    </xsl:otherwise>
            	        	  </xsl:choose>
	                	</xsl:attribute>
            	        	<xsl:attribute name="length">
        	      			<xsl:value-of select="string-length($Result)"/>
	                	</xsl:attribute>
				<xsl:element  name="Content">
					<xsl:copy>
        	      				<xsl:copy-of select="$Result"/>
					</xsl:copy>
				</xsl:element>
			</Item>
		  </Message>
     </xsl:template>
</xsl:stylesheet>