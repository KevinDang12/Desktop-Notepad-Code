﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace Notepad
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();

            // Create a Frame to act as the navigation context and navigate to the first page
            Frame rootFrame = new Frame();
            rootFrame.NavigationFailed += OnNavigationFailed;
            // Navigate to the first page, configuring the new page
            // by passing required information as a navigation parameter
            rootFrame.Navigate(typeof(LoginPage), args.Arguments);

            // Place the frame in the current Window
            m_window.Content = rootFrame;

            m_window.Title = "Notepad";
            

            // Ensure the MainWindow is active
            m_window.Activate();
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private Window m_window;
    }
}
