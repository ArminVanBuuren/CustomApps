using System.ComponentModel.Design;

namespace Utils.WinForm.Expander
{
    /// <summary>
    /// Designer for the ExpandCollapsePanel control with support for a smart tag panel.
    /// <remarks>http://msdn.microsoft.com/en-us/library/ms171829.aspx</remarks>
    /// </summary>
    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
    public class ExpandCollapsePanelDesigner : System.Windows.Forms.Design.ScrollableControlDesigner
    {
        private DesignerActionListCollection actionLists;

        // Use pull model to populate smart tag menu. 
        public override DesignerActionListCollection ActionLists => actionLists ?? (actionLists = new DesignerActionListCollection {new ExpandCollapsePanelActionList(Component)});
    }
}