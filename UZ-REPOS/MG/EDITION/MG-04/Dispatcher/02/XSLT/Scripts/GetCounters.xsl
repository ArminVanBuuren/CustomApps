<?xml version="1.0" encoding="utf-8"?>
<!-- 
Created by Vladimir Khovanskiy  
-->
<xsl:stylesheet version="1.0" 
  xmlns:ext="urn:CRMFilter"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
  xmlns:user="urn:my-scripts"
  >
  <msxsl:script language="C#" implements-prefix="user">
  <![CDATA[
        const string COUNTERS_PATH = @"C:\FORIS\Messaging Gateway\Config\Service\COUNTERS.xml";
        public static String parce_accumulators(string Accum, string Type, string MsgSession, string LangIn, string Prefix)
        {
            string result = string.Empty;
            try
            {
                //Example incoming format Accum - MBPACK_Интернет-пакет_500_30.05.2015 00:04:12;MIN250_250 минут (on-net)_168
                System.Xml.XPath.XPathDocument xmlSetting = new System.Xml.XPath.XPathDocument(COUNTERS_PATH);
                System.Xml.XPath.XPathNavigator navigator = xmlSetting.CreateNavigator();

                string Object;
                if (string.IsNullOrEmpty(Prefix))
                {
                    Object = MsgSession.Split('_')[1].ToLower() + "_";
                }
                else
                {
                    Object = Prefix.ToLower() + "_";
                }

                string Lang;
                if (string.IsNullOrEmpty(LangIn))
                {
                    Lang = "_" + MsgSession.Split('_')[2].ToLower();
                }
                else
                {
                    Lang = "_" + LangIn.ToLower();
                }

                string[] strAccums = Accum.Split(';');
                string trsf = @"\.";
                int i = 0;


                foreach (string Counters in strAccums)
                {
                    string[] strCounter = Counters.Split('_');
                    string groupId;
                    if (Type != string.Empty)
                    { groupId = " and @group='" + Expression(navigator, "Counters/GroupList/Item[@name='" + Type + "']/@id") + "'"; }
                    else
                    { groupId = string.Empty; }
                    //Проверка на корректность настройки
                    string counterTypeParce = Expression(navigator, "Counters/Group[@name='"
                                                                    + Expression(navigator, "Counters/GroupList/Item[@id='"
                                                                                            + Expression(navigator, "Counters/Counter[@counter_code='"
                                                                                                                    + strCounter[0] + "'" + groupId + "]/@group")
                                                                                            + "']/@name") + "']/@name");
                    if ((counterTypeParce == Type && counterTypeParce != string.Empty) || (Type == string.Empty))
                    {
                        string counterName = Expression(navigator, "Counters/Counter[@counter_code='"
                                                         + strCounter[0] + "'" + groupId + "]/@" + Object + "name" + Lang);
                        string counterOutText = string.Empty;
                        if (strCounter.Length == 3)
                        {
                            if (strCounter[0] == "MB3000SN" || strCounter[0] == "MB3500NN" || strCounter[0] == "MB4500NN") 
                            {
                               strCounter[2] = strCounter[2] + " (night)";
                            }
                            counterOutText = Expression(navigator, "Counters/Group[@name='" + counterTypeParce + "']/@" + Object + "nodate" + Lang);
                            counterOutText = string.Format(counterOutText.Replace("[", "{").Replace("]", "}"), strCounter[2], counterName);
                        }
                        else if (strCounter.Length > 3)
                        {
                            counterOutText = Expression(navigator, "Counters/Group[@name='" + counterTypeParce + "']/@" + Object + "full" + Lang);
                            counterOutText = string.Format(counterOutText.Replace("[", "{").Replace("]", "}"), strCounter[2], strCounter[3], counterName);
                        }
                        if (counterOutText != string.Empty)
                        {
                            i++;
                            if (i == 1)
                            {
                                result = result + counterOutText;
                            }
                            else
                            {
                                result = result + trsf + counterOutText;
                            }
                        }
                    }
                }
                if (result == string.Empty && Type != string.Empty)
                {
                    result = Expression(navigator, "Counters/Group[@name='" + Type + "']/@" + Object + "nocounters" + Lang);
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
            return result.Replace(@",", ".").Replace(@"\.", ",").Replace(".0001", "");
        }
        public static String Expression(System.Xml.XPath.XPathNavigator navigator, string xpath)
        {
            System.Xml.XPath.XPathExpression expression = System.Xml.XPath.XPathExpression.Compile(xpath);
            System.Xml.XmlNamespaceManager manager = new System.Xml.XmlNamespaceManager(navigator.NameTable);
            manager.AddNamespace("bk", "http://www.contoso.com/books");
            expression.SetContext(manager);
            String Out = string.Empty;
            switch (expression.ReturnType)
            {
                case System.Xml.XPath.XPathResultType.Number:
                    return navigator.Evaluate(expression).ToString();

                case System.Xml.XPath.XPathResultType.NodeSet:
                    System.Xml.XPath.XPathNodeIterator nodes = navigator.Select(expression);
                    while (nodes.MoveNext())
                    {
                        Out = Out + nodes.Current.ToString();
                        break;
                    }
                    return Out;

                case System.Xml.XPath.XPathResultType.Boolean:
                    return navigator.Evaluate(expression).ToString();

                case System.Xml.XPath.XPathResultType.String:
                    return navigator.Evaluate(expression).ToString();
            }
            return String.Empty;
        }
  ]]>
  </msxsl:script>
<xsl:template match="/">
  
        <xsl:variable name="accumulator"    select="/Message/Item/Attribute[@name='accumulator']/@value"/>
        <xsl:variable name="session_lang"   select="/Message/@session_id"/>
        <xsl:variable name="lang"           select="/Message/Item/@lang"/>
        <xsl:variable name="prefix"         select="/Message/Item/@prefix"/>
        <xsl:variable name="ResultFull"     select="user:parce_accumulators($accumulator,     '', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultGprs"     select="user:parce_accumulators($accumulator, 'gprs', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultVoice"    select="user:parce_accumulators($accumulator,'voice', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultSms"      select="user:parce_accumulators($accumulator,  'sms', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultBonusSms" select="user:parce_accumulators($accumulator,  'bonussms', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultWMAXIIVR" select="user:parce_accumulators($accumulator,  'WMAXIIVR', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultWOPTIMAI" select="user:parce_accumulators($accumulator,  'WOPTIMAI', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultInetsms"  select="user:parce_accumulators($accumulator,  'inetsms', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultAutoinet" select="user:parce_accumulators($accumulator,  'autoinet', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultHuawei"   select="user:parce_accumulators($accumulator,  'huawei', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultNight"    select="user:parce_accumulators($accumulator,  'night', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultBonus"    select="user:parce_accumulators($accumulator,  'bonus', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultSuperzero" select="user:parce_accumulators($accumulator,  'superzero', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultSuperzero30" select="user:parce_accumulators($accumulator,  'superzero30', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultSamsungs7" select="user:parce_accumulators($accumulator,  'samsungs7', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultScginet" select="user:parce_accumulators($accumulator,  'scginet', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultInetweek" select="user:parce_accumulators($accumulator,  'inetweek', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultGprstosms" select="user:parce_accumulators($accumulator,  'gprstosms', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultAutoinettosms" select="user:parce_accumulators($accumulator,  'autoinettosms', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultBlackfriday" select="user:parce_accumulators($accumulator,  'blackfriday', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultUniversal" select="user:parce_accumulators($accumulator,  'universal', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultBzs" select="user:parce_accumulators($accumulator,  'bzs', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultMbpack" select="user:parce_accumulators($accumulator,  'mbpack', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultMbpackd" select="user:parce_accumulators($accumulator,  'mbpackd', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultVernis" select="user:parce_accumulators($accumulator,  'vernis', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultHuawei2017" select="user:parce_accumulators($accumulator,  'huawei2017', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultDayinet" select="user:parce_accumulators($accumulator,  'dayinet', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultComebacktosms" select="user:parce_accumulators($accumulator,  'comebacktosms', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultWelcometosms" select="user:parce_accumulators($accumulator,  'welcometosms', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultBit" select="user:parce_accumulators($accumulator,  'bit', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultFizbit" select="user:parce_accumulators($accumulator,  'fizbit', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultBonuscorp" select="user:parce_accumulators($accumulator,  'bonuscorp', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultWelcome" select="user:parce_accumulators($accumulator,  'welcome', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultAjoyib" select="user:parce_accumulators($accumulator,  'ajoyib', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultArtel" select="user:parce_accumulators($accumulator,  'artel', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultSamsung" select="user:parce_accumulators($accumulator,  'samsung', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultBonushuawei" select="user:parce_accumulators($accumulator,  'bonushuawei', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultAutosms" select="user:parce_accumulators($accumulator,  'autosms', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultSmstosms" select="user:parce_accumulators($accumulator,  'smstosms', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultXiaomi" select="user:parce_accumulators($accumulator,  'xiaomi', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultSmsbesh" select="user:parce_accumulators($accumulator,  'smsbesh', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultVoicetosms" select="user:parce_accumulators($accumulator,  'voicetosms', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultStart" select="user:parce_accumulators($accumulator,  'start', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultBonusminutes" select="user:parce_accumulators($accumulator,  'bonusminutes', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultBonusinternet" select="user:parce_accumulators($accumulator,  'bonusinternet', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultVoicebesh" select="user:parce_accumulators($accumulator,  'voicebesh', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultAutosmscorp" select="user:parce_accumulators($accumulator,  'autosmscorp', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultCombo" select="user:parce_accumulators($accumulator,  'combo', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultEnrollee" select="user:parce_accumulators($accumulator,  'enrollee', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultGbon" select="user:parce_accumulators($accumulator,  'gbon', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultCompliment" select="user:parce_accumulators($accumulator,  'compliment', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultSupervoice" select="user:parce_accumulators($accumulator,  'supervoice', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultIct" select="user:parce_accumulators($accumulator,  'ict', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultWelcomebonus" select="user:parce_accumulators($accumulator,  'welcomebonus', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultWhirl" select="user:parce_accumulators($accumulator,  'whirl', $session_lang, $lang, $prefix)"/>
        <xsl:variable name="ResultSpasibo" select="user:parce_accumulators($accumulator,  'spasibo', $session_lang, $lang, $prefix)"/>
            
    <Message type="{/Message/@type}" hostname="dispatcher" id="{ext:newGuid()}" session_id="{/Message/@session_id}" >
    <xsl:attribute name="request_id">
		<xsl:choose>
			<xsl:when test="/Message/@request_id != ''">
				<xsl:value-of select="/Message/@request_id"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="/Message/@id"/>
			</xsl:otherwise>
		</xsl:choose>
    </xsl:attribute>
        <Item customer="{/Message/Item/@customer}">
			<xsl:attribute name="dbresult">
				<xsl:choose>
					<xsl:when test="/Message/Item/@dbresult != ''">
						<xsl:value-of select="/Message/Item/@dbresult"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="0"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:attribute>
			<Attribute name="accumulator" value="{/Message/Item/Attribute[@name='accumulator']/@value}"/>
			<Attribute name="balanceintpartdotpart" value="{/Message/Item/Attribute[@name='balanceintpartdotpart']/@value}" />
			<Attribute name="balance_expiration_date" value="{/Message/Item/Attribute[@name='balance_expiration_date']/@value}"/>
			<Attribute name="currency" value="{/Message/Item/Attribute[@name='currency']/@value}" />
			<xsl:element  name="Attribute">
				<xsl:attribute name='name'>full</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultFull' />
				</xsl:attribute>
			</xsl:element>
			<xsl:element  name="Attribute">
				<xsl:attribute name='name'>gprs</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultGprs' />
				</xsl:attribute>
			</xsl:element>
			<xsl:element  name="Attribute">
				<xsl:attribute name='name'>voice</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultVoice' />
				</xsl:attribute>
			</xsl:element>
			<xsl:element  name="Attribute">
				<xsl:attribute name='name'>sms</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultSms' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>bonussms</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultBonusSms' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>WMAXIIVR</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultWMAXIIVR' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>WOPTIMAI</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultWOPTIMAI' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>inetsms</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultInetsms' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>autoinet</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultAutoinet' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>huawei</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultHuawei' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>night</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultNight' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>bonus</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultBonus' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>superzero</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultSuperzero' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>superzero30</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultSuperzero30' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>samsungs7</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultSamsungs7' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>scginet</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultScginet' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>inetweek</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultInetweek' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>gprstosms</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultGprstosms' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>autoinettosms</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultAutoinettosms' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>blackfriday</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultBlackfriday' />
				</xsl:attribute>
			</xsl:element>
			<xsl:element  name="Attribute">
				<xsl:attribute name='name'>universal</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultUniversal' />
				</xsl:attribute>
			</xsl:element>
			<xsl:element  name="Attribute">
				<xsl:attribute name='name'>bzs</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultBzs' />
				</xsl:attribute>
			</xsl:element>
			<xsl:element  name="Attribute">
				<xsl:attribute name='name'>mbpack</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultMbpack' />
				</xsl:attribute>
			</xsl:element>
			<xsl:element  name="Attribute">
				<xsl:attribute name='name'>mbpackd</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultMbpackd' />
				</xsl:attribute>
			</xsl:element>
			<xsl:element  name="Attribute">
				<xsl:attribute name='name'>vernis</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultVernis' />
				</xsl:attribute>
			</xsl:element>
			<xsl:element  name="Attribute">
				<xsl:attribute name='name'>huawei2017</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultHuawei2017' />
				</xsl:attribute>
			</xsl:element>
			<xsl:element  name="Attribute">
				<xsl:attribute name='name'>dayinet</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultDayinet' />
				</xsl:attribute>
			</xsl:element>
			<xsl:element  name="Attribute">
				<xsl:attribute name='name'>comebacktosms</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultComebacktosms' />
				</xsl:attribute>
			</xsl:element>
			<xsl:element  name="Attribute">
				<xsl:attribute name='name'>welcometosms</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultWelcometosms' />
				</xsl:attribute>
			</xsl:element>
			<xsl:element  name="Attribute">
				<xsl:attribute name='name'>bit</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultBit' />
				</xsl:attribute>
			</xsl:element>
			<xsl:element  name="Attribute">
				<xsl:attribute name='name'>fizbit</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultFizbit' />
				</xsl:attribute>
			</xsl:element>
			<xsl:element  name="Attribute">
				<xsl:attribute name='name'>bonuscorp</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultBonuscorp' />
				</xsl:attribute>
			</xsl:element>
			<xsl:element  name="Attribute">
				<xsl:attribute name='name'>welcome</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultWelcome' />
				</xsl:attribute>
			</xsl:element>
			<xsl:element  name="Attribute">
				<xsl:attribute name='name'>ajoyib</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultAjoyib' />
				</xsl:attribute>
			</xsl:element>
			<xsl:element  name="Attribute">
				<xsl:attribute name='name'>artel</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultArtel' />
				</xsl:attribute>
			</xsl:element>
			<xsl:element  name="Attribute">
				<xsl:attribute name='name'>samsung</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultSamsung' />
				</xsl:attribute>
			</xsl:element>
			<xsl:element  name="Attribute">
				<xsl:attribute name='name'>bonushuawei</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultBonushuawei' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>autosms</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultAutosms' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>smstosms</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultSmstosms' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>xiaomi</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultXiaomi' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>smsbesh</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultSmsbesh' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>voicetosms</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultVoicetosms' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>start</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultStart' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>bonusminutes</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultBonusminutes' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>bonusinternet</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultBonusinternet' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>voicebesh</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultVoicebesh' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>autosmscorp</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultAutosmscorp' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>combo</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultCombo' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>enrollee</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultEnrollee' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>gbon</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultGbon' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>compliment</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultCompliment' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>supervoice</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultSupervoice' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>ict</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultIct' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>welcomebonus</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultWelcomebonus' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>whirl</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultWhirl' />
				</xsl:attribute>
			</xsl:element>
                        <xsl:element  name="Attribute">
				<xsl:attribute name='name'>spasibo</xsl:attribute>
				<xsl:attribute name='value'>
					<xsl:value-of select='$ResultSpasibo' />
				</xsl:attribute>
			</xsl:element>
        </Item>
    </Message>
</xsl:template>
</xsl:stylesheet>