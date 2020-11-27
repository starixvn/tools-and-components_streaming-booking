namespace StreamingBooking.Services.Excel
{
    using NPOI.SS.UserModel;
    using NPOI.XSSF.UserModel;
    using StreamingBooking.Models;
    using System.IO;
    using System.Linq;
    using System.Web.Hosting;

    public class HomeStreamingBookingExcel : IHomeStreamingBookingExcel
    {
        public const string TemplateSheetName = "Template";

        protected internal virtual ISheet TemplateSheet { get; private set; }
        public virtual IWorkbook Workbook { get; private set; }

        public virtual IWorkbook Export(StreamViewModel excelModel)
        {
            ISheet sheet;
            var rowIndex = 1;
            var colIndex = 0;

            LoadTemplate();
            TemplateSheet.CopyTo(Workbook, "Data", true, true);
            sheet = Workbook.GetSheet("Data");
            if (excelModel.DistinctStreams.Any())
            {
                foreach (var item in excelModel.DistinctStreams)
                {
                    colIndex = 0;
                    var row = sheet.GetRow(rowIndex);
                    row.Cells[colIndex++].SetCellValue(item.KickOffDate);
                    row.Cells[colIndex++].SetCellValue(item.SportName);
                    row.Cells[colIndex++].SetCellValue(item.ProviderName);
                    row.Cells[colIndex++].SetCellValue(item.LeagueName);
                    row.Cells[colIndex++].SetCellValue(item.HomeName);
                    row.Cells[colIndex++].SetCellValue(item.AwayName);
                    row.Cells[colIndex++].SetCellValue(item.ProviderName);
                    row.Cells[colIndex].SetCellValue(item.StreamId);
                    if (rowIndex < excelModel.DistinctStreams.Count)
                    {
                        sheet.CopyRow(rowIndex, rowIndex + 1);
                    }
                    rowIndex++;
                }
            }
            else
            {
                Workbook.RemoveSheetAt(2);
                Workbook.RemoveSheetAt(0);
                return Workbook;
            }

            Workbook.RemoveSheetAt(1);
            Workbook.RemoveSheetAt(0);

            return Workbook;
        }

        protected internal virtual void LoadTemplate()
        {
            string templatePath = HostingEnvironment.MapPath(@"~/App_Data/Template/StreamingBookingReport.xlsx");
            FileStream stream = null;
            try
            {
                stream = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
                Workbook = new XSSFWorkbook(stream);
                TemplateSheet = Workbook.GetSheet(TemplateSheetName);
                var defaultStyle = Workbook.CreateCellStyle();
                defaultStyle.ShrinkToFit = true;
                defaultStyle.WrapText = true;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
        }
    }
}