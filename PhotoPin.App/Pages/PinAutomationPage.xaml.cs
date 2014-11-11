using System;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;
using PhotoPin.App.Resources;
using PhotoPin.App.Model;
using PhoneKit.Framework.Core.Storage;
using PhoneKit.Framework.Tile;
using PhoneKit.Framework.Core.Tile;
using System.Collections.Generic;

namespace PhotoPin.App.Pages
{
    public partial class PinAutomationPage : PhoneApplicationPage
    {
        private MediaLibrary _mediaLibrary;

        public PinAutomationPage()
        {
            InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // query string lookup
            bool success = false;
            if (NavigationContext.QueryString != null)
            {
                if (e.NavigationMode == NavigationMode.Back)
                {
                    BackOrTerminate();
                }

                if (NavigationContext.QueryString.ContainsKey(AppConstants.PARAM_FILE_TOKEN))
                {
                    var token = NavigationContext.QueryString[AppConstants.PARAM_FILE_TOKEN];

                    var image = GetImageFromToken(token);
                    if (image != null)
                    {
                        if(SaveAndPinImage(image))
                        {
                            success = true;
                        }
                    }
                }

                if (NavigationContext.QueryString.ContainsKey(AppConstants.PARAM_SELECTED_FILE_NAME))
                {
                    var selectedFileName = NavigationContext.QueryString[AppConstants.PARAM_SELECTED_FILE_NAME];

                    var image = GetImageFromFileName(selectedFileName);
                    if (image != null)
                    {
                        if (SaveAndPinImage(image))
                        {
                            success = true;
                        }
                    }
                }

                if (NavigationContext.QueryString.ContainsKey(AppConstants.PARAM_FILE_NAME))
                {
                    var fileName = NavigationContext.QueryString[AppConstants.PARAM_FILE_NAME];
                    var imagePath = string.Format("{0}{1}", LiveTileHelper.SHARED_SHELL_CONTENT_PATH, fileName);

                    if (StorageHelper.FileExists(imagePath))
                    {
                        var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri(StorageHelper.APPDATA_LOCAL_SCHEME + imagePath));
                        await Windows.System.Launcher.LaunchFileAsync(file);
                    }
                }

                // error handling - warning and go back or exit
                if (!success)
                {
                    MessageBox.Show(AppResources.MessageBoxNoImageFound, AppResources.MessageBoxWarning, MessageBoxButton.OK);
                    BackOrTerminate();
                    return;
                }
            }
        }

        private bool SaveAndPinImage(PinableImage image)
        {
            var imagePath = string.Format("{0}{1}", LiveTileHelper.SHARED_SHELL_CONTENT_PATH, image.FullName);
            if (StorageHelper.SaveFileFromStream(imagePath, image.FullImageStream))
            {
                var imageUri = new Uri(StorageHelper.ISTORAGE_SCHEME + imagePath, UriKind.Absolute);

                var tile = new FlipTileData();
                tile = new FlipTileData()
                {
                    SmallBackgroundImage = imageUri,
                    WideBackgroundImage = imageUri,
                    BackgroundImage = imageUri,
                };
                var navigationUri = new Uri(string.Format("/Pages/PinAutomationPage.xaml?{0}={1}", AppConstants.PARAM_FILE_NAME, image.FullName), UriKind.Relative);
                if (!LiveTileHelper.TileExists(navigationUri))
                {
                    LiveTilePinningHelper.PinOrUpdateTile(navigationUri, tile);
                }
                else
                {
                    MessageBox.Show(AppResources.MessageBoxAlreadyPinned, AppResources.MessageBoxInfo, MessageBoxButton.OK);
                }
                return true;
            }
            return false;
        }

        private PinableImage GetImageFromToken(string token)
        {
            Picture image = null;

            try
            {
                image = MediaLibrary.GetPictureFromToken(token);
            }
            catch (InvalidOperationException ioex)
            {
                Debug.WriteLine("Could not retrieve photo from library with error: " + ioex.Message);
            }

            return (image == null) ? null : new PinableImage(image);
        }

        private PinableImage GetImageFromFileName(string fileName)
        {
            IEnumerable<Picture> images = null;

            try
            {
                foreach (var pic in MediaLibrary.Pictures)
                {
                    if (pic.Name.StartsWith(fileName.Substring(0, 8)))
                    {
                        string name = pic.Name;
                    }
                }

                images = (from photos in MediaLibrary.Pictures
                          where photos.Name.Equals(fileName)
                          select photos);
            }
            catch (InvalidOperationException ioex)
            {
                Debug.WriteLine("Could not retrieve photo from library with error: " + ioex.Message);
            }

            // second try, because sometime the file extenstion was not applied.
            if (images == null || images.Count() == 0)
            {
                foreach (var pic in MediaLibrary.Pictures)
                {
                    if (pic.Name.Contains(fileName) || fileName.Contains(pic.Name))
                    {
                        return new PinableImage(pic);
                    }
                }
                return null;
            }

            return new PinableImage(images.First());

        }

        private void BackOrTerminate()
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
            else
                App.Current.Terminate();
        }

        public MediaLibrary MediaLibrary
        {
            get
            {
                if (_mediaLibrary == null)
                    _mediaLibrary = new MediaLibrary();
                return _mediaLibrary;
            }
        }
    }
}