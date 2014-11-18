using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StreamingBooking.Models
{
    public class LicenseeModel
    {
        public int PartnerId { get; set; }

        public string PartnerName { get; set; }

        public string Authentication { get; set; }

        public string PathTest { get; set; }
    }
}