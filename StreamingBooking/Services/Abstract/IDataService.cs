using StreamingBooking.Models;
using System.Collections.Generic;

namespace StreamingBooking.Services
{
    public interface IDataService
    {
        List<StreamModel> Get(StreamMetalModel filter);

        bool ClearCache(string licensee);
    }
}