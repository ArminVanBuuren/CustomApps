using System;
using System.IO;
using System.Linq;
using Script.Control.Handlers.Timesheet.Stats;
using Script.Handlers;
using XPackage;

namespace Script.Control.Handlers.Timesheet.WriteData
{
    public class WriteDataPerformer
    {
        /// <summary>
        /// путь к файлу экспорта
        /// </summary>
        private string WriteDataPath { get; }
        /// <summary>
        /// Экспортировать в word документ
        /// </summary>
        private bool ExportByWordDoc { get; } = false;

        public WriteDataPerformer(string exportFile, string userName)
        {
            if (exportFile == null || exportFile.EndsWith("doc", StringComparison.CurrentCultureIgnoreCase) || exportFile.EndsWith("docx", StringComparison.CurrentCultureIgnoreCase))
                ExportByWordDoc = true;

            if(exportFile == null)
                return;

            exportFile = Path.GetFullPath(exportFile);

            if (Functions.IsPossiblyFile(exportFile))
                WriteDataPath = exportFile;
            else
                WriteDataPath = Path.Combine(TimesheetHandler.SerializationDir, string.Format("{0}{1}", userName, ExportByWordDoc ? ".docx" : ".txt"));
        }

        public bool Export(Statistic stats)
        {
            if (ExportByWordDoc)
                ExportWordDocFile(stats);
            else
                ExportTxtFile(stats);
            return true;
        }

        void ExportWordDocFile(Statistic stats)
        {
            var wd = new WriteWordData(WriteDataPath);
            wd.AddDocHeader(stats.Name, stats.GetStat());
            foreach (var statByGroup in stats.ChildItems)
            {
                var sortedCollection = statByGroup.ChildItems.OrderBy(x => x.TotalTimeByAnyDay, new SemiNumericComparer()).ToList();

                wd.AddTableHeader(statByGroup.Name, 2);
                wd.AddColumnsName("Проекты", "Задачи");

                foreach (var statTfsPrj in sortedCollection)
                {
                    wd.AddCellsData(statTfsPrj.Name, statTfsPrj.PM, statTfsPrj.PM_Mail, statTfsPrj.GetStat(), string.Join(Environment.NewLine, statTfsPrj.ChildItems.Select(x => x.GetStat())));
                }

                wd.AddTableFooter(statByGroup.GetStat());
            }

            wd.SaveDocument();
        }
        void ExportTxtFile(Statistic stats)
        {
            var wd = new WriteStringData(WriteDataPath);
            wd.AddDocHeader(new[] { stats.GetStat() });
            foreach (var statByGroup in stats.ChildItems)
            {
                //.ThenBy(x => string.Join("_", stats.collection[0].ProjectName.Split('_').Skip(1).ToArray()))
                var sortedCollection = statByGroup.ChildItems.OrderBy(x => x.TotalTimeByAnyDay, new SemiNumericComparer()).ToList();
                var maxLengthColumn1 = sortedCollection.Max(x => x.Name.Length);
                var maxLengthColumn2 = sortedCollection.Max(x => x.ChildItems.Max(p => p.GetStat().Length));

                wd.AddTableHeader(statByGroup.Name, maxLengthColumn1 + maxLengthColumn2 + 2);
                wd.AddColumnsName(new[] { "Проекты", "Задачи" }, new[] { maxLengthColumn1, maxLengthColumn2 });

                foreach (var statTfsPrj in sortedCollection)
                {
                    var cell_second = wd.GetRows(new[]
                    {
                        statTfsPrj.GetStat(),
                        string.Join(Environment.NewLine, statTfsPrj.ChildItems.Select(x => x.GetStat()))
                    }, maxLengthColumn2);
                    var getRow = wd.GetColumns(new[] { statTfsPrj.Name, cell_second }, new[] { maxLengthColumn1, maxLengthColumn2 });

                    wd.WriteLines(new[] { getRow });

                }

                wd.AddTableFooter(new[] { statByGroup.GetStat() }, maxLengthColumn1 + maxLengthColumn2 + 2);

            }
        }
    }
}
