<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:output method="xml" cdata-section-elements="Content"/>
	<xsl:template match="/">
		<Message type="deliveryrequest" hostname="dispatcher" id="{/Message/@id}" session_id="{/Message/@session_id}">
			<Package package_id="{/Message/Package/@package_id}" template_id="SMSAutoInformation">
				<Item sender="111" customer="{/Message/Package/Item/@customer}" priority="{/Message/Package/Item/@priority}" contentType="{/Message/Package/Item/@contentType}">
					<xsl:copy-of select="/Message/Package/Item/Content"/>
				</Item>
			</Package>
		</Message>
	</xsl:template>
</xsl:stylesheet>
