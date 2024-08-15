using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace HawkEye
{
    static class Security
    {
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int description, int reservedValue);

        private static bool IsConnected()
        {
            int description;
            return InternetGetConnectedState(out description, 0);
        }

        public static bool CheckInternetConnection()
        {
            return IsConnected();
        }

        public static void EmailAdmin(string errorLog, string unauthUseLog, Machine machine)
        {
            StringBuilder bodyBuilder = new StringBuilder();
            bodyBuilder.AppendLine($"Date: {DateTime.Now}");
            bodyBuilder.AppendLine($"Machine: {machine.Name}");
            bodyBuilder.AppendLine($"Current Private IP: {machine.PrivateIP}");
            bodyBuilder.AppendLine($"Current Public IP: {machine.PublicIP}");
            bodyBuilder.AppendLine();
            bodyBuilder.AppendLine("GeoLocation");
            bodyBuilder.AppendLine($"City: {machine.Location.City}");
            bodyBuilder.AppendLine($"State: {machine.Location.State}");
            bodyBuilder.AppendLine($"Country: {machine.Location.Country}");
            bodyBuilder.AppendLine($"Latitude: {machine.Location.Latitude}");
            bodyBuilder.AppendLine($"Longitude: {machine.Location.Longitude}");
            bodyBuilder.AppendLine($"Please check the 'Hawkeye_Alert' file for any alerts that may not have been sent.");
            bodyBuilder.AppendLine($"'Hawkeye_Alert' File: {unauthUseLog}");
            bodyBuilder.AppendLine();
            bodyBuilder.AppendLine($"Please email the 'Hawkeye_ErrorLog' file to my developer for any bug fixes and updates.");
            bodyBuilder.AppendLine($"'Hawkeye_ErrorLog' File: {errorLog}");

            SmtpClient client = new SmtpClient();
            client.Port = Convert.ToInt32(ConfigurationManager.AppSettings["smtpPort"]);
            client.EnableSsl = true;
            MailMessage emailMessage = new MailMessage();
            emailMessage.To.Add(new MailAddress(ConfigurationManager.AppSettings["admin"]));
            emailMessage.Subject = "HAWKEYE ALERT - UNAUTHORIZED USE";
            emailMessage.IsBodyHtml = false;
            emailMessage.Body = bodyBuilder.ToString();
            client.Send(emailMessage);
        }

        public static string GetLocalIpAddress()
        {
            return RetieveLocalIpAddress();
        }

        private static string RetieveLocalIpAddress()
        {
            string privateIP = string.Empty;
            NetworkInterface[] networkInterfaceList = NetworkInterface.GetAllNetworkInterfaces()
                                                                      .Where(n => n.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 && n.OperationalStatus == OperationalStatus.Up).ToArray();
            foreach (NetworkInterface network in networkInterfaceList)
            {
                UnicastIPAddressInformation[] unicastIPs = network.GetIPProperties().UnicastAddresses
                                                                                    .Where(i => i.Address.AddressFamily == AddressFamily.InterNetwork).ToArray();
                foreach (UnicastIPAddressInformation ip in unicastIPs)
                {
                    privateIP = ip.Address.ToString();
                    break;
                }
            }
            return privateIP;
        }

        public static Machine GetMachineDetails()
        {
            return RetrieveMachineSpecs();
        }

        private static Machine RetrieveMachineSpecs()
        {
            Machine thisMachine = new Machine();
            return thisMachine;
        }

        public static Dictionary<string, string> GetGeoLocation() 
        {
            return GeoLocateAPI();
        }

        private static Dictionary<string, string> GeoLocateAPI() 
        {
            WebRequest request = WebRequest.Create(ConfigurationManager.AppSettings["geoLocationApi"]);
            WebResponse response = request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string json = reader.ReadToEnd().Trim();
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }

        public static void Log(string errorLog, string errorStack, string unauthUseLog, Machine machine)
        {
            GenerateUnauthorizedUseLog(unauthUseLog, "Possible VPN/Network Drop", machine);
            GenerateErrorLogEntry(errorLog, errorStack, machine);
        }

        private static void GenerateUnauthorizedUseLog(string unauthUseLog, string reason, Machine machine)
        {
            StringBuilder data = new StringBuilder();
            data.AppendLine();
            data.AppendLine("Subject: UNAUTHORIZED USE");
            data.AppendLine($"Date: {DateTime.Now}");
            data.AppendLine($"Reason: {reason}");
            data.AppendLine($"Machine: {machine.Name}");
            data.AppendLine($"Private IP: {machine.PrivateIP}");
            data.AppendLine($"Public IP: {machine.PublicIP}");
            data.AppendLine("GeoLocation");
            data.AppendLine($"City: {machine.Location.City}");
            data.AppendLine($"State: {machine.Location.State}");
            data.AppendLine($"Country: {machine.Location.Country}");
            data.AppendLine($"Latitude: {machine.Location.Latitude}");
            data.AppendLine($"Longitude: {machine.Location.Longitude}");
            data.AppendLine("<===============================>");

            WriteToFile(unauthUseLog, data.ToString());
        }

        private static void GenerateErrorLogEntry(string errorLog, string errorStack, Machine machine)
        {
            StringBuilder data = new StringBuilder();
            data.AppendLine();
            data.AppendLine("Subject: FATAL ERROR");
            data.AppendLine($"Date: {DateTime.Now}");
            data.AppendLine($"error: {errorStack}");
            data.AppendLine($"Machine: {machine.Name}");
            data.AppendLine("<===============================>");

            WriteToFile(errorLog, data.ToString());
        }

        private static void WriteToFile(string filePath, string content)
        {
            if (!File.Exists(filePath))
            {
                using (StreamWriter writer = File.CreateText(filePath))
                {
                    writer.WriteLine(content);
                }
            }
            else
            {
                File.WriteAllText(filePath, content);
            }
        }
    }
}