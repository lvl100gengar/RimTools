using System.Xml;

namespace RimTools;

public class Map(XmlDocument xml)
{
    internal readonly XmlDocument m_xml = xml ?? throw new ArgumentNullException(nameof(xml));

    public string UniqueID
    {
        get => Helpers.GetXmlElementText(m_xml, "/li/uniqueID");
        set => Helpers.SetXmlElementText(m_xml, "/li/uniqueID", value);
    }

    public string Parent
    {
        get => Helpers.GetXmlElementText(m_xml, "/li/mapInfo/parent");
        set => Helpers.SetXmlElementText(m_xml, "/li/mapInfo/parent", value);
    }
}