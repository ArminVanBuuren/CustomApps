<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ext="urn:CRMFilter">
    <xsl:output method="xml" indent="no" encoding="utf-16"/>
    <xsl:template match="/">
        <Message type="{/Message/@type}result" hostname="dispatcher" id="{ext:newGuid()}" session_id="{/Message/@session_id}" request_id="{/Message/@id}">
            <Item customer="{/Message/Item/@customer}" dbresult="320155"/>
        </Message>
    </xsl:template>
</xsl:stylesheet>