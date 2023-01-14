using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace RetroPass
{
    //use custom thumbnail caching to avoid increasing system thumb database storage and to easily delete
    public class ThumbnailCache
    {
        private static DataSource dataSource;
        private static DataSourceManager.DataSourceLocation activeDataSourceLocation;

        private static string folderNameRemovable = "ImageCacheRemovable";
        private static string folderNameLocal = "ImageCacheLocal";

        private static StorageFolder storageFolderRemovable;
        private static StorageFolder storageFolderLocal;

        private static ThumbnailCache instance = null;

        public static ThumbnailCache Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ThumbnailCache();
                }
                return instance;
            }
        }

        private async Task SaveSoftwareBitmapToFile(SoftwareBitmap softwareBitmap, StorageFile outputFile)
        {
            Trace.TraceInformation("Thumbnail Cache: SaveSoftwareBitmapToFile {0}", outputFile.Path);

            using (IRandomAccessStream stream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                // Create an encoder with the desired format
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);

                // Set the software bitmap
                encoder.SetSoftwareBitmap(softwareBitmap);

                // Set additional encoding parameters, if needed
                //encoder.BitmapTransform.ScaledWidth = 320;
                encoder.BitmapTransform.ScaledWidth = (uint)(256 * (float)softwareBitmap.PixelWidth / softwareBitmap.PixelHeight);
                encoder.BitmapTransform.ScaledHeight = 256;
                encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;
                encoder.IsThumbnailGenerated = false;

                try
                {
                    await encoder.FlushAsync();
                    Trace.TraceInformation("Thumbnail Cache: FlushAsync {0}", outputFile.Path);
                }
                catch (Exception e)
                {
                    Trace.TraceError("Thumbnail Cache: FlushAsync FAIL {0}", outputFile.Path);
                    Trace.TraceError(e.Message);
                }
            }
        }

        public async Task<StorageFile> CreateThumbnailFileAsync(StorageFolder sourceFolder, string name)
        {
            Trace.TraceInformation("ThumbnailCache: CreateThumbnailFileAsync {0} {1}", sourceFolder.Path, name);
            IStorageItem outputFile = await sourceFolder.TryGetItemAsync(name);

            if (outputFile != null)
            {
                return null;
            }

            StorageFile file = await sourceFolder.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);
            return file;
        }

        public async Task CreateFolderAsync(StorageFolder source, StorageFolder destinationContainer, string desiredName = null)
        {
            Trace.TraceInformation("ThumbnailCache: CreateThumbnailFileAsync CreateFolderAsync {0} {1} {2}", source.Path, destinationContainer.Path, desiredName);
            StorageFolder destinationFolder = null;
            destinationFolder = await destinationContainer.CreateFolderAsync(
                desiredName ?? source.Name, CreationCollisionOption.OpenIfExists);
        }

        public string Base64Encode(string str)
        {
            var strBytes = System.Text.Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(strBytes);
        }

        public StorageFolder cacheFolder = null;
        private int numImagesProcessed = 0;
        object numTasks = new object();
        object lockWriteThumb = new object();
        List<string> writeThumb = new List<string>();

        public async Task<BitmapImage> GetThumbnailAsync(StorageFile sourceFile)
        {
            //if(dataSource != null)
            string path = Path.GetRelativePath(dataSource.rootFolder, sourceFile.Path);
            string destPath = "";

            while (numImagesProcessed > 5)
            {
                await Task.Delay(10);
            }

            lock (numTasks)
            {
                numImagesProcessed++;
            }

            string encodedName = Base64Encode(path) + ".jpg";

            if (activeDataSourceLocation == DataSourceManager.DataSourceLocation.Local)
            {
                if (storageFolderLocal == null)
                {
                    storageFolderLocal = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync(folderNameLocal, CreationCollisionOption.OpenIfExists);
                }
                destPath = storageFolderLocal.Path;
            }
            else if (activeDataSourceLocation == DataSourceManager.DataSourceLocation.Removable)
            {
                if (storageFolderRemovable == null)
                {
                    storageFolderRemovable = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync(folderNameRemovable, CreationCollisionOption.OpenIfExists);
                }
                destPath = storageFolderRemovable.Path;
            }
            else
            {
                lock (numTasks)
                {
                    numImagesProcessed--;
                }

                return null;
            }

            //if file exists that is not older than original, just return that file
            BitmapImage thumbnail = await GetThumbnailAsync2(encodedName, destPath);

            if (thumbnail != null)
            {
                lock (numTasks)
                {
                    numImagesProcessed--;
                }

                return thumbnail;
            }

            while (writeThumb.Contains(encodedName) == true)
            {
                await Task.Delay(10);
            }

            //prevent reading image if writing is in process
            lock (lockWriteThumb)
            {
                if (writeThumb.Contains(encodedName) == false)
                {
                    writeThumb.Add(encodedName);
                }
            }

            // If thumbnail doesn't exists, create one
            // Create the decoder from the stream
            using (IRandomAccessStream fileStream = await sourceFile.OpenAsync(FileAccessMode.Read))
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream);
                // Get the SoftwareBitmap representation of the file
                var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                //StorageFolder destinationFolder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("Dest",);
                if (cacheFolder == null)
                {
                    cacheFolder = await StorageUtils.GetFolderFromPathAsync(destPath);
                }

                //StorageFolder destinationFolder = await StorageUtils.GetFolderFromPathAsync(destPath);
                StorageFile outputFile = await CreateThumbnailFileAsync(cacheFolder, encodedName);
                if (outputFile != null)
                {
                    await SaveSoftwareBitmapToFile(softwareBitmap, outputFile);
                }
            }

            lock (lockWriteThumb)
            {
                if (writeThumb.Contains(encodedName))
                {
                    writeThumb.Remove(encodedName);
                }
            }

            thumbnail = await GetThumbnailAsync2(encodedName, destPath);

            lock (numTasks)
            {
                numImagesProcessed--;
            }


            return thumbnail;
        }

        private async Task<BitmapImage> GetThumbnailAsync2(string fileName, string destPath)
        {
            if (cacheFolder == null)
            {
                cacheFolder = await StorageUtils.GetFolderFromPathAsync(destPath);
            }

            IStorageItem outputFile = await cacheFolder.TryGetItemAsync(fileName);

            BitmapImage bitmapImage = null;

            if (outputFile == null)
            {
                return bitmapImage;
            }

            while (writeThumb.Contains(fileName) == true)
            {
                await Task.Delay(10);
            }

            lock (lockWriteThumb)
            {
                if (writeThumb.Contains(fileName) == false)
                {
                    writeThumb.Add(fileName);
                }
            }

            using (IRandomAccessStream fileStream = await ((StorageFile)outputFile).OpenAsync(FileAccessMode.Read))
            {
                // Set the image source to the selected bitmap
                bitmapImage = new BitmapImage();

                try
                {
                    await bitmapImage.SetSourceAsync(fileStream);
                }
                catch (Exception e)
                {
                    Trace.TraceError("ThumbnailCache: bitmapImage.SetSourceAsync: {0}" + ((StorageFile)outputFile).Path);
                }
                //bitmapImage.UriSource = new Uri(((StorageFile)outputFile).Path);
            }

            lock (lockWriteThumb)
            {
                if (writeThumb.Contains(fileName))
                {
                    writeThumb.Remove(fileName);
                }
            }

            return bitmapImage;
        }

        public void Set(DataSource dataSource, DataSourceManager.DataSourceLocation activeDataSourceLocation)
        {
            ThumbnailCache.dataSource = dataSource;
            ThumbnailCache.activeDataSourceLocation = activeDataSourceLocation;
        }

        public async Task Delete(DataSourceManager.DataSourceLocation location)
        {
            IStorageItem assetItem = null;

            if (location == DataSourceManager.DataSourceLocation.Local)
            {
                assetItem = await ApplicationData.Current.LocalCacheFolder.TryGetItemAsync(folderNameLocal);
            }
            else if (location == DataSourceManager.DataSourceLocation.Removable)
            {
                assetItem = await ApplicationData.Current.LocalCacheFolder.TryGetItemAsync(folderNameRemovable);
            }

            if (assetItem != null)
            {
                await assetItem.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
        }
    }
}
