<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" 
  xmlns:ext="urn:CRMFilter"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
  xmlns:user="urn:my-scripts"
  >
  <msxsl:script language="C#" implements-prefix="user">
  <![CDATA[
        public static String get_codelist(string customer, string StringFormat, string Params, string Split)
        {
            String result = string.Empty;
            try
            {
                string[] strArray = Params.Split(new char[] { Split.ToCharArray()[0] });
                result = result + string.Format(StringFormat.Replace("[", "{").Replace("]", "}"), strArray);
                result = result.Replace("*", "\"").Replace("^", "'").Replace("\\.", ",").Replace("\\\"", "*").Replace("\\'", "^").Replace("\\r", "\r").Replace("\\n", "\n");
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
            return result;
        }
  ]]>
  </msxsl:script>
     <xsl:template match="/">
	          <xsl:variable name="Customer"     select="/Message/Item/@customer"/>
	          <xsl:variable name="StringFormat" select="/Message/Item/Attribute[@name='stringformat']/@value"/>
                  <xsl:variable name="Params"       select="/Message/Item/Attribute[@name='params']/@value"/>
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
				<xsl:element  name="Attribute">
					<xsl:attribute name='name'>result</xsl:attribute>
					<xsl:attribute name='value'>
							<xsl:value-of select='$Result' />
					</xsl:attribute>
				</xsl:element>
			</Item>
		  </Message>
     </xsl:template>
</xsl:stylesheet>