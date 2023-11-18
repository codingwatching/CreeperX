using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using WinRT.Interop;

namespace CreeperX.Utils
{
    // Helper class to allow the app to find the Window that contains an
    // arbitrary UIElement (GetWindowForElement).  To do this, we keep track
    // of all active Windows.  The app code must call WindowHelper.CreateWindow
    // rather than "new Window" so we can keep track of all the relevant windows.
    // See https://github.com/microsoft/WinUI-Gallery/blob/main/WinUIGallery/Helper/WindowHelper.cs
    public class WindowHelper
    {
        static public Window CreateWindow(Type windowType)
        {
            if (!windowType.IsAssignableTo(typeof (Window)))
            {
                throw new ArgumentException("Target type is not a window!");
            }

            var newWindow = Activator.CreateInstance(windowType) as Window;

            TrackWindow(newWindow);
            return newWindow;
        }

        static public void TrackWindow(Window window)
        {
            window.Closed += (sender, args) => {
                _activeWindows.Remove(window);
            };
            _activeWindows.Add(window);
        }

        static public AppWindow GetAppWindow(Window window)
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(window);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(wndId);
        }

        static public Window GetWindowForElement(UIElement element)
        {
            if (element.XamlRoot != null)
            {
                foreach (Window window in _activeWindows)
                {
                    if (element.XamlRoot == window.Content.XamlRoot)
                    {
                        return window;
                    }
                }
            }
            return null;
        }
        // get dpi for an element
        static public double GetRasterizationScaleForElement(UIElement element)
        {
            if (element.XamlRoot != null)
            {
                foreach (Window window in _activeWindows)
                {
                    if (element.XamlRoot == window.Content.XamlRoot)
                    {
                        return element.XamlRoot.RasterizationScale;
                    }
                }
            }
            return 0.0;
        }

        static public List<Window> ActiveWindows { get => _activeWindows; }

        private static readonly List<Window> _activeWindows = new List<Window>();
    }
}
