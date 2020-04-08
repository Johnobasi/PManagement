using System.IO;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PermissionManagement.Utility
{
    public class Export
    {
        [HttpPost]
        public void ToFile(object sourceData, System.Web.HttpResponseBase Response, string reportName = "ReportsDownload", EXPORTTYPE exportType = EXPORTTYPE.EXCEL)
        {
            var grid = new GridView();
            grid.DataSource = sourceData;
            grid.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            Response.Charset = "";
            StringWriter sw = new StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);
            switch (exportType)
            {
                case EXPORTTYPE.EXCEL:
                    {
                        Response.AddHeader("content-disposition", "attachment; filename=" + reportName + ".xls");
                        Response.ContentType = "application/ms-excel";
                        break;
                    }
                case EXPORTTYPE.WORD:
                    {
                        Response.AddHeader("content-disposition", "attachment; filename=" + reportName + ".doc");
                        Response.ContentType = "application/ms-word";
                        break;
                    }
                case EXPORTTYPE.PDF:
                    break;
            }
            grid.RenderControl(htw);
            Response.Output.Write(sw.ToString());
            Response.Flush();
            Response.End();
        }
    }
    public enum EXPORTTYPE
    {
        EXCEL,
        WORD,
        PDF
    }
}
