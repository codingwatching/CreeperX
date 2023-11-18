using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.ComponentModel;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml.Navigation;
using CreeperX.Tasks;
using CreeperX.Profiles;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CreeperX;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ProfilePage : Page
{
    public event PropertyChangedEventHandler PropertyChanged = delegate { };

    private ObservableCollection<CreeperTask> rootTasks;

    private readonly ObservableCollection<CreeperTask> runningTasks;
    private readonly ObservableCollection<CreeperTask> finishedTasks;

    private CreeperProfile activeProfile;

    public ProfilePage()
    {
        this.InitializeComponent();

        runningTasks = new ObservableCollection<CreeperTask>();
        finishedTasks = new ObservableCollection<CreeperTask>();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        activeProfile = e.Parameter as CreeperProfile;

        activeProfile.OnTaskStart += (task) => runningTasks.Add(task);
        activeProfile.OnTaskEnd += (task) =>
        {
            runningTasks.Remove(task);

            if (finishedTasks.Contains(task)) // Previous running of the same task (unsuccessful), remove it
            {
                finishedTasks.Remove(task);
            }

            finishedTasks.Add(task);
        };

        rootTasks = activeProfile.RootTasks;
    }

    private void SetSelectedItem(CreeperTask task)
    {
        TaskTreeView.SelectedItem = task;

        SelectedTaskView.Content = task;
    }

    private async void TaskTreeView_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
    {
        var task = args.InvokedItem as CreeperTask;

        if (task is not null)
        {
            SelectedTaskView.Content = task;

            // Try running the selected task
            await activeProfile.RunTask(task);
        }
    }

    private void FinishedTaskList_SelectionChanged(object sender, SelectionChangedEventArgs args)
    {
        if (args.AddedItems.Count == 0)
        {
            return;
        }

        var task = args.AddedItems.First() as CreeperTask;

        if (task is not null)
        {
            SetSelectedItem(task);
        }
    }

    private void RunningTaskList_SelectionChanged(object sender, SelectionChangedEventArgs args)
    {
        if (args.AddedItems.Count == 0)
        {
            return;
        }

        var task = args.AddedItems.First() as CreeperTask;

        if (task is not null)
        {
            SetSelectedItem(task);
        }
    }

    private void CopyWorkDirButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var package = new DataPackage();
        package.SetText(activeProfile.WorkDirectory.FullName);
        Clipboard.SetContent(package);
    }

    private void OpenWorkDirButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        try
        {
            Process.Start("explorer.exe", activeProfile.WorkDirectory.FullName);
        }
        catch (Win32Exception)
        {
            // The system cannot find the file specified...
        }
    }
}
