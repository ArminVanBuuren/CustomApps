using System.Windows;
using SPAMessageSaloon.Common;

namespace SPAMessageSaloon
{
    /// <inheritdoc cref="" />
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class AboutWindow
    {
        public AboutWindow()
        {
            InitializeComponent();
            Loaded += AboutWindow_Loaded;

            #region Logs Reader

            LRDescriptionTxt.Text = LogsReader.Properties.Resources.Txt_LogsReaderForm_Description;

            LRSearch.Header = LogsReader.Properties.Resources.Txt_LogsReaderForm_Search;
            LRSearchTxt.Text = LogsReader.Properties.Resources.Txt_Form_SearchComment;
            LRUseRegex.Header = LogsReader.Properties.Resources.Txt_LogsReaderForm_UseRegex;
            LRUseRegexTxt.Text = LogsReader.Properties.Resources.Txt_LRSettings_UseRegexComment;

            LRDateTxt.Text = LogsReader.Properties.Resources.Txt_Form_DateFilterComment;
            LRTraceNameTxt.Text = LogsReader.Properties.Resources.Txt_Form_TraceNameFilterComment;
            LRTraceTxt.Text = LogsReader.Properties.Resources.Txt_Form_TraceFilterComment;
            LRUseWhenSearching.Header = LogsReader.Properties.Resources.Txt_LogsReaderForm_UseFilterWhenSearching;
            LRUseWhenSearchingTxt.Text = LogsReader.Properties.Resources.Txt_Form_AlreadyUseFilterComment;
            LRExport.Header = LogsReader.Properties.Resources.Txt_LogsReaderForm_Export;
            LRExportTxt.Text = LogsReader.Properties.Resources.Txt_LogsReaderForm_ExportComment;

            LRServers.Header = LogsReader.Properties.Resources.Txt_LogsReaderForm_Servers;
            LRServersTxt.Text = LogsReader.Properties.Resources.Txt_LRSettingsScheme_Servers;
            LRLogsfolder.Header = LogsReader.Properties.Resources.Txt_LogsReaderForm_LogsFolder;
            LRLogsfolderTxt.Text = LogsReader.Properties.Resources.Txt_LRSettingsScheme_LogsDirectory;
            LRFileTypes.Header = LogsReader.Properties.Resources.Txt_LogsReaderForm_FilteTypes;
            LRFileTypesTxt.Text = LogsReader.Properties.Resources.Txt_LRSettingsScheme_Types;
            LRMaxLines.Header = LogsReader.Properties.Resources.Txt_LogsReaderForm_MaxLines;
            LRMaxLinesTxt.Text = LogsReader.Properties.Resources.Txt_LRSettingsScheme_MaxTraceLines;
            LRMaxThreads.Header = LogsReader.Properties.Resources.Txt_LogsReaderForm_MaxThreads;
            LRMaxThreadsTxt.Text = LogsReader.Properties.Resources.Txt_LRSettingsScheme_MaxThreads;
            LRRowsLimit.Header = LogsReader.Properties.Resources.Txt_LogsReaderForm_RowsLimit;
            LRRowsLimitTxt.Text = LogsReader.Properties.Resources.Txt_LRSettingsScheme_RowsLimit;
            LROrderBy.Header = LogsReader.Properties.Resources.Txt_LogsReaderForm_OrderBy;
            LROrderByTxt.Text = LogsReader.Properties.Resources.Txt_LRSettingsScheme_OrderBy;

            LRTraceParseTxt.Text = LogsReader.Properties.Resources.Txt_LRSettingsScheme_TraceParse;

            #endregion

            #region SPA Filter

            SFDescriptionTxt.Text = SPAFilter.Properties.Resources.Form_Description;

            SFProcessTxt.Text = SPAFilter.Properties.Resources.Form_ToolTip_ProcessesButtonOpen;
            SFROBPOpsTxt.Text = SPAFilter.Properties.Resources.Form_ToolTip_ROBPOperationButtonOpen;
            SFServiceCatalogTxt.Text = SPAFilter.Properties.Resources.Form_ToolTip_ServiceCatalogOpenButton;

            SFProcessFilterTxt.Text = SPAFilter.Properties.Resources.Form_ToolTip_SearchPattern;
            SFROBPOpsFilterTxt.Text = SPAFilter.Properties.Resources.Form_ToolTip_SearchPattern;
            SFHostTypeFilterTxt.Text = SPAFilter.Properties.Resources.Form_ToolTip_SearchPattern;

            SFFilter.Header = SPAFilter.Properties.Resources.Form_Filter;
            SFFilterTxt.Text = SPAFilter.Properties.Resources.Form_ToolTip_FilterButton;
            SFReset.Header = SPAFilter.Properties.Resources.Form_Reset.Trim();
            SFResetTxt.Text = SPAFilter.Properties.Resources.Form_ToolTip_buttonReset;
            SFPrettyPrint.Header = SPAFilter.Properties.Resources.Form_PrintXMLFiles_Button;
            SFPrettyPrintTxt.Text = SPAFilter.Properties.Resources.Form_PrintXMLFiles_ToolTip;

            SFAddInstancesTxt.Text = SPAFilter.Properties.Resources.Form_AddActivator;
            SFRemoveInstancesTxt.Text = SPAFilter.Properties.Resources.Form_RemoveInstance;
            SFRefreshInstancesTxt.Text = SPAFilter.Properties.Resources.Form_Refresh;
            SFReloadInstancesTxt.Text = SPAFilter.Properties.Resources.Form_Reload;

            SFExportSCPath.Header = SPAFilter.Properties.Resources.Form_Root.Trim();
            SFExportSCPathTxt.Text = SPAFilter.Properties.Resources.Form_ToolTip_RootSCExportPathButton;
            SFOpenRDExcel.Header = SPAFilter.Properties.Resources.Form_OpenXksx.Trim();
            SFOpenRDExcelTxt.Text = string.Format(SPAFilter.Properties.Resources.Form_ToolTip_OpenSevExelButton, string.Join("','", SPAFilter.SPAFilterForm.MandatoryXslxColumns));
            SFGenerateSC.Header = SPAFilter.Properties.Resources.Form_GenerateSC;
            SFGenerateSCTxt.Text = SPAFilter.Properties.Resources.Form_ToolTip_ButtonGenerateSC;

            #endregion
        }

        private void AboutWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MaxButton.Visibility = Visibility.Collapsed;
            MinButton.Visibility = Visibility.Collapsed;
            VisibleResizeMode = false;
        }
    }
}
