using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using CreeperX.Tasks;

namespace CreeperX.Profiles;

public abstract class CreeperProfile : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged = delegate { };

    // See https://learn.microsoft.com/en-us/windows/apps/develop/data-binding/data-binding-in-depth
    private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public readonly Dictionary<string, Dictionary<string, object>> TempEntryItems; // Won't be stored into data file
    public readonly Dictionary<string, Dictionary<string, object>> EntryItems;

    public readonly ObservableCollection<CreeperTask> RootTasks;

    public readonly ObservableCollection<CreeperTask> RunningTasks;
    public readonly ObservableCollection<CreeperTask> FinishedTasks;

    public event Action<CreeperTask> OnTaskStart = delegate { };
    public event Action<CreeperTask> OnTaskEnd = delegate { };

    public string TypeName => GetType().Name;
    public readonly DirectoryInfo WorkDirectory;

    public string m_name = string.Empty;
    public string Name
    {
        get => m_name;
        set
        {
            m_name = value;
            NotifyPropertyChanged();
        }
    }

    public string m_currentPreset = string.Empty;
    public string CurrentPreset
    {
        get => m_currentPreset;
        set
        {
            m_currentPreset = value;
            NotifyPropertyChanged();
        }
    }

    public bool m_autoRunRootTasks = false;
    public bool AutoRunRootTasks
    {
        get => m_autoRunRootTasks;
        set
        {
            m_autoRunRootTasks = value;
            if (m_autoRunRootTasks)
            {
                TryRunNextRootTask();
            }
            NotifyPropertyChanged();
        }
    }

    private CreeperProfileStatus m_Status;
    public CreeperProfileStatus Status
    {
        get => m_Status;
        private set
        {
            m_Status = value;
            NotifyPropertyChanged();
        }
    }

    public CreeperProfile(string workDirectory)
    {
        WorkDirectory = new DirectoryInfo(workDirectory);

        var dataPath = Path.Combine(WorkDirectory.FullName, "data.json");

        EntryItems = LoadOrCreateEntryData(dataPath);
        TempEntryItems = new(); // Create an empty dictionary

        RunningTasks = new();
        FinishedTasks = new();

        RootTasks = GetInitialTasks();

        OnTaskStart += (task) =>
        {
            if (Status == CreeperProfileStatus.Idle)
            {
                Status = CreeperProfileStatus.Running;
            }

            RunningTasks.Add(task);
        };

        OnTaskEnd += (task) =>
        {
            RunningTasks.Remove(task);

            if (FinishedTasks.Contains(task)) // Previous running of the same task (unsuccessful), remove it
            {
                FinishedTasks.Remove(task);
            }

            FinishedTasks.Add(task);

            if (AutoRunRootTasks) // Try find next root task to run
            {
                TryRunNextRootTask();
            }

            if (RunningTasks.Count == 0) // No new task started
            {
                Status = CreeperProfileStatus.Idle;
            }
        };
    }

    private void TryRunNextRootTask()
    {
        if (RunningTasks.Count > 0) // Don't start if there's still any task running
        {
            return;
        }

        CreeperTask next = null;

        foreach (var task in RootTasks)
        {
            if (task.Status == CreeperTaskStatus.Pending || task.Status == CreeperTaskStatus.Failed)
            {
                next = task;
                break;
            }
        }

        if (next is not null) // Found a root task to run
        {
            _ = RunTask(next);
        }
        else // All root tasks completed successfully
        {
            // Store profile data
            StoreData();
        }
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

    public abstract string[] GetPresetNames();

    protected abstract ObservableCollection<CreeperTask> GetInitialTasks(string presetName = "");

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
