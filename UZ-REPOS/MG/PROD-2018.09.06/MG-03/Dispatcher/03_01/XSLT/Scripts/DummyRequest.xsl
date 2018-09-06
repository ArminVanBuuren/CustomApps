<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ext="urn:CRMFilter">
    <xsl:output method="xml" indent="no" encoding="utf-16"/>
    <xsl:template match="/">
	<Message type="dummyresult" hostname="dispatcher" initiator="{/Message/@initiator}" id="{ext:newGuid()}" request_id="{/Message/@id}" session_id="{/Message/@session_id}">
		<Item customer="{/Message/Item/@customer}"
			Sender="{/Message/Item/Sender/@value}"
			Recipient="{/Message/Item/Recipient/@value}"
			dbresult="0">
		</Item>
	</Message>
    </xsl:template>
</xsl:stylesheet>