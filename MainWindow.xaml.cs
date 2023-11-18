using CreeperX.Profiles;
using CreeperX.Utils;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CreeperX
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(DraggableTitleBar);

            SystemBackdrop = new MicaBackdrop() { Kind = MicaKind.BaseAlt };

            var lastProfilePath = "D:\\SchoolWork\\CreeperX Test\\profile.json";
            var lastProfileJson = JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(lastProfilePath));

            var def = lastProfileJson["Definition"].ToString();
            var nam = lastProfileJson["Name"].ToString();
            var dir = lastProfileJson["WorkDir"].ToString();

            var profile = ProfileHelper.CreateProfile(def, nam, dir);

            // Create a tab on start
            MakeNewProfile(MainTabView, nam, typeof (ProfilePage), profile);
        }

        private static void MakeNewProfile(TabView tabView, string tabName, Type pageType, object payload)
        {
            // The Content of a TabViewItem is often a frame which hosts a page.
            var frame = new Frame();

            var newTab = new TabViewItem
            {
                IconSource = new SymbolIconSource() { Symbol = Symbol.Placeholder },
                Header = tabName,
                Content = frame
            };

            frame.Navigate(pageType, payload);

            // Add and select the new tab
            tabView.TabItems.Add(newTab);
            tabView.SelectedItem = newTab;
        }

        // Add a new Tab to the TabView
        private void MainTabView_AddNewTab(TabView sender, object args)
        {
            MakeNewProfile(MainTabView, "Profile Creator", typeof (ProfileCreatorPage), null);
        }

        // Remove the requested tab from the TabView
        private void MainTabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            sender.TabItems.Remove(args.Tab);
        }
    }
}
