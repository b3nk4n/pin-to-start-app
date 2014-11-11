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
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media;
using System.IO;
using System.Windows.Threading;

namespace PhotoPin.App.Pages
{
    public partial class MainPage : PhoneApplicationPage
    {
        private bool _isInfoVisible = false;

        /// <summary>
        /// The photo chooser task.
        /// </summary>
        /// <remarks>Must be defined at class level to work properly in tombstoning.</remarks>
        private static PhotoChooserTask photoTask = new PhotoChooserTask();

        private static string fileNameToPin;

        /// <summary>
        /// Used for delayed pin, because there is an issue when we pin directly after the photo-task returns.
        /// </summary>
        private DispatcherTimer _delayedNavigaionTimer = new DispatcherTimer();

        // Konstruktor
        public MainPage()
        {
            InitializeComponent();

            BuildLocalizedApplicationBar();

            Loaded += (s, e) =>
            {
                CleanupSharedContentFolder();
            };

            _delayedNavigaionTimer.Interval = TimeSpan.FromMilliseconds(100);
            _delayedNavigaionTimer.Tick += (s, e) =>
            {
                _delayedNavigaionTimer.Stop();

                var uriString = new Uri(string.Format("/Pages/PinAutomationPage.xaml?{0}={1}", AppConstants.PARAM_SELECTED_FILE_NAME, fileNameToPin), UriKind.Relative);
                NavigationService.Navigate(uriString);
            };

            // init photo chooser task
            photoTask.ShowCamera = true;
            photoTask.Completed += (se, pr) =>
            {
                if (pr.Error != null || pr.TaskResult != TaskResult.OK)
                    return;

                fileNameToPin = Path.GetFileName(pr.OriginalFileName);
                _delayedNavigaionTimer.Start();
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
            photoTask.Show();
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