using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Core {

    /// <summary>
    /// Базовый класс для отчета, формируемого в формате HTML
    /// </summary>
    abstract class HtmlReportBase {

        public void ShowPreview() {
            string html = GetHtml();
            string fileName = CreateTempFile(html);
            var process = System.Diagnostics.Process.Start(fileName);
        }

        private string GetHtml() {
            StringBuilder sb = new StringBuilder();
            sb.Append("<html>");
            sb.AppendFormat("<head><title>{0}</title>", GetReportTitle());
            sb.Append("<meta http-equiv=\"content-type\" content=\"text/html; charset=utf-8\" /></head>");
            sb.Append(@"
<style>
    body {
        font-family: arial;
        font-size: 10pt;
    } 
    h1 {
        font-size: 16pt;
        font-weight: bold;
        text-align: center;
        line-height: 100%;
        margin-block-start: 0pt;
        margin-block-end: 4pt;
    }
    h2 {
        font-size: 14pt;
        font-weight: bold;
        text-align: center;
        line-height: 100%;
        margin-block-start: 0pt;
        margin-block-end: 4pt;
    }
    thead {
        background-color: #e0e0e0;
        text-align: center;
    }
    td {
        padding: 2px;
    }
    table {
        border-collapse: collapse; 
        width: 100%;
    }
</style>");
            sb.Append("<body>");

            foreach (string h1 in GetReportHeader1()) {
                sb.AppendFormat("<h1>{0}</h1>", h1);
            }
            foreach (string h2 in GetReportHeader2()) {
                sb.AppendFormat("<h2>{0}</h2>", h2);
            }

            sb.AppendLine("<br/>");

            IEnumerable<string> dataHeader = GetDataHeader();
            if (dataHeader != null && dataHeader.Any()) {
                sb.Append("<p>");
                foreach (string s in dataHeader) {
                    sb.AppendFormat("{0}<br/>", s);
                }
                sb.Append("</p>");
            }

            IEnumerable<object> dataItems = GetDataItems().Cast<object>();
            if (dataItems != null && dataItems.Any()) {
                PropertyInfo[] properties = ReflectionHelper.GetVisibleProperties(dataItems.First());
                sb.AppendFormat("<table border='1'>{0}", FormatTableHeader(properties));
                foreach (object dataItem in dataItems) {
                    sb.Append(FormatTableRow(properties, dataItem));
                }
                sb.AppendFormat("</table>");
            }

            IEnumerable<string> dataFooter = GetDataFooter();
            if (dataFooter != null && dataFooter.Any()) {
                sb.Append("<p>");
                foreach (string s in dataFooter) {
                    sb.AppendFormat("{0}<br/>", s);
                }
                sb.Append("</p>");
            }

            sb.Append("</body>");
            sb.Append("</html>");
            return sb.ToString();
        }

        private string FormatTableHeader(PropertyInfo[] props) {
            StringBuilder sb = new StringBuilder();
            sb.Append("<thead>");
            sb.Append("<tr>");
            foreach (var prop in props) {
                string propName = ReflectionHelper.GetPropertyName(prop);
                sb.AppendFormat("<td>{0}</td>", propName);
            }
            sb.Append("</tr>");
            sb.Append("</thead>");
            return sb.ToString();
        }

        private string FormatTableRow(PropertyInfo[] props, object dataItem) {
            StringBuilder sb = new StringBuilder();
            sb.Append("<tr>");
            foreach (var prop in props) {
                object value = prop.GetValue(dataItem);
                sb.AppendFormat("<td>{0}</td>", ReflectionHelper.FormatPropertyValue(prop, value));
            }
            sb.Append("</tr>");
            return sb.ToString();
        }

        private string CreateTempFile(string html) {
            string fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".html");
            File.WriteAllText(fileName, html, Encoding.UTF8);
            return fileName;
        }

        protected abstract string GetReportTitle();
        protected abstract IEnumerable<string> GetReportHeader1();
        protected virtual IEnumerable<string> GetReportHeader2() {
            return new string[0];
        }
        protected abstract IEnumerable<string> GetDataHeader();    
        protected abstract IEnumerable GetDataItems();
        protected abstract IEnumerable<string> GetDataFooter();
    }
}
