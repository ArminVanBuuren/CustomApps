<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ext="urn:CRMFilter">
    <xsl:output method="xml" indent="no" encoding="utf-16"/>
    <xsl:template match="/">
	<Message type="{/Message/@type}result" hostname="dispatcher" initiator="{/Message/@initiator}" id="{ext:newGuid()}" request_id="{/Message/@id}" session_id="{/Message/@session_id}">
		<Item customer="{/Message/Item/@customer}" dbresult="0">
			<xsl:copy-of select="/Message/Item/Content"/>
		</Item>
	</Message>
    </xsl:template>
</xsl:stylesheet>