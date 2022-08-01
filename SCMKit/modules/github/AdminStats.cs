using System;
using System.Threading.Tasks;
using Octokit;

namespace SCMKit.modules.github
{
    class AdminStats
    {

        public static async Task execute(string credential, string url, string option, string system)
        {

            // Generate module header
            Console.WriteLine(library.Utils.GenerateHeader("adminstats", credential, url, option, system));

            try
            {
                await library.Utils.HeartbeatRequest(url);

                GitHubClient client = library.GitHubUtils.AuthToGitHub(credential, url);
                var data = await client.Enterprise.AdminStats.GetStatisticsAll();

                // create table headers
                string tableHeader = string.Format("{0,16} | {1,16} | {2,16}", "Admin Users", "Suspended Users", "Total Users");
                Console.WriteLine(tableHeader);
                Console.WriteLine(new String('-', tableHeader.Length));

                Console.WriteLine("{0,16} | {1,16} | {2,16}", data.Users.AdminUsers, data.Users.SuspendedUsers, data.Users.TotalUsers);

                Console.WriteLine("");
                Console.WriteLine("");
                string tableHeader2 = string.Format("{0,16} | {1,16}", "Total Repos", "Total Wikis");
                Console.WriteLine(tableHeader2);
                Console.WriteLine(new String('-', tableHeader2.Length));

                Console.WriteLine("{0,16} | {1,16}", data.Repos.TotalRepos, data.Repos.TotalWikis);


                Console.WriteLine("");
                Console.WriteLine("");
                string tableHeader3 = string.Format("{0,16} | {1,20} | {2,16}", "Total Orgs", "Total Team Members", "Total Teams");
                Console.WriteLine(tableHeader3);
                Console.WriteLine(new String('-', tableHeader3.Length));

                Console.WriteLine("{0,16} | {1,20} | {2,16}", data.Orgs.TotalOrgs, data.Orgs.TotalTeamMembers, data.Orgs.TotalTeams);

                Console.WriteLine("");
                Console.WriteLine("");
                string tableHeader4 = string.Format("{0,16} | {1,16}", "Private Gists", "Public Gists");
                Console.WriteLine(tableHeader4);
                Console.WriteLine(new String('-', tableHeader4.Length));

                Console.WriteLine("{0,16} | {1,16}", data.Gists.PrivateGists, data.Gists.PublicGists);



            }

            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Could not retrieve listing of admin stats. Exception: " + ex.ToString());
                Console.WriteLine("");
            }


        }


    }
}
