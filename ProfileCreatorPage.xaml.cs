using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage;
using CreeperX.TaskProfiles;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CreeperX
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProfileCreatorPage : Page
    {
        private readonly Dictionary<string, Type> classMap;

        private readonly ObservableCollection<string> profileNames;

        public ProfileCreatorPage()
        {
            this.InitializeComponent();

            var classes = new List<Type>()
            {
                typeof (TingRoomProfile),
                typeof (EroImageProfile)
            };

            classMap = classes.ToDictionary(x => x.Name, x => x);

            profileNames = new ObservableCollection<string>();
            classes.ForEach(x => profileNames.Add(x.Name));
        }

        private async void PickFolderButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Clear previous returned file name, if it exists, between iterations of this scenario
            PickFolderOutputTextBlock.Text = "";

            // Create a folder picker
            FolderPicker openPicker = new Windows.Storage.Pickers.FolderPicker();

            /*

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var window = WindowHelper.GetWindowForElement(this);
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            // Initialize the folder picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            */

            // Set options for your folder picker
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.FileTypeFilter.Add("*");

            // Open the picker for the user to pick a folder
            StorageFolder folder = await openPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                PickFolderOutputTextBlock.Text = "Picked folder: " + folder.Name;
            }
            else
            {
                PickFolderOutputTextBlock.Text = "Operation cancelled.";
            }
        }
    }
}
