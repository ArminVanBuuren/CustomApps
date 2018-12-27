<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:template match="/">

<Message type="groupinterrogationresult"
id="{/Message/@id}"
session_id="{/Message/@session_id}"
request_id="{/Message/@request_id}">

 <Item customer="{/Message/Item/@customer}" state="{/Message/Item/@state}" dbresult="{/Message/Item/@dbresult}">
	  <Attribute name="service_id" value="{//Service/@id}" />
 </Item>
</Message>

	</xsl:template>
</xsl:stylesheet>
