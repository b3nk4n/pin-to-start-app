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
        private bool _isInfoVisible = false;

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
            ApplicationBar.Mode = ApplicationBarMode.Minimized;
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

        private void ChoosePhotoClicked(object sender, RoutedEventArgs e)
        {

        }

        private void InfoArrowClicked(object sender, RoutedEventArgs e)
        {
            _isInfoVisible = !_isInfoVisible;

            if (_isInfoVisible) {
                VisualStateManager.GoToState(this, "InfoState", true);
            } else {
                VisualStateManager.GoToState(this, "NormalState", true);
            }
        }
    }
}