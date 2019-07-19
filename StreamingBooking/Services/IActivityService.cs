using StreamingBooking.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamingBooking.Services
{
    public interface IActivityService
    {
        bool UpdateRemark(int streamId, string remark);
        bool ActiveRow(int streamId);
        IList<StreamModel> GetActiveStream();
    }
}
