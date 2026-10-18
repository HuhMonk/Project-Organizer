using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

Console.Title = "Unity Project Sorter";

void Main()
{
    while(true)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("==============================");
        Console.WriteLine("     UNITY PROJECT SORTER");
        Console.WriteLine("          BY HUHMONKE");
        Console.WriteLine("==============================\n\n");
        Console.ResetColor();

        Console.Write("Select An Option\n" +
            "1. Select Unity Project\n" +
            "2. View folders to skip\n" +
            "3. Add folder to skip\n" +
            "4. Exit\n\n");

        string? input = Console.ReadLine();

        switch (input)
        {
            case "1":
                Console.Clear();
                Console.Write("Enter Unity project path: ");
                string? projectPath = Console.ReadLine()?.Trim().Trim('"');

                Console.Clear();

                if (!string.IsNullOrWhiteSpace(projectPath))
                {
                    SortUnityProject(projectPath);
                }
                else
                {
                    Console.WriteLine("Project path cannot be empty");
                    Console.ReadKey();
                    return;
                }
                break;
            case "2":
                Console.Clear();
                Console.WriteLine("List of All protected folders");
                PrintSecuredFolders();
                Console.WriteLine("Press any Key to return");
                Console.ReadKey();
                break;
            case "3":
                Console.Clear();
                Console.WriteLine("Write Folder to add (casing doesnt matter!!)");
                Console.WriteLine("Example: Resources, Editor, MyCustomFolder\n\n");
                string? folderToAdd = Console.ReadLine();

                AddSecuredFolder(folderToAdd);
                    

                Console.WriteLine("\nPress Any Key to Return");
                Console.ReadKey();
                break;
            case "4":
                Console.WriteLine("Closing Unity Project Sorter...");
                return;
            default:
                Console.Clear();
                Console.WriteLine("Input doesnt contain verified number try again\nPress any key to return");
                Console.ReadKey();
                break;
        }
    }

    
}





#region Folders
var fileTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
    // General
    [".cs"] = "Scripts",
    [".unity"] = "Scenes",
    [".mat"] = "Materials",
    [".shader"] = "Shaders",
    [".shadergraph"] = "Shaders",
    [".compute"] = "Shaders",

    // Models
    [".fbx"] = "Models",
    [".obj"] = "Models",
    [".blend"] = "Models",

    // Images
    [".png"] = "Textures",
    [".jpg"] = "Textures",
    [".jpeg"] = "Textures",
    [".hdr"] = "Textures",

    // Audio
    [".wav"] = "Audio/SFX",
    [".mp3"] = "Audio/SFX",
    [".ogg"] = "Audio/SFX",

    // Animation
    [".anim"] = "Animations",
    [".controller"] = "Animations",
    [".overrideController"] = "Animations",

    // Fonts
    [".ttf"] = "Fonts",
    [".otf"] = "Fonts",

    // Vids
    [".mp4"] = "Videos",
    [".mov"] = "Videos",
    [".webm"] = "Videos"
};

List<string> securedFolders = new List<string>()
    {
    "Editor",
    "Resources",
    "StreamingAssets",
    "Plugins",
    "Gizmos",
    "Standard Assets",
    "TextMesh Pro",
    "Settings",
    "XR",
    "XRI",
    "XRInputs",
    "Mirror",
    "MirrorVR",
    "Oculus",
    "NaughtyAttributes",
    "BetterFolders",
    "Photon",
    "PlayFabEditorExtensions",
    "PlayFabSDK",
    "MetaXR",
    "Bakery",
    "AutoVROptimizer",
    "OPS",
    "Mfuscator",
    "EasyInputs",
    "URP",
    "Samples",
    "GUPS",
    "ParrelSync"
    };

void PrintSecuredFolders()
{
    foreach(string folder in securedFolders)
    {
        Console.WriteLine($"- [{folder}]");
    }
}

void AddSecuredFolder(string folderName)
{
    if (String.IsNullOrWhiteSpace(folderName))
    {
        Console.WriteLine("Folder name cannot be empty");
        return;
    }
    folderName = folderName.Trim();

    

    if(folderName.Contains("/") || folderName.Contains("\\"))
    {
        Console.WriteLine("Enter a folder name not a folder path");
        return;
    }

    if(!securedFolders.Contains(folderName, StringComparer.OrdinalIgnoreCase))
    {
        securedFolders.Add(folderName);
        Console.WriteLine($"Added: {folderName} to protected folders");
    }
    else
    {
        Console.WriteLine($"Folder: {folderName} is already protected");
    }
}
#endregion

Main();

void SortUnityProject(string? projectPath)
{
    string assetsPath = Path.Combine(projectPath, "Assets");

    if (!Directory.Exists(assetsPath))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Invalid Unity project or missing Assets folder\nPress any key to return");
        Console.ResetColor();
        Console.ReadKey();
        return;
    }

    string[] targetFolders = fileTypes.Values.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

    Console.WriteLine("\nCreating missing folders...\n");

    foreach(string folder in targetFolders)
    {
        Directory.CreateDirectory(Path.Combine(assetsPath, folder));
    }

    Console.WriteLine("Scanning files...\n");

    string[] files = Directory.GetFiles(assetsPath, "*", SearchOption.AllDirectories);

    int moved = 0;
    int skipped = 0;
    int errors = 0;
    foreach(string file in files)
    {
        string? destination = null;
        bool assetMoved = false;

        try
        {
            if(!File.Exists(file))
            {
                skipped++;
                continue;
            }

            string relative = Path.GetRelativePath(assetsPath, file);

            string[] parts = relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);


            if(parts.Any(p => securedFolders.Contains(p, StringComparer.OrdinalIgnoreCase)))
            {
                skipped++;
                continue;
            }

            string extension = Path.GetExtension(file);
            if(!fileTypes.TryGetValue(extension, out string? targetFolder))
            {
                skipped++;
                continue;
            }

            string destinationFolder = Path.Combine(assetsPath, targetFolder);

            if (Path.GetFullPath(Path.GetDirectoryName(file)!).Equals(Path.GetFullPath(destinationFolder), StringComparison.OrdinalIgnoreCase))
            {
                skipped++;
                continue;
            }

            destination = Path.Combine(destinationFolder, Path.GetFileName(file));

            string metaFile = file + ".meta";
            string destinationMeta = destination + ".meta";

            if (File.Exists(destination) || File.Exists(destinationMeta))
            {
                Console.WriteLine($"SKIP (meta conflict): {relative}");
                skipped++;
                continue;
            }

            File.Move(file, destination);
            assetMoved = true;
            

            if (File.Exists(metaFile))
            {
                File.Move(metaFile, destinationMeta);
            }

            moved++;

            Console.WriteLine($"MOVED: {relative} -> {targetFolder}");
        }
        catch(Exception ex)
        {
            errors++;

            if(assetMoved && destination != null && File.Exists(destination) && !File.Exists(file))
            {
                try
                {
                    File.Move(destination, file);
                    Console.WriteLine($"ROLLBACK: Restored {file}");
                }
                catch (Exception ex2)
                {
                    Console.WriteLine($"ROLLBACK FAILED : {file}\n{ex2.Message}");
                }
            }

            Console.WriteLine($"ERROR: {file}");
            Console.WriteLine($"       {ex.Message}");
        }
    }

    Console.WriteLine("\n==============================");
    Console.WriteLine("          COMPLETE");
    Console.WriteLine("==============================");
    Console.WriteLine($"Moved: {moved}");
    Console.WriteLine($"Skipped: {skipped}");
    Console.WriteLine($"Errors: {errors}");

    Console.WriteLine("\nPress any key to return");
    Console.ReadKey();
    return;
}