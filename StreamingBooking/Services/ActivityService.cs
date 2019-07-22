using Fanex.Caching;

using StreamingBooking.Models;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
namespace StreamingBooking.Services
{
    public class ActivityService : IActivityService
    {
        private readonly ICacheService cacheService = new CacheService();
        private readonly string key = ConfigurationManager.AppSettings["ActivityCacheKey"];
        public bool ActiveRow(int streamId)
        {
            var values = cacheService.Get<List<StreamModel>>(key) ?? new List<StreamModel>();
            var updatedItem = values.FirstOrDefault(x => x.StreamId == streamId);
            if (updatedItem != null && updatedItem.IsActive)
            {
                values.Remove(updatedItem);
            }
            else
            {
                values.Add(new StreamModel { StreamId = streamId, IsActive = true });
            }

            cacheService.Set(key, values, new CacheItemOptions().SetSlidingExpiration(TimeSpan.FromMinutes(20)));
            return true;
        }

        public bool UpdateRemark(int streamId, string remark)
        {
            var values = cacheService.Get<List<StreamModel>>(key) ?? new List<StreamModel>();

            var updatedItem = values.FirstOrDefault(x => x.StreamId == streamId);

            if (updatedItem != null)
            {
                if (string.IsNullOrEmpty(remark))
                {
                    values.Remove(updatedItem);
                }
                else
                {
                    updatedItem.Remark = remark;
                }
            }
            else if (!string.IsNullOrEmpty(remark))
            {
                values.Add(new StreamModel { StreamId = streamId, Remark = remark });
            }

            cacheService.Set(key, values, new CacheItemOptions().SetSlidingExpiration(TimeSpan.FromMinutes(20)));

            return true;

        }

        public List<StreamModel> GetActiveStream()
        {
            return cacheService.Get<List<StreamModel>>(key);
        }
    }
}