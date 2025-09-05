using BPUtil;
using System;
using System.IO;
using System.IO.Compression;

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
			using (ZipArchive archive = new ZipArchive(zipInputStream, ZipArchiveMode.Read, leaveOpen: true))
			{
				foreach (ZipArchiveEntry entry in archive.Entries)
				{
					if (entry.Name.IEquals("iTachCommands.json")
						|| entry.Name.IEquals("BroadLinkCommands.json")
						|| entry.Name.IEquals("HotkeyConfig.cfg"))
					{
						string outputPath = Path.Combine(Globals.ApplicationDirectoryBase, entry.Name);
						using (Stream entryStream = entry.Open())
						using (FileStream fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None))
						{
							entryStream.CopyTo(fileStream);
						}
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
			using (ZipArchive archive = new ZipArchive(zipOutputStream, ZipArchiveMode.Create, leaveOpen: true))
			{
				success |= AddFileIfExists(archive, Path.Combine(Globals.ApplicationDirectoryBase, "iTachCommands.json"));
				success |= AddFileIfExists(archive, Path.Combine(Globals.ApplicationDirectoryBase, "BroadLinkCommands.json"));
				success |= AddFileIfExists(archive, Path.Combine(Globals.ApplicationDirectoryBase, "HotkeyConfig.cfg"));
			}
			return success;
		}

		private static bool AddFileIfExists(ZipArchive archive, string filePath)
		{
			FileInfo fi = new FileInfo(filePath);
			if (fi.Exists)
			{
				ZipArchiveEntry entry = archive.CreateEntry(fi.Name, CompressionLevel.Fastest);
				using (Stream entryStream = entry.Open())
				using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					fileStream.CopyTo(entryStream);
				}
				return true;
			}
			return false;
		}
	}
}
