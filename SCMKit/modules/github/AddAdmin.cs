using Octokit;
using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace SCMKit.modules.github
{
    class AddAdmin
    {

        public static async Task execute(string credential, string url, string options, string system)
        {

            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("addadmin", credential, url, options, system));

            try
            {
                await library.Utils.HeartbeatRequest(url);


                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

                GitHubClient client = library.GitHubUtils.AuthToGitHub(credential, url);
                await client.User.Administration.Promote(options);

                Console.WriteLine("[+] SUCCESS: The user " + options + " has been added to site admins");


            }

            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Could not add user provided to site admin role. Exception: " + ex.ToString());
                Console.WriteLine("");
            }


        }

    }
}
