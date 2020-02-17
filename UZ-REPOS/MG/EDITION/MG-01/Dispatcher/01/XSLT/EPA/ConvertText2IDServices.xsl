<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output method="xml"/>

    <xsl:template match="*|/" />
    <xsl:template match="node()|text()|@*" />

    <xsl:template match="Service">
        <xsl:element name="Service">
            <xsl:attribute name="id">
                <xsl:value-of select="./@id"/>
            </xsl:attribute>
            <xsl:element name="allowed">
                <xsl:value-of select="./Attribute[@name='allowed']/@value"/>
            </xsl:element>
            <xsl:element name="subscription">
                <xsl:value-of select="./Attribute[@name='subscription']/@value"/>
            </xsl:element>
        </xsl:element>
    </xsl:template>

    <xsl:template match="/">
        <xsl:element name="Services">
            <xsl:apply-templates select="Message/Item/Service" />
        </xsl:element>
    </xsl:template>

</xsl:stylesheet>
	
	
	
	