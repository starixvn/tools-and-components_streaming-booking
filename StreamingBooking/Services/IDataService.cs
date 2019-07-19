using StreamingBooking.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamingBooking.Services
{
    public interface IDataService
    {
        List<StreamModel> Get(StreamMetalModel filter);
    }
}
