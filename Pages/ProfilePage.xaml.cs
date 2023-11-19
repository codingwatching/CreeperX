using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.ComponentModel;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml.Navigation;
using CreeperX.Tasks;
using CreeperX.Profiles;
using System.Runtime.CompilerServices;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CreeperX;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ProfilePage : Page, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged = delegate { };

    // See https://learn.microsoft.com/en-us/windows/apps/develop/data-binding/data-binding-in-depth
    private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private ObservableCollection<CreeperTask> rootTasks;

    public bool AutoRun
    {
        get => ActiveProfile?.AutoRunRootTasks ?? false;
        set
        {
            if (ActiveProfile is not null)
            {
                ActiveProfile.AutoRunRootTasks = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(AutoRunToggleText));
            }
        }
    }

    public string AutoRunToggleText
    {
        get => AutoRun ? "Disable AutoRun" : "Enable AutoRun";
    }

    private CreeperProfile m_activeProfile;
    public CreeperProfile ActiveProfile
    {
        get => m_activeProfile;
        private set
        {
            m_activeProfile = value;
            NotifyPropertyChanged();
        }
    }

    public ProfilePage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        ActiveProfile = e.Parameter as CreeperProfile;

        rootTasks = ActiveProfile.RootTasks;
    }

    private void SetSelectedItem(CreeperTask task)
    {
        TaskTreeView.SelectedItem = task;

        SelectedTaskView.Content = task;
    }

    private void TaskTreeView_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
    {
        var task = args.InvokedItem as CreeperTask;

        if (task is not null)
        {
            SelectedTaskView.Content = task;
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
        package.SetText(m_activeProfile.WorkDirectory.FullName);
        Clipboard.SetContent(package);
    }

    private void OpenWorkDirButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        try
        {
            Process.Start("explorer.exe", m_activeProfile.WorkDirectory.FullName);
        }
        catch (Win32Exception)
        {
            // The system cannot find the file specified...
        }
    }

    private void StoreProfileDataItem_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        m_activeProfile.StoreData();
    }

    private void ToggleAutoRunItem_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        AutoRun = !AutoRun;
    }

    private void ForceStopItem_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {

    }

    private void SwitchPresetItem_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {

    }
}
