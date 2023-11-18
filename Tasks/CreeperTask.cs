using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CreeperX.Profiles;

namespace CreeperX.Tasks;

public class CreeperTask : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged = delegate { };

    // See https://learn.microsoft.com/en-us/windows/apps/develop/data-binding/data-binding-in-depth
    private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private string m_title = string.Empty;
    public string Title
    {
        get => m_title;
        set
        {
            m_title = value;
            NotifyPropertyChanged();
        }
    }

    private CreeperTaskStatus m_status = CreeperTaskStatus.Pending;
    public CreeperTaskStatus Status
    {
        get => m_status;
        set
        {
            m_status = value;
            NotifyPropertyChanged();
        }
    }

    public static string GetGlyphForStatus(CreeperTaskStatus status)
    {
        return status switch
        {
            CreeperTaskStatus.Pending   => "\uE823",   // Icon name: Recent
            CreeperTaskStatus.Running   => "\uEBE7",   // Icon name: RightArrowKeyTime0
            CreeperTaskStatus.Failed    => "\uE894",   // Icon name: Clear
            CreeperTaskStatus.Succeeded => "\uE73E",   // Icon name: CheckMark

            _                    => "\uE823",   // Icon name: Recent
        };
    }

    public virtual string GetInfo() => "Task";

    public CreeperTask Parent
    {
        get; set;
    }

    private ObservableCollection<CreeperTask> m_children = new();
    public ObservableCollection<CreeperTask> Children
    {
        get => m_children;
        set => m_children = value;
    }

    protected void AddChild(CreeperTask child)
    {
        child.Parent = this;
        Children.Add(child);
    }

    protected void AddSibling(CreeperProfile profile, CreeperTask sibling)
    {
        if (Parent != null) // Use own parent
        {
            sibling.Parent = Parent;
            Parent.Children.Add(sibling);
        }
        else // Add to profile's root tasks
        {
            profile.RootTasks.Add(sibling);
        }
    }

    private bool m_isExpanded;
    public bool IsExpanded
    {
        get => m_isExpanded;
        set
        {
            m_isExpanded = value;
            NotifyPropertyChanged();
        }
    }

    public async virtual Task<bool> Run(CreeperProfile profile)
    {
        // Succeeded flag, remains true only if all children succeeded
        var succeeded = true;

        if (Children.Count > 0)
        {
            foreach (var child in Children)
            {
                succeeded = succeeded && await profile.RunTask(child);
            }
        }

        return succeeded;
    }
}
