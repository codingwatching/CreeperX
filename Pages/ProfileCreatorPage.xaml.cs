using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage;
using CreeperX.Utils;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CreeperX
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProfileCreatorPage : Page, INotifyPropertyChanged
    {
        private readonly ObservableCollection<string> profileDefNames;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        // See https://learn.microsoft.com/en-us/windows/apps/develop/data-binding/data-binding-in-depth
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string m_selectedProfileDef = null;
        public string SelectedProfileDef
        {
            get => m_selectedProfileDef ?? "Select from the left panel";
            set
            {
                m_selectedProfileDef = value;
                NotifyPropertyChanged();
            }
        }

        private string m_workDir = null;
        public string WorkDir
        {
            get => m_workDir ?? "Directory not set";
            set
            {
                m_workDir = value;
                NotifyPropertyChanged();
            }
        }

        public ProfileCreatorPage()
        {
            this.InitializeComponent();

            profileDefNames = new ObservableCollection<string>();
            ProfileHelper.ProfileTypeDictionary.Keys.ToList().ForEach(x => profileDefNames.Add(x));

            // Update create button usability when work directory or profile is changed
            PropertyChanged += (_, _) =>
            {
                if (m_selectedProfileDef is null || m_workDir is null)
                {
                    CreateProfileButton.IsEnabled = false;
                }
                else
                {
                    CreateProfileButton.IsEnabled = true;
                }
            };

            // Disable create button on start
            CreateProfileButton.IsEnabled = false;
        }

        private async void PickFolderButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Create a folder picker
            var openPicker = new FolderPicker();

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var window = WindowHelper.GetWindowForElement(this);
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            // Initialize the folder picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            // Set options for your folder picker
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.FileTypeFilter.Add("*");

            // Open the picker for the user to pick a folder
            StorageFolder folder = await openPicker.PickSingleFolderAsync();
            if (folder is not null)
            {
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                WorkDir = folder.Path;
            }
            else
            {
                WorkDir = null;
            }
        }

        private void SelectProfileDef(string profileDef)
        {
            SelectedProfileDef = profileDef;
        }

        private void ProfileListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var selected = e.AddedItems.First();

                SelectProfileDef(selected as string);
            }
        }

        private void CreateProfileButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var workDir = PickFolderOutputTextBlock.Text;
            var name = ProfileNameTextBox.Text;

            if (m_selectedProfileDef is null || m_selectedProfileDef is null)
            {
                return;
            }

            try
            {
                var dir = new DirectoryInfo(workDir);

                if (!dir.Exists) // Create the directory if not present
                {
                    dir.Create();
                }

                // Store profile as json file
                var profileData = new Dictionary<string, object>()
                {
                    ["Definition"] = SelectedProfileDef,
                    ["Name"] = name,
                    ["WorkDir"] = dir.FullName
                };

                File.WriteAllText(Path.Combine(dir.FullName, "profile.json"),
                        JsonSerializer.Serialize(profileData, options: new() { WriteIndented = true }));

                // Create profile object
                var profile = ProfileHelper.CreateProfile(m_selectedProfileDef, name, m_workDir);

                Frame.NavigateToType(typeof(ProfilePage), profile, new() { IsNavigationStackEnabled = false } );
            }
            catch
            {
                // TODO: Handle exceptions
                throw;
            }
        }
    }
}
