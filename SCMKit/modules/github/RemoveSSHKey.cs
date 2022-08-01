using Octokit;
using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;


namespace SCMKit.modules.github
{
    class RemoveSSHKey
    {

        public static async Task execute(string credential, string url, string options, string system)
        {

            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("removesshkey", credential, url, options, system));

            try
            {

                // if user didn't specify an ID, display message and return
                if (options.Equals(""))
                {
                    Console.WriteLine("");
                    Console.WriteLine("[-] ERROR: Must supply ID of SSH key to remove.");
                    Console.WriteLine("");
                    return;
                }

                await library.Utils.HeartbeatRequest(url);


                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

                GitHubClient client = library.GitHubUtils.AuthToGitHub(credential, url);


                await client.User.GitSshKey.Delete(Int32.Parse(options));

                Console.WriteLine("");
                Console.WriteLine("[+] SUCCESS: The user SSH key was successfully removed.");
                Console.WriteLine("");



            }

            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Could not remove user SSH key. Exception: " + ex.ToString());
                Console.WriteLine("");
            }


        }

    }
}
