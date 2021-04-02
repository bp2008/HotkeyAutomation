using BPUtil;
using Ionic.Zip;
using System;
using System.IO;

namespace HotkeyAutomation
{
	public static class ConfigurationIO
	{
		/// <summary>
		/// Reads zipped configuration data from the specified stream.
		/// </summary>
		/// <param name="zipInputStream">Stream containing zip file content.</param>
		/// <returns>True if the operation is successful.</returns>
		public static bool ReadFromStream(Stream zipInputStream)
		{
			bool success = false;
			using (ZipFile zipper = ZipFile.Read(zipInputStream))
			{
				foreach(ZipEntry zipEntry in zipper)
				{
					if (zipEntry.FileName.Equals("iTachCommands.json", StringComparison.OrdinalIgnoreCase)
						|| zipEntry.FileName.Equals("BroadLinkCommands.json", StringComparison.OrdinalIgnoreCase)
						|| zipEntry.FileName.Equals("HotkeyConfig.cfg", StringComparison.OrdinalIgnoreCase))
					{
						zipEntry.Extract(Globals.ApplicationDirectoryBase, ExtractExistingFileAction.OverwriteSilently);
						success = true;
					}
				}
			}
			return success;
		}
		/// <summary>
		/// Writes zipped configuration data to the specified stream.
		/// </summary>
		/// <param name="zipOutputStream">Stream to write zip file content into.</param>
		/// <returns>True if the operation is successful.</returns>
		public static bool WriteToStream(Stream zipOutputStream)
		{
			bool success = false;

			// So ... writing with ZipOutputStream doesn't work.  It throws an exception when writing the first file.

			//using (ZipOutputStream zipper = new ZipOutputStream(zipOutputStream, true))
			using (ZipFile zipper = new ZipFile())
			{
				zipper.CompressionMethod = CompressionMethod.Deflate;
				zipper.CompressionLevel = Ionic.Zlib.CompressionLevel.BestSpeed;

				success |= AddFileIfExists(zipper, Path.Combine(Globals.ApplicationDirectoryBase, "iTachCommands.json"));
				success |= AddFileIfExists(zipper, Path.Combine(Globals.ApplicationDirectoryBase, "BroadLinkCommands.json"));
				success |= AddFileIfExists(zipper, Path.Combine(Globals.ApplicationDirectoryBase, "HotkeyConfig.cfg"));

				zipper.Save(zipOutputStream);
			}
			return success;
		}
		private static bool AddFileIfExists(ZipOutputStream zos, string filePath)
		{
			FileInfo fi = new FileInfo(filePath);
			if (fi.Exists)
			{
				zos.PutNextEntry(fi.Name);
				using (FileStream fsInput = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					fsInput.CopyTo(zos);
				}
				return true;
			}
			return false;
		}
		private static bool AddFileIfExists(ZipFile zipFile, string filePath)
		{
			FileInfo fi = new FileInfo(filePath);
			if (fi.Exists)
			{
				byte[] buffer = File.ReadAllBytes(filePath);
				zipFile.AddEntry(fi.Name, buffer);
				return true;
			}
			return false;
		}
	}
}