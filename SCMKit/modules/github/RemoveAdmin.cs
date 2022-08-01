using Octokit;
using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace SCMKit.modules.github
{
    class RemoveAdmin
    {

        public static async Task execute(string credential, string url, string options, string system)
        {


            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("removeadmin", credential, url, options, system));


            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

                await library.Utils.HeartbeatRequest(url);

                GitHubClient client = library.GitHubUtils.AuthToGitHub(credential, url);
                await client.User.Administration.Demote(options);

                Console.WriteLine("[+] SUCCESS: The user " + options + " has been removed from site admins");



            }

            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Could not remove user provided from site admin role. Exception: " + ex.ToString());
                Console.WriteLine("");
            }


        }
    }
}
