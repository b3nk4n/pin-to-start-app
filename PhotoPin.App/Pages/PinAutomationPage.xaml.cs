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
using System.IO;
using System.Windows.Media.Imaging;

namespace PhotoPin.App.Pages
{
    public partial class PinAutomationPage : PhoneApplicationPage
    {
        private MediaLibrary _mediaLibrary;

        private static readonly Random RANDOM = new Random();

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
            // generate random number, so that each file name is unique, even when we pin a picture multiple times.
            var randomPrefix = RANDOM.Next(10000);
            var randomizedFileName = string.Format("{0:0000}_{1}", randomPrefix, image.FullName);

            var imagePath = string.Format("{0}{1}", LiveTileHelper.SHARED_SHELL_CONTENT_PATH, randomizedFileName);
            if (StorageHelper.SaveFileFromStream(imagePath, image.ImageStream))
            {
                var imageUri = new Uri(StorageHelper.ISTORAGE_SCHEME + imagePath, UriKind.Absolute);
                var tile = new CycleTileData()
                {
                    SmallBackgroundImage = imageUri,
                    CycleImages = new List<Uri>() { imageUri, imageUri }
                };

                var navigationUri = new Uri(string.Format("/Pages/PinAutomationPage.xaml?{0}={1}",
                    AppConstants.PARAM_FILE_NAME, randomizedFileName), UriKind.Relative);
                if (!LiveTileHelper.TileExists(navigationUri))
                {
                    LiveTilePinningHelper.PinOrUpdateTile(navigationUri, tile, true);
                }
                else
                {
                    // NOTE: since Version 1.3, we should basically never reach this message
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
            try
            {
                foreach (var pic in MediaLibrary.Pictures)
                {
                    if (pic.Name == fileName)
                    {
                        return new PinableImage(pic);
                    }
                }
                Debug.WriteLine("pic: " + fileName);
            }
            catch (InvalidOperationException ioex)
            {
                Debug.WriteLine("Could not retrieve photo from library with error: " + ioex.Message);
            }

            // second try, because sometime the file extenstion was not applied.
            // TODO: check if still necessary?!?
            foreach (var pic in MediaLibrary.Pictures)
            {
                var nameWithoutCounter = RemoveImageCopyCounter(fileName);
                if (pic.Name.Contains(fileName) || fileName.Contains(pic.Name) ||
                    pic.Name.Contains(nameWithoutCounter) || pic.Name.Contains(nameWithoutCounter))
                {
                    return new PinableImage(pic);
                }
            }
            return null;
        }

        private static string RemoveImageCopyCounter(string fileName)
        {
            if (fileName.Length <= 3)
                return fileName;

            var bracketsStart = fileName.IndexOf('(');
            var bracketsEnd = fileName.IndexOf(')');

            if (bracketsStart != -1 && bracketsEnd != -1 && bracketsStart < bracketsEnd)
            {
                try
                {
                    return fileName.Substring(0, bracketsStart);
                }
                catch (Exception) { }
            }

            return fileName;
        }

        /// <summary>
        /// Replacement for Path.GetFileNameWithoutExtension(), which throw "ArgumentException: Illegal characters in path".
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string ExtractFileExtension(string fileName)
        {
            var extensionStartIndex = fileName.LastIndexOf('.');

            if (extensionStartIndex != -1)
            {
                fileName = fileName.Substring(0, Math.Max(1, extensionStartIndex));
            }

            return fileName;
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