using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Utils
{
	public static class IO
	{
		public static Regex CHECK_PATH = new Regex(@"^(?<DISC>\w{1})(\$|\:)\\(?<FULL>([^\\/\r\n\t]*[\\/])*(?<LAST>[^\\/\r\n\t]*))$",
			RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

		/// <summary>
		/// Determines a text file's encoding by analyzing its byte order mark (BOM).
		/// Defaults to ASCII when detection of the text file's endianness fails.
		/// </summary>
		/// <param name="filename">The text file to analyze.</param>
		/// <returns>The detected encoding.</returns>
		public static Encoding GetEncoding(string filename)
		{
			// Read the BOM
			var bom = new byte[4];
			using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
			{
				file.Read(bom, 0, 4);
			}

			// Analyze the BOM
			if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76)
				return Encoding.UTF7;
			if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf)
				return
					Encoding.UTF8; // BOM is EF BB BF. But you can't rely on this. Lots of UTF-8 files don't have a BOM, especially if they originated on non-Windows systems. But you can safely assume that if a file validates as UTF-8, it is UTF-8. False positives are rare.
			if ((bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) || (bom[0] == 0xff && bom[1] == 0xfe && bom[2] == 0 && bom[3] == 0))
				return
					Encoding.UTF32; // BOM is 00 00 FE FF (for BE) or FF FE 00 00 (for LE). But UTF-32 is easy to detect even without a BOM. This is because the Unicode code point range is restricted to U+10FFFF, and thus UTF-32 units always have the pattern 00 {00-10} xx xx (for BE) or xx xx {00-10} 00 (for LE). If the data has a length that's a multiple of 4, and follows one of these patterns, you can safely assume it's UTF-32. False positives are nearly impossible due to the rarity of 00 bytes in byte-oriented encodings.
			if (bom[0] == 0xff && bom[1] == 0xfe)
				return Encoding.Unicode; //UTF-16LE
			if (bom[0] == 0xfe && bom[1] == 0xff)
				return Encoding.BigEndianUnicode; //UTF-16BE
			if (bom[0] == 0 && bom[1] == 0)
				return Encoding.ASCII; // US-ASCII (No BOM, but you don't need one. ASCII can be easily identified by the lack of bytes in the 80-FF range.)
			return Encoding.UTF8;
			//using (var reader = new StreamReader(filename, Encoding.Default, true))
			//{
			//    if (reader.Peek() >= 0) // you need this!
			//        reader.Read();

			//    return reader.CurrentEncoding;
			//}
		}

		public static string GetPartialPath(string path, string excludePath)
		{
			var excludeRoot = Path.GetPathRoot(excludePath);
			var logsPathWithoutRoot = !excludeRoot.IsNullOrWhiteSpace() ? excludePath.Replace(excludeRoot, string.Empty, StringComparison.InvariantCultureIgnoreCase) : excludePath;
			var last = path.LastIndexOf(logsPathWithoutRoot, StringComparison.CurrentCultureIgnoreCase);
			if (last == -1)
				return path;

			var indexStart = last + logsPathWithoutRoot.Length;
			return path.Substring(indexStart, path.Length - indexStart).Trim('\\');
		}

		public static string SafeReadFile(string filePath, Encoding encoding = null)
		{
			if (!File.Exists(filePath))
				return null;

			var attempts = 0;
			while (!IsFileReady(filePath))
			{
				attempts++;
				System.Threading.Thread.Sleep(500);
				if (attempts > 3)
					return null;
			}

			string context;
			using (var sr = new StreamReader(filePath, encoding ?? GetEncoding(filePath)))
			{
				context = sr.ReadToEnd();
			}

			return context;
		}

		public static long CountLinesReader(string file)
		{
			return CountLinesReader(new FileInfo(file));
		}

		public static long CountLinesReader(FileInfo file)
		{
			if (!file.Exists)
				return -1;

			var lineCounter = 0L;
			using (var reader = new StreamReader(file.FullName, GetEncoding(file.FullName)))
				while (reader.ReadLine() != null)
					lineCounter++;

			return lineCounter;
		}

		private const char CR = '\r';
		private const char LF = '\n';
		private const char NULL = (char) 0;

		public static long CountLinesSmarter(string filePath)
		{
			using (var inputStream = File.OpenRead(filePath))
				return CountLinesSmarter(inputStream);
		}

		public static long CountLinesSmarter(Stream stream)
		{
			var lineCount = 0L;

			var byteBuffer = new byte[1024 * 1024];
			var prevChar = NULL;
			var pendingTermination = false;

			int bytesRead;
			while ((bytesRead = stream.Read(byteBuffer, 0, byteBuffer.Length)) > 0)
			{
				for (var i = 0; i < bytesRead; i++)
				{
					var currentChar = (char) byteBuffer[i];

					switch (currentChar)
					{
						case NULL:
							continue;
						case CR:
						case LF:
						{
							if (prevChar == CR && currentChar == LF)
							{
								continue;
							}

							lineCount++;
							pendingTermination = false;
							break;
						}
						default:
						{
							if (!pendingTermination)
							{
								pendingTermination = true;
							}

							break;
						}
					}

					prevChar = currentChar;
				}
			}

			if (pendingTermination)
				lineCount++;
			return lineCount;
		}

		public static void WriteFile(string path, string content, bool append = false, Encoding encoding = null)
		{
			if (encoding == null && File.Exists(path))
				using (var writetext = new StreamWriter(path, append, GetEncoding(path)))
					writetext.Write(content);
			else if (encoding != null)
				using (var writetext = new StreamWriter(path, append, encoding))
					writetext.Write(content);
			else
				using (var writetext = new StreamWriter(path, append))
					writetext.Write(content);
		}

		/// <summary>
		/// If the file can be opened for exclusive access it means that the file
		/// is no longer locked by another process.
		/// </summary>
		/// <param name="filename"></param>
		/// <returns></returns>
		public static bool IsFileReady(string filename)
		{
			try
			{
				using (var inputStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None))
				{
					return inputStream.Length > 0;
				}
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Get last name file or directory
		/// </summary>
		/// <param name="fileOrDirectoryPath"></param>
		/// <param name="excludeExtension"></param>
		/// <returns></returns>
		public static string GetLastNameInPath(string fileOrDirectoryPath, bool excludeExtension = false)
		{
			var splitStr = fileOrDirectoryPath.Split('\\');
			var lastName = splitStr[splitStr.Length - 1];

			if (!excludeExtension)
			{
				return lastName;
			}
			else
			{
				var splitFile = lastName.Split('.');
				return splitFile.Length <= 1 ? lastName : string.Join(".", splitFile.Take(splitFile.Length - 1));
			}
		}

		public static bool SetAllAccessPermissions(string filePath)
		{
			try
			{
				File.SetAttributes(filePath, FileAttributes.Normal);
				var dInfo = new DirectoryInfo(filePath);
				var dSecurity = dInfo.GetAccessControl();
				dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl,
					InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
				dInfo.SetAccessControl(dSecurity);

				var access = File.GetAccessControl(filePath);
				var everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
				access.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.ReadAndExecute, AccessControlType.Allow));
				File.SetAccessControl(filePath, access);
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		/// <summary>
		/// Add all access permissions to file
		/// </summary>
		/// <param name="filePath"></param>
		public static void GetAccessToFile(string filePath)
		{
			GetAccessToDirectory(filePath);
			var access = File.GetAccessControl(filePath);
			var everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
			access.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.ReadAndExecute, AccessControlType.Allow));
			//access.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
			File.SetAccessControl(filePath, access);
		}

		/// <summary>
		/// Add all access permissions to directory
		/// </summary>
		/// <param name="dirPath"></param>
		public static void GetAccessToDirectory(string dirPath)
		{
			var dInfo = new DirectoryInfo(dirPath);
			var dSecurity = dInfo.GetAccessControl();
			dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl,
				InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
			dInfo.SetAccessControl(dSecurity);
		}

		public static bool CheckFolderPermission(string folderPath)
		{
			var dirInfo = new DirectoryInfo(folderPath);
			try
			{
				dirInfo.GetAccessControl(AccessControlSections.All);
				return true;
			}
			catch (PrivilegeNotHeldException)
			{
				return false;
			}
		}

		public static string EvaluateFirstMatchPath(string mainDirPath, string absoluteFilePath)
		{
			if (Directory.Exists(mainDirPath))
				return mainDirPath;

			var firstPathParts = mainDirPath.Trim(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar).ToList();
			var secondPathParts = absoluteFilePath.Trim(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);

			for (var i = 0; i < firstPathParts.Count; i++)
			{
				var current = firstPathParts[i];
				if (!current.Equals(".") && !current.Equals(".."))
					continue;
				firstPathParts.RemoveAt(i);
				i--;
			}

			var relatNew = string.Join("\\", firstPathParts);
			for (var index = 0; index < secondPathParts.Length; index++)
			{
				var path = string.Join("\\", secondPathParts.Take(secondPathParts.Length - index));
				var prep = Path.Combine(path, relatNew);
				if (Directory.Exists(prep))
					return prep;
			}

			return string.Empty;
		}

		public static string EvaluateRelativePath(string mainDirPath, string absoluteFilePath)
		{
			if (mainDirPath == null)
				throw new ArgumentNullException(nameof(mainDirPath));
			if (absoluteFilePath == null)
				throw new ArgumentNullException(nameof(absoluteFilePath));

			var firstPathParts = mainDirPath.Trim(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);
			var secondPathParts = absoluteFilePath.Trim(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);
			var sameCounter = 0;

			for (var i = 0; i < Math.Min(firstPathParts.Length, secondPathParts.Length); i++)
			{
				if (!firstPathParts[i].Equals(secondPathParts[i], StringComparison.InvariantCultureIgnoreCase))
				{
					break;
				}

				sameCounter++;
			}

			if (sameCounter == 0)
			{
				return absoluteFilePath;
			}

			var newPath = string.Empty;
			for (var i = sameCounter; i < firstPathParts.Length; i++)
			{
				if (i > sameCounter)
				{
					newPath += Path.DirectorySeparatorChar;
				}

				newPath += "..";
			}

			if (newPath.Length == 0)
			{
				newPath = ".";
			}

			for (var i = sameCounter; i < secondPathParts.Length; i++)
			{
				newPath += Path.DirectorySeparatorChar;
				newPath += secondPathParts[i];
			}

			return newPath;
		}

		public static string MakeRelativeName(string basePath, string name)
		{
			if (basePath == null)
				throw new ArgumentNullException(nameof(basePath));
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			if (Path.IsPathRooted(basePath) == false)
				throw new ArgumentException("Base path must be rooted", nameof(basePath));
			if (name.EndsWith(@"\"))
				throw new ArgumentException("File name can't be directory path", nameof(name));

			if (!Path.IsPathRooted(name))
				return name;

			if (basePath.EndsWith(@"\") == false)
				basePath += @"\";

			var newUri = new Uri(name);
			var baseUri = new Uri(basePath);
			name = baseUri.MakeRelativeUri(newUri).ToString().Replace(@"/", @"\");

			return name;
		}

		public static string MakeRelativePath(string basePath, string path)
		{
			if (basePath == null)
				throw new ArgumentNullException(nameof(basePath));
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			if (Path.IsPathRooted(basePath) == false)
				throw new ArgumentException("Base path must be rooted", nameof(basePath));

			if (Path.IsPathRooted(path))
			{
				if (basePath.EndsWith(@"\") == false)
					basePath += @"\";
				if (path.EndsWith(@"\") == false)
					path += @"\";
				var newUri = new Uri(path);
				var baseUri = new Uri(basePath);
				path = baseUri.MakeRelativeUri(newUri).ToString().Replace(@"/", @"\");
			}

			if (path.IsNullOrEmpty())
				path = @".\";
			else if (path.EndsWith(@"\") == false)
				path += @"\";

			return path;
		}

		public static void SetFullControlPermissionsToEveryone(string dirPath)
		{
			const FileSystemRights rights = FileSystemRights.FullControl;

			var allUsers = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);

			// Add Access Rule to the actual directory itself
			var accessRule = new FileSystemAccessRule(
				allUsers,
				rights,
				InheritanceFlags.None,
				PropagationFlags.NoPropagateInherit,
				AccessControlType.Allow);

			var info = new DirectoryInfo(dirPath);
			var security = info.GetAccessControl(AccessControlSections.Access);

			security.ModifyAccessRule(AccessControlModification.Set, accessRule, out var result);

			if (!result)
			{
				throw new InvalidOperationException("Failed to give full-control permission to all users for path " + dirPath);
			}

			// add inheritance
			var inheritedAccessRule = new FileSystemAccessRule(
				allUsers,
				rights,
				InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
				PropagationFlags.InheritOnly,
				AccessControlType.Allow);

			security.ModifyAccessRule(AccessControlModification.Add, inheritedAccessRule, out var inheritedResult);

			if (!inheritedResult)
			{
				throw new InvalidOperationException($"Failed to give full-control permission inheritance to all users for \"{dirPath}\"");
			}

			info.SetAccessControl(security);
		}

		public static void CopyDirectory(string sourcePath, string destinationPath)
		{
			//Now Create all of the directories
			foreach (var dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
				Directory.CreateDirectory(dirPath.Replace(sourcePath, destinationPath));

			//Copy all the files & Replaces any files with the same name
			foreach (var newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
				File.Copy(newPath, newPath.Replace(sourcePath, destinationPath), true);
		}

		/// <summary>
		/// Recursively deletes a directory as well as any subdirectories and files. If the files are read-only, they are flagged as normal and then deleted.
		/// </summary>
		/// <param name="directory">The name of the directory to remove.</param>
		public static async Task DeleteReadOnlyDirectoryAsync(string directory)
		{
			await Task.Factory.StartNew(() => DeleteReadOnlyDirectory(directory));
		}

		/// <summary>
		/// Recursively deletes a directory as well as any subdirectories and files. If the files are read-only, they are flagged as normal and then deleted.
		/// </summary>
		/// <param name="directory">The name of the directory to remove.</param>
		public static void DeleteReadOnlyDirectory(string directory)
		{
			foreach (var subDirectory in Directory.EnumerateDirectories(directory))
			{
				DeleteReadOnlyDirectory(subDirectory);
			}

			foreach (var fileName in Directory.EnumerateFiles(directory))
			{
				var fileInfo = new FileInfo(fileName)
				{
					Attributes = FileAttributes.Normal
				};
				fileInfo.Delete();
			}

			Directory.Delete(directory);
		}

		public static string GetTemporaryDirectory()
		{
			var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(tempDirectory);
			return tempDirectory;
		}

		public static Hashtable GetFileProcesses(string file)
		{
			var myProcessArray = new Hashtable();
			var processes = Process.GetProcesses();
			var i = 0;
			for (i = 0; i <= processes.GetUpperBound(0) - 1; i++)
			{
				try
				{
					var myProcess = processes[i];
					//if (!myProcess.HasExited) //This will cause an "Access is denied" error
					if (myProcess.Threads.Count <= 0)
						continue;

					var modules = myProcess.Modules;
					var j = 0;
					for (j = 0; j <= modules.Count - 1; j++)
					{
						if (modules[j].FileName.IndexOf(file, StringComparison.InvariantCultureIgnoreCase) != -1)
						{
							myProcessArray.Add(modules[j].FileName, myProcess);
							break;
							// TODO: might not be correct. Was : Exit For
						}
					}
				}
				catch (Exception ex)
				{
					//MsgBox(("Error : " & exception.Message)) 
				}
			}

			return myProcessArray;
		}

		public static long GetTotalFreeSpace(string driveName)
		{
			foreach (var drive in DriveInfo.GetDrives())
			{
				if (drive.IsReady && drive.Name == driveName)
				{
					return drive.TotalFreeSpace;
				}
			}

			return -1;
		}


		/// <summary>
		/// Formats the byte count to closest byte type
		/// </summary>
		/// <param name="bytes">The amount of bytes</param>
		/// <param name="newBytes"></param>
		/// <param name="decimalPlaces">How many decimal places to show</param>
		/// <param name="showByteType">Add the byte type on the end of the string</param>
		/// <returns>The bytes formatted as specified</returns>
		public static string FormatBytes(long bytes, out double newBytes, int decimalPlaces = 1, bool showByteType = true)
		{
			newBytes = bytes;
			var formatString = "{0";
			var byteType = "b";

			if (newBytes <= 1024)
			{
				newBytes = bytes;
			}
			else if (newBytes > 1024 && newBytes < 1048576)
			{
				newBytes /= 1024;
				byteType = "Kb";
			}
			else if (newBytes > 1048576 && newBytes < 1073741824)
			{
				// Check if best size in MB
				newBytes /= 1048576;
				byteType = "Mb";
			}
			else
			{
				// Best size in GB
				newBytes /= 1073741824;
				byteType = "Gb";
			}

			// Show decimals
			if (decimalPlaces > 0)
				formatString += ":0.";

			// Add decimals
			for (var i = 0; i < decimalPlaces; i++)
				formatString += "0";

			// Close placeholder
			formatString += "}";

			// Add byte type
			if (showByteType)
				formatString += byteType;

			return string.Format(formatString, newBytes);
		}

		public static int ToKilobytes(this long bytes)
		{
			return ((int) bytes) / 1024;
		}

		public static int ToMegabytes(this long bytes)
		{
			return ((int) bytes) / 1048576;
		}

		public static int ToGigabytes(this long bytes)
		{
			return ((int) bytes) / 1073741824;
		}

		/// <summary>
		/// Return a string describing the value as a file size.
		/// For example, 1.23 MB.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string ToFileSize(this int value)
		{
			return ToFileSize((double) value);
		}

		/// <summary>
		/// Return a string describing the value as a file size.
		/// For example, 1.23 MB.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string ToFileSize(this long value)
		{
			return ToFileSize((double) value);
		}

		static readonly string[] suffixes = {"bytes", "Kb", "Mb", "Gb", "Tb", "Pb", "Eb", "Zb", "Yb"};

		/// <summary>
		/// Return a string describing the value as a file size.
		/// For example, 1.23 MB.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string ToFileSize(this double value)
		{
			for (var i = 0; i < suffixes.Length; i++)
			{
				if (value <= (Math.Pow(1024, i + 1)))
				{
					return ThreeNonZeroDigits(value / Math.Pow(1024, i)) + " " + suffixes[i];
				}
			}

			return ThreeNonZeroDigits(value / Math.Pow(1024, suffixes.Length - 1)) + " " + suffixes[suffixes.Length - 1];
		}

		private static string ThreeNonZeroDigits(double value)
		{
			if (value >= 100)
			{
				// No digits after the decimal.
				return value.ToString("0,0");
			}
			else if (value >= 10)
			{
				// One digit after the decimal.
				return value.ToString("0.0");
			}
			else
			{
				// Two digits after the decimal.
				return value.ToString("0.00");
			}
		}

		public static bool HasReadPermissionOnDir(string path)
		{
			try
			{
				var readAllow = false;
				var readDeny = false;
				var accessControlList = Directory.GetAccessControl(path);

				//get the access rules that pertain to a valid SID/NTAccount.
				var accessRules = accessControlList?.GetAccessRules(true, true, 
					typeof(System.Security.Principal.SecurityIdentifier));

				if (accessRules == null)
					return false;

				//we want to go over these rules to ensure a valid SID has access
				foreach (FileSystemAccessRule rule in accessRules)
				{
					if ((FileSystemRights.Read & rule.FileSystemRights) != FileSystemRights.Read) continue;

					if (rule.AccessControlType == AccessControlType.Allow)
						readAllow = true;
					else if (rule.AccessControlType == AccessControlType.Deny)
						readDeny = true;
				}

				return readAllow && !readDeny;
			}
			catch (UnauthorizedAccessException ex)
			{
				return false;
			}
		}

		public static bool HasWritePermissionOnDir(string path)
		{
			try
			{
				var writeAllow = false;
				var writeDeny = false;
				var accessControlList = Directory.GetAccessControl(path);

				var accessRules = accessControlList?.GetAccessRules(true, true,
					typeof(System.Security.Principal.SecurityIdentifier));

				if (accessRules == null)
					return false;

				foreach (FileSystemAccessRule rule in accessRules)
				{
					if ((FileSystemRights.Write & rule.FileSystemRights) != FileSystemRights.Write)
						continue;

					if (rule.AccessControlType == AccessControlType.Allow)
						writeAllow = true;
					else if (rule.AccessControlType == AccessControlType.Deny)
						writeDeny = true;
				}

				return writeAllow && !writeDeny;
			}
			catch (UnauthorizedAccessException ex)
			{
				return false;
			}
		}

		static readonly string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

		/// <summary>
		/// Корректирует пути
		/// </summary>
		/// <param name="sample"></param>
		/// <returns></returns>
		public static string RemoveInvalidChars(string sample)
		{
			var encoded = Encoding.GetEncoding(1251).GetBytes(sample);
			var corrected = Encoding.GetEncoding(1250).GetString(encoded);

			foreach (var c in invalid)
			{
				corrected = corrected.Replace(c.ToString(), "");
			}

			return corrected;
		}

		#region Who is looking of file

		[StructLayout(LayoutKind.Sequential)]
		struct RM_UNIQUE_PROCESS
		{
			public int dwProcessId;
			public System.Runtime.InteropServices.ComTypes.FILETIME ProcessStartTime;
		}

		const int RmRebootReasonNone = 0;
		const int CCH_RM_MAX_APP_NAME = 255;
		const int CCH_RM_MAX_SVC_NAME = 63;

		enum RM_APP_TYPE
		{
			RmUnknownApp = 0,
			RmMainWindow = 1,
			RmOtherWindow = 2,
			RmService = 3,
			RmExplorer = 4,
			RmConsole = 5,
			RmCritical = 1000
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		struct RM_PROCESS_INFO
		{
			public RM_UNIQUE_PROCESS Process;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCH_RM_MAX_APP_NAME + 1)]
			public string strAppName;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCH_RM_MAX_SVC_NAME + 1)]
			public string strServiceShortName;

			public RM_APP_TYPE ApplicationType;
			public uint AppStatus;
			public uint TSSessionId;
			[MarshalAs(UnmanagedType.Bool)] public bool bRestartable;
		}

		[DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
		static extern int RmRegisterResources(uint pSessionHandle,
			UInt32 nFiles,
			string[] rgsFilenames,
			UInt32 nApplications,
			[In] RM_UNIQUE_PROCESS[] rgApplications,
			UInt32 nServices,
			string[] rgsServiceNames);

		[DllImport("rstrtmgr.dll", CharSet = CharSet.Auto)]
		static extern int RmStartSession(out uint pSessionHandle, int dwSessionFlags, string strSessionKey);

		[DllImport("rstrtmgr.dll")]
		static extern int RmEndSession(uint pSessionHandle);

		[DllImport("rstrtmgr.dll")]
		static extern int RmGetList(uint dwSessionHandle,
			out uint pnProcInfoNeeded,
			ref uint pnProcInfo,
			[In, Out] RM_PROCESS_INFO[] rgAffectedApps,
			ref uint lpdwRebootReasons);

		/// <summary>
		/// Find out what process(es) have a lock on the specified file.
		/// </summary>
		/// <param name="path">Path of the file.</param>
		/// <returns>Processes locking the file</returns>
		/// <remarks>See also:
		/// http://msdn.microsoft.com/en-us/library/windows/desktop/aa373661(v=vs.85).aspx
		/// http://wyupdate.googlecode.com/svn-history/r401/trunk/frmFilesInUse.cs (no copyright in code at time of viewing)
		/// 
		/// </remarks>
		public static List<Process> WhoIsLocking(string path)
		{
			var key = Guid.NewGuid().ToString();
			var processes = new List<Process>();
			var res = RmStartSession(out var handle, 0, key);
			if (res != 0)
				throw new Exception("Could not begin restart session.  Unable to determine file locker.");

			try
			{
				const int ERROR_MORE_DATA = 234;
				uint pnProcInfo = 0,
					lpdwRebootReasons = RmRebootReasonNone;

				var resources = new[] {path}; // Just checking on one resource.

				res = RmRegisterResources(handle, (uint) resources.Length, resources, 0, null, 0, null);

				if (res != 0) throw new Exception("Could not register resource.");

				//Note: there's a race condition here -- the first call to RmGetList() returns
				//      the total number of process. However, when we call RmGetList() again to get
				//      the actual processes this number may have increased.
				res = RmGetList(handle, out var pnProcInfoNeeded, ref pnProcInfo, null, ref lpdwRebootReasons);

				if (res == ERROR_MORE_DATA)
				{
					// Create an array to store the process results
					var processInfo = new RM_PROCESS_INFO[pnProcInfoNeeded];
					pnProcInfo = pnProcInfoNeeded;

					// Get the list
					res = RmGetList(handle, out pnProcInfoNeeded, ref pnProcInfo, processInfo, ref lpdwRebootReasons);
					if (res == 0)
					{
						processes = new List<Process>((int) pnProcInfo);

						// Enumerate all of the results and add them to the 
						// list to be returned
						for (var i = 0; i < pnProcInfo; i++)
						{
							try
							{
								processes.Add(Process.GetProcessById(processInfo[i].Process.dwProcessId));
							}
							// catch the error -- in case the process is no longer running
							catch (ArgumentException)
							{
							}
						}
					}
					else throw new Exception("Could not list processes locking resource.");
				}
				else if (res != 0) throw new Exception("Could not list processes locking resource. Failed to get size of result.");
			}
			finally
			{
				RmEndSession(handle);
			}

			return processes;
		}

		#endregion
	}
}