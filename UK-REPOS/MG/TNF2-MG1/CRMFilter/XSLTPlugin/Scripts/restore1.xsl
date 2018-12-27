<xsl:transform xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
    <xsl:output method="xml"/>

    <xsl:template match="node()|@*">
        <xsl:copy>
            <xsl:apply-templates select="node()|@*" />
        </xsl:copy>
    </xsl:template>

    <xsl:template match="Message">
        <Message>
            <xsl:attribute name="session_id">
                <xsl:value-of select="./@id"/>
            </xsl:attribute>
            
            <xsl:apply-templates select="node()|@*[name()!='session_id']" />
        </Message>
    </xsl:template>

</xsl:transform>
