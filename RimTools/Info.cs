using System.Xml;

namespace RimTools;

public class Info(XmlDocument xml)
{
    internal readonly XmlDocument m_xml = xml ?? throw new ArgumentNullException(nameof(xml));
}
