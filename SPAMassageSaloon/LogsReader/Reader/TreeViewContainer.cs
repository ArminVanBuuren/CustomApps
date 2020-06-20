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
		        AssignTreeViewText(treeView);

	        _contextTreeMainMenuStrip?.Dispose();
	        _contextTreeMainMenuStrip = new ContextMenuStrip {Tag = Main};
	        _contextTreeMainMenuStrip.Items.Add(Resources.Txt_LogsReaderForm_AddServerGroup, Resources.server_group, AddServerGroup);
	        _contextTreeMainMenuStrip.Items.Add(Resources.Txt_LogsReaderForm_AddServer, Resources.server, AddServer);
	        _contextTreeMainMenuStrip.Items.Add(new ToolStripSeparator());
	        _contextTreeMainMenuStrip.Items.Add(Resources.Txt_LogsReaderForm_AddFileTypeGroup, Resources.types_group, AddFileTypeGroup);
	        _contextTreeMainMenuStrip.Items.Add(Resources.Txt_LogsReaderForm_AddFileType, Resources.type, AddFileType);
	        _contextTreeMainMenuStrip.Items.Add(new ToolStripSeparator());
	        _contextTreeMainMenuStrip.Items.Add(Resources.Txt_LogsReaderForm_AddFolder, Resources.folder, AddFolder);
	        _contextTreeMainMenuStrip.Items.Add(new ToolStripSeparator());
	        _contextTreeMainMenuStrip.Items.Add(Resources.Txt_LogsReaderForm_RemoveSelected, Resources.remove, RemoveSelected);
	        _contextTreeMainMenuStrip.Items.Add(Resources.Txt_LogsReaderForm_Properties, Resources.properies, OpenProperties);
        }

        public CustomTreeView CreateNewCopy()
        {
            var copy = new CustomTreeView();
            _copyList.Add(InitializeTreeView(copy));
            UpdateContainer(Main);
            return copy;
        }

        CustomTreeView InitializeTreeView(CustomTreeView treeView)
        {
            treeView.ImageList = _imageList;
            treeView.ItemHeight = 18;
            treeView.Indent = 18;
            treeView.CheckBoxes = true;
            treeView.BackColor = Color.FromArgb(255, 255, 255);
            treeView.DrawMode = TreeViewDrawMode.OwnerDrawAll;
            treeView.ForeColor = Color.FromArgb(0, 0, 0);
            treeView.LineColor = Color.FromArgb(109, 109, 109);
            AssignTreeViewText(treeView);

            treeView.MouseDown += TreeMain_MouseDown;
            treeView.AfterCheck += TrvMain_AfterCheck;
            treeView.AfterExpand += TreeView_AfterExpandOrCollapse;
            treeView.AfterCollapse += TreeView_AfterExpandOrCollapse;
            return treeView;
        }

        static void AssignTreeViewText(TreeView treeView)
        {
	        void AssignText(string name, string value)
	        {
		        var treeNode = treeView.Nodes[name];
		        if (treeNode != null)
			        treeNode.Text = value;
	        }

	        AssignText(TRVServers, Resources.Txt_LogsReaderForm_Servers);
	        AssignText(TRVTypes, Resources.Txt_LogsReaderForm_Types);
	        AssignText(TRVFolders, Resources.Txt_LogsReaderForm_LogsFolder);
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
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
	            UpdateContainerProperties(Current, false);

                foreach (var treeView in _copyList)
		            treeView.AfterCheck += TrvMain_AfterCheck;

                OnChanged?.Invoke(true, false);
            }
        }

        private void TreeView_AfterExpandOrCollapse(object sender, TreeViewEventArgs e)
        {
	        if (!(sender is CustomTreeView current))
		        return;
	        Current = current;

	        DisableExpandersEvents();
	        UpdateContainerProperties(Current, true);
	        EnableExpandersEvents();
        }

        void DisableExpandersEvents()
        {
	        foreach (var treeView in _copyList)
	        {
		        treeView.AfterExpand -= TreeView_AfterExpandOrCollapse;
		        treeView.AfterCollapse -= TreeView_AfterExpandOrCollapse;
	        }
        }

        void EnableExpandersEvents()
        {
	        foreach (var treeView in _copyList)
	        {
		        treeView.AfterExpand += TreeView_AfterExpandOrCollapse;
		        treeView.AfterCollapse += TreeView_AfterExpandOrCollapse;
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
				        AddFolder(this, EventArgs.Empty);
				        break;
			        case Keys.Delete:
				        RemoveSelected(this, EventArgs.Empty);
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
	        try
	        {
		        DisableExpandersEvents();
		        SetTreeNodes(GroupType.Server, ProcessingType.CreateGroupItem);
            }
	        finally
	        {
		        UpdateContainer(Current);
		        EnableExpandersEvents();
	        }
        }

        void AddServer(object sender, EventArgs args)
        {
	        try
	        {
		        DisableExpandersEvents();
		        SetTreeNodes(GroupType.Server, ProcessingType.CreateGroupChildItem);
            }
	        finally
	        {
		        UpdateContainer(Current);
		        EnableExpandersEvents();
	        }
        }

        void AddFileTypeGroup(object sender, EventArgs args)
        {
	        try
	        {
		        DisableExpandersEvents();
		        SetTreeNodes(GroupType.FileType, ProcessingType.CreateGroupItem);
            }
	        finally
	        {
		        UpdateContainer(Current);
		        EnableExpandersEvents();
	        }
        }

        void AddFileType(object sender, EventArgs args)
        {
	        try
	        {
		        DisableExpandersEvents();
		        SetTreeNodes(GroupType.FileType, ProcessingType.CreateGroupChildItem);
            }
	        finally
	        {
		        UpdateContainer(Current);
		        EnableExpandersEvents();
	        }
        }

        void AddFolder(object sender, EventArgs e)
        {
	        if (Current.Nodes[TRVFolders] == null) 
		        return;

	        try
	        {
		        DisableExpandersEvents();
		        SetFolder(null, GetFolders(Current, false), true);
            }
	        finally
	        {
		        UpdateContainer(Current);
		        EnableExpandersEvents();
	        }
        }

        void RemoveSelected(object sender, EventArgs e)
        {
            // запрещено удалять дочерние ноды, если выбраны родительские ноды
            if (Current == null
	            || !Current.Enabled
	            || Current.SelectedNode == null
	            || Current.SelectedNode.Name == TRVServers
	            || Current.SelectedNode.Name == TRVTypes
	            || Current.SelectedNode.Name == TRVFolders)
		        return;

            var isChanged = false;
            try
	        {
		        DisableExpandersEvents();

                if (Compare(Current.SelectedNode, TRVServers))
		        {
			        SetTreeNodes(GroupType.Server, ProcessingType.Remove);
			        isChanged = true;
		        }
		        else if (Compare(Current.SelectedNode, TRVTypes))
		        {
			        SetTreeNodes(GroupType.FileType, ProcessingType.Remove);
			        isChanged = true;
                }
		        else if (Current.SelectedNode.Parent.Name == TRVFolders)
		        {
			        var treeNodeFolders = Current.Nodes[TRVFolders];
			        if (treeNodeFolders == null)
				        return;

			        treeNodeFolders.Nodes.Remove(Current.SelectedNode);
			        CurrentSettings.LogsFolder = new LRFolderGroup(GetFolders(Current, false).Select(fodler => new LRFolder(fodler.Key, fodler.Value)).ToArray());

			        var checkedNode = treeNodeFolders.Nodes.OfType<TreeNode>().FirstOrDefault(x => x.Checked);
			        if (checkedNode != null)
			        {
				        checkedNode.Checked = true;
				        Current.SelectedNode = checkedNode;
			        }
			        isChanged = true;
                }
	        }
	        catch (Exception ex)
	        {
		        OnError?.Invoke(ex);
	        }
	        finally
	        {
		        if (isChanged)
		        {
			        UpdateContainer(Current);
			        OnChanged?.Invoke(true, true);
		        }
		        EnableExpandersEvents();
            }
        }

        void OpenProperties(object sender, EventArgs args)
        {
            if (Current.SelectedNode == null || Compare(Current.SelectedNode, TRVServers))
                SetTreeNodes(GroupType.Server, ProcessingType.CreateGroupChildItem);
            else if (Compare(Current.SelectedNode, TRVTypes))
                SetTreeNodes(GroupType.FileType, ProcessingType.CreateGroupChildItem);
            else if (Current.SelectedNode.Name == TRVFolders)
                SetFolder(Current.SelectedNode.FirstNode, GetFolders(Current, false), true);
            else if(Current.Nodes[TRVFolders] != null)
                SetFolder(Current.SelectedNode, GetFolders(Current, false), true);
        }

        void SetTreeNodes(GroupType groupType, ProcessingType processingType)
        {
            try
            {
                var treeNode = groupType == GroupType.Server ? Current.Nodes[TRVServers] : Current.Nodes[TRVTypes];
                if(treeNode == null)
                    return;

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
                        dialogResul = new AddGroupForm(treeGroups, groupType).ShowDialog();
                    }
                    else
                    {
                        var groupName = treeGroups.First().Key;
                        if (Current.SelectedNode?.Parent == treeNode)
                            groupName = Current.SelectedNode.Text;
                        else if (Current.SelectedNode?.Parent?.Parent == treeNode)
                            groupName = Current.SelectedNode.Parent.Text;

                        dialogResul = AddGroupForm.ShowGroupItemsForm(groupName, treeGroups, groupType);
                    }

                    if (dialogResul == DialogResult.Cancel)
                        return;
                }

                // подготавливаем два списка для конфига и для TreeView из обновленного treeGroups
                var nodes = new List<TreeNode>();
                var groupItems = new List<LRGroupItem>(treeGroups.Count);
                foreach (var (groupName, items) in treeGroups.OrderBy(x => x.Key))
                {
	                if (items.Count == 0 || items.All(x => x.IsNullOrEmptyTrim()))
		                continue;

	                var childTreeNode = GetGroupItems(groupName, items, groupType);
	                nodes.Add(childTreeNode);
	                groupItems.Add(new LRGroupItem(groupName, string.Join(", ", items)));

	                if (!clone.TryGetValue(groupName, out var existGroup) || existGroup.Except(items).Any() || items.Except(existGroup).Any())
		                childTreeNode.Expand();
                }

                // ассайнем в конфиг
                if (groupType == GroupType.Server)
                    CurrentSettings.Servers = new LRGroups(groupItems.ToArray());
                else
                    CurrentSettings.FileTypes = new LRGroups(groupItems.ToArray());

                // бэкапим ноды, для чтобы в новом листе установить Checked и Expand
                var prevNodes = new Dictionary<string, TreeNodeItem>();
                foreach (var group in treeNode.Nodes.OfType<TreeNode>())
                {
                    prevNodes.Add(group.Text, new TreeNodeItem(group.IsExpanded, group.Checked));
                    foreach (var item in group.Nodes.OfType<TreeNode>())
                        prevNodes.Add($"{group.Text}_{item.Text}", new TreeNodeItem(item.IsExpanded, item.Checked));
                }

                treeNode.Nodes.Clear();
                treeNode.Checked = false;
                treeNode.Nodes.AddRange(nodes.ToArray());

                // устанавливаем в новый список Checked и Expand из бэкапа
                foreach (var group in treeNode.Nodes.OfType<TreeNode>())
                {
	                if(prevNodes.TryGetValue(group.Text, out var prevGroup))
	                {
		                group.Checked = prevGroup.Checked;
		                if(prevGroup.IsExpanded)
			                group.Expand();

		                if (group.Checked)
			                continue;
	                }

                    foreach (var item in group.Nodes.OfType<TreeNode>())
	                {
		                if (prevNodes.TryGetValue($"{group.Text}_{item.Text}", out var prevItem))
		                {
			                if (prevItem.Checked)
				                item.Checked = true;
		                }
	                }
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
                TreeNode newNode = null;
                foreach (var (folderPath, searchType) in items.OrderBy(x => x.Key))
                {
	                var searchTypeDisplay = searchType ? @"[All]" : @"[Top]";
	                var childFolder = new TreeNode($"{searchTypeDisplay} {folderPath}")
	                {
		                Name = FolderTreeNodeName
	                };
	                SetIndexImageTreeNode(childFolder);
	                nodes.Add(childFolder);

	                foldersList.Add(new LRFolder(folderPath, searchType));

	                if (clone.TryGetValue(folderPath, out var value) && searchType == value)
		                continue;

	                newNode = childFolder;
                }

                CurrentSettings.LogsFolder = new LRFolderGroup(foldersList.ToArray());

                var treeNodeFolders = Current.Nodes[TRVFolders];
                var prevNodes = treeNodeFolders.Nodes.ToArray<TreeNode>().ToDictionary(x => x.Text);
                treeNodeFolders.Nodes.Clear();
                treeNodeFolders.Checked = false;
                treeNodeFolders.Nodes.AddRange(nodes.ToArray());
                treeNodeFolders.Expand();

                if (newNode != null)
	                Current.SelectedNode = newNode;

                foreach (var node in treeNodeFolders.Nodes.OfType<TreeNode>())
	                if (prevNodes.TryGetValue(node.Text, out var prevNode))
		                node.Checked = prevNode.Checked;
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
            var parentTreeNode = new TreeNode(text)
            {
                Name = name,
                Checked = false,
                NodeFont = _defaultParentFont
            };

            foreach (var (groupName, items) in groups)
                parentTreeNode.Nodes.Add(GetGroupItems(groupName, items, groupType));
            parentTreeNode.Expand();

            return parentTreeNode;
        }

        static TreeNode GetGroupItems(string groupName, IEnumerable<string> items, GroupType groupType)
        {
            var parentName = ServerGroupTreeNodeName;
            var childName = ServerTreeNodeName;
            if (groupType == GroupType.FileType)
            {
                parentName = FileTypeGroupTreeNodeName;
                childName = FileTypeTreeNodeName;
            }

            var groupTreeNode = new TreeNode(groupName.Trim().ToUpper())
            {
                Name = parentName
            };
            foreach (var item in items.Distinct(StringComparer.InvariantCultureIgnoreCase).OrderBy(p => p))
            {
                var child = groupTreeNode.Nodes.Add(item.Trim().ToUpper());
                child.Name = childName;
                SetIndexImageTreeNode(child);
            }
            SetIndexImageTreeNode(groupTreeNode);
            return groupTreeNode;
        }

        static void CheckTreeViewNode(TreeNode node, bool isChecked)
        {
            CheckChildTreeViewNode(node, isChecked);
            CheckParentTreeView(node);
        }

        static void CheckChildTreeViewNode(TreeNode node, bool isChecked)
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

        /// <summary>
        /// Обновить все ноды TreeView контейнера
        /// </summary>
        /// <param name="changedTreeView"></param>
        void UpdateContainer(TreeView changedTreeView)
        {
	        foreach (var bindedTreeView in _copyList.Where(x => x != changedTreeView))
	        {
		        bindedTreeView.Nodes.Clear();
		        foreach (var treeNode in changedTreeView.Nodes.Cast<TreeNode>())
			        bindedTreeView.Nodes.Add((TreeNode)treeNode.Clone());
            }

	        UpdateContainerProperties(changedTreeView, true);
        }

        /// <summary>
        /// Обновить свойства IsExpanded и Checeked для всех форм контейнера
        /// </summary>
        /// <param name="changedTreeView"></param>
        /// <param name="disableAfterCheck"></param>
        void UpdateContainerProperties(TreeView changedTreeView, bool disableAfterCheck)
        {
	        if (disableAfterCheck)
	        {
		        foreach (var treeView in _copyList)
			        treeView.AfterCheck -= TrvMain_AfterCheck;
	        }

	        var template = GetTemplate(changedTreeView);

	        foreach (var bindedTreeView in _copyList.Where(x => x != changedTreeView))
		        SetTreeNodeItemsProperies(bindedTreeView, template);

	        if (disableAfterCheck)
	        {
		        foreach (var treeView in _copyList)
			        treeView.AfterCheck += TrvMain_AfterCheck;
	        }
        }

        public static Dictionary<string, TreeNodeItem> GetTemplate(TreeView treeView)
        {
	        var template = new Dictionary<string, TreeNodeItem>();
	        foreach (var parent in treeView.Nodes.OfType<TreeNode>())
	        {
		        template.Add(parent.Name, new TreeNodeItem(parent.IsExpanded, parent.Checked));
		        foreach (var group in parent.Nodes.OfType<TreeNode>())
		        {
			        template.Add($"{parent.Name}_{group.Text}", new TreeNodeItem(group.IsExpanded, group.Checked));
			        foreach (var item in group.Nodes.OfType<TreeNode>())
			        {
				        template.Add($"{parent.Name}_{group.Text}_{item.Text}", new TreeNodeItem(item.IsExpanded, item.Checked));
			        }
		        }
	        }

	        return template;
        }

        public void UpdateContainerByTemplate(Dictionary<string, TreeNodeItem> template)
        {
	        DisableExpandersEvents();
            foreach (var treeView in _copyList)
		        treeView.AfterCheck -= TrvMain_AfterCheck;

            foreach (var bindedTreeView in _copyList)
		        SetTreeNodeItemsProperies(bindedTreeView, template);

            EnableExpandersEvents();
            foreach (var treeView in _copyList)
	            treeView.AfterCheck += TrvMain_AfterCheck;
        }

        static void SetTreeNodeItemsProperies(TreeView bindedTreeView, IReadOnlyDictionary<string, TreeNodeItem> template)
        {
	        foreach (var parent in bindedTreeView.Nodes.OfType<TreeNode>())
	        {
		        try
		        {
			        if (template.TryGetValue(parent.Name, out var newParent))
			        {
				        parent.Checked = newParent.Checked;
				        if (newParent.IsExpanded)
					        parent.Expand();
				        else
					        parent.Collapse();
			        }

			        foreach (var group in parent.Nodes.OfType<TreeNode>())
			        {
				        if (template.TryGetValue($"{parent.Name}_{group.Text}", out var newGroup))
				        {
					        group.Checked = newGroup.Checked;
					        if (newGroup.IsExpanded)
						        group.Expand();
					        else
						        group.Collapse();
				        }

				        foreach (var item in group.Nodes.OfType<TreeNode>())
				        {
					        if (template.TryGetValue($"{parent.Name}_{group.Text}_{item.Text}", out var newItem))
					        {
						        item.Checked = newItem.Checked;
					        }
				        }
			        }
                }
		        catch (Exception)
		        {
			        // ignored
		        }
	        }
        }
    }

    [Serializable]
    public class TreeNodeItem
    {
	    public bool IsExpanded { get; }
	    public bool Checked { get; }

	    public TreeNodeItem(bool isExpanded, bool @checked)
	    {
		    IsExpanded = isExpanded;
		    Checked = @checked;
	    }
    }
}
