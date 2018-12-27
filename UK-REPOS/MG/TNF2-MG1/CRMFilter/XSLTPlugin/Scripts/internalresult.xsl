<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output method="xml"  indent="yes"/>
	<xsl:template match="/">
	<xsl:variable name="StartDate" select="/Message/Item/Attribute/@value [../@name='startdate']"/>
	<xsl:variable name="DBResult" select="0"/>
	
	
	
<Message type="internalresult" id="{/Message/@id}"
		session_id="{/Message/@session_id}"
		request_id="{/Message/@request_id}">
	<Item customer="{/Message/Item/@customer}">
		<Attribute name="startdate" value="{$StartDate}" />
		<Attribute name="enddate" value="{/Message/Item/Attribute/@value[../@name='enddate']}" />
		<xsl:if test="$StartDate=''">
			<Attribute name="dbresult" value="320198" />
		</xsl:if>
		<xsl:if test="$StartDate!=''">
			<Attribute name="dbresult" value="0" />
		</xsl:if>
	</Item>
</Message>
	</xsl:template>
</xsl:stylesheet>
