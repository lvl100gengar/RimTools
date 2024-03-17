using System.Formats.Tar;
using System.IO.Compression;
using System.Xml;

const string savePath = @"C:\Users\seanf\Desktop\rimworld-saves\save.zip";
const string test1Path = @"C:\Users\seanf\Desktop\rimworld-saves\test1.zip";
const string saveModifiedPath = @"C:\Users\seanf\Desktop\rimworld-saves\save-processed.zip";
const string processedPath = @"C:\Users\seanf\Desktop\rimworld-saves\save\processed";

var mainSave = new RimTools.SaveFile(savePath);
var newMergeRequest = new RimTools.SaveFile(test1Path);

//foreach (var map in mainSave.MapSave)
//{
//    Console.WriteLine($"Parsed map {map.Key} and unique ID of {map.Value.UniqueID} and parent {map.Value.Parent}");
//}

//foreach (var w in mainSave.WorldSave.GetWorldObjects())
//{
//    Console.WriteLine($"WorldOject id={w.ID} def={w.Def} faction={w.Faction} tile={w.Tile}");
//}

//Console.WriteLine($"Next WorldObject ID is {mainSave.WorldSave.NextWorldObjectID} and next Map ID is {mainSave.WorldSave.NextMapID}");

//// Test 1: Modify faction name to demo that changing elements works as expected
//mainSave.WorldSave.GetFactionByID("38").Name = "Raznoo AND Gow117's Colony";

//foreach (var o in newMergeRequest.WorldSave.GetWorldObjects())
//{
//    Console.WriteLine($"WorldObject: {o.Def} {o.ID} {o.Tile} {o.Faction}");
//}

//foreach (var faction in newMergeRequest.WorldSave.GetFactions())
//{
//    Console.WriteLine($"Faction: {faction.Def} {faction.Name} {faction.ID}");
//}

// Check world seed
if (mainSave.WorldSave.SeedString != newMergeRequest.WorldSave.SeedString)
{
    Console.WriteLine($"Warning: World seed of source does not match destination ('{mainSave.WorldSave.SeedString}' != '{newMergeRequest.WorldSave.SeedString}')");
}
else
{
    Console.WriteLine($"World seed is '{mainSave.WorldSave.SeedString}'");
}

// Select which map should be copied from the new save file
Console.WriteLine("\nList of maps in source save file:");
foreach (var mapEntry in newMergeRequest.MapSave)
{
    var outerId = mapEntry.Key;
    var innerId = mapEntry.Value.UniqueID;

    var mapWorldObject = newMergeRequest.WorldSave.GetWorldObjectByID(mapEntry.Value.Parent);
    var mapFaction = newMergeRequest.WorldSave.GetFactionByID(mapWorldObject.Faction);

    Console.WriteLine($"\t{outerId}) '{mapWorldObject.NameInt}' {mapWorldObject.Def}@{mapWorldObject.Tile} {mapFaction.Def} '{mapFaction.Name}'");
}

Console.Write("\nEnter map ID to be copied: ");

if (!int.TryParse(Console.ReadLine(), out int selectedMap) || !newMergeRequest.MapSave.ContainsKey(selectedMap))
{
    Console.WriteLine("Invalid map ID.");
    return;
}

// Select faction that the colony should be in
Console.WriteLine("\nList of factions in destination save file:");

var factions = mainSave.WorldSave.GetFactions();

foreach (var faction in factions)
{
    Console.WriteLine($"\t{faction.ID}) {faction.Def} '{faction.Name}'");
}

Console.Write("\nEnter faction ID that map will belong to: ");

if (!int.TryParse(Console.ReadLine(), out int selectedFaction) || !factions.Any(f => f.ID == selectedFaction.ToString()))
{
    Console.WriteLine("Invalid faction ID.");
    return;
}

Console.Write($"Enter a name of the colony [{newMergeRequest.WorldSave.GetWorldObjectByID(newMergeRequest.MapSave[selectedMap].Parent).NameInt}]: ");

string colonyName = Console.ReadLine()!;

// Perform the copy
Console.WriteLine("\nMaking requested changes...");

// Use destination ID manager values to assign IDs to new map
int nextMapID = int.Parse(mainSave.WorldSave.NextMapID);
int nextWorldObjectID = int.Parse(mainSave.WorldSave.NextWorldObjectID);

var worldObject = newMergeRequest.WorldSave.GetWorldObjectByID(newMergeRequest.MapSave[selectedMap].Parent)!;
worldObject.ID = nextWorldObjectID.ToString();
worldObject.Faction = $"Faction_{selectedFaction}";

if (!string.IsNullOrWhiteSpace(colonyName))
{
    worldObject.NameInt = colonyName;
}

mainSave.MapCmd[nextMapID] = newMergeRequest.MapCmd[selectedMap];
mainSave.MapSave[nextMapID] = newMergeRequest.MapSave[selectedMap];
mainSave.MapSave[nextMapID].UniqueID = nextMapID.ToString();
mainSave.MapSave[nextMapID].Parent = $"WorldObject_{nextWorldObjectID}";

// Change any faction references to the ID of the new world faction
var factionRefs1 = mainSave.MapSave[nextMapID].m_xml.SelectNodes("//faction[text() = 'Faction_12']")!;
foreach (XmlNode node in factionRefs1)
{
    Console.WriteLine("Updating faction reference 1");
    node.InnerText = "Faction_38";
}

var factionRefs2 = mainSave.MapSave[nextMapID].m_xml.SelectNodes("//mpMapComp/currentFactionId")!;
foreach (XmlNode node in factionRefs2)
{
    Console.WriteLine("Updating faction reference 2");
    node.InnerText = "38";
}

var factionRefs3 = mainSave.MapSave[nextMapID].m_xml.SelectNodes("//customFactionMapData/keys/li[text() = '12']")!;
foreach (XmlNode node in factionRefs3)
{
    Console.WriteLine("Updating faction reference 3");
    node.InnerText = "38";
}

var factionRefs4 = mainSave.MapSave[nextMapID].m_xml.SelectNodes("//customFactionMapData/values/li/factionId[text() = '12']")!;
foreach (XmlNode node in factionRefs4)
{
    Console.WriteLine("Updating faction reference 4");
    node.InnerText = "38";
}

// Add the world object to the destination
mainSave.WorldSave.AddWorldObject(worldObject);

// Increment next map ID
mainSave.WorldSave.NextMapID = (nextMapID + 1).ToString();

//foreach (var o in mainSave.WorldSave.GetWorldObjects())
//{
//    Console.WriteLine($"WorldObject: {o.Def} {o.ID} {o.Tile} {o.Faction}");
//}

mainSave.Save(saveModifiedPath);

ZipFile.ExtractToDirectory(saveModifiedPath, processedPath, true);
File.Copy(saveModifiedPath, @"C:\Users\seanf\Desktop\Multiplayer-master\Source\Server\bin\Debug\net6.0\save.zip", true);

//zipFile.Info.Xml.ImportNode(null, )