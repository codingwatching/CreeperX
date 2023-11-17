using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CreeperX.Tasks;

namespace CreeperX.TaskProfiles;

public abstract class CreeperProfile
{
    public readonly Dictionary<string, Dictionary<string, bool>> VisitedUris = new();

    public readonly ObservableCollection<CreeperTask> RootTasks;

    public event Action<CreeperTask> OnTaskStart = delegate { };

    public event Action<CreeperTask> OnTaskEnd = delegate { };

    public readonly DirectoryInfo WorkDirectory;

    public CreeperProfile(string workDirectory)
    {
        RootTasks = GetInitialTasks();

        WorkDirectory = new DirectoryInfo(workDirectory);
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
}
