namespace StreamingBooking.Services.Excel
{
    using NPOI.SS.UserModel;
    using System.Web.Mvc;

    public class ExcelResult : ActionResult
    {
        private readonly IWorkbook _hssfWorkBook;

        public ExcelResult(IWorkbook hssfWorkbook)
        {
            this._hssfWorkBook = hssfWorkbook;
        }

        /// <summary>
        /// Execute result.
        /// </summary>
        /// <param name="context">The controller context.</param>
        public override void ExecuteResult(ControllerContext context)
        {
            var response = context.HttpContext.Response;
            response.Clear();
            this._hssfWorkBook.Write(response.OutputStream);
        }
    }
}