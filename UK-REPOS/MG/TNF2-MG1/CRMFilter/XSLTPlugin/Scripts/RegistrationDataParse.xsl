<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
  xmlns:user="urn:my-scripts">
  <msxsl:script language="C#" implements-prefix="user">
  <![CDATA[
        public static String ParseRegistrationData(string registrationData, int index)
        {
/*
example of registrationData:
Mphone=79163515044
puk1=1234567890
...
card-note=456
outsourcer-note=outsourcer-note
date  =13.10.2008
*/
            try
            {
                int count = 0;
                string[] lines = registrationData.Split('\n');
                foreach (string line in lines)
                {
                    if(count++ == index)
					{
	                    string[] tokens = line.Split('=');
    	                if(tokens.Length < 2)break;
    	                
	                    string name = tokens[0].Trim();
    	                string value = tokens[1].Trim();
                    	if (String.IsNullOrEmpty(name))break;

                    	return name+ ";" + value;
					}
                }
            }
            catch (Exception)
            {
            }
            return "";
        }

        public static String GetCustomer(string registrationData)
        {
            try
            {
                string[] lines = registrationData.Split('\n');
                foreach (string line in lines)
                {
                    string[] tokens = line.Split('=');
                    if(tokens.Length < 2)continue;

                    string name = tokens[0].Trim();
                    string value = tokens[1].Trim();

                    if (!String.IsNullOrEmpty(name) && String.Compare(name,"mphone",true)==0) return value;
                }
            }
            catch (Exception)
            {
            }
            return "";
        }
  ]]>
  </msxsl:script>

    <xsl:output method="xml"/>

    <xsl:template match="/">
    	<Message type="serviceorder" initiator="{/Message/@initiator}" id="{/Message/@id}" session_id="{/Message/@session_id}">
            <xsl:call-template name="parser">
                <xsl:with-param name="registrationData" select="/Message/Item/Content"/>
   	        </xsl:call-template>
		</Message>
    </xsl:template>


    <xsl:template name="parser">
        <xsl:param name="registrationData" select="0"/>

        <xsl:element name="Item">
			<xsl:attribute name='customer'>
						<xsl:value-of select='user:GetCustomer($registrationData)'/>
			</xsl:attribute>

	        <xsl:element name="Service">
				<xsl:attribute name='id'>
							<xsl:value-of select="'registrationrequest'"/>
				</xsl:attribute>
				<xsl:attribute name='action'>
							<xsl:value-of select="'4'"/>
				</xsl:attribute>

	            <xsl:call-template name="insertdata">
    	            <xsl:with-param name="registrationData" select="$registrationData"/>
    	            <xsl:with-param name="index" select="0"/>
   	    	    </xsl:call-template>
        	</xsl:element>
        </xsl:element>
    </xsl:template>


    <xsl:template name="insertdata">
        <xsl:param name="registrationData" select="0"/>
        <xsl:param name="index" select="0"/>

        <xsl:variable name="pair">
            <xsl:value-of select="user:ParseRegistrationData($registrationData, $index)"/>
        </xsl:variable>

        <xsl:choose>
            <xsl:when test="string-length($pair)>0">
		        <xsl:element name="Attribute">
					<xsl:attribute name='name'>
						<xsl:value-of select="substring-before($pair,';')"/>
					</xsl:attribute>
					<xsl:attribute name='value'>
						<xsl:value-of select="substring-after($pair,';')"/>
					</xsl:attribute>
		        </xsl:element>
            </xsl:when>
            <xsl:otherwise>
            </xsl:otherwise>
        </xsl:choose>

        <xsl:choose>
            <xsl:when test="string-length($pair)=0">
            </xsl:when>
            <xsl:otherwise>
                <xsl:call-template name="insertdata">
    	            <xsl:with-param name="registrationData" select="$registrationData"/>
    	            <xsl:with-param name="index" select="$index+1"/>
                </xsl:call-template>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

</xsl:stylesheet>
