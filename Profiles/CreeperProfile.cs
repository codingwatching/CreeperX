using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using CreeperX.Tasks;

namespace CreeperX.Profiles;

public abstract class CreeperProfile
{
    public readonly Dictionary<string, Dictionary<string, object>> TempEntryItems; // Won't be stored into data file
    public readonly Dictionary<string, Dictionary<string, object>> EntryItems;

    public readonly ObservableCollection<CreeperTask> RootTasks;

    public event Action<CreeperTask> OnTaskStart = delegate { };
    public event Action<CreeperTask> OnTaskEnd = delegate { };

    public string Name { get; set; }
    public readonly DirectoryInfo WorkDirectory;

    public CreeperProfile(string workDirectory)
    {
        WorkDirectory = new DirectoryInfo(workDirectory);

        var dataPath = Path.Combine(WorkDirectory.FullName, "data.json");

        EntryItems = LoadOrCreateEntryData(dataPath);
        TempEntryItems = new(); // Create an empty dictionary

        RootTasks = GetInitialTasks();
    }

    public void StoreData()
    {
        var dataPath = Path.Combine(WorkDirectory.FullName, "data.json");
        StoreEntryData(EntryItems, dataPath);
    }

    public async virtual Task<bool> RunTask(CreeperTask task)
    {
        var succeeded = true;

        OnTaskStart(task);

        if (task.Status == CreeperTaskStatus.Pending || task.Status == CreeperTaskStatus.Failed)
        {
            succeeded = await task.Run(this);
        }

        OnTaskEnd(task);

        return succeeded;
    }

    protected abstract ObservableCollection<CreeperTask> GetInitialTasks();

    public static void StoreEntryData(Dictionary<string, Dictionary<string, object>> data, string path)
    {
        var jsonText = JsonSerializer.Serialize(data, options: new() { WriteIndented = true });
        File.WriteAllText(path, jsonText);
    }

    public static Dictionary<string, Dictionary<string, object>> LoadOrCreateEntryData(string path)
    {
        if (File.Exists(path))
        {
            var jsonText = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(jsonText);
        }
        // File not present, create a new dictionary
        return new();
    }
}
