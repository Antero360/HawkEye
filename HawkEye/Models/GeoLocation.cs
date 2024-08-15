using System.Collections.Generic;

namespace HawkEye
{
    class GeoLocation
    {
        public string City { get;  private set; }
        public string State { get; private set; }
        public string Country { get; private set; }
        public string Latitude { get; private set; }
        public string Longitude { get; private set; }

        public GeoLocation(Dictionary<string, string> ipInfo)
        {
            this.City = ipInfo["city"];
            this.State = ipInfo["regionName"];
            this.Country = ipInfo["country"];
            this.Latitude = ipInfo["lat"];
            this.Longitude = ipInfo["lon"];
        }
    }
}