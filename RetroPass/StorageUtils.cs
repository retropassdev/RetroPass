using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace RetroPass
{
	public class StorageUtils
	{
		public static async Task<StorageFile> GetFileAsync(StorageFolder storageFolder, string fileName)
		{
			try
			{
				Trace.TraceInformation("StorageUtils: GetFileAsync: {0} {1}", storageFolder.Path, fileName);
				var file = await storageFolder.GetFileAsync(fileName);
				return file;
			}
			catch (Exception e)
			{
				Trace.TraceError("StorageUtils: GetFileAsync FAIL: {0} {1}", storageFolder.Path, fileName);
				Trace.TraceError(e.Message);
				return null;
			}
		}

		public static async Task<StorageFile> GetFileFromPathAsync(string path)
		{
			try
			{
				Trace.TraceInformation("StorageUtils: GetFolderFromPathAsync: {0}", path);
				var file = await StorageFile.GetFileFromPathAsync(path);
				return file;
			}
			catch (Exception e)
			{
				Trace.TraceError("StorageUtils: GetFolderFromPathAsync FAIL: {0}", path);
				Trace.TraceError(e.Message);
				return null;
			}
		}

		public static async Task<StorageFolder> GetFolderFromPathAsync(string path)
		{
			try
			{
				Trace.TraceInformation("StorageUtils: GetFolderFromPathAsync: {0}", path);
				var folder = await StorageFolder.GetFolderFromPathAsync(path);
				return folder;
			}
			catch (Exception e)
			{

				Trace.TraceError("StorageUtils: GetFolderFromPathAsync FAIL: {0}", path);
				Trace.TraceError(e.Message);
				return null;
			}
		}

		public static string NormalizePath(string path)
		{
			return Path.GetFullPath(new Uri(path).LocalPath)
					   .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
					   .ToUpperInvariant();
		}
	}
}
