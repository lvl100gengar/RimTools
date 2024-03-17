using System.Xml;

namespace RimTools;

public class World(XmlDocument xml)
{
    internal readonly XmlDocument m_xml = xml ?? throw new ArgumentNullException(nameof(xml));

    public string NextWorldObjectID
    {
        get => Helpers.GetXmlElementText(m_xml, "//uniqueIDsManager/nextWorldObjectID");
        set => Helpers.SetXmlElementText(m_xml, "//uniqueIDsManager/nextWorldObjectID", value);
    }

    public string NextMapID
    {
        get => Helpers.GetXmlElementText(m_xml, "//uniqueIDsManager/nextMapID");
        set => Helpers.SetXmlElementText(m_xml, "//uniqueIDsManager/nextMapID", value);
    }

    public string SeedString
    {
        get => Helpers.GetXmlElementText(m_xml, "/savegame/game/world/info/seedString");
    }

    public IReadOnlyList<WorldObject> GetWorldObjects()
    {
        var worldObjects = new List<WorldObject>();

        foreach (var element in Helpers.GetXmlElements(m_xml, "/savegame/game/world/worldObjects/worldObjects/li"))
        {
            worldObjects.Add(new WorldObject(element));
        }

        return worldObjects;
    }

    public WorldObject? GetWorldObjectByID(string id)
    {
        // Id may be prefixed if pulled from map file (e.g. WorldObject_588)
        if (id.StartsWith("WorldObject_"))
        {
            id = id[12..];
        }

        foreach (var element in Helpers.GetXmlElements(m_xml, "/savegame/game/world/worldObjects/worldObjects/li"))
        {
            if (Helpers.GetXmlElementText(element, "ID") == id)
            {
                return new WorldObject(element);
            }
        }

        return null;
    }

    public void AddWorldObject(WorldObject worldObject)
    {
        ArgumentNullException.ThrowIfNull(worldObject);

        var importedNode = (XmlElement)m_xml.ImportNode(worldObject.m_xml, true);
        var importedObject = new WorldObject(importedNode)
        {
            ID = NextWorldObjectID
        };

        Helpers.GetXmlElement(m_xml, "/savegame/game/world/worldObjects/worldObjects").AppendChild(importedNode);

        NextWorldObjectID = (int.Parse(NextWorldObjectID) + 1).ToString();
    }

    public IReadOnlyList<Faction> GetFactions()
    {
        var factions = new List<Faction>();

        foreach (var element in Helpers.GetXmlElements(m_xml, "/savegame/game/world/factionManager/allFactions/li"))
        {
            factions.Add(new Faction(element));
        }

        return factions;
    }

    public Faction? GetFactionByID(string id)
    {
        // Id may be prefixed if pulled from map file (e.g. WorldObject_588)
        if (id.StartsWith("Faction_"))
        {
            id = id[8..];
        }

        foreach (var element in Helpers.GetXmlElements(m_xml, "/savegame/game/world/factionManager/allFactions/li"))
        {
            if (Helpers.GetXmlElementTextOrDefault(element, "loadID", "0") == id)
            {
                return new Faction(element);
            }
        }

        return null;
    }
}

public class WorldObject(XmlElement xml)
{
    internal readonly XmlElement m_xml = xml ?? throw new ArgumentNullException(nameof(xml));

    public string Def { get => Helpers.GetXmlElementText(m_xml, "def"); set => Helpers.SetXmlElementText(m_xml, "def", value); }
    public string ID { get => Helpers.GetXmlElementText(m_xml, "ID"); set => Helpers.SetXmlElementText(m_xml, "ID", value); }
    public string Tile { get => Helpers.GetXmlElementText(m_xml, "tile"); set => Helpers.SetXmlElementText(m_xml, "tile", value); }
    public string Faction { get => Helpers.GetXmlElementText(m_xml, "faction"); set => Helpers.SetXmlElementText(m_xml, "faction", value); }
    public string NameInt { get => Helpers.GetXmlElementText(m_xml, "nameInt"); set => Helpers.SetXmlElementText(m_xml, "nameInt", value); }
}

public class Faction(XmlElement xml)
{
    internal readonly XmlElement m_xml = xml ?? throw new ArgumentNullException(nameof(xml));

    public string Def { get => Helpers.GetXmlElementText(m_xml, "def"); set => Helpers.SetXmlElementText(m_xml, "def", value); }
    public string ID { get => Helpers.GetXmlElementTextOrDefault(m_xml, "loadID", "0"); set => Helpers.SetXmlElementText(m_xml, "loadID", value); }
    public string Name { get => Helpers.GetXmlElementText(m_xml, "name"); set => Helpers.SetXmlElementText(m_xml, "name", value); }
}