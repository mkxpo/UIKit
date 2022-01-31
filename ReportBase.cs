using FastReport;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core {

    /// <summary>
    /// Базовый класс для отчетов формируемых с помощью генератора отчетов FastReport.NET
    /// </summary>
    /// 
    [EditorForm(Width = 450, Title = "Формирование отчета", CenterToParent = true)]
    public abstract class ReportBase : DataObjectBase {

        private void InitReportEngine() {
            int frUiStyle = Convert.ToInt32("10");
            FastReport.Utils.Res.LoadLocale(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "localization", "Russian.frl"));
            FastReport.Utils.Config.UIStyle = (FastReport.Utils.UIStyle)frUiStyle;
        }     

        private string ReportTemplateName {
            get {
                return GetType().Name + ".frx";
            }
        }

        private Report LoadReport() {
            String reportFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ReportTemplates", ReportTemplateName);
            Report report = new Report();
            if (File.Exists(reportFileName)) {
                report.Load(reportFileName);
            }
            return report;
        }

        private void SaveReport(Report report) {
            String reportFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ReportTemplates", ReportTemplateName);
            report.Save(reportFileName);
        }

        protected abstract Dictionary<string, object> GetReportParameters();

        protected abstract Dictionary<string, IEnumerable> GetReportDatasets();

        public void ShowPreview() {
            InitReportEngine();
            Report report = LoadReport();
            Dictionary<string, object> reportParams = GetReportParameters();
            Dictionary<string, IEnumerable> datasets = GetReportDatasets();
            foreach (var param in reportParams) {
                report.SetParameterValue(param.Key, param.Value);
            }
            foreach (var data in datasets) {
                report.RegisterData(data.Value, data.Key, 10);
                report.GetDataSource(data.Key).Enabled = true;
            }
            SaveReport(report);
            report.Prepare();
            report.ShowPrepared(true);
        }
    }
}
