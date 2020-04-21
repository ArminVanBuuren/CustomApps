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
        }

        private void AboutWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MaxButton.Visibility = Visibility.Collapsed;
            MinButton.Visibility = Visibility.Collapsed;
            VisibleResizeMode = false;
        }
    }
}
