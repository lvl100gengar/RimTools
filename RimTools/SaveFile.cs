using System.IO.Compression;
using System.Xml;

namespace RimTools;

public class SaveFile : IDisposable
{
    public Dictionary<int, MemoryStream> MapCmd = [];
    public Dictionary<int, Map> MapSave = [];
    public MemoryStream WorldCmd = new();
    public World WorldSave;
    public Info Info;

    public SaveFile(string filename)
    {
        var zipFile = ZipFile.OpenRead(filename);

        Console.WriteLine($"Save file: {filename}");

        foreach (var file in zipFile.Entries)
        {
            if (file.FullName.StartsWith("maps/"))
            {
                Console.WriteLine($"Map: {file.Name}");

                var parts = file.FullName.Split('_');
                var id = int.Parse(parts[1]);

                if (parts[2] == "cmds")
                {
                    MapCmd[id] = new MemoryStream();
                    file.Open().CopyTo(MapCmd[id]);
                }
                else if (parts[2] == "save")
                {
                    var mapSaveXml = new XmlDocument
                    {
                        // To match game-generated XML files, preserve inner XML whitespace
                        PreserveWhitespace = true
                    };
                    mapSaveXml.Load(file.Open());
                    MapSave[id] = new Map(mapSaveXml);
                }
                else
                {
                    Console.WriteLine("Unrecognized file in map folder.");
                }
            }
            else if (file.FullName.StartsWith("world/"))
            {
                Console.WriteLine($"World: {file.Name}");

                var parts = file.FullName.Split('_');

                if (parts[1] == "cmds")
                {
                    file.Open().CopyTo(WorldCmd);
                }
                else if (parts[1] == "save")
                {
                    var worldSaveXml = new XmlDocument()
                    {
                        // To match game-generated XML files, preserve inner XML whitespace
                        PreserveWhitespace = true
                    };
                    worldSaveXml.Load(file.Open());
                    WorldSave = new World(worldSaveXml);
                }
                else
                {
                    Console.WriteLine("Unrecognized file in world folder.");
                }
            }
            else if (file.FullName == "info")
            {
                var saveInfoXml = new XmlDocument()
                {
                    // To match game-generated XML files, preserve inner XML whitespace
                    PreserveWhitespace = true
                };
                saveInfoXml.Load(file.Open());
                Info = new Info(saveInfoXml);
            }
            else
            {
                Console.WriteLine($"Unknown Zip entry: {file.FullName}");
            }
        }

        if (MapSave.Count == 0)
        {
            throw new Exception("This save does not have any map saves");
        }
        if (WorldSave == null)
        {
            throw new Exception("This save does not have a world save.");
        }
        if (Info == null)
        {
            throw new Exception("This save does not have a save info.");
        }
    }

    public void Save(string filename)
    {
        // Create XML files that are formatted like those the game creates
        // No XML declaration and no whitepace outside of elements
        var settings = new XmlWriterSettings
        {
            Indent = false,
            OmitXmlDeclaration = true,
        };

        using var zipFile = File.Create(filename);
        using var zipArchive = new ZipArchive(zipFile, ZipArchiveMode.Create, true);

        foreach (var item in MapSave)
        {
            using var mapSaveStream = zipArchive.CreateEntry($"maps/000_{item.Key}_save").Open();
            using var mapSaveWriter = XmlWriter.Create(mapSaveStream, settings);

            item.Value.m_xml.Save(mapSaveWriter);
        }

        foreach (var item in MapCmd)
        {
            using var mapCmdsStream = zipArchive.CreateEntry($"maps/000_{item.Key}_cmds").Open();

            item.Value.Position = 0;
            item.Value.CopyTo(mapCmdsStream);
        }

        {
            using var worldSaveStream = zipArchive.CreateEntry("world/000_save").Open();
            using var worldSaveWriter = XmlWriter.Create(worldSaveStream, settings);

            WorldSave.m_xml.Save(worldSaveWriter);
        }

        {
            using var worldCmdsStream = zipArchive.CreateEntry("world/000_cmds").Open();

            WorldCmd.Position = 0;
            WorldCmd.CopyTo(worldCmdsStream);
        }

        {
            using var infoStream = zipArchive.CreateEntry("info").Open();
            using var infoWriter = XmlWriter.Create(infoStream, settings);

            Info.m_xml.Save(infoWriter);
        }
    }

    public void Dispose()
    {
        foreach (var item in MapCmd)
        {
            item.Value.Dispose();
        }

        WorldCmd.Dispose();
    }
}