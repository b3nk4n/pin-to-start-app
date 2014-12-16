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
using PhoneKit.Framework.Core.Storage;
using PhoneKit.Framework.Core.Tile;

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

        /// <summary>
        /// Blocks multiple Show() calls, which can rise an Exception:
        /// <remarks>
        /// BUGSENSE: 15.12.14 (but in Photo Marker)
        /// Photo Note (1.0.0.0): Not allowed to call Show() multiple times before an invocation returns
        /// </remarks>
        /// </summary>
        private bool _multipleShowBlocker = false;

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
            ApplicationBar.Opacity = 0.99f;

            // add tile
            ApplicationBarIconButton appBarTileButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.tiles.plus.png", UriKind.Relative));
            appBarTileButton.Text = AppResources.AppBarCreateTile;
            appBarTileButton.Click += (s, e) =>
            {
                if (_multipleShowBlocker)
                    return;

                _multipleShowBlocker = true;
                photoTask.Show();
                _multipleShowBlocker = false;
                
            };
            ApplicationBar.Buttons.Add(appBarTileButton);

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
            var localFileNames = StorageHelper.GetFileNames(LiveTileHelper.SHARED_SHELL_CONTENT_PATH);

            if (localFileNames != null)
            {
                foreach (var fileName in localFileNames)
                {
                    var navigationUri = new Uri(string.Format("/Pages/PinAutomationPage.xaml?{0}={1}", AppConstants.PARAM_FILE_NAME, fileName), UriKind.Relative);
                    if (!LiveTileHelper.TileExists(navigationUri))
                    {
                        try
                        {
                            StorageHelper.DeleteFile(LiveTileHelper.SHARED_SHELL_CONTENT_PATH + fileName);
                        }
                        catch (Exception) { }
                    }
                }
            }
        }

        private void ChoosePhotoClicked(object sender, RoutedEventArgs e)
        {
            if (_multipleShowBlocker)
                return;
            _multipleShowBlocker = true;
            photoTask.Show();
            _multipleShowBlocker = false;
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