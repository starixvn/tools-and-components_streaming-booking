using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StreamingBooking.Models;

namespace StreamingBooking
{
    public class StreamingComparer : IEqualityComparer<StreamModel>
    {
        public bool Equals(StreamModel x, StreamModel y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.StreamId == y.StreamId;
        }

        public int GetHashCode(StreamModel stream)
        {
            if (Object.ReferenceEquals(stream, null)) return 0;

            int hashStreamId = stream.StreamId.GetHashCode();

            return hashStreamId;
        }
    }
}