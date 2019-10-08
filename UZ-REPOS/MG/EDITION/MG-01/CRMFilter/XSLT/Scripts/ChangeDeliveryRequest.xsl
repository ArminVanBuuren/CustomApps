<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ext="urn:CRMFilter">
    <xsl:output method="xml" indent="yes" encoding="utf-16" cdata-section-elements="Content"/>
    <xsl:template match="/">
        <Message type="deliveryrequest" hostname="dispatcher" id="{/Message/@id}" session_id="{/Message/@session_id}">
            <Package package_id="{/Message/Package/@package_id}" template_id="{/Message/Package/@template_id}">
                    <Item sender="{/Message/Package/Item/@sender}" customer="{/Message/Package/Item/@customer}" priority="{/Message/Package/Item/@priority}" contentType="{/Message/Package/Item/@contentType}">
                         <xsl:copy-of select="/Message/Package/Item/*" />
                    </Item>
            </Package>
        </Message>
    </xsl:template>
</xsl:stylesheet>