using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace SCMKit.library
{
    class Utils
    {

        /**
        * Parse command line arguments
         * 
        * */
        public static Dictionary<string, string> ParseTheArguments(string[] args)
        {
            try
            {
                Dictionary<string, string> ret = new Dictionary<string, string>();
                if (args.Length % 2 == 0 && args.Length > 0)
                {
                    for (int i = 0; i < args.Length; i = i + 2)
                    {
                        ret.Add(args[i].Substring(1, args[i].Length - 1).ToLower(), args[i + 1]);

                    }
                }
                return ret;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] You specified duplicate switches. Check your command again. Exception: " + ex.ToString());
                return null;
            }

        } // end



        /**
        * print help
        * 
        * */
        public static void HelpMe()
        {
            Console.Write("\nPlease read the README page for proper usage of the tool.\n\n");


        } // end print help method



        /**
        * Generate module header
        * 
        * */
        public static string GenerateHeader(string module, string credential, string url, string options, string system)
        {
            string output = String.Empty;
            string delim = "==================================================";
            string authType = "";

            if (credential.Contains(":"))
            {
                authType = "Username/Password";
            }
            else
            {
                authType = "API Key";
            }

            output += "\n" + delim + "\n";
            output += "Module:\t\t" + module + "\n";
            output += "System:\t\t" + system + "\n";
            output += "Auth Type:\t" + authType + "\n";
            output += "Options:\t" + options + "\n";
            output += "Target URL:\t" + url + "\n\n";
            output += "Timestamp:\t" + DateTime.Now + "\n";
            output += delim + "\n";

            return output;
        }


        /**
         * Heartbeat request to indicate it is SCMKit being used for modules not using raw HTTP requests
         * 
         */
        public static async Task<WebResponse> HeartbeatRequest(string url)
        {



            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            WebResponse myWebResponse = null;

            try
            {
                var webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url);
                if (webRequest != null)
                {
                    // set header values
                    webRequest.Method = "GET";
                    webRequest.ContentType = "application/json";
                    webRequest.UserAgent = "SCMKIT-5dc493ada400c79dd318abbe770dac7c";

                    // get web response
                    myWebResponse = await webRequest.GetResponseAsync();


                }
            }

            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Could not perform heartbeat request. Exception: " + ex.ToString());
                Console.WriteLine("");
                Environment.Exit(1);
            }

            return myWebResponse;

        }


        /**
         * method to get all indexes where a value exists in a string
         * 
         * */
        public static int[] AllIndexesOf(string str, string substr, bool ignoreCase = false)
        {
            if (string.IsNullOrWhiteSpace(str) ||
                string.IsNullOrWhiteSpace(substr))
            {
                throw new ArgumentException("String or substring is not specified.");
            }

            var indexes = new List<int>();
            int index = 0;

            while ((index = str.IndexOf(substr, index, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)) != -1)
            {
                indexes.Add(index++);
            }

            return indexes.ToArray();
        }
       

    }

}
