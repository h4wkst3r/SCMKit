using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;

namespace SCMKit.modules.github
{
    class OrgList
    {
        public static async Task execute(string credential, string url, string options, string system)
        {

            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("listorg", credential, url, options, system));

            try
            {
                await library.Utils.HeartbeatRequest(url);

                GitHubClient client = library.GitHubUtils.AuthToGitHub(credential, url);

                var currentOrgs = await client.Organization.GetAllForCurrent();

                IEnumerator<Organization> currentUserOrgs = currentOrgs.GetEnumerator();

                // create table header
                string tableHeader = string.Format("{0,30} | {1,50}", "Name", "URL");
                Console.WriteLine(tableHeader);
                Console.WriteLine(new String('-', tableHeader.Length));

                while (currentUserOrgs.MoveNext())
                {


                    Console.WriteLine("{0,30} | {1,50}", currentUserOrgs.Current.Login, currentUserOrgs.Current.Url);


                }



            }

            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Could not retrieve listing of organizations. Exception: " + ex.ToString());
                Console.WriteLine("");
            }


        }

    }
}
