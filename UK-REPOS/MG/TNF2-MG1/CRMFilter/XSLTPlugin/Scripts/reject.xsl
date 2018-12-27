<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
	xmlns:ext="urn:CRMFilter">
	<xsl:template match="/">
		
<Message type="deliveryrequest" id="{ext:newGuid()}" session_id="{ext:newGuid()}">
	<Package package_id="{ext:newGuid()}" template_id="SMSAutoInformation">
		<Item customer="{/Message/Item/@sender}" >
		      <Content encoding="7bit" charset="iso-8859-2">
				Content of the last message was <xsl:value-of select="ext:session('abc','content')" />
		      </Content>
		      <Attribute name="sender" value="{/Message/Item/@recipient}" />

		</Item>
	</Package>
</Message>

	</xsl:template>
</xsl:stylesheet>