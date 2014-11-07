using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PhotoPin.App.Resources;

namespace PhotoPin.App.Pages
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Konstruktor
        public MainPage()
        {
            InitializeComponent();

            BuildLocalizedApplicationBar();

            Loaded += (s, e) =>
            {
                CleanupSharedContentFolder();
            };
        }

        /// <summary>
        /// Builds the application bar.
        /// </summary>
        private void BuildLocalizedApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Opacity = 0.99f;

            // about
            ApplicationBarMenuItem appBarAboutMenuItem = new ApplicationBarMenuItem(AppResources.AboutTitle);
            appBarAboutMenuItem.Click += (s, e) =>
            {
                NavigationService.Navigate(new Uri("/Pages/AboutPage.xaml", UriKind.Relative));
            };
            ApplicationBar.MenuItems.Add(appBarAboutMenuItem);
        }

        private void CleanupSharedContentFolder()
        {
            // TODO: cleanup deprecated image copies in isolated storge.
        }
    }
}