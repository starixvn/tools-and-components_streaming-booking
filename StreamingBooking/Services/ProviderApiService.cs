using StreamingBooking.Common;
using StreamingBooking.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StreamingBooking.Services
{
    public abstract class ProviderApiService<T> : IEventProviderService
        where T : BaseEventModel
    {
        public abstract IEnumerable<LicenseeModel> Licensees { get; }

        public IEnumerable<StreamModel> GetStreamEventData(DateTime? from, DateTime? to, params LicenseeModel[] licensees)
        {
            var result = GetStreamEventDataAsync(from, to, licensees).Result;
            return result;
        }

        public async Task<IEnumerable<StreamModel>> GetStreamEventDataAsync(DateTime? from, DateTime? to, params LicenseeModel[] licensees)
        {
            var result = new List<StreamModel>();
            foreach (var licensee in licensees)
            {
                var events = await GetEvents(licensee, from, to).ConfigureAwait(false);
                var streamModels = ConvertToStreamModels(events, licensee.PartnerName);
                result.AddRange(streamModels);
            }
            return result;
        }

        protected abstract Task<IEnumerable<T>> GetEvents(LicenseeModel licensee, DateTime? from, DateTime? to);

        private IEnumerable<StreamModel> ConvertToStreamModels(IEnumerable<T> events, string licensee)
        {
            if (events == null || !events.Any())
            {
                return new List<StreamModel>();
            }

            var result = events.Select(_ => new StreamModel
            {
                EventId = _.Id,
                StreamId = _.Id,
                KickOffDate = _.KickOffDate,
                SportName = _.SportName,
                LeagueName = _.LeagueName,
                HomeName = _.HomeName,
                AwayName = _.AwayName,
                ALog = licensee == LicenseeName.ALog,
                IBCBet = licensee == LicenseeName.IBCBet,
                Dafabet = licensee == LicenseeName.Dafabet,
                M88 = licensee == LicenseeName.M88,
                Fun88 = licensee == LicenseeName.Fun88,
                Mayfair = licensee == LicenseeName.Mayfair
            });

            return result;
        }
    }
}