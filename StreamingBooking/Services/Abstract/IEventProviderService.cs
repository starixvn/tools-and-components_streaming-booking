using StreamingBooking.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StreamingBooking.Services
{
    public interface IEventProviderService
    {
        IEnumerable<LicenseeModel> Licensees { get; }

        IEnumerable<StreamModel> GetStreamEventData(DateTime? from, DateTime? to, params LicenseeModel[] licensees);

        Task<IEnumerable<StreamModel>> GetStreamEventDataAsync(DateTime? from, DateTime? to, params LicenseeModel[] licensees);
    }
}