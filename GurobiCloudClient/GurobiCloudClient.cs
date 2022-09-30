using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Net;
using System.IO;
using System.Web;

namespace GurobiCloud
{
    [DataContract]
    public class GurobiLicense
    {
        [DataMember]
        public string licenseId;

        [DataMember]
        public string credit;

        [DataMember]
        public string expiration;

        [DataMember]
        public string ratePlan;
    }

    [DataContract]
    public class GurobiMachine
    {
        [DataMember]
        public string _id;

        [DataMember]
        public string state;

        [DataMember]
        public string DNSName;

        [DataMember]
        public string machineType;

        [DataMember]
        public string createTime;

        [DataMember]
        public string region;

        [DataMember]
        public string licenseType;

        [DataMember]
        public int idleShutdown;

        [DataMember]
        public string licenseId;

        [DataMember]
        public string userPassword;
    }

    public class GurobiInstantCloudClient
    {
        public const string BASE_URL = "https://cloud.gurobi.com/api/";
        public string AccessId { get; set; }
        public string SecretKey { get; set; }

        public GurobiInstantCloudClient(string id = "Tn7KSHANzcPyNkXJC", string key = "t9yrr8CaQxylmapqkMdebA")
        {
            AccessId = id;
            SecretKey = key;
        }
        private string SignRequest(string raw_str)
        {
            HMACSHA1 myhmacsha1 = new HMACSHA1(Encoding.ASCII.GetBytes(SecretKey));
            byte[] byteArray = Encoding.UTF8.GetBytes(raw_str);
            byte[] hash = myhmacsha1.ComputeHash(byteArray);
            return Convert.ToBase64String(hash);
        }
        private string SendCommand(string command, Dictionary<string, string> paramDict)
        {
            var commandMethod = new Dictionary<string, string> {{"licenses", "GET"},
                                                                {"machines", "GET"},
                                                                {"launch",   "POST"},
                                                                {"kill",     "POST"}};
            string method = commandMethod[command];
            string requestStr = method;
            string urlStr = BASE_URL + command;
            string query = null;

            if (method.Equals("GET"))
                urlStr = urlStr + "?id=" + AccessId;

            paramDict["id"] = AccessId;
            foreach (KeyValuePair<string, string> pair in paramDict)
            {
                //string keyval = pair.Key + '=' + WebUtility.UrlEncode(pair.Value);
                string keyval = pair.Key + '=' + HttpUtility.UrlEncode(pair.Value);
                requestStr = requestStr + '&' + keyval;
                if (method.Equals("POST"))
                {
                    if (query == null)
                        query = keyval;
                    else
                        query = query + '&' + keyval;
                }
            }

            //  add date
            string now = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            requestStr = requestStr + '&' + now;

            // sign request
            string signature = SignRequest(requestStr);

            // construct HTTP request
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(urlStr);
            webRequest.Headers.Add("X-Gurobi-Signature", signature);
            webRequest.Headers.Add("X-Gurobi-Date", now);
            if (method.Equals("POST"))
            {
                byte[] postData = Encoding.UTF8.GetBytes(query);
                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.ContentLength = postData.Length;
                using (Stream sendStream = webRequest.GetRequestStream())
                {
                    sendStream.Write(postData, 0, postData.Length);
                }
            }

            string output = null;
            // get HTTP response
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)webRequest.GetResponse();

                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    output = reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                // print HTTP errors
                response = (HttpWebResponse)ex.Response;
                System.Console.WriteLine("Server Error: " + (int)response.StatusCode + " " + response.StatusDescription);
                using (var stream = ex.Response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    Console.WriteLine(reader.ReadToEnd());
                }
            }
            return output;
        }

        public GurobiLicense[] GetLicenses()
        {
            Dictionary<string, string> paramDict = new Dictionary<string, string>();
            string jsonStr = SendCommand("licenses", paramDict);
            var serializer = new DataContractJsonSerializer(typeof(GurobiLicense[]));
            GurobiLicense[] licenses = null;
            using (var tempStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonStr)))
            {
                licenses = (GurobiLicense[])serializer.ReadObject(tempStream);
            }
            return licenses;
        }

        private GurobiMachine[] ParseMachines(string jsonStr)
        {
            var serializer = new DataContractJsonSerializer(typeof(GurobiMachine[]));
            GurobiMachine[] machines = null;
            using (var tempStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonStr)))
            {
                machines = (GurobiMachine[])serializer.ReadObject(tempStream);
            }
            return machines;
        }

        public GurobiMachine[] GetMachines()
        {
            Dictionary<string, string> paramDict = new Dictionary<string, string>();
            string jsonStr = SendCommand("machines", paramDict);
            return ParseMachines(jsonStr);
        }

        public GurobiMachine[] KillMachines(string[] machineIds)
        {
            Dictionary<string, string> paramDict = new Dictionary<string, string>();
            string machineJSON = "[";
            for (int i = 0; i < machineIds.Length; i++)
            {
                machineJSON = machineJSON + '"' + machineIds[i] + '"';
                if (i < machineIds.Length - 1)
                {
                    machineJSON = machineJSON + ',';
                }
            }
            machineJSON = machineJSON + ']';
            paramDict.Add("machineIds", machineJSON);
            string jsonStr = SendCommand("kill", paramDict);
            return ParseMachines(jsonStr);
        }

        public GurobiMachine[] LaunchMachines(int numMachines, string licenseType, string licenseId, string userPassword,
                                              string region, int idleShutdown, string machineType, string GRBVersion)
        {
            Dictionary<string, string> paramDict = new Dictionary<string, string>();
            paramDict.Add("numMachines", numMachines.ToString());
            if (licenseType == null)
                paramDict.Add("licenseType", licenseType);
            if (licenseId == null)
                paramDict.Add("licenseId", licenseId);
            if (userPassword == null)
                paramDict.Add("userPassword", userPassword);
            if (region != null)
                paramDict.Add("region", region);
            if (idleShutdown != -1)
                paramDict.Add("idleShutdown", idleShutdown.ToString());
            if (machineType != null)
                paramDict.Add("machineType", machineType);
            if (GRBVersion != null)
                paramDict.Add("GRBVersion", GRBVersion);
            string jsonStr = SendCommand("launch", paramDict);
            return ParseMachines(jsonStr);
        }

        public GurobiMachine LaunchSingleMachine(ref string error)
        {
            GurobiMachine[] machinesLaunched = null;

            int numMachines = 1;
            string licenseType = null;
            string userPassword = null;
            string licenseId = null;
            string region = null;
            int idleShutdown = 60;
            string machineType = null;
            string GRBVersion = null;

            try
            {
                machinesLaunched = LaunchMachines(numMachines, licenseType, licenseId, userPassword,
                                    region, idleShutdown, machineType, GRBVersion);
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            if ((machinesLaunched != null) && (machinesLaunched.Length > 0))
                return machinesLaunched[0];
            else
                return null;
        }

        public GurobiMachine GetCurrentMachine(ref string error)
        {
            GurobiMachine[] machines = null;
            try
            {
                machines = GetMachines();
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            if ((machines != null) && (machines.Length > 0))
                return machines[0];
            else
                return null;
        }
    }
}
