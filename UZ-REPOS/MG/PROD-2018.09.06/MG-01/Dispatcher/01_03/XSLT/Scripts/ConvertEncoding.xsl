<?xml version="1.0"?>
<xsl:stylesheet version="1.0" 
   xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
   xmlns:ext="urn:CRMFilter"
   xmlns:msxsl="urn:schemas-microsoft-com:xslt"
   xmlns:user="urn:my-scripts">
  <msxsl:script language="C#" implements-prefix="user">
  <![CDATA[
       public static String ReplaceSpecSimbol(string Content)
        {
            if (Content != string.Empty)
            {
                return Content.Replace("&amp;", "&")
                              .Replace("&quot;", "\"")
                              .Replace("&apos;", "'")
                              .Replace("&lt;", "<")
                              .Replace("&gt;", ">")
                                .Replace("&#32;", " ")
                                .Replace("&#143;", "")
                                .Replace("&#33;", "!")
                                .Replace("&#144;", "")
                                .Replace("&#34;", "\"")
                                .Replace("&#145;", "'")
                                .Replace("&#35;", "#")
                                .Replace("&#146;", "'")
                                .Replace("&#36;", "$")
                                .Replace("&#147;", "“")
                                .Replace("&#37;", "%")
                                .Replace("&#148;", "”")
                                .Replace("&#38;", "&")
                                .Replace("&#149;", "•")
                                .Replace("&#39;", "'")
                                .Replace("&#150;", "–")
                                .Replace("&#40;", "(")
                                .Replace("&#151;", "—")
                                .Replace("&#41;", ")")
                                .Replace("&#152;", "˜")
                                .Replace("&#42;", "*")
                                .Replace("&#153;", "™")
                                .Replace("&#43;", "+")
                                .Replace("&#154;", "š")
                                .Replace("&#44;", ",")
                                .Replace("&#155;", "›")
                                .Replace("&#45;", "-")
                                .Replace("&#156;", "œ")
                                .Replace("&#46;", ".")
                                .Replace("&#157;", "")
                                .Replace("&#47;", "/")
                                .Replace("&#158;", "ž")
                                .Replace("&#48;", "0")
                                .Replace("&#159;", "Ÿ")
                                .Replace("&#49;", "1")
                                .Replace("&#50;", "2")
                                .Replace("&#160;", "")
                                .Replace("&#161;", "¡")
                                .Replace("&#51;", "3")
                                .Replace("&#162;", "¢")
                                .Replace("&#52;", "4")
                                .Replace("&#163;", "£")
                                .Replace("&#53;", "5")
                                .Replace("&#164;", "¤")
                                .Replace("&#54;", "6")
                                .Replace("&#165;", "¥")
                                .Replace("&#55;", "7")
                                .Replace("&#166;", "¦")
                                .Replace("&#56;", "8")
                                .Replace("&#167;", "§")
                                .Replace("&#57;", "9")
                                .Replace("&#168;", "¨")
                                .Replace("&#58;", ":")
                                .Replace("&#169;", "©")
                                .Replace("&#59;", ";")
                                .Replace("&#170;", "ª")
                                .Replace("&#60;", "<")
                                .Replace("&#171;", "«")
                                .Replace("&#61;", "=")
                                .Replace("&#172;", "¬")
                                .Replace("&#62;", ">")
                                .Replace("&#173;", "­")
                                .Replace("&#63;", "?")
                                .Replace("&#174;", "®")
                                .Replace("&#64;", "@")
                                .Replace("&#175;", "¯")
                                .Replace("&#65;", "A")
                                .Replace("&#176;", "°")
                                .Replace("&#66;", "B")
                                .Replace("&#177;", "±")
                                .Replace("&#67;", "C")
                                .Replace("&#178;", "²")
                                .Replace("&#68;", "D")
                                .Replace("&#179;", "³")
                                .Replace("&#69;", "E")
                                .Replace("&#180;", "´")
                                .Replace("&#70;", "F")
                                .Replace("&#181;", "µ")
                                .Replace("&#71;", "G")
                                .Replace("&#182;", "¶")
                                .Replace("&#72;", "H")
                                .Replace("&#183;", "·")
                                .Replace("&#73;", "I")
                                .Replace("&#184;", "¸")
                                .Replace("&#74;", "J")
                                .Replace("&#185;", "¹")
                                .Replace("&#75;", "K")
                                .Replace("&#186;", "º")
                                .Replace("&#76;", "L")
                                .Replace("&#187;", "»")
                                .Replace("&#77;", "M")
                                .Replace("&#188;", "¼")
                                .Replace("&#78;", "N")
                                .Replace("&#189;", "½")
                                .Replace("&#79;", "O")
                                .Replace("&#190;", "¾")
                                .Replace("&#80;", "P")
                                .Replace("&#191;", "¿")
                                .Replace("&#81;", "Q")
                                .Replace("&#192;", "À")
                                .Replace("&#82;", "R")
                                .Replace("&#193;", "Á")
                                .Replace("&#83;", "S")
                                .Replace("&#194;", "Â")
                                .Replace("&#84;", "T")
                                .Replace("&#195;", "Ã")
                                .Replace("&#85;", "U")
                                .Replace("&#196;", "Ä")
                                .Replace("&#86;", "V")
                                .Replace("&#197;", "Å")
                                .Replace("&#87;", "W")
                                .Replace("&#198;", "Æ")
                                .Replace("&#88;", "X")
                                .Replace("&#199;", "Ç")
                                .Replace("&#89;", "Y")
                                .Replace("&#200;", "È")
                                .Replace("&#90;", "Z")
                                .Replace("&#201;", "É")
                                .Replace("&#91;", "[")
                                .Replace("&#202;", "Ê")
                                .Replace("&#92;", @"\")
                                .Replace("&#203;", "Ë")
                                .Replace("&#93;", "]")
                                .Replace("&#204;", "Ì")
                                .Replace("&#94;", "^")
                                .Replace("&#205;", "Í")
                                .Replace("&#95;", "_")
                                .Replace("&#206;", "Î")
                                .Replace("&#96;", "`")
                                .Replace("&#207;", "Ï")
                                .Replace("&#97;", "a")
                                .Replace("&#208;", "Ð")
                                .Replace("&#98;", "b")
                                .Replace("&#209;", "Ñ")
                                .Replace("&#99;", "c")
                                .Replace("&#210;", "Ò")
                                .Replace("&#100;", "d")
                                .Replace("&#211;", "Ó")
                                .Replace("&#101;", "e")
                                .Replace("&#212;", "Ô")
                                .Replace("&#102;", "f")
                                .Replace("&#213;", "Õ")
                                .Replace("&#103;", "g")
                                .Replace("&#214;", "Ö")
                                .Replace("&#104;", "h")
                                .Replace("&#215;", "×")
                                .Replace("&#105;", "i")
                                .Replace("&#216;", "Ø")
                                .Replace("&#106;", "j")
                                .Replace("&#217;", "Ù")
                                .Replace("&#107;", "k")
                                .Replace("&#218;", "Ú")
                                .Replace("&#108;", "l")
                                .Replace("&#219;", "Û")
                                .Replace("&#109;", "m")
                                .Replace("&#220;", "Ü")
                                .Replace("&#110;", "n")
                                .Replace("&#221;", "Ý")
                                .Replace("&#111;", "o")
                                .Replace("&#222;", "Þ")
                                .Replace("&#112;", "p")
                                .Replace("&#223;", "ß")
                                .Replace("&#113;", "q")
                                .Replace("&#224;", "à")
                                .Replace("&#114;", "r")
                                .Replace("&#225;", "á")
                                .Replace("&#115;", "s")
                                .Replace("&#226;", "â")
                                .Replace("&#116;", "t")
                                .Replace("&#227;", "ã")
                                .Replace("&#117;", "u")
                                .Replace("&#228;", "ä")
                                .Replace("&#118;", "v")
                                .Replace("&#229;", "å")
                                .Replace("&#119;", "w")
                                .Replace("&#230;", "æ")
                                .Replace("&#120;", "x")
                                .Replace("&#231;", "ç")
                                .Replace("&#121;", "y")
                                .Replace("&#232;", "è")
                                .Replace("&#122;", "z")
                                .Replace("&#233;", "é")
                                .Replace("&#123;", "{")
                                .Replace("&#234;", "ê")
                                .Replace("&#124;", "|")
                                .Replace("&#235;", "ë")
                                .Replace("&#125;", "}")
                                .Replace("&#236;", "ì")
                                .Replace("&#126;", "~")
                                .Replace("&#237;", "í")
                                .Replace("&#127;", "")
                                .Replace("&#238;", "î")
                                .Replace("&#128;", "€")
                                .Replace("&#239;", "ï")
                                .Replace("&#129;", "")
                                .Replace("&#240;", "ð")
                                .Replace("&#130;", "‚")
                                .Replace("&#241;", "ñ")
                                .Replace("&#131;", "ƒ")
                                .Replace("&#242;", "ò")
                                .Replace("&#132;", "„")
                                .Replace("&#243;", "ó")
                                .Replace("&#133;", "…")
                                .Replace("&#244;", "ô")
                                .Replace("&#134;", "†")
                                .Replace("&#245;", "õ")
                                .Replace("&#135;", "‡")
                                .Replace("&#246;", "ö")
                                .Replace("&#136;", "ˆ")
                                .Replace("&#247;", "÷")
                                .Replace("&#137;", "‰")
                                .Replace("&#248;", "ø")
                                .Replace("&#138;", "Š")
                                .Replace("&#249;", "ù")
                                .Replace("&#139;", "‹")
                                .Replace("&#250;", "ú")
                                .Replace("&#140;", "Œ")
                                .Replace("&#251;", "û")
                                .Replace("&#141;", "")
                                .Replace("&#252;", "ü")
                                .Replace("&#142;", "Ž")
                                .Replace("&#253;", "ý")
                                .Replace("&#143;", "")
                                .Replace("&#254;", "þ");
            }
            else 
            {
                return string.Empty;
            }
        }
  ]]>
  </msxsl:script>


    <xsl:output method="xml" indent="no" encoding="utf-16"/>
    <xsl:template match="/">
	<xsl:variable name="Content"  select="/Message/Item/Content/@value"/>
	<xsl:variable name="Result"   select="user:ReplaceSpecSimbol($Content)"/>
	<Message type="{/Message/@type}result" hostname="dispatcher" initiator="{/Message/initiator}" id="{ext:newGuid()}" request_id="{/Message/@id}" session_id="{/Message/@session_id}">
		<Item customer="{/Message/Item/@customer}"
			Sender="{/Message/Item/Sender/@value}"
			Recipient="{/Message/Item/Recipient/@value}"
			dbresult="0">
			<xsl:attribute name="Content">
				<xsl:value-of select="$Result"/>
			</xsl:attribute>
			<xsl:attribute name="length">
				<xsl:value-of select="string-length($Result)"/>
			</xsl:attribute>
		</Item>
	</Message>
    </xsl:template>
</xsl:stylesheet>