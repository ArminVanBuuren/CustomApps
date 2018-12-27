<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ext="urn:CRMFilter">
	<xsl:template match="/">
	            <xsl:variable name="sender" select="normalize-space(ext:session('sender'))" />
 
		<Message type="verificationresult" id="{/Message/@id}" session_id="{/Message/@session_id}" request_id="{/Message/@request_id}">
			<Item customer="{/Message/Item/@customer}" sender="7{substring($sender,string-length($sender)-9)}" dbresult="{/Message/Item/@dbresult}" tariffplan="{/Message/Item/@tariffplan}" state="{/Message/Item/@state}">
			    <xsl:copy-of select="/Message/Item/*" />
			</Item>
		</Message>
	</xsl:template>
</xsl:stylesheet>
