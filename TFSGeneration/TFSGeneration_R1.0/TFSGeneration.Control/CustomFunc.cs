using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Win32;

namespace TFSAssist.Control
{
	public static class CustomFunc
	{
		/// <summary>
		/// Получаем все существующие папки на почте exchange
		/// </summary>
		/// <param name="service"></param>
		/// <returns></returns>
		public static List<Folder> GetAllFolders(ExchangeService service)
		{
			List<Folder> completeListOfFolderIds = new List<Folder>();
			FolderView folderView = new FolderView(int.MaxValue);
			//FindFoldersResults findFolderResults = service.FindFolders(WellKnownFolderName.PublicFoldersRoot, folderView);
			FindFoldersResults findFolderResults = service.FindFolders(WellKnownFolderName.MsgFolderRoot, folderView);
			foreach (Folder folder in findFolderResults)
			{
				completeListOfFolderIds.Add(folder);
				FindAllSubFolders(service, folder.Id, completeListOfFolderIds);
			}
			return completeListOfFolderIds;
		}

		private static void FindAllSubFolders(ExchangeService service, FolderId parentFolderId, List<Folder> completeListOfFolderIds)
		{
			//search for sub folders
			FolderView folderView = new FolderView(int.MaxValue);
			FindFoldersResults foundFolders = service.FindFolders(parentFolderId, folderView);

			// Add the list to the growing complete list
			completeListOfFolderIds.AddRange(foundFolders);

			// Now recurse
			foreach (Folder folder in foundFolders)
			{
				FindAllSubFolders(service, folder.Id, completeListOfFolderIds);
			}
		}

    }
}
