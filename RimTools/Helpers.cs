using System.Xml;

namespace RimTools
{
    internal static class Helpers
    {
        public static XmlNode GetXmlElement(XmlNode xml, string xpath)
        {
            return xml.SelectSingleNode(xpath) ?? throw new InvalidDataException($"The XPath '{xpath}' does not exist in the XML element.");
        }

        public static string GetXmlElementText(XmlNode xml, string xpath)
        {
            XmlNode node = xml.SelectSingleNode(xpath) ?? throw new InvalidDataException($"The XPath '{xpath}' does not exist in the XML element.");
            return node.InnerText;
        }

        public static string GetXmlElementTextOrDefault(XmlNode xml, string xpath, string defaultValue)
        {
            XmlNode? node = xml.SelectSingleNode(xpath);

            if (node == null)
            {
                return defaultValue;
            }

            return node.InnerText;
        }

        public static void SetXmlElementText(XmlNode xml, string xpath, string value)
        {
            XmlNode node = xml.SelectSingleNode(xpath) ?? throw new InvalidDataException($"The XPath '{xpath}' does not exist in the XML document.");
            node.InnerText = value;
        }

        public static IEnumerable<XmlElement> GetXmlElements(XmlNode xml, string xpath)
        {
            foreach (XmlElement w in xml.SelectNodes(xpath) ?? throw new InvalidDataException($"The XPath '{xpath}' does not exist in the XML document."))
            {
                yield return w;
            }
        }
    }
}
