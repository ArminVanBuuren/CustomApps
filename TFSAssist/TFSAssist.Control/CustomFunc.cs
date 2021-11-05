using System.Collections.Generic;
using Microsoft.Exchange.WebServices.Data;

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
			var completeListOfFolderIds = new List<Folder>();
			var folderView = new FolderView(int.MaxValue);
			//FindFoldersResults findFolderResults = service.FindFolders(WellKnownFolderName.PublicFoldersRoot, folderView);
			var findFolderResults = service.FindFolders(WellKnownFolderName.MsgFolderRoot, folderView);
			foreach (var folder in findFolderResults)
			{
				completeListOfFolderIds.Add(folder);
				FindAllSubFolders(service, folder.Id, completeListOfFolderIds);
			}
			return completeListOfFolderIds;
		}

		private static void FindAllSubFolders(ExchangeService service, FolderId parentFolderId, List<Folder> completeListOfFolderIds)
		{
			//search for sub folders
			var folderView = new FolderView(int.MaxValue);
			var foundFolders = service.FindFolders(parentFolderId, folderView);

			// Add the list to the growing complete list
			completeListOfFolderIds.AddRange(foundFolders);

			// Now recurse
			foreach (var folder in foundFolders)
			{
				FindAllSubFolders(service, folder.Id, completeListOfFolderIds);
			}
		}

    }
}
