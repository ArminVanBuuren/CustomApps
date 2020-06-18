using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LogsReader.Config;
using LogsReader.Properties;
using LogsReader.Reader.Forms;
using Utils;
using Utils.WinForm;

namespace LogsReader.Reader
{
	public enum GroupType
	{
		Server = 0,
		FileType = 1
	}

	public enum ProcessingType
	{
		CreateGroupItem = 0,
		CreateGroupChildItem = 1,
		Remove = 2
	}

    public delegate void TreeViewContainerChanged(bool clearStatus, bool onSchemeChanged);
	public delegate void TreeViewContainerError(Exception ex);

    public class TreeViewContainer
    {
	    public const string TRVServers = "trvServers";
	    public const string TRVTypes = "trvTypes";
	    public const string TRVFolders = "trvFolders";

        private const string ServerGroupTreeNodeName = "ServerGroup";
        private const string ServerTreeNodeName = "Server";

        private const string FileTypeGroupTreeNodeName = "FileTypeGroup";
	    private const string FileTypeTreeNodeName = "FileType";

	    private const string FolderTreeNodeName = "Folder";

	    private readonly List<CustomTreeView> _copyList;
	    private readonly ImageList _imageList;
	    private readonly Font _defaultParentFont = new Font("Segoe UI", 8.5F, FontStyle.Bold);

        private ContextMenuStrip _contextTreeMainMenuStrip;

        /// <summary>
        /// Текущая схема настроек
        /// </summary>
        private LRSettingsScheme CurrentSettings { get; }

        private CustomTreeView Current { get; set; }

        private CustomTreeView Main { get; }

        public event TreeViewContainerChanged OnChanged;
        public event TreeViewContainerError OnError;

        public TreeViewContainer(
	        LRSettingsScheme schemeSettings,
	        CustomTreeView main,
	        Dictionary<string, IEnumerable<string>> servers,
	        Dictionary<string, IEnumerable<string>> fileTypes,
	        Dictionary<string, bool> folders)
        {
	        CurrentSettings = schemeSettings;
	        Current = Main = main;
	        var treeNodeServersGroup = GetGroupNodes(TRVServers, Resources.Txt_LogsReaderForm_Servers, servers, GroupType.Server);
	        Main.Nodes.Add(treeNodeServersGroup);
	        CheckTreeViewNode(treeNodeServersGroup, false);

	        var treeNodeTypesGroup = GetGroupNodes(TRVTypes, Resources.Txt_LogsReaderForm_Types, fileTypes, GroupType.FileType);
	        Main.Nodes.Add(treeNodeTypesGroup);
	        CheckTreeViewNode(treeNodeTypesGroup, false);

	        var treeNodeFolders = new TreeNode(Resources.Txt_LogsReaderForm_LogsFolder)
	        {
		        Name = TRVFolders,
		        Checked = true,
		        NodeFont = _defaultParentFont
	        };
	        Main.Nodes.Add(treeNodeFolders);
	        SetFolder(null, folders, false);
	        treeNodeFolders.Expand();
	        CheckTreeViewNode(treeNodeFolders, true);

	        _copyList = new List<CustomTreeView>();

	        _imageList = new ImageList {ImageSize = new Size(14, 14)};
	        _imageList.Images.Add("0", Resources._default);
	        _imageList.Images.Add("1", Resources.server_group);
	        _imageList.Images.Add("2", Resources.types_group);
	        _imageList.Images.Add("3", Resources.server);
	        _imageList.Images.Add("4", Resources.type);
	        _imageList.Images.Add("5", Resources.folder);

	        _copyList.Add(InitializeTreeView(Main));
	        Main.EnabledChanged += (sender, args) =>
	        {
		        foreach (var treeView in _copyList.Where(treeView => treeView != Main))
			        treeView.Enabled = Main.Enabled;
	        };
        }

        public void ApplySettings()
        {
	        foreach (var treeView in _copyList)
            {
	            AssignTreeViewText(treeView, TRVServers, Resources.Txt_LogsReaderForm_Servers);
	            AssignTreeViewText(treeView, TRVTypes, Resources.Txt_LogsReaderForm_Types);
	            AssignTreeViewText(treeView, TRVFolders, Resources.Txt_LogsReaderForm_LogsFolder);
            }

            _contextTreeMainMenuStrip?.Dispose();
            _contextTreeMainMenuStrip = new ContextMenuStrip { Tag = Main };
            _contextTreeMainMenuStrip.Items.Add(Resources.Txt_LogsReaderForm_AddServerGroup, Resources.server_group, AddServerGroup);
            _contextTreeMainMenuStrip.Items.Add(Resources.Txt_LogsReaderForm_AddServer, Resources.server, AddServer);
            _contextTreeMainMenuStrip.Items.Add(new ToolStripSeparator());
            _contextTreeMainMenuStrip.Items.Add(Resources.Txt_LogsReaderForm_AddFileTypeGroup, Resources.types_group, AddFileTypeGroup);
            _contextTreeMainMenuStrip.Items.Add(Resources.Txt_LogsReaderForm_AddFileType, Resources.type, AddFileType);
            _contextTreeMainMenuStrip.Items.Add(new ToolStripSeparator());
            _contextTreeMainMenuStrip.Items.Add(Resources.Txt_LogsReaderForm_AddFolder, Resources.folder, SetFolder);
            _contextTreeMainMenuStrip.Items.Add(new ToolStripSeparator());
            _contextTreeMainMenuStrip.Items.Add(Resources.Txt_LogsReaderForm_RemoveSelected, Resources.remove, RemoveSelectedNodeItem);
            _contextTreeMainMenuStrip.Items.Add(Resources.Txt_LogsReaderForm_Properties, Resources.properies, OpenProperties);
        }

        void AssignTreeViewText(TreeView treeView, string name, string value)
        {
	        var treeNode = treeView.Nodes[name];
	        if (treeNode != null)
		        treeNode.Text = value;
        }

        public CustomTreeView CreateNewCopy()
        {
            var copy = new CustomTreeView();
            _copyList.Add(InitializeTreeView(copy));
            return copy;
        }

        CustomTreeView InitializeTreeView(CustomTreeView treeView)
        {
            treeView.ImageList = _imageList;
            treeView.ItemHeight = 18;
            treeView.Indent = 18;
            AssignTreeViewText(treeView, TRVServers, Resources.Txt_LogsReaderForm_Servers);
            AssignTreeViewText(treeView, TRVTypes, Resources.Txt_LogsReaderForm_Types);
            AssignTreeViewText(treeView, TRVFolders, Resources.Txt_LogsReaderForm_LogsFolder);

            treeView.MouseDown += TreeMain_MouseDown;
            treeView.AfterCheck += TrvMain_AfterCheck;
            return treeView;
        }

        void TreeMain_MouseDown(object sender, MouseEventArgs e)
        {
            if (!(sender is CustomTreeView current))
                return;
            Current = current;

            try
            {
	            Current.SelectedNode = Current.GetNodeAt(e.X, e.Y);
                if (e.Button != MouseButtons.Right)
                    return;

                _contextTreeMainMenuStrip?.Show(Current, e.Location);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
            }
        }

        void TrvMain_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (!(sender is CustomTreeView current))
                return;
            Current = current;

            foreach (var treeView in _copyList)
	            treeView.AfterCheck -= TrvMain_AfterCheck;

            try
            {
	            CheckTreeViewNode(e.Node, e.Node.Checked);
                OnChanged?.Invoke(true, false);
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
	            foreach (var treeView in _copyList)
		            treeView.AfterCheck += TrvMain_AfterCheck;
            }
        }

        public void MainFormKeyDown(CustomTreeView treeView, KeyEventArgs e)
        {
	        try
	        {
		        Current = treeView;

                switch (e.KeyCode)
		        {
			        case Keys.G when e.Control && Main.Enabled:
				        AddServerGroup(this, EventArgs.Empty);
				        break;
			        case Keys.R when e.Control && Main.Enabled:
				        AddServer(this, EventArgs.Empty);
				        break;
			        case Keys.H when e.Control && Main.Enabled:
				        AddFileTypeGroup(this, EventArgs.Empty);
				        break;
			        case Keys.T when e.Control && Main.Enabled:
				        AddFileType(this, EventArgs.Empty);
				        break;
			        case Keys.J when e.Control && Main.Enabled:
				        SetFolder(this, EventArgs.Empty);
				        break;
			        case Keys.Delete:
				        RemoveSelectedNodeItem(this, EventArgs.Empty);
				        break;
                    case Keys.Enter when Main.Enabled && Main.Focused:
			        case Keys.P when e.Control && Main.Enabled:
				        OpenProperties(this, EventArgs.Empty);
				        break;
		        }
            }
	        catch (Exception ex)
	        {
		        OnError?.Invoke(ex);
	        }
        }

        void AddServerGroup(object sender, EventArgs args)
        {
            SetTreeNodes(GroupType.Server, ProcessingType.CreateGroupItem);
        }

        void AddServer(object sender, EventArgs args)
        {
            SetTreeNodes(GroupType.Server, ProcessingType.CreateGroupChildItem);
        }

        void AddFileTypeGroup(object sender, EventArgs args)
        {
            SetTreeNodes(GroupType.FileType, ProcessingType.CreateGroupItem);
        }

        void AddFileType(object sender, EventArgs args)
        {
            SetTreeNodes(GroupType.FileType, ProcessingType.CreateGroupChildItem);
        }

        void OpenProperties(object sender, EventArgs args)
        {
            if (Current.SelectedNode == null || Compare(Current.SelectedNode, TRVServers))
                SetTreeNodes(GroupType.Server, ProcessingType.CreateGroupChildItem);
            else if (Compare(Current.SelectedNode, TRVTypes))
                SetTreeNodes(GroupType.FileType, ProcessingType.CreateGroupChildItem);
            else if (Current.SelectedNode.Name == TRVFolders)
                SetFolder(Current.SelectedNode.FirstNode, GetFolders(Current, false), true);
            else
                SetFolder(Current.SelectedNode, GetFolders(Current, false), true);
        }

        void SetTreeNodes(GroupType _groupType, ProcessingType processingType)
        {
            try
            {
                var treeNode = _groupType == GroupType.Server ? Current.Nodes[TRVServers] : Current.Nodes[TRVTypes];

                var treeGroups = GetGroups(treeNode);
                var clone = new Dictionary<string, List<string>>(treeGroups, treeGroups.Comparer);

                if (processingType == ProcessingType.Remove)
                {
	                Current.SelectedNode?.Remove();
                    treeGroups = GetGroups(treeNode);
                }
                else
                {
                    DialogResult dialogResul;
                    if (processingType == ProcessingType.CreateGroupItem || treeNode.Nodes.Count == 0)
                    {
                        dialogResul = new AddGroupForm(treeGroups, _groupType).ShowDialog();
                    }
                    else
                    {
                        var groupName = treeGroups.First().Key;
                        if (Current.SelectedNode?.Parent == treeNode)
                            groupName = Current.SelectedNode.Text;
                        else if (Current.SelectedNode?.Parent?.Parent == treeNode)
                            groupName = Current.SelectedNode.Parent.Text;

                        dialogResul = AddGroupForm.ShowGroupItemsForm(groupName, treeGroups, _groupType);
                    }

                    if (dialogResul == DialogResult.Cancel)
                        return;
                }

                var nodes = new List<TreeNode>();
                var groupItems = new List<LRGroupItem>(treeGroups.Count);
                foreach (var newGroup in treeGroups.OrderBy(x => x.Key))
                {
                    if (newGroup.Value.Count == 0 || newGroup.Value.All(x => x.IsNullOrEmptyTrim()))
                        continue;

                    var childTreeNode = GetGroupItems(newGroup.Key, newGroup.Value, _groupType);
                    nodes.Add(childTreeNode);
                    groupItems.Add(new LRGroupItem(newGroup.Key, string.Join(", ", newGroup.Value)));

                    if (!clone.TryGetValue(newGroup.Key, out var existGroup)
                        || existGroup.Except(newGroup.Value).Any()
                        || newGroup.Value.Except(existGroup).Any())
                    {
                        childTreeNode.Expand();
                        childTreeNode.Checked = true;
                        CheckTreeViewNode(childTreeNode, true);
                    }
                }

                if (_groupType == GroupType.Server)
                    CurrentSettings.Servers = new LRGroups(groupItems.ToArray());
                else
                    CurrentSettings.FileTypes = new LRGroups(groupItems.ToArray());

                treeNode.Nodes.Clear();
                treeNode.Checked = false;
                treeNode.Nodes.AddRange(nodes.ToArray());

                var checkedNode = nodes.FirstOrDefault(x => x.Checked);
                if (checkedNode != null)
                {
                    checkedNode.Checked = true;
                    Current.SelectedNode = checkedNode;
                }
            }
            catch (Exception ex)
            {
	            OnError?.Invoke(ex);
            }
            finally
            {
	            OnChanged?.Invoke(false, true);
            }
        }

        static Dictionary<string, List<string>> GetGroups(TreeNode treeNode)
        {
            return treeNode
                .Nodes.OfType<TreeNode>()
                .OrderBy(x => x.Text)
                .ToDictionary(x => x.Text,
                    x => new List<string>(x.Nodes.OfType<TreeNode>().Select(p => p.Text)),
                    StringComparer.InvariantCultureIgnoreCase);
        }

        void SetFolder(object sender, EventArgs e)
        {
            SetFolder(null, GetFolders(Current, false), true);
        }

        void SetFolder(TreeNode selectedNode, Dictionary<string, bool> items, bool showForm)
        {
            try
            {
                var clone = new Dictionary<string, bool>(items, items.Comparer);
                if (showForm)
                {
                    AddFolder folderForm;
                    if (selectedNode != null)
                        folderForm = new AddFolder(selectedNode.Text.Substring(5, selectedNode.Text.Length - 5).Trim(), selectedNode.Text.StartsWith("[All]", StringComparison.InvariantCultureIgnoreCase));
                    else
                        folderForm = new AddFolder(null, true);

                    var result = folderForm.ShowDialog();

                    if (result == DialogResult.Cancel || folderForm.FolderPath.IsNullOrEmptyTrim())
                        return;

                    if (items.TryGetValue(folderForm.FolderPath, out var allDirSearching1))
                    {
                        if (allDirSearching1 != folderForm.AllDirectoriesSearching)
                            items[folderForm.FolderPath] = folderForm.AllDirectoriesSearching;
                        else
                            return;
                    }
                    else if (folderForm.SourceFolder != null && items.TryGetValue(folderForm.SourceFolder, out var _))
                    {
                        items.RenameKey(folderForm.SourceFolder, folderForm.FolderPath);
                        items[folderForm.FolderPath] = folderForm.AllDirectoriesSearching;
                    }
                    else
                    {
                        items.Add(folderForm.FolderPath, folderForm.AllDirectoriesSearching);
                    }
                }

                var nodes = new List<TreeNode>();
                var foldersList = new List<LRFolder>();
                foreach (var folder in items.OrderBy(x => x.Key))
                {
                    var folderType = folder.Value ? @"[All]" : @"[Top]";
                    var childFolder = new TreeNode($"{folderType} {folder.Key}")
                    {
                        Name = FolderTreeNodeName
                    };
                    SetIndexImageTreeNode(childFolder);
                    nodes.Add(childFolder);

                    foldersList.Add(new LRFolder(folder.Key, folder.Value));

                    if (clone.TryGetValue(folder.Key, out var value) && folder.Value == value)
                        continue;

                    childFolder.Checked = true;
                }

                CurrentSettings.LogsFolder = new LRFolderGroup(foldersList.ToArray());

                var treeNodeFolders = Current.Nodes[TRVFolders];
                treeNodeFolders.Nodes.Clear();
                treeNodeFolders.Checked = false;
                treeNodeFolders.Nodes.AddRange(nodes.ToArray());
                treeNodeFolders.Expand();

                var checkedNode = nodes.FirstOrDefault(x => x.Checked);
                if (checkedNode != null)
                {
                    checkedNode.Checked = true;
                    Current.SelectedNode = checkedNode;
                }
            }
            catch (Exception ex)
            {
	            OnError?.Invoke(ex);
            }
            finally
            {
	            OnChanged?.Invoke(false, true);
            }
        }

        void RemoveSelectedNodeItem(object sender, EventArgs e)
        {
	        try
	        {
		        if (!Current.Enabled
		            || Current.SelectedNode == null
		            || Current.SelectedNode.Name == TRVServers
		            || Current.SelectedNode.Name == TRVTypes
		            || Current.SelectedNode.Name == TRVFolders)
			        return;

		        if (Compare(Current.SelectedNode, TRVServers))
		        {
			        SetTreeNodes(GroupType.Server, ProcessingType.Remove);
		        }
		        else if (Compare(Current.SelectedNode, TRVTypes))
		        {
			        SetTreeNodes(GroupType.FileType, ProcessingType.Remove);
		        }
		        else if (Current.SelectedNode.Parent.Name == TRVFolders)
		        {
			        var treeNodeFolders = Current.Nodes[TRVFolders];
			        treeNodeFolders.Nodes.Remove(Current.SelectedNode);
			        CurrentSettings.LogsFolder = new LRFolderGroup(GetFolders(Current, false).Select(fodler => new LRFolder(fodler.Key, fodler.Value)).ToArray());

			        var checkedNode = treeNodeFolders.Nodes.OfType<TreeNode>().FirstOrDefault(x => x.Checked);
			        if (checkedNode != null)
			        {
				        checkedNode.Checked = true;
				        Current.SelectedNode = checkedNode;
			        }
		        }
	        }
	        catch (Exception ex)
	        {
		        OnError?.Invoke(ex);
	        }
	        finally
	        {
		        OnChanged?.Invoke(true, true);
	        }
        }

        static bool Compare(TreeNode treeNode, string toFind)
        {
	        if (treeNode == null)
		        return false;
            var isCurrent = treeNode.Name == toFind;
            return isCurrent || (treeNode.Parent != null && Compare(treeNode.Parent, toFind));
        }

        public static Dictionary<string, bool> GetFolders(CustomTreeView treeView, bool getOnlyChecked)
        {
            var folders = new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var folderWithType in treeView.Nodes[TRVFolders].Nodes.OfType<TreeNode>().Where(x => !getOnlyChecked || x.Checked).Select(x => x.Text))
            {
                var folder = folderWithType.Substring(5, folderWithType.Length - 5).Trim();
                if (folders.TryGetValue(folder, out var type))
                {
                    if (type)
                        folders[folder] = true;
                }
                else
                {
                    folders.Add(folder, folderWithType.Substring(0, 5).Equals("[All]", StringComparison.InvariantCultureIgnoreCase));
                }
            }

            return folders;
        }

        TreeNode GetGroupNodes(string name, string text, Dictionary<string, IEnumerable<string>> groups, GroupType groupType)
        {
            var treeNode = new TreeNode(text)
            {
                Name = name,
                Checked = false,
                NodeFont = _defaultParentFont
            };

            foreach (var groupItem in groups)
                treeNode.Nodes.Add(GetGroupItems(groupItem.Key, groupItem.Value, groupType));
            treeNode.Expand();

            return treeNode;
        }

        static TreeNode GetGroupItems(string name, IEnumerable<string> items, GroupType groupType)
        {
            var parentName = ServerGroupTreeNodeName;
            var childName = ServerTreeNodeName;
            if (groupType == GroupType.FileType)
            {
                parentName = FileTypeGroupTreeNodeName;
                childName = FileTypeTreeNodeName;
            }

            var treeNode = new TreeNode(name.Trim().ToUpper())
            {
                Name = parentName
            };
            foreach (var item in items.Distinct(StringComparer.InvariantCultureIgnoreCase).OrderBy(p => p))
            {
                var child = treeNode.Nodes.Add(item.Trim().ToUpper());
                child.Name = childName;
                SetIndexImageTreeNode(child);
            }
            SetIndexImageTreeNode(treeNode);
            return treeNode;
        }

        private static void CheckTreeViewNode(TreeNode node, bool isChecked)
        {
            CheckChildTreeViewNode(node, isChecked);
            CheckParentTreeView(node);
        }

        private static void CheckChildTreeViewNode(TreeNode node, bool isChecked)
        {
            if (node == null)
                return;

            foreach (TreeNode item in node.Nodes)
            {
                item.Checked = isChecked;
                SetIndexImageTreeNode(item);
                if (item.Nodes.Count > 0)
                    CheckChildTreeViewNode(item, isChecked);
            }

            node.Checked = isChecked;
            SetIndexImageTreeNode(node);
        }

        static void CheckParentTreeView(TreeNode node)
        {
            if (node?.Parent == null)
                return;

            var isAllChecked = true;
            foreach (var nodeParent in node.Parent.Nodes.OfType<TreeNode>())
            {
                if (!nodeParent.Checked)
                {
                    isAllChecked = false;
                    break;
                }
            }

            node.Parent.Checked = isAllChecked;
            SetIndexImageTreeNode(node.Parent);
            CheckParentTreeView(node.Parent);
        }

        static void SetIndexImageTreeNode(TreeNode node)
        {
            var result = 0;
            //if (node.Checked)
            switch (node.Name)
            {
                case ServerGroupTreeNodeName:
                    result = 1;
                    break;
                case FileTypeGroupTreeNodeName:
                    result = 2;
                    break;
                case ServerTreeNodeName:
                    result = 3;
                    break;
                case FileTypeTreeNodeName:
                    result = 4;
                    break;
                case FolderTreeNodeName:
                    result = 5;
                    break;
            }

            var resultStr = result.ToString();

            node.ImageKey = resultStr;
            node.ImageIndex = result;
            node.SelectedImageKey = resultStr;
            node.SelectedImageIndex = result;
            node.StateImageKey = resultStr;
            node.StateImageIndex = result;
        }
    }
}
