namespace StreamingBooking.Services.Excel
{
    using NPOI.SS.UserModel;
    using StreamingBooking.Models;

    public interface IHomeStreamingBookingExcel
    {
        IWorkbook Export(StreamViewModel excelModel);
    }
}