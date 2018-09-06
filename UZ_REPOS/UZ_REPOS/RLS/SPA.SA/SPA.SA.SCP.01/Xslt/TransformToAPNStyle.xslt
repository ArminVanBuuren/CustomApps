<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" encoding="utf-8" indent="yes"/>
  <xsl:template match="node()|@*">
    <xsl:copy>
      <xsl:apply-templates select="node()|@*" />
    </xsl:copy>
  </xsl:template>
  <xsl:template match="discount_type|discount_value|discount_counter">
    <xsl:if test="Value">
      <xsl:copy>
        <xsl:apply-templates select="node()|@*" />
      </xsl:copy>
    </xsl:if>
    <xsl:if test="not(Value)">
      <xsl:copy>
        <Value>
          <xsl:apply-templates select="node()|@*" />
        </Value>
        <xsl:if test="parent::*//DoNotProcessingInFinancialPlatform[text()='0' or text()='False']">
          <AvailableSystem>
            <FinancialPlatform/>
          </AvailableSystem>
        </xsl:if>
      </xsl:copy>
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>