using StreamingBooking.Models;
using System.Collections.Generic;

namespace StreamingBooking.Services
{
    public interface IActivityService
    {
        bool UpdateRemark(int streamId, string remark);

        bool ActiveRow(int streamId);

        List<StreamModel> GetActiveStream();
    }
}