using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace HawkEye
{
    class Machine
    {
        public string Name { get; private set; }
        public string PrivateIP { get; private set; }
        public string PublicIP { get; private set; }
        public GeoLocation Location { get; private set; }

        public Machine()
        {
            this.Name = Environment.MachineName;
        }

        public void RefreshDetails()
        {
            GetSpecifications();
        }

        private void GetSpecifications() {
            this.PrivateIP = Security.GetLocalIpAddress();
            Dictionary<string, string> ipInfo = Security.GetGeoLocation();
            Location = new GeoLocation(ipInfo);
        }
    }
}