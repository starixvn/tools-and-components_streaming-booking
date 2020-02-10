using StreamingBooking.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StreamingBooking.Services
{
    public interface IEventProviderService
    {
        IEnumerable<LicenseeModel> Licensees { get; }

        IEnumerable<StreamModel> GetStreamEventData(params LicenseeModel[] licensees);

        Task<IEnumerable<StreamModel>> GetStreamEventDataAsync(params LicenseeModel[] licensees);
    }
}